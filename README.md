# KK_Growth

A BepInEx plugin for **Koikatsu** that adds a body growth/scaling system driven by H-scene events, using the [KKABMX](https://github.com/ManlyMarco/ABMX) bone modifier framework.

Originally based on [KoikatuGameplayMods](https://github.com/ManlyMarco/KoikatuGameplayMods) by ManlyMarco.

---

## Features

- **H-scene inflation** — characters visually grow during an H-scene as orgasms accumulate, and shrink on pull-out (optional).
- **Persistent growth** — orgasm count is saved to each character card and affects bone scaling (height, bust, hips) across sessions.
- **Growth target** — configure who grows: the heroine, the player character, or both.
- **Residual growth** — optional weekly passive growth for characters you have been intimate with.
- **Menstruation schedule overrides** — per-character safe/risky day settings with custom HUD icons.
- **Maker integration** — adjust growth week and enable/disable progression directly in the character maker.
- **Studio support** — timeline-compatible growth week slider for all selected characters.

---

## Configuration

All settings are exposed in BepInEx's ConfigurationManager (press **F1** in-game).

| Setting | Default | Description |
|---|---|---|
| Growth Target | `Both` | Who grows on inside finish: `Female`, `Player`, or `Both` |
| Orgasms to final size | `1000` | Total orgasms needed to reach maximum scaling |
| Height Scaling slider | `50` | Height growth modifier per orgasm |
| Breast Expansion slider | `10` | Bust growth modifier per orgasm |
| Ass Expansion slider | `10` | Hip growth modifier per orgasm |
| Enable H-scene growth | `true` | Toggle in-scene visual inflation |
| Scaling speed modifier | `1.0` | How fast the visual inflation animates |
| Open clothes at max inflation | `true` | Bursts clothes open when inflation peaks |
| Residual Growth | `false` | Passive growth each in-game week |
| Rate of Residual Growth | `1` | Equivalent orgasms added per week |
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
2. Place game/plugin DLLs in `../../plugins/BepInEx/` relative to the project root (matching the `.csproj` hint paths).
3. Open `KK_Growth.csproj` in Visual Studio or build with MSBuild:
   ```
   msbuild KK_Growth.csproj /p:Configuration=Release
   ```
4. Output is `bin/Release/KK_Growth.dll` — copy to `BepInEx/plugins/`.

---

## License

See original project: [KoikatuGameplayMods](https://github.com/ManlyMarco/KoikatuGameplayMods)
