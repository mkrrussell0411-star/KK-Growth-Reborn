# KK_Growth

A BepInEx plugin for **Koikatsu** that adds a body growth/scaling system driven by H-scene events, using the [KKABMX](https://github.com/ManlyMarco/ABMX) bone modifier framework.

Originally based on [KoikatuGameplayMods](https://github.com/ManlyMarco/KoikatuGameplayMods) by ManlyMarco.

---

## Features

- **H-scene inflation** — characters visually grow during an H-scene as orgasms accumulate, and shrink on pull-out (optional).
- **Persistent growth** — orgasm count is saved to each character card and affects bone scaling (height, bust, hips) across sessions.
- **Growth target** — configure who grows: the heroine, the player character, or both.
- **Residual growth** — optional weekly passive growth for characters you have been intimate with.
- **Residual decay** — optional weekly shrink for characters you haven't been with recently.
- **Menstruation schedule overrides** — per-character safe/risky day settings with custom HUD icons.
- **Maker integration** — adjust growth level and enable/disable progression directly in the character maker.
- **Studio support** — timeline-compatible growth slider for all selected characters.
- **AI Chat** — browser-based chat UI that lets you talk to the characters in-scene; the AI can trigger growth and shrink in real time.

---

## AI Chat

Press **F8** (configurable) while in-game to open the AI chat in your browser. The chat connects to any OpenAI-compatible API.

### Setup

1. Open the chat (F8) and click the gear icon ⚙.
2. Enter your **API Base URL** (e.g. `https://nano-gpt.com/api/v1` or `https://api.openai.com/v1`).
3. Enter your **API Key** and **Model** name.
4. Save and start chatting.

### How growth works

- The AI roleplays as the characters in the scene and naturally triggers growth or shrink by including a hidden `<growth>` tag in its responses.
- Growth amounts are **float values** in the range **0.05 – 1.0** per message (roughly 0.29 – 5.71 ft).
- Use **"player says"** at the start of any message to override AI behaviour — e.g. `player says grow Iroha a lot` or `player says shrink everyone`.
- Character ID 0 is always the male protagonist.

### Configuration (F1 → AI Chat section)

| Setting | Default | Description |
|---|---|---|
| API Key | *(blank)* | Your API key |
| API Base URL | `https://api.openai.com/v1` | Any OpenAI-compatible endpoint |
| Model | `gpt-4o` | Model name |
| Web UI Port | `7734` | Local port for the chat server |
| Open Chat Hotkey | `F8` | Hotkey to open the chat in your browser |
| Min Tokens | `0` | Minimum tokens per response (0 = disabled) |
| Max Tokens | `512` | Maximum tokens per response (0 = API default) |

<details>
<summary><strong>Deep dive — how the AI chat system works internally</strong></summary>

<br>

### Architecture overview

The AI chat system is split into two C# files compiled into the plugin DLL:

- **`AiChatServer.cs`** — a `MonoBehaviour` that runs an embedded HTTP server, proxies API calls to the LLM, and applies growth commands to the in-game characters.
- **`AiChatWebUi.cs`** — a static class that returns a complete self-contained single-page HTML application as a C# string, served at `http://localhost:7734/`.

When you press the hotkey, `System.Diagnostics.Process.Start` opens that URL in your default browser. No external files, no Unity UI, no overlay — just a local web server.

---

### Threading model

The game runs on Unity's main thread. The HTTP server cannot block it, so the plugin uses a dedicated background thread:

```
Background thread (AiChatServer._listenerThread)
  └─ HttpListener.GetContext() — blocks waiting for requests
  └─ ThreadPool.QueueUserWorkItem — handles each request on a pool thread

Pool thread (HandleGrow)
  └─ Enqueues a GrowthJob into _growthQueue
  └─ job.Done.WaitOne(3000) — blocks pool thread up to 3 seconds

Unity main thread (AiChatServer.Update(), runs every frame)
  └─ Dequeues GrowthJob, applies growth to the character controller
  └─ job.Done.Set() — unblocks the waiting pool thread
```

Growth **must** happen on the main thread because `PregnancyCharaController` and Unity's bone modifier framework (`KKABMX`) are not thread-safe. The `ManualResetEvent` pattern bridges the gap: the background thread enqueues work and parks itself, the main thread does the Unity work and signals completion, and the background thread sends the HTTP response back to the browser.

---

### HTTPS / TLS proxy

The browser page calls `/api/proxy/chat`, which the plugin forwards to the real LLM API over HTTPS. Koikatsu runs on Unity 5.x / 2018.x with Mono, whose `HttpWebRequest` stack has broken TLS 1.2 support on many systems — connections to modern APIs simply fail with a TLS handshake error.

The fix: use `UnityEngine.Networking.UnityWebRequest` (backed by Unity's native libcurl, which handles TLS 1.2 correctly) instead of Mono's `HttpWebRequest`. Because `UnityWebRequest` is coroutine-based, the proxy runs as a Unity coroutine on the main thread:

```
Pool thread (HandleProxyChat)
  └─ Enqueues a ProxyJob into _proxyQueue
  └─ job.Done.WaitOne(60000) — blocks up to 60 seconds

Unity main thread (Update)
  └─ Dequeues ProxyJob, calls StartCoroutine(RunProxyJob(pj))

Coroutine (RunProxyJob) — still on main thread, yielded across frames
  └─ yield return www.Send()  ← Unity's libcurl does the HTTPS request
  └─ job.Done.Set() — unblocks the pool thread
```

The `Send()` / `isError` API is the Unity 5.x form of `UnityWebRequest` — newer Unity versions renamed them to `SendWebRequest()` / `isNetworkError`, but KK uses the older form. The reference must come from `UnityEngine.dll` (not `UnityEngine.Networking.dll`, which is UNET multiplayer).

---

### Character detection and ID assignment

Every frame, `RefreshCharCache()` calls `FindObjectsOfType<PregnancyCharaController>()` and rebuilds the character list. IDs are simply the 0-based index of each controller in the returned array. **Character ID 0 is always the male protagonist** because Unity's object enumeration consistently returns the player character first in KK's scene setup.

Each character entry exposes:
- `id` — used by the AI to target growth commands
- `name` — from `ChaControl.chaFile.parameter.fullname`
- `type` — `"heroine"` or `"player"`
- `bust` — `GetInflationEffectPercent()`, a 0–1 float representing current visual inflation
- `week` — `Data.Week`, the persistent cumulative growth level

The sidebar in the UI polls `/api/characters` every 6 seconds to keep stats current.

---

### System prompt and roleplay design

Each time the user sends a message, the frontend rebuilds a **system prompt** from scratch using the latest character list. This means the AI always sees up-to-date growth stats, even mid-conversation.

The prompt structure:

1. **Role declaration** — tells the AI it's roleplaying as the listed characters with distinct personalities.
2. **Character list** — name, ID, type, current growth %, and week value for every character in the scene.
3. **Growth mechanic rules** — mandatory `<growth>` tag format, amount range (0.05–1.0), and the instruction to default to 0.25.
4. **`player says` override** — a special command prefix that bypasses all AI judgment and executes a direct instruction.

The conversation `history` array stored in the browser only contains the **display text** of each message — the `<growth>` tag is stripped before being pushed to history. This means the AI never sees its own past growth commands in context, which keeps the history clean and avoids confusing the model.

---

### Growth tag parsing

The AI appends a tag at the end of its response in this format:

```
<growth>{"commands":[{"id":0,"amount":0.25}]}</growth>
```

The frontend extracts it with a simple regex:

```js
var GROWTH_RE = /<growth>([\s\S]*?)<\/growth>/;
```

The matched JSON is parsed and each command is sent as a separate `POST /api/grow` request with `{"id": N, "amount": F}`. The server parses the float amount with `ParseJsonFloat`, which handles decimal points and uses `InvariantCulture` so the `.` separator is always correct regardless of the user's Windows locale.

The amount is clamped server-side to `[0.05, NutTotal]` for positive values and `[-NutTotal, -0.05]` for negative, so the AI can never send a zero or an absurdly large value even if it hallucinates one.

---

### Growth units and feet conversion

The internal growth unit is a `float` on the same scale as `NutTotal` (default 1000). The UI converts to feet for display:

```
0.35 units = 2 feet  →  1 unit = 5.714 ft
feet = Math.abs(amount) * (2 / 0.35)
```

This is purely cosmetic — only the notification bubble uses feet. The sidebar still shows the raw `week` value and a 0–100% bar.

---

### `SetInflationDirect` and the `_inflationChange` problem

The visual size of a character is determined by:

```
GetInflationEffectPercent() = (InflationAmount + _inflationChange) / NutTotal
IsInflated = (InflationAmount + _inflationChange) > 0.01
```

`_inflationChange` is a transient float that the game uses for smooth animation — `AddInflation(n)` sets `_inflationChange -= n` so the visual starts at the old value and animates toward the new one. The problem: if `_inflationChange` is a large negative value (from a game orgasm), then even setting `InflationAmount = 1` leaves `IsInflated = false`, so KKABMX's `GetEffect()` returns null and no bones are modified.

The fix is `SetInflationDirect(float newAmount)`, added to `PregnancyCharaController`:

```csharp
public void SetInflationDirect(float newAmount)
{
    this.InflationAmount = newAmount;
    this._inflationChange = 0f;  // clear animation state
}
```

AI growth commands use this instead of `AddInflation`, guaranteeing `IsInflated = (newAmount > 0.01)` regardless of any pending animation from the game's own inflation system.

---

### Float growth amounts

`Week` and `InflationAmount` were changed from `int` to `float` throughout to support sub-unit amounts like `0.25`. Key implications:

- **Save format** — `PregnancyData.Save()` serializes via reflection. Old saves that stored `Week` as `int` will silently fail to load the Week field (type mismatch in `field.SetValue`) and reset to `0`. This is a one-time data loss on first run after the update.
- **Game compatibility** — `++`/`--` on float is valid C# (increments by 1.0). All game systems that used `Week++` continue to work.
- **Timeline** — the Studio timeline interpolable was updated from `<int>` to `<float>` so keyframes interpolate smoothly between fractional values.
- **Maker slider** — previously snapped to integers via `Mathf.RoundToInt`; now passes float values through directly. The display label still shows a rounded integer since the slider's `ValueToString` formats with `Mathf.RoundToInt(f).ToString()`.

---

### `player says` override command

If the user's message begins with `player says` (case-insensitive), the system prompt instructs the AI to treat it as a direct game command and **always** include a `<growth>` tag with the commanded effect, overriding the normal 0.05–1.0 range. This lets you say things like:

- `player says grow Iroha 50 units` — large positive amount
- `player says shrink everyone` — large negative amounts on all characters
- `player says stop growing` — AI omits the `<growth>` tag entirely

If the AI responds to a `player says` message without a `<growth>` tag, the UI shows a red warning pill: `⚠ AI responded without a <growth> tag — try rephrasing or check the model.`

---

### API compatibility

The proxy forwards the request body unchanged to `{API_BASE_URL}/chat/completions`. Any API that implements the OpenAI `/v1/chat/completions` endpoint works, including:

- OpenAI (`https://api.openai.com/v1`)
- nano-gpt (`https://nano-gpt.com/api/v1`)
- Ollama (`http://localhost:11434/v1`)
- LM Studio (`http://localhost:1234/v1`)
- Any other OpenAI-compatible proxy

The frontend sends `stream: false` and accepts both plain JSON (`choices[0].message.content`) and SSE line-delimited format (`data: {...}`) since some providers return streaming format even when not requested.

When **Max Tokens** is set to a non-zero value, `max_tokens` is added to the request body. When **Min Tokens** is non-zero, `min_tokens` is added as well. Both are omitted entirely when set to 0, leaving the API to use its own defaults. Note that `min_tokens` is not supported by all providers and may be silently ignored.

---

### Recommended models

Any instruction-following model works, but models that are better at consistently following structured output instructions (the hidden `<growth>` tag) will be more reliable:

- **GPT-4o / GPT-4o-mini** — very consistent tag output
- **Claude 3.5 / 3.7** — highly reliable, good roleplay quality
- **Llama 3.1 70B+** (via Ollama or compatible host) — works well locally
- Smaller models (7B–13B) tend to forget the `<growth>` tag more often, especially mid-conversation

If growth commands stop triggering, check the BepInEx log for `[AiChat]` entries — a missing tag from the AI is the most common cause, not a bug in the plugin.

</details>

---

## Growth Configuration

All settings are exposed in BepInEx's ConfigurationManager (press **F1** in-game).

| Setting | Default | Description |
|---|---|---|
| Growth Target | `Both` | Who grows on inside finish: `Female`, `Player`, or `Both` |
| Orgasms to final size | `1000` | Total orgasms needed to reach maximum scaling |
| Height Scaling slider | `50` | Height growth modifier |
| Breast Expansion slider | `10` | Bust growth modifier |
| Ass Expansion slider | `10` | Hip growth modifier |
| Enable H-scene growth | `true` | Toggle in-scene visual inflation |
| Scaling speed modifier | `1.0` | How fast the visual inflation animates |
| Open clothes at max inflation | `true` | Bursts clothes open when inflation peaks |
| Residual Growth | `false` | Passive growth each in-game week |
| Rate of Residual Growth | `1` | Equivalent orgasms added per week |
| Residual Decay | `false` | Passive shrink for inactive characters each week |
| Rate of Residual Decay | `1` | Equivalent orgasms subtracted per week |
| H-scene Shrink | `true` | Shrink on pull-out |
| Use custom safe/risky icons | `true` | Custom pregnancy-aware menstruation icons in H-scenes |
| Show pregnancy icon early | `false` | Show status icon immediately instead of after a delay |

---

## Dependencies

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx)
- [KKAPI](https://github.com/ManlyMarco/KKAPI) v1.30+
- [KKABMX](https://github.com/ManlyMarco/ABMX) v4.4.1+
- [BepisPlugins](https://github.com/IllusionMods/BepisPlugins) (ExtensibleSaveFormat)

---

## Building

1. Clone the repository.
2. Place game/plugin DLLs in `BepInEx/` relative to the project root (matching the `.csproj` hint paths).
3. Build with MSBuild:
   ```
   msbuild KK_Growth.csproj /p:Configuration=Release
   ```
4. Output is `bin/Release/KK_Growth.dll` — copy to `BepInEx/plugins/`.

---

## License

See original project: [KoikatuGameplayMods](https://github.com/ManlyMarco/KoikatuGameplayMods)
