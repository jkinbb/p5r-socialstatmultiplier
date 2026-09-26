# Social Stat Multiplier — Reloaded-II mod for Persona 5 Royal

A small quality-of-life mod that multiplies **every social stat point you gain**
(Knowledge, Guts, Proficiency, Kindness, Charm). The multiplier is configurable in the mod's
settings, the mod can be toggled on and off at any moment, and **no game files are modified** —
it only works with the values the game itself keeps in memory.

Русская версия мода входит в тот же архив: папка `RU_Русская_версия`, подробная инструкция —
[README_RU.md](README_RU.md).

## Features

| | |
|---|---|
| Multiplier | 1…10, set in the launcher's mod settings or in `Config.json` (fractions allowed, e.g. 2.5) |
| Toggle | on/off switch in the mod list, `"Enabled": false` in the settings, or the Suspend/Resume buttons |
| Applied live | settings are picked up while the game is running — no restart |
| Coverage | every source of points: classroom answers, books, part-time jobs, gym, cooking, confidant meetings and ranks, story bonuses |
| Safety | never fights the game: a jump larger than 100 points (save load) is not multiplied; the 16-bit counter is never allowed to overflow (32767) |
| No file edits | nothing is written to the game folder, the CPK archives or your saves |

## Install

1. Download the release archive from the [Releases](../../releases) page and unpack it.
2. Copy the folder `P5R.SocialStatMultiplier` from `EN_English_version/Mods/` into `<Reloaded-II>\Mods\`.
3. Start Reloaded-II, find **Social Stat Multiplier** and make sure its switch is on.
4. Make sure the library mod **Reloaded.Memory.SigScan.ReloadedII** is installed and enabled
   (it ships with Reloaded-II).
5. Launch the game through Reloaded-II.

## Setting the multiplier

* **Launcher:** cog / *Configure* next to the mod → **Point multiplier**.
* **File:** `<Reloaded-II>\User\ModConfigs\P5R.SocialStatMultiplier\Config.json`

  ```json
  {
    "Enabled": true,
    "Multiplier": 2,
    "VerboseLog": false
  }
  ```

| Setting | Meaning |
|---|---|
| `Enabled` | master switch; `false` = vanilla 1:1 gains |
| `Multiplier` | how many times to multiply the points gained (1…10) |
| `VerboseLog` | log every single gain (debugging) |

## How it works

Social stat points are kept in a single array of five 16-bit values. The mod locates that array in
the game's memory with a signature scan, reads it every 20 ms and, as soon as the game has awarded
points, multiplies the increase and writes the result back. Because the value itself is multiplied,
the mod works with *every* source of points instead of a hand-picked list of scenes, and it never
invents points — it only scales what the game already gave you.

## Building from source

Requires the .NET SDK 8 (or newer) and python3.

```bash
# Russian build
dotnet build -c Release P5R.SocialStatMultiplier/P5R.SocialStatMultiplier.csproj

# English build
dotnet build -c Release -p:ModLang=EN P5R.SocialStatMultiplier/P5R.SocialStatMultiplier.csproj
```

`build.sh` does everything at once — both builds, the automated tests against both of them and the
release archive in `dist/`:

```bash
DOTNET=dotnet bash build.sh
```

The same script is used by GitHub Actions (`.github/workflows/build.yml`), so every tag produces a
ready-to-install archive.

## Tests

The test project (`P5R.SocialStatMultiplier.Tests`) creates the real mod object, hands it a stand-in
array of points and plays the role of the game: **19 of 19 scenarios pass** — ×2/×3 gains, repeated
awards, no self-acceleration, save loading, value resets, changing the multiplier on the fly,
`Enabled = false`, Suspend/Resume, the call sequence the mod loader uses before the mod starts,
the 32767 overflow guard, multiplier ×1 and clean shutdown.

The only thing that cannot be verified outside a real game is the memory signature of a particular
`p5r.exe` build. If it does not match, the mod writes `Social stat points signature was not found`
to the log and changes nothing at all.

## Credits

* [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) — the mod loader and the
  mod template this project is based on.
* [Reloaded.Memory.SigScan](https://github.com/Reloaded-Project/Reloaded.Memory) — signature scanning.
* The memory signature of the social stat array comes from the open-source
  [p5rpc.SocialStatTracker](https://github.com/AnimatedSwine37/p5rpc.SocialStatTracker) by
  **AnimatedSwine37** — thanks for publishing it.

## Disclaimer

An unofficial fan-made mod. Not affiliated with or endorsed by Atlus or Sega. No game assets,
executables or extracted data are included in this repository or in the release archives — only
original code. *Persona 5 Royal* and all related marks are trademarks of their respective owners.

## License

[MIT](LICENSE)
