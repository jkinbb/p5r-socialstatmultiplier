# Social Stat Multiplier — Reloaded-II mod for Persona 5 Royal

A small quality-of-life mod that multiplies **every social stat point you gain**
(Knowledge, Guts, Proficiency, Kindness, Charm). The multiplier is configurable in the mod's
settings, the mod can be toggled on and off at any moment, and **no game files are modified** —
it only works with the values the game itself keeps in memory.

Русская версия мода входит в тот же архив: папка `RU_Русская_версия`. Полный перевод этой страницы приведён ниже; отдельный файл: [README_RU.md](README_RU.md).

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

---

# Множитель социальных очков — мод для Reloaded-II (Persona 5 Royal)

Небольшой мод, который умножает **все получаемые очки социальных характеристик**
(Знания, Смелость, Ловкость, Доброта, Обаяние). Множитель задаётся в настройках мода,
мод включается и выключается в любой момент, а **файлы игры не изменяются** — он работает
только со значениями, которые игра сама держит в памяти.

Английская версия входит в тот же архив: папка `EN_English_version`, инструкция — [README.md](README.md).

## Возможности

| | |
|---|---|
| Множитель | 1…10, задаётся в настройках мода в лоадере или в `Config.json` (можно дробное, например 2.5) |
| Включение/выключение | тумблер мода в списке, `"Enabled": false` в настройках или кнопки Suspend/Resume |
| Применяется на ходу | настройки подхватываются во время игры, перезапуск не нужен |
| Покрытие | все источники очков: ответы в классе, книги, подработки, спортзал, готовка, встречи и ранги конфидантов, сюжетные бонусы |
| Безопасность | скачок больше 100 очков (загрузка сейва) не умножается; переполнение 16-битного счётчика (32767) не допускается |
| Ничего не правит в файлах | ни в папке игры, ни в CPK-архивах, ни в сейвах |

## Установка

1. Скачать архив из раздела [Releases](../../releases) и распаковать.
2. Папку `P5R.SocialStatMultiplier` из `RU_Русская_версия/Mods/` скопировать в `<Reloaded-II>\Mods\`.
3. Запустить Reloaded-II, найти «Множитель социальных очков» и убедиться, что тумблер включён.
4. Проверить, что установлен и включён мод-библиотека **Reloaded.Memory.SigScan.ReloadedII**
   (идёт в комплекте с Reloaded-II).
5. Запускать игру через Reloaded-II как обычно.

## Как менять множитель

* **В лоадере:** шестерёнка / «Configure» рядом с модом → «Множитель получаемых очков».
* **В файле:** `<Reloaded-II>\User\ModConfigs\P5R.SocialStatMultiplier\Config.json`

  ```json
  {
    "Enabled": true,
    "Multiplier": 2,
    "VerboseLog": false
  }
  ```

| Настройка | Смысл |
|---|---|
| `Enabled` | общий выключатель; `false` — обычное начисление 1 к 1 |
| `Multiplier` | во сколько раз умножать получаемые очки (1…10) |
| `VerboseLog` | писать в лог каждое начисление (для отладки) |

## Как это работает

Очки социальных характеристик лежат в памяти игры одним массивом из пяти 16-битных значений.
Мод находит этот массив по сигнатуре, раз в 20 мс читает его и, как только игра начислила очки,
умножает прирост и записывает обратно. Поэтому работают **все** источники очков, а не только
избранные сцены; при этом мод ничего не выдумывает — он масштабирует только то, что игра уже
выдала.

## Сборка из исходников

Нужен .NET SDK 8 (или новее) и python3.

```bash
# русская версия
dotnet build -c Release P5R.SocialStatMultiplier/P5R.SocialStatMultiplier.csproj

# английская версия
dotnet build -c Release -p:ModLang=EN P5R.SocialStatMultiplier/P5R.SocialStatMultiplier.csproj
```

`build.sh` делает всё сразу — обе сборки, автотесты против них и релизный архив в `dist/`:

```bash
DOTNET=dotnet bash build.sh
```

Тот же скрипт запускается в GitHub Actions (`.github/workflows/build.yml`), поэтому каждый тег
автоматически даёт готовый к установке архив.

## Тесты

Тестовый проект (`P5R.SocialStatMultiplier.Tests`) создаёт настоящий объект мода, подсовывает ему
поддельный массив очков и играет роль игры: **19 из 19 сценариев проходят** — ×2/×3, повторные
начисления, отсутствие саморазгона, загрузка сейва, сброс значения, смена множителя на ходу,
`Enabled = false`, Suspend/Resume, вызовы лоадера до старта мода, защита от переполнения 32767,
множитель ×1 и корректная остановка.

Единственное, что нельзя проверить вне реальной игры, — сигнатура памяти конкретной сборки
`p5r.exe`. Если она не совпадёт, мод напишет в лог `Сигнатура массива очков соц. характеристик
не найдена` и не сделает ничего.

## Благодарности

* [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) — загрузчик модов и шаблон,
  на котором основан проект.
* [Reloaded.Memory.SigScan](https://github.com/Reloaded-Project/Reloaded.Memory) — поиск сигнатур.
* Сигнатура массива социальных очков взята из открытого мода
  [p5rpc.SocialStatTracker](https://github.com/AnimatedSwine37/p5rpc.SocialStatTracker)
  (автор **AnimatedSwine37**) — спасибо, что он выложен в открытый доступ.

## Дисклеймер

Неофициальный фанатский мод. Не связан с Atlus и Sega и не одобрен ими. Ни в репозитории, ни в
архивах релизов нет файлов игры, исполняемых файлов или извлечённых данных — только собственный
код. *Persona 5 Royal* и связанные названия принадлежат их правообладателям.

## Лицензия

[MIT](LICENSE)
