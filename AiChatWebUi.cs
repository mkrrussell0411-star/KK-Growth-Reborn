namespace KK_Growth
{
    internal static class AiChatWebUi
    {
        internal static string GetHtml()
        {
            return @"<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width,initial-scale=1'>
<title>KK Growth AI Chat</title>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
html,body{height:100%;overflow:hidden}
body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#0d0d1a;color:#dde;font-size:14px;line-height:1.5}

/* Layout */
#app{display:flex;height:100vh}

/* Sidebar */
#sidebar{width:210px;min-width:210px;background:#13132b;border-right:1px solid #2a2a50;display:flex;flex-direction:column;overflow:hidden}
.sb-header{display:flex;align-items:center;justify-content:space-between;padding:14px 12px 10px;border-bottom:1px solid #1e1e40}
.sb-title{font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:1.2px;color:#e0407f}
#refresh-btn{background:none;border:1px solid #2a2a50;color:#777;border-radius:5px;width:26px;height:26px;cursor:pointer;font-size:14px;line-height:1;display:flex;align-items:center;justify-content:center;transition:all .2s}
#refresh-btn:hover{border-color:#e0407f;color:#e0407f}
#char-list{flex:1;overflow-y:auto;padding:10px 8px}
.char-card{background:#1a1a35;border:1px solid #25255a;border-radius:8px;padding:10px;margin-bottom:8px;transition:border-color .2s}
.char-card:hover{border-color:#e0407f44}
.char-name{font-weight:600;color:#eef;font-size:13px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.char-type{font-size:10px;color:#e0407f;text-transform:uppercase;letter-spacing:.8px;margin-top:1px}
.char-stat{font-size:11px;color:#666;margin-top:5px}
.bar-wrap{background:#0d0d1a;border-radius:3px;height:5px;margin-top:5px;overflow:hidden}
.bar-fill{height:5px;border-radius:3px;background:linear-gradient(90deg,#e0407f,#ff7eb3);transition:width .6s ease}
.no-chars{color:#444;font-size:12px;text-align:center;padding:30px 10px;font-style:italic}

/* Main */
#main{flex:1;display:flex;flex-direction:column;overflow:hidden}
#topbar{background:#13132b;border-bottom:1px solid #1e1e40;padding:0 16px;height:52px;display:flex;align-items:center;justify-content:space-between;flex-shrink:0}
.app-title{font-size:16px;font-weight:700;color:#e0407f;letter-spacing:.5px}
.app-sub{font-size:11px;color:#555;margin-left:10px}
#cfg-btn{background:none;border:1px solid #2a2a50;color:#777;border-radius:6px;padding:6px 10px;cursor:pointer;font-size:16px;transition:all .2s}
#cfg-btn:hover{border-color:#e0407f;color:#e0407f}

/* Messages */
#messages{flex:1;overflow-y:auto;padding:16px;display:flex;flex-direction:column;gap:10px}
.msg{max-width:78%;border-radius:12px;padding:10px 14px;word-wrap:break-word;line-height:1.6;animation:fadeIn .2s ease}
@keyframes fadeIn{from{opacity:0;transform:translateY(6px)}to{opacity:1;transform:translateY(0)}}
.msg-user{align-self:flex-end;background:#1b3a6b;color:#e8f0ff;border-bottom-right-radius:4px}
.msg-ai{align-self:flex-start;background:#1a1a35;color:#dde;border:1px solid #2a2a50;border-bottom-left-radius:4px}
.msg-ai .char-label{font-size:11px;color:#e0407f;margin-bottom:4px;font-weight:700;text-transform:uppercase;letter-spacing:.5px}
.msg-info{align-self:center;background:transparent;color:#444;font-size:11px;font-style:italic;padding:4px 10px;max-width:90%;text-align:center}
.msg-growth{align-self:center;background:#0d2b1a;border:1px solid #1e6b3a;color:#4ecf82;font-size:12px;padding:7px 14px;border-radius:20px;max-width:90%;text-align:center}
.msg-error{align-self:center;background:#2b0d0d;border:1px solid #6b1e1e;color:#e05050;font-size:12px;padding:7px 14px;border-radius:20px;max-width:90%;text-align:center}
.typing-dots{display:inline-block}
.typing-dots span{display:inline-block;width:6px;height:6px;background:#666;border-radius:50%;margin:0 2px;animation:dot-bounce .9s infinite}
.typing-dots span:nth-child(2){animation-delay:.15s}
.typing-dots span:nth-child(3){animation-delay:.3s}
@keyframes dot-bounce{0%,80%,100%{transform:translateY(0)}40%{transform:translateY(-6px)}}

/* Input */
#input-area{padding:12px 16px;background:#13132b;border-top:1px solid #1e1e40;display:flex;gap:10px;align-items:flex-end;flex-shrink:0}
#user-input{flex:1;background:#0d0d1a;border:1px solid #2a2a50;color:#dde;border-radius:10px;padding:10px 14px;font-size:14px;resize:none;font-family:inherit;line-height:1.5;max-height:120px;transition:border-color .2s}
#user-input:focus{outline:none;border-color:#e0407f}
#user-input::placeholder{color:#444}
#send-btn{background:#e0407f;color:#fff;border:none;border-radius:10px;padding:10px 18px;cursor:pointer;font-weight:700;font-size:14px;white-space:nowrap;transition:background .2s;align-self:flex-end}
#send-btn:hover:not(:disabled){background:#c42f6a}
#send-btn:disabled{background:#333;color:#666;cursor:not-allowed}

/* Config Modal */
.modal-backdrop{position:fixed;inset:0;background:rgba(0,0,0,.75);z-index:1000;display:flex;align-items:center;justify-content:center;animation:fadeIn .15s ease}
.modal-backdrop.hidden{display:none}
.modal{background:#13132b;border:1px solid #2a2a50;border-radius:14px;padding:26px;width:440px;max-width:95vw}
.modal h2{color:#e0407f;font-size:18px;margin-bottom:20px;display:flex;align-items:center;gap:8px}
.field{margin-bottom:14px}
.field label{display:block;font-size:11px;color:#888;text-transform:uppercase;letter-spacing:.8px;margin-bottom:5px;font-weight:600}
.field input{width:100%;background:#0d0d1a;border:1px solid #2a2a50;color:#dde;border-radius:8px;padding:9px 12px;font-size:14px;transition:border-color .2s;font-family:inherit}
.field input:focus{outline:none;border-color:#e0407f}
.field input::placeholder{color:#444}
.field .hint{font-size:11px;color:#555;margin-top:4px}
.modal-footer{display:flex;gap:10px;justify-content:flex-end;margin-top:20px}
.btn-primary{background:#e0407f;color:#fff;border:none;border-radius:8px;padding:9px 22px;cursor:pointer;font-weight:700;font-size:14px;transition:background .2s}
.btn-primary:hover{background:#c42f6a}
.btn-secondary{background:transparent;color:#888;border:1px solid #2a2a50;border-radius:8px;padding:9px 18px;cursor:pointer;font-size:14px;transition:all .2s}
.btn-secondary:hover{border-color:#e0407f;color:#e0407f}

/* Scrollbar */
::-webkit-scrollbar{width:5px}
::-webkit-scrollbar-track{background:#0d0d1a}
::-webkit-scrollbar-thumb{background:#2a2a50;border-radius:3px}
::-webkit-scrollbar-thumb:hover{background:#e0407f}
</style>
</head>
<body>
<div id='app'>
  <aside id='sidebar'>
    <div class='sb-header'>
      <span class='sb-title'>In Scene</span>
      <button id='refresh-btn' title='Refresh character list'>&#8635;</button>
    </div>
    <div id='char-list'><div class='no-chars'>Loading...</div></div>
  </aside>
  <section id='main'>
    <header id='topbar'>
      <div style='display:flex;align-items:baseline;gap:8px'>
        <span class='app-title'>Growth AI Chat</span>
        <span class='app-sub'>powered by your LLM</span>
      </div>
      <button id='cfg-btn' title='Configure AI settings'>&#9881;</button>
    </header>
    <div id='messages' role='log' aria-live='polite'></div>
    <div id='input-area'>
      <textarea id='user-input' placeholder='Talk to the characters...' rows='2'></textarea>
      <button id='send-btn'>Send</button>
    </div>
  </section>
</div>

<div id='cfg-modal' class='modal-backdrop hidden'>
  <div class='modal'>
    <h2>&#9881; AI Configuration</h2>
    <div class='field'>
      <label for='cfg-url'>API Base URL</label>
      <input id='cfg-url' type='text' placeholder='https://api.openai.com/v1'>
      <div class='hint'>Any OpenAI-compatible endpoint (OpenAI, Ollama, LM Studio, etc.)</div>
    </div>
    <div class='field'>
      <label for='cfg-key'>API Key</label>
      <input id='cfg-key' type='password' placeholder='sk-... (leave blank for local APIs)'>
    </div>
    <div class='field'>
      <label for='cfg-model'>Model</label>
      <input id='cfg-model' type='text' placeholder='gpt-4o'>
    </div>
    <div class='modal-footer'>
      <button class='btn-secondary' id='cfg-cancel'>Cancel</button>
      <button class='btn-primary' id='cfg-save'>Save &amp; Close</button>
    </div>
  </div>
</div>

<script>
(function() {
  'use strict';

  var cfg = { url: 'https://api.openai.com/v1', key: '', model: 'gpt-4o' };
  var history = [];
  var busy = false;
  var chars = [];

  var GROWTH_RE = /<growth>([\s\S]*?)<\/growth>/;

  /* ---- init ---- */
  function init() {
    loadConfig().then(function() {
      return refreshChars();
    }).then(function() {
      addInfo('Chat with the characters. The AI will decide if and when to trigger growth.');
    });
    setInterval(refreshChars, 6000);
  }

  function loadConfig() {
    return fetch('/api/config').then(function(r) {
      return r.json();
    }).then(function(d) {
      if (d.apiUrl) cfg.url = d.apiUrl;
      if (d.apiKey) cfg.key = d.apiKey;
      if (d.model) cfg.model = d.model;
    }).catch(function() {});
  }

  /* ---- characters ---- */
  function refreshChars() {
    return fetch('/api/characters').then(function(r) {
      return r.json();
    }).then(function(data) {
      chars = data;
      renderChars();
    }).catch(function() {});
  }

  function renderChars() {
    var el = document.getElementById('char-list');
    if (!chars || chars.length === 0) {
      el.innerHTML = '<div class=\'no-chars\'>No characters detected.<br>Enter a scene first.</div>';
      return;
    }
    var html = '';
    for (var i = 0; i < chars.length; i++) {
      var c = chars[i];
      var pct = Math.round(c.bust * 100);
      html += '<div class=\'char-card\'>';
      html += '<div class=\'char-name\'>' + esc(c.name) + '</div>';
      html += '<div class=\'char-type\'>' + esc(c.type) + ' &bull; ID ' + c.id + '</div>';
      html += '<div class=\'char-stat\'>Week ' + c.week + ' &bull; ' + pct + '% grown</div>';
      html += '<div class=\'bar-wrap\'><div class=\'bar-fill\' style=\'width:' + pct + '%\'></div></div>';
      html += '</div>';
    }
    el.innerHTML = html;
  }

  /* ---- system prompt ---- */
  function buildPrompt() {
    var charList = '';
    if (!chars || chars.length === 0) {
      charList = '(No characters detected in scene. Enter an H-scene or free roam.)';
    } else {
      for (var i = 0; i < chars.length; i++) {
        var c = chars[i];
        charList += '- ' + c.name + ' (ID: ' + c.id + ', ' + c.type +
                    ', current growth: ' + Math.round(c.bust * 100) + '%, week: ' + c.week + ')\n';
      }
    }

    return 'You are roleplaying as the characters listed below in an interactive growth fantasy game.\n' +
           'Give each character a distinct personality and voice. Speak AS the characters.\n' +
           'IMPORTANT: Character ID 0 is always the MALE protagonist. Do not refer to them as female.\n\n' +
           'Characters currently in the scene:\n' + charList + '\n' +
           '=== GROWTH MECHANIC ===\n' +
           'MANDATORY: Every response that involves any growth or shrink MUST end with a <growth> tag.\n' +
           'There are NO exceptions. If you describe growth or shrink in text, you MUST include the tag.\n' +
           'The tag is parsed by the game engine — without it, nothing actually happens in-game.\n\n' +
           'CRITICAL RULE: amounts are DECIMAL and small — range is 0.05 to 1.0 per message.\n' +
           'Common values: 0.1, 0.25, 0.5, 0.75, 1.0. Default to 0.25 unless context suggests more.\n' +
           'Growth should feel slow, subtle, and gradual — like the characters are barely noticing small changes.\n' +
           'Never exceed 1.0 unless matthew says to. Use negative values to shrink.\n' +
           'Positive amount = grow. Negative amount = shrink.\n\n' +
           'Format — append this at the very end of your response (the game strips it before display):\n' +
           '<growth>{""commands"":[{""id"":CHARACTER_ID,""amount"":AMOUNT}]}</growth>\n\n' +
           'Example — Iroha shrinks by 0.25:\n' +
           'Iroha shivers. ""Wait... did I just shrink a little?"" <growth>{""commands"":[{""id"":0,""amount"":-0.25}]}</growth>\n\n' +
           'Example — two characters both grow by 0.5:\n' +
           'They both feel a strange warmth. <growth>{""commands"":[{""id"":0,""amount"":0.5},{""id"":1,""amount"":0.5}]}</growth>\n\n' +
           'You should trigger growth frequently and naturally — small amounts often, rather than large amounts rarely.\n\n' +
           '=== MATTHEW SAYS COMMANDS ===\n' +
           'If ANY user message starts with the phrase matthew says (case-insensitive), treat it as a direct game command.\n' +
           'You MUST obey it immediately and ALWAYS include the <growth> tag with the commanded effect.\n' +
           'Matthew says commands override ALL other rules including the tiny-amount rule.\n\n' +
           'Examples with required output:\n' +
           '  User: matthew says shrink Iroha 1 unit\n' +
           '  You: *Iroha feels a brief shiver.* ""Mm?"" <growth>{""commands"":[{""id"":0,""amount"":-1.0}]}</growth>\n\n' +
           '  User: matthew says grow everyone a lot\n' +
           '  You: *A wave of warmth fills the room.* <growth>{""commands"":[{""id"":0,""amount"":20.0},{""id"":1,""amount"":20.0}]}</growth>\n\n' +
           '  User: matthew says stop growing\n' +
           '  You: (roleplay response, NO <growth> tag)\n\n' +
           'After a matthew says command, return to normal character roleplay.\n' +
           'Be creative, playful, and immersive. This is a growth expansion fantasy scenario.';
  }

  /* ---- send message ---- */
  function sendMessage() {
    if (busy) return;
    var inputEl = document.getElementById('user-input');
    var text = inputEl.value.trim();
    if (!text) return;

    inputEl.value = '';
    autoResize(inputEl);
    addUserMsg(text);
    history.push({ role: 'user', content: text });

    setBusy(true);
    var typingEl = addTyping();

    var messages = [{ role: 'system', content: buildPrompt() }].concat(history);
    // send stream:false for maximum compatibility; we handle both formats below
    var body = JSON.stringify({ model: cfg.model, messages: messages, stream: false });

    fetch('/api/proxy/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: body
    }).then(function(resp) {
      if (!resp.ok) {
        return resp.text().then(function(t) {
          var msg = t.slice(0, 400);
          try { var obj = JSON.parse(t); if (obj.error) msg = obj.error.message || msg; } catch(x) {}
          throw new Error('API error ' + resp.status + ': ' + msg);
        });
      }

      // Read the full response body then detect format
      return resp.text().then(function(raw) {
        typingEl.remove();
        var aiEl = addAiPlaceholder();
        var full = '';

        // Try plain JSON first (non-streaming response)
        try {
          var obj = JSON.parse(raw);
          if (obj.error) throw new Error(obj.error.message || JSON.stringify(obj.error));
          if (obj.choices && obj.choices[0]) {
            var msg = obj.choices[0].message || obj.choices[0].delta || {};
            full = msg.content || '';
          }
        } catch(e) {
          // Not plain JSON — try SSE format (data: {...} lines)
          var lines = raw.split('\n');
          for (var i = 0; i < lines.length; i++) {
            var line = lines[i].trim();
            if (line.indexOf('data: ') !== 0) continue;
            var chunk = line.slice(6).trim();
            if (chunk === '[DONE]') continue;
            try {
              var parsed = JSON.parse(chunk);
              if (parsed.error) throw new Error(parsed.error.message || JSON.stringify(parsed.error));
              var delta = '';
              if (parsed.choices && parsed.choices[0]) {
                var c = parsed.choices[0];
                delta = (c.delta && c.delta.content) || (c.message && c.message.content) || '';
              }
              full += delta;
            } catch(ex) {}
          }
        }

        if (!full) throw new Error('Empty response — check your model name and API URL in settings.');

        var match = GROWTH_RE.exec(full);
        var display = stripGrowthTag(full).trim();
        aiEl.innerHTML = formatMsg(display);
        history.push({ role: 'assistant', content: display });
        scrollBottom();
        if (match) return applyGrowth(match[1]);
        // Warn when a matthew says command produced no growth tag
        var lastUser = history.length >= 2 ? history[history.length - 2] : null;
        if (lastUser && lastUser.role === 'user' &&
            lastUser.content.toLowerCase().indexOf('matthew says') === 0) {
          addError('⚠ AI responded without a <growth> tag — try rephrasing or check the model.');
        }
      });

    }).then(function() {
      setBusy(false);
      return refreshChars();
    }).catch(function(e) {
      try { typingEl.remove(); } catch(x) {}
      addError('Error: ' + (e.message || e));
      if (history.length && history[history.length - 1].role === 'user')
        history.pop();
      setBusy(false);
    });
  }

  /* ---- growth ---- */
  function applyGrowth(jsonStr) {
    var data;
    try { data = JSON.parse(jsonStr); } catch(e) { return; }
    var commands = data.commands;
    if (!commands || !commands.length) return;

    var promises = [];
    for (var i = 0; i < commands.length; i++) {
      promises.push(applyOne(commands[i]));
    }
    return Promise.all(promises);
  }

  function applyOne(cmd) {
    var charName = 'Character ' + cmd.id;
    for (var i = 0; i < chars.length; i++) {
      if (chars[i].id === cmd.id) { charName = chars[i].name; break; }
    }
    return fetch('/api/grow', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ id: cmd.id, amount: cmd.amount })
    }).then(function(r) {
      return r.json();
    }).then(function(res) {
      if (res.ok) {
        var dir = res.direction || (cmd.amount >= 0 ? 'grew' : 'shrank');
        var feet = (Math.abs(cmd.amount) * (2 / 0.35)).toFixed(2);
        addGrowth('✨ ' + esc(charName) + ' ' + dir + ' by ' + feet + ' ft!');
      } else
        addGrowth('⚠ Growth failed for ' + esc(charName) + ': ' + esc(res.error || '?'));
    }).catch(function() {
      addGrowth('⚠ Could not send growth for ' + esc(charName));
    });
  }

  /* ---- config modal ---- */
  function openCfg() {
    document.getElementById('cfg-url').value = cfg.url;
    document.getElementById('cfg-key').value = cfg.key;
    document.getElementById('cfg-model').value = cfg.model;
    document.getElementById('cfg-modal').classList.remove('hidden');
  }

  function closeCfg() {
    document.getElementById('cfg-modal').classList.add('hidden');
  }

  function saveCfg() {
    cfg.url   = document.getElementById('cfg-url').value.trim();
    cfg.key   = document.getElementById('cfg-key').value.trim();
    cfg.model = document.getElementById('cfg-model').value.trim();
    fetch('/api/config', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ apiUrl: cfg.url, apiKey: cfg.key, model: cfg.model })
    }).catch(function() {});
    closeCfg();
    addInfo('Configuration saved.');
  }

  /* ---- helpers ---- */
  function stripGrowthTag(s) {
    return s.replace(GROWTH_RE, '');
  }

  function formatMsg(s) {
    return s
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\n/g, '<br>');
  }

  function esc(s) {
    if (!s) return '';
    return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
  }

  function scrollBottom() {
    var m = document.getElementById('messages');
    m.scrollTop = m.scrollHeight;
  }

  function setBusy(val) {
    busy = val;
    document.getElementById('send-btn').disabled = val;
  }

  function autoResize(el) {
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 120) + 'px';
  }

  function addUserMsg(text) {
    var el = document.createElement('div');
    el.className = 'msg msg-user';
    el.innerHTML = formatMsg(text);
    document.getElementById('messages').appendChild(el);
    scrollBottom();
  }

  function addAiPlaceholder() {
    var el = document.createElement('div');
    el.className = 'msg msg-ai';
    el.innerHTML = '<div class=\'char-label\'>Characters</div><span>...</span>';
    document.getElementById('messages').appendChild(el);
    scrollBottom();
    return el;
  }

  function addTyping() {
    var el = document.createElement('div');
    el.className = 'msg msg-ai';
    el.innerHTML = '<div class=\'typing-dots\'><span></span><span></span><span></span></div>';
    document.getElementById('messages').appendChild(el);
    scrollBottom();
    return el;
  }

  function addInfo(text) {
    var el = document.createElement('div');
    el.className = 'msg msg-info';
    el.textContent = text;
    document.getElementById('messages').appendChild(el);
    scrollBottom();
  }

  function addGrowth(html) {
    var el = document.createElement('div');
    el.className = 'msg msg-growth';
    el.innerHTML = html;
    document.getElementById('messages').appendChild(el);
    scrollBottom();
  }

  function addError(text) {
    var el = document.createElement('div');
    el.className = 'msg msg-error';
    el.textContent = text;
    document.getElementById('messages').appendChild(el);
    scrollBottom();
  }

  /* ---- event wiring ---- */
  document.getElementById('send-btn').addEventListener('click', sendMessage);
  document.getElementById('cfg-btn').addEventListener('click', openCfg);
  document.getElementById('cfg-save').addEventListener('click', saveCfg);
  document.getElementById('cfg-cancel').addEventListener('click', closeCfg);
  document.getElementById('refresh-btn').addEventListener('click', refreshChars);
  document.getElementById('cfg-modal').addEventListener('click', function(e) {
    if (e.target === this) closeCfg();
  });
  document.getElementById('user-input').addEventListener('keydown', function(e) {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
  });
  document.getElementById('user-input').addEventListener('input', function() {
    autoResize(this);
  });

  init();
})();
</script>
</body>
</html>";
        }
    }
}
