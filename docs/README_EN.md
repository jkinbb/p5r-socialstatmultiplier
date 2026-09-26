# Social Stat Multiplier — Reloaded-II mod for Persona 5 Royal

Multiplies **all** social stat points you gain (Knowledge, Guts, Proficiency, Kindness, Charm).
The multiplier is set in the mod's settings, the mod can be enabled or disabled at any moment,
and **no game files are modified** — it only works with the values the game already keeps in memory.

---

## Install

1. Unpack the archive.
2. Copy the folder `P5R.SocialStatMultiplier` into `<Reloaded-II>\Mods\`.
   Inside it you should see `P5R.SocialStatMultiplier.dll` and `ModConfig.json`.
3. Start Reloaded-II, find **Social Stat Multiplier** in the mod list and make sure its switch is ON.
4. Make sure **Reloaded.Memory.SigScan.ReloadedII** is installed and enabled — this mod needs it
   (it normally ships with Reloaded-II; if it is missing, install it from the mod list).
5. Launch the game through Reloaded-II as usual.

## Setting the multiplier

* **In the launcher:** click the cog / *Configure* next to the mod → **Point multiplier** → a number from 1 to 10.
* **In the file:** `<Reloaded-II>\User\ModConfigs\P5R.SocialStatMultiplier\Config.json`

  ```json
  {
    "Enabled": true,
    "Multiplier": 2,
    "VerboseLog": false
  }
  ```

  Both settings are applied **on the fly** — you do not have to restart the game.

| Setting | Meaning |
|---|---|
| Enable mod | master switch; unchecked = vanilla 1:1 gains |
| Point multiplier | how many times to multiply the points gained (1…10, fractions allowed) |
| Verbose log | write every point gain to the Reloaded-II log (debugging only) |

## Enable / disable at any moment

* the mod switch in the launcher — applies on the next game launch;
* `"Enabled": false` in the settings — applies **immediately**, even in the middle of a scene;
* the **Suspend / Resume** buttons — pause and resume while the game is running.

## What exactly gets multiplied

Social stat points live in one array of five numbers in the game's memory. The mod finds that array
by a signature, reads it every 20 ms and, as soon as the game has awarded points, multiplies the
increase and writes the result back. That is why **every** source of points is covered: classroom
answers, books, part-time jobs, the gym, cooking, confidant meetings and ranks, story bonuses.
The mod never invents points — it only multiplies what the game actually gave you.

## Limits and details

* A single jump of more than 100 points (a save load) is **not** multiplied.
* The internal values are 16-bit; the mod never lets them overflow (max 32767), otherwise the game
  would reset the rank.
* Ranks and thresholds stay vanilla — you simply fill them faster.
* If the stat value in the menu does not update instantly, close and reopen the menu.
* Battle EXP, money, HP/SP and confidant points are **not** touched by this mod.

## Troubleshooting

1. **The mod is not in the list** — check that the dll and `ModConfig.json` are exactly in
   `Mods\P5R.SocialStatMultiplier\` (no extra nested folder).
2. **“Incompatible with this application”** — in `ModConfig.json` replace `"p5r.exe"` in
   `"SupportedAppId"` with the name of your own exe.
3. **In the Reloaded-II log** (Log tab / `Log.txt`), search for `[Social Stat Multiplier]`:
   * `Social stat points array found: 0x...` — everything works;
   * `Social stat points signature was not found` — your `p5r.exe` is a different build;
     nothing is broken, just report it to the mod author (the signature can be fixed quickly);
   * `Could not get the signature scanner controller` — `Reloaded.Memory.SigScan.ReloadedII`
     is not installed or not enabled.
4. **Too many points** — set the multiplier to 1 or uncheck “Enable mod”; gains go back to vanilla.
   Points you already have, of course, stay.

## Uninstall

Delete the folder `Mods\P5R.SocialStatMultiplier`. Nothing is left behind in game files or saves
(the mod only wrote to memory).

## Tested

Built from source and verified with an automated harness that feeds the real mod code a stand-in
array of points and plays the role of the game: **19 of 19 scenarios pass** — ×2 and ×3 gains,
repeated awards, no self-acceleration, save loading, value resets, changing the multiplier on the
fly, `Enabled = false`, Suspend/Resume, a call sequence that the mod loader uses, the 32767
overflow guard, multiplier ×1 and shutdown. Full log: `test_results_EN.txt` in this folder.

The only thing that cannot be tested outside your PC is the memory signature of your particular
`p5r.exe`. If it differs, the mod says so in the log and changes nothing.

Source code is in the `Исходники` folder of the archive (C#, .NET 7). Both language versions are
built from the same sources: `dotnet build -c Release` (Russian) and
`dotnet build -c Release -p:ModLang=EN` (English).
