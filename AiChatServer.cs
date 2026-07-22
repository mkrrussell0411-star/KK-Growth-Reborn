using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Manager;
using UnityEngine;
using UnityEngine.Networking;

namespace KK_Growth
{
    public class AiChatServer : MonoBehaviour
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private volatile bool _running;

        private readonly Queue<GrowthJob> _growthQueue = new Queue<GrowthJob>();
        private readonly object _growthLock = new object();

        private readonly Queue<ProxyJob> _proxyQueue = new Queue<ProxyJob>();
        private readonly object _proxyLock = new object();

        private readonly List<CharEntry> _charCache = new List<CharEntry>();
        private volatile string _charJson = "[]";

        private void Start()
        {
            int port = GrowthPlugin.AiServerPort.Value;
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:" + port + "/");
                _listener.Start();
                _running = true;
                _listenerThread = new Thread(ListenLoop);
                _listenerThread.IsBackground = true;
                _listenerThread.Name = "AiChatServer";
                _listenerThread.Start();
                GrowthPlugin.Logger.LogInfo("[AiChat] Server running at http://localhost:" + port + "/");
                GrowthPlugin.Logger.LogInfo("[AiChat] Open hotkey: " + GrowthPlugin.AiChatHotkey.Value);
            }
            catch (Exception ex)
            {
                GrowthPlugin.Logger.LogError("[AiChat] Failed to start server: " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            _running = false;
            try { if (_listener != null) _listener.Stop(); } catch { }
        }

        private void Update()
        {
            if (GrowthPlugin.AiChatHotkey.Value.IsDown())
                System.Diagnostics.Process.Start("http://localhost:" + GrowthPlugin.AiServerPort.Value + "/");

            RefreshCharCache();

            // Process growth jobs
            while (true)
            {
                GrowthJob gj;
                lock (_growthLock)
                {
                    if (_growthQueue.Count == 0) break;
                    gj = _growthQueue.Dequeue();
                }
                try
                {
                    if (gj.Id >= 0 && gj.Id < _charCache.Count)
                    {
                        CharEntry c = _charCache[gj.Id];
                        float max = GrowthPlugin.NutTotal.Value;
                        float weekBefore = c.Ctrl.Data.Week;
                        float infBefore = c.Ctrl.InflationAmount;
                        float newWeek = Mathf.Clamp(weekBefore + gj.Amount, 0f, max);
                        c.Ctrl.Data.Week = newWeek;
                        // SetInflationDirect resets _inflationChange to 0 so IsInflated is
                        // always (newWeek > 0), regardless of game orgasm animation state.
                        c.Ctrl.SetInflationDirect(newWeek);
                        c.Ctrl.SaveData();
                        GrowthPlugin.Logger.LogInfo("[AiChat] " + c.Name + " amount=" + gj.Amount
                            + " week " + weekBefore + "->" + newWeek
                            + " inf " + infBefore + "->" + c.Ctrl.InflationAmount
                            + " isInflated=" + c.Ctrl.IsInflated);
                        string dir = (gj.Amount >= 0) ? "grew" : "shrank";
                        gj.Result = "{\"ok\":true,\"name\":\"" + EscapeJson(c.Name) + "\",\"amount\":" + gj.Amount + ",\"direction\":\"" + dir + "\"}";
                    }
                    else
                    {
                        GrowthPlugin.Logger.LogWarning("[AiChat] Invalid char id=" + gj.Id + " cacheSize=" + _charCache.Count);
                        gj.Result = "{\"ok\":false,\"error\":\"invalid character id\"}";
                    }
                }
                catch (Exception ex)
                {
                    gj.Result = "{\"ok\":false,\"error\":\"" + EscapeJson(ex.Message) + "\"}";
                }
                finally
                {
                    gj.Done.Set();
                }
            }

            // Start proxy coroutines for pending proxy jobs
            ProxyJob pj;
            lock (_proxyLock)
            {
                pj = (_proxyQueue.Count > 0) ? _proxyQueue.Dequeue() : null;
            }
            if (pj != null)
                StartCoroutine(RunProxyJob(pj));
        }

        private IEnumerator RunProxyJob(ProxyJob job)
        {
            GrowthPlugin.Logger.LogInfo("[AiChat] Proxy -> " + job.Url);

            byte[] bodyBytes = Encoding.UTF8.GetBytes(job.Body);
            UnityWebRequest www = new UnityWebRequest(job.Url, "POST");
            www.uploadHandler = new UploadHandlerRaw(bodyBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(job.ApiKey))
                www.SetRequestHeader("Authorization", "Bearer " + job.ApiKey);

            yield return www.Send();

            job.ResponseCode = (int)www.responseCode;
            if (www.isError)
            {
                string detail = www.downloadHandler != null ? www.downloadHandler.text : "";
                GrowthPlugin.Logger.LogError("[AiChat] UnityWebRequest error: " + www.error
                    + " (HTTP " + www.responseCode + ") body: " + detail);
                job.ResponseBody = string.IsNullOrEmpty(detail)
                    ? "{\"error\":{\"message\":\"" + EscapeJson(www.error) + "\"}}"
                    : detail;
            }
            else
            {
                job.ResponseBody = www.downloadHandler.text;
                GrowthPlugin.Logger.LogInfo("[AiChat] Proxy OK " + www.responseCode
                    + " (" + (job.ResponseBody ?? "").Length + " chars)");
            }

            job.Done.Set();
        }

        private void RefreshCharCache()
        {
            _charCache.Clear();
            var sb = new StringBuilder("[");
            int id = 0;
            try
            {
                PregnancyCharaController[] ctrls = UnityEngine.Object.FindObjectsOfType<PregnancyCharaController>();
                Game game = Singleton<Game>.Instance;
                SaveData.Player player = (game != null) ? game.Player : null;

                foreach (PregnancyCharaController ctrl in ctrls)
                {
                    if (ctrl == null) continue;
                    string name = "Character";
                    string type = "heroine";
                    try
                    {
                        ChaControl chaCtrl = ctrl.GetComponent<ChaControl>();
                        if (chaCtrl != null && chaCtrl.chaFile != null && chaCtrl.chaFile.parameter != null)
                        {
                            string n = chaCtrl.chaFile.parameter.fullname;
                            if (!string.IsNullOrEmpty(n)) name = n;
                        }
                        if (player != null && player.chaCtrl == chaCtrl)
                            type = "player";
                    }
                    catch { }

                    _charCache.Add(new CharEntry(name, type, ctrl));
                    if (id > 0) sb.Append(',');
                    sb.Append("{\"id\":").Append(id)
                      .Append(",\"name\":\"").Append(EscapeJson(name)).Append("\"")
                      .Append(",\"type\":\"").Append(type).Append("\"")
                      .Append(",\"bust\":").Append(ctrl.GetInflationEffectPercent().ToString("F2"))
                      .Append(",\"week\":").Append(ctrl.Data.Week).Append("}");
                    id++;
                }
            }
            catch { }
            sb.Append(']');
            _charJson = sb.ToString();
        }

        private void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    HttpListenerContext ctx = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(HandleRequestWI, ctx);
                }
                catch (HttpListenerException)
                {
                    if (!_running) break;
                }
                catch (Exception ex)
                {
                    if (_running) GrowthPlugin.Logger.LogError("[AiChat] Listener error: " + ex.Message);
                }
            }
        }

        private void HandleRequestWI(object state)
        {
            HandleRequest((HttpListenerContext)state);
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            HttpListenerResponse resp = ctx.Response;
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            resp.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

            if (ctx.Request.HttpMethod == "OPTIONS")
            {
                resp.StatusCode = 204;
                resp.Close();
                return;
            }

            string path = ctx.Request.Url.AbsolutePath;
            string method = ctx.Request.HttpMethod;

            try
            {
                if ((path == "/" || path == "/index.html") && method == "GET")
                    RespondText(ctx, "text/html; charset=utf-8", AiChatWebUi.GetHtml());
                else if (path == "/api/characters" && method == "GET")
                    RespondText(ctx, "application/json", _charJson);
                else if (path == "/api/grow" && method == "POST")
                    HandleGrow(ctx);
                else if (path == "/api/config" && method == "GET")
                    HandleGetConfig(ctx);
                else if (path == "/api/config" && method == "POST")
                    HandleSetConfig(ctx);
                else if (path == "/api/proxy/chat" && method == "POST")
                    HandleProxyChat(ctx);
                else
                {
                    resp.StatusCode = 404;
                    resp.Close();
                }
            }
            catch (Exception ex)
            {
                GrowthPlugin.Logger.LogError("[AiChat] Request error on " + path + ": " + ex.Message);
                try { resp.StatusCode = 500; resp.Close(); } catch { }
            }
        }

        private void HandleGrow(HttpListenerContext ctx)
        {
            string body = ReadBody(ctx.Request);
            int id = ParseJsonInt(body, "id", -1);
            float raw = ParseJsonFloat(body, "amount", 1f);
            float max = GrowthPlugin.NutTotal.Value;
            float amount = (raw >= 0f)
                ? Math.Max(0.05f, Math.Min(raw, max))
                : Math.Max(-max, Math.Min(raw, -0.05f));

            GrowthJob job = new GrowthJob(id, amount);
            lock (_growthLock) _growthQueue.Enqueue(job);
            bool signaled = job.Done.WaitOne(3000);
            RespondText(ctx, "application/json", signaled ? (job.Result ?? "{\"ok\":false}") : "{\"ok\":false,\"error\":\"timeout\"}");
        }

        private void HandleGetConfig(HttpListenerContext ctx)
        {
            RespondText(ctx, "application/json",
                "{\"apiKey\":\"" + EscapeJson(GrowthPlugin.AiApiKey.Value) +
                "\",\"apiUrl\":\"" + EscapeJson(GrowthPlugin.AiApiUrl.Value) +
                "\",\"model\":\"" + EscapeJson(GrowthPlugin.AiModel.Value) +
                "\",\"minTokens\":" + GrowthPlugin.AiMinTokens.Value +
                ",\"maxTokens\":" + GrowthPlugin.AiMaxTokens.Value + "}");
        }

        private void HandleSetConfig(HttpListenerContext ctx)
        {
            string body = ReadBody(ctx.Request);
            string apiKey = ParseJsonString(body, "apiKey");
            string apiUrl = ParseJsonString(body, "apiUrl");
            string model = ParseJsonString(body, "model");
            if (apiKey != null) GrowthPlugin.AiApiKey.Value = apiKey;
            if (apiUrl != null) GrowthPlugin.AiApiUrl.Value = apiUrl;
            if (model != null) GrowthPlugin.AiModel.Value = model;
            int minTokens = ParseJsonInt(body, "minTokens", -1);
            int maxTokens = ParseJsonInt(body, "maxTokens", -1);
            if (minTokens >= 0) GrowthPlugin.AiMinTokens.Value = minTokens;
            if (maxTokens >= 0) GrowthPlugin.AiMaxTokens.Value = maxTokens;
            RespondText(ctx, "application/json", "{\"ok\":true}");
        }

        private void HandleProxyChat(HttpListenerContext ctx)
        {
            string body = ReadBody(ctx.Request);
            string url = GrowthPlugin.AiApiUrl.Value.TrimEnd('/') + "/chat/completions";
            string apiKey = GrowthPlugin.AiApiKey.Value;

            ProxyJob job = new ProxyJob(url, body, apiKey);
            lock (_proxyLock) _proxyQueue.Enqueue(job);
            bool signaled = job.Done.WaitOne(60000);

            if (!signaled)
            {
                GrowthPlugin.Logger.LogError("[AiChat] Proxy timed out after 60s");
                RespondText(ctx, "application/json", "{\"error\":{\"message\":\"Request timed out after 60 seconds\"}}");
                return;
            }

            ctx.Response.StatusCode = (job.ResponseCode > 0) ? job.ResponseCode : 200;
            RespondText(ctx, "application/json", job.ResponseBody ?? "{\"error\":{\"message\":\"empty response\"}}");
        }

        private static void RespondText(HttpListenerContext ctx, string contentType, string body)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                ctx.Response.ContentType = contentType;
                ctx.Response.ContentLength64 = bytes.Length;
                ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                ctx.Response.OutputStream.Close();
            }
            catch { }
        }

        private static string ReadBody(HttpListenerRequest req)
        {
            using (StreamReader r = new StreamReader(req.InputStream, Encoding.UTF8))
                return r.ReadToEnd();
        }

        internal static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private static int ParseJsonInt(string json, string key, int def)
        {
            string kk = "\"" + key + "\"";
            int i = json.IndexOf(kk);
            if (i < 0) return def;
            i += kk.Length;
            while (i < json.Length && (json[i] == ':' || json[i] == ' ')) i++;
            int e = i;
            if (e < json.Length && json[e] == '-') e++;
            while (e < json.Length && char.IsDigit(json[e])) e++;
            int result;
            return int.TryParse(json.Substring(i, e - i), out result) ? result : def;
        }

        private static float ParseJsonFloat(string json, string key, float def)
        {
            string kk = "\"" + key + "\"";
            int i = json.IndexOf(kk);
            if (i < 0) return def;
            i += kk.Length;
            while (i < json.Length && (json[i] == ':' || json[i] == ' ')) i++;
            int e = i;
            if (e < json.Length && json[e] == '-') e++;
            while (e < json.Length && (char.IsDigit(json[e]) || json[e] == '.')) e++;
            float result;
            return float.TryParse(json.Substring(i, e - i),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out result) ? result : def;
        }

        private static string ParseJsonString(string json, string key)
        {
            string kk = "\"" + key + "\"";
            int i = json.IndexOf(kk);
            if (i < 0) return null;
            i += kk.Length;
            while (i < json.Length && (json[i] == ':' || json[i] == ' ')) i++;
            if (i >= json.Length || json[i] != '"') return null;
            i++;
            var sb = new StringBuilder();
            while (i < json.Length && json[i] != '"')
            {
                if (json[i] == '\\' && i + 1 < json.Length)
                {
                    i++;
                    switch (json[i])
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        default: sb.Append(json[i]); break;
                    }
                }
                else sb.Append(json[i]);
                i++;
            }
            return sb.ToString();
        }

        private class CharEntry
        {
            public string Name;
            public string Type;
            public PregnancyCharaController Ctrl;
            public CharEntry(string name, string type, PregnancyCharaController ctrl)
            { Name = name; Type = type; Ctrl = ctrl; }
        }

        private class GrowthJob
        {
            public int Id;
            public float Amount;
            public string Result;
            public ManualResetEvent Done = new ManualResetEvent(false);
            public GrowthJob(int id, float amount) { Id = id; Amount = amount; }
        }

        private class ProxyJob
        {
            public string Url;
            public string Body;
            public string ApiKey;
            public string ResponseBody;
            public int ResponseCode;
            public ManualResetEvent Done = new ManualResetEvent(false);
            public ProxyJob(string url, string body, string apiKey)
            { Url = url; Body = body; ApiKey = apiKey; }
        }
    }
}
