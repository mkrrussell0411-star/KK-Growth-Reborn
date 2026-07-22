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
- Use **"matthew says"** at the start of any message to override AI behaviour — e.g. `matthew says grow Iroha a lot` or `matthew says shrink everyone`.
- Character ID 0 is always the male protagonist.

### Configuration (F1 → AI Chat section)

| Setting | Default | Description |
|---|---|---|
| API Key | *(blank)* | Your API key |
| API Base URL | `https://api.openai.com/v1` | Any OpenAI-compatible endpoint |
| Model | `gpt-4o` | Model name |
| Web UI Port | `7734` | Local port for the chat server |
| Open Chat Hotkey | `F8` | Hotkey to open the chat in your browser |

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
