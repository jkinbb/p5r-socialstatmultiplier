# Social Stat Multiplier — Reloaded-II mod for Persona 5 Royal

A small quality-of-life mod that multiplies **every social stat point you gain**
(Knowledge, Guts, Proficiency, Kindness, Charm). The multiplier is configurable in the mod's
settings, the mod can be toggled on and off at any moment, and **no game files are modified** —
it only works with the values the game itself keeps in memory.

Русская версия этой страницы: [README_RU.md](README_RU.md)

![Social Stat Multiplier](P5R.SocialStatMultiplier/Preview.png)

## Features

| | |
|---|---|
| Multiplier | 1…10, set in the launcher's mod settings or in `Config.json` (fractions allowed, e.g. 2.5) |
| Toggle | on/off switch in the mod list, `"Enabled": false` in the settings, or the Suspend/Resume buttons |
| Applied live | settings are picked up while the game is running — no restart |
| Coverage | every source of points: classroom answers, books, part-time jobs, gym, cooking, confidant meetings and ranks, story bonuses |
| Safety | never fights the game: a jump larger than 100 points (save load) is not multiplied; the 16-bit counter is never allowed to overflow (32767) |
| No file edits | nothing is written to the game folder, the CPK archives or your saves |
| Updates | the launcher offers new versions automatically (see below) |

## Which build to install

The release contains two builds that are identical inside — they differ only in the language of
the launcher text (mod name, setting labels, log messages):

| Build | Launcher name | Use |
|---|---|---|
| `P5R.SocialStatMultiplier_EN.zip` | Social Stat Multiplier | English |
| `P5R.SocialStatMultiplier_RU.zip` | Множитель социальных очков | Russian |
| `P5R.SocialStatMultiplier_RU_EN.zip` | both of the above inside one archive | if you want to read both READMEs / build from source |

Install **one** build. Both have the same mod id, so only one of them can sit in the `Mods`
folder at a time.

## Install

1. Download the archive for your language from the [Releases](../../releases) page.
2. Copy the folder `P5R.SocialStatMultiplier` (from the `Mods` folder inside the archive) into
   `<Reloaded-II>\Mods\`.
3. Start Reloaded-II, find the mod and make sure its switch is on.
4. Make sure the library mod **Reloaded.Memory.SigScan.ReloadedII** is installed and enabled
   (it ships with Reloaded-II).
5. Launch the game through Reloaded-II.

If you have the [GameBanana](https://gamebanana.com/) page of this mod open in the launcher's
mod browser, the archive installs itself — it carries the `RELOADED` marker.

## Setting the multiplier

* Launcher → the cog (**Configure**) button next to the mod → **Multiplier**: any value from 1
  to 10 (`2` doubles every award, `3` triples it, `1` is normal game behaviour).
* Or edit the file
  `<Reloaded-II>\User\ModConfigs\P5R.SocialStatMultiplier\Config.json`:

  ```json
  {
    "Enabled": true,
    "Multiplier": 2,
    "VerboseLog": false
  }
  ```

  `VerboseLog: true` prints every award to the launcher log — handy to check that the mod is
  working (e.g. `Charm: +2 → +4 (total 46)`).

Changes apply on the fly; the game does not need to be restarted. The **Suspend**/**Resume**
buttons in the launcher switch the mod off and on while the game is running.

## Automatic updates

The mod is wired to this repository's releases: the launcher asks for the newest tag and offers
an update when one appears, downloading the asset whose name is written in `ModConfig.json`
(`P5R.SocialStatMultiplier_RU.zip` / `P5R.SocialStatMultiplier_EN.zip`). Your `Config.json`
settings are preserved. Manual update: download the fresh archive and replace the mod folder.

## Screenshots

![Mod icon](P5R.SocialStatMultiplier/Preview.png)

<!-- Раздел ниже показывается только когда в docs/img/ появятся картинки.
     Как снять — см. docs/img/README.md, потом убери эти комментарии.

![Mod settings in the Reloaded-II launcher](docs/img/01-launcher-settings.png)
![Multiplier in the mod config](docs/img/02-multiplier.png)
![Social stat gain in game](docs/img/03-in-game.png)
![Launcher log](docs/img/04-log.png)
-->

## How it works

The game keeps the five social stats as five 16-bit values next to each other in memory. The mod
finds that array with a byte signature, remembers the current value of each stat and checks it
every 20 ms. When a value grows, the gain is multiplied and written back; the new value is
remembered as the base for the next check, so awards are never multiplied twice. A jump larger
than 100 points is treated as a save load or a reset and is left alone, and the resulting value
is clamped to 32767 — the game stores these stats as 16-bit signed numbers.

The signature itself comes from the open-source [p5rpc.SocialStatTracker](https://github.com/AnimatedSwine37/p5rpc.SocialStatTracker)
by **AnimatedSwine37** — see Credits.

## Troubleshooting

| Symptom | What to do |
|---|---|
| “Несовместим с приложением” / “Incompatible with application” in the launcher | Your `p5r.exe` is not the one the mod was built for — open an issue with your game build. |
| The log says the signature was not found | Your `p5r.exe` build differs; the mod changes nothing at all in that case. Please report the game version (repack / Steam / Microsoft Store). |
| Points are not multiplied | Check `Multiplier` is above 1 and `Enabled` is `true`; enable `VerboseLog` and look at the launcher log while gaining points. |
| The launcher cannot load the mod | Make sure `Reloaded.Memory.SigScan.ReloadedII` is installed and enabled, and that the .NET 7 runtime installed with Reloaded-II is up to date. |

## Building from source

Requires the .NET SDK 8 (or newer) and python3:

```bash
bash build.sh              # builds both languages, runs the tests, packs dist/*.zip
DOTNET=/path/to/dotnet bash build.sh    # if dotnet is not on PATH
```

`build.sh` builds the Russian and the English version from the same sources (`-p:ModLang=EN`
switches the texts), runs the test project against both DLLs and packs the three release archives
into `dist/`. The same script runs in GitHub Actions
([.github/workflows/build.yml](.github/workflows/build.yml)): every push builds and tests, and a
tag like `v1.1.1` publishes a release with the archives attached.

## Tests

The test project (`P5R.SocialStatMultiplier.Tests`) creates the real mod object, hands it a
stand-in array of points and plays the role of the game: **19 of 19 scenarios pass** — ×2/×3
gains, repeated awards, no self-acceleration, save loading, value resets, changing the
multiplier on the fly, `Enabled = false`, Suspend/Resume, the call sequence the mod loader uses
before the mod starts, the 32767 overflow guard, multiplier ×1 and clean shutdown.

The only thing that cannot be verified outside a real game is the memory signature of a
particular `p5r.exe` build. If it does not match, the mod writes
`Social stat points signature was not found` to the log and changes nothing at all.

## Credits

* [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) — the mod loader and the
  mod template this project is based on.
* [Reloaded.Memory.SigScan](https://github.com/Reloaded-Project/Reloaded.Memory) — signature
  scanning.
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
(Знания, Смелость, Ловкость, Доброта, Обаяние). Множитель задаётся в настройках мода, мод
можно включать и выключать в любой момент, а **файлы игры не изменяются** — мод работает
только со значениями, которые игра сама держит в памяти.

English version of this page: [README.md](README.md)

![Множитель социальных очков](P5R.SocialStatMultiplier/Preview.png)

## Возможности

| | |
|---|---|
| Множитель | 1…10 — в настройках мода в лоадере или в `Config.json` (можно дробный, например 2.5) |
| Включение/выключение | переключатель в списке модов, `"Enabled": false` в настройках или кнопки Suspend/Resume |
| Применяется на ходу | настройки подхватываются во время игры, перезапуск не нужен |
| Что покрыто | все источники очков: ответы в классе, книги, подработки, спортзал, готовка, встречи и ранги конфидантов, сюжетные бонусы |
| Безопасность | мод не спорит с игрой: скачок больше 100 очков (загрузка сейва) не умножается; 16-битный счётчик не переполняется (32767) |
| Файлы игры | ничего не пишется в папку игры, в CPK-архивы и в сохранения |
| Обновления | лоадер сам предлагает новые версии (см. ниже) |

## Какую версию ставить

В релизе две сборки, внутри они одинаковые — отличаются только языком надписей (название
мода в лоадере, подписи настроек, сообщения в логе):

| Архив | Название в лоадере | Для кого |
|---|---|---|
| `P5R.SocialStatMultiplier_RU.zip` | Множитель социальных очков | русская версия |
| `P5R.SocialStatMultiplier_EN.zip` | Social Stat Multiplier | английская версия |
| `P5R.SocialStatMultiplier_RU_EN.zip` | обе версии в одном архиве | если хочется почитать оба README или собрать из исходников |

Ставить нужно **одну** версию: у них одинаковый `ModId`, поэтому одновременно в папке `Mods`
может лежать только одна.

## Установка

1. Скачать архив для своего языка со страницы [Releases](../../releases).
2. Скопировать папку `P5R.SocialStatMultiplier` (из папки `Mods` внутри архива) в
   `<папка Reloaded-II>\Mods\`.
3. Запустить Reloaded-II, найти мод и убедиться, что его переключатель включён.
4. Убедиться, что установлен и включён мод-библиотека **Reloaded.Memory.SigScan.ReloadedII**
   (идёт в комплекте с Reloaded-II).
5. Запустить игру через Reloaded-II.

Если страница мода на [GameBanana](https://gamebanana.com/) открыта во встроенном браузере
модов, архив поставится сам: внутри лежит маркер `RELOADED` для 1-click установки.

## Как менять множитель

* Лоадер → кнопка с шестерёнкой (**Configure**) рядом с модом → **Множитель**: любое значение
  от 1 до 10 (`2` — удваивает каждое начисление, `3` — утраивает, `1` — обычное поведение игры).
* Либо файл `<папка Reloaded-II>\User\ModConfigs\P5R.SocialStatMultiplier\Config.json`:

  ```json
  {
    "Enabled": true,
    "Multiplier": 2,
    "VerboseLog": false
  }
  ```

  `VerboseLog: true` пишет каждое начисление в лог лоадера — удобно проверить, что мод
  работает (например, `Обаяние: +2 → +4 (всего 46)`).

Значения применяются на ходу, перезапускать игру не нужно. Кнопки **Suspend**/**Resume**
в лоадере выключают и включают мод прямо во время игры.

## Автообновление

Мод привязан к релизам этого репозитория: лоадер спрашивает последний тег и предлагает
обновление, когда оно появилось, — скачивая файл, имя которого прописано в `ModConfig.json`
(`P5R.SocialStatMultiplier_RU.zip` / `P5R.SocialStatMultiplier_EN.zip`). Настройки из
`Config.json` при обновлении сохраняются. Вручную: скачать свежий архив и заменить папку мода.

## Скриншоты

![Иконка мода](P5R.SocialStatMultiplier/Preview.png)

<!-- Раздел ниже показывается только когда в docs/img/ появятся картинки.
     Как снять — см. docs/img/README.md, потом убери эти комментарии.

![Настройки мода в лоадере Reloaded-II](docs/img/01-launcher-settings.png)
![Множитель в настройках мода](docs/img/02-multiplier.png)
![Начисление очков в игре](docs/img/03-in-game.png)
![Лог лоадера](docs/img/04-log.png)
-->

## Как это работает

Игра хранит пять социальных характеристик как пять 16-битных значений подряд в памяти. Мод
находит этот массив по сигнатуре байтов, запоминает текущее значение каждой характеристики и
раз в 20 мс проверяет его. Если значение выросло — прирост умножается и записывается обратно,
а новое значение становится базой для следующей проверки, поэтому одно начисление никогда не
умножается дважды. Скачок больше 100 очков считается загрузкой сейва или сбросом и не
трогается, а результат ограничивается значением 32767 — игра хранит эти характеристики как
16-битные знаковые числа.

Сама сигнатура взята из открытого мода [p5rpc.SocialStatTracker](https://github.com/AnimatedSwine37/p5rpc.SocialStatTracker)
автора **AnimatedSwine37** — см. «Благодарности».

## Если что-то не работает

| Симптом | Что делать |
|---|---|
| В лоадере «Несовместим с приложением» | Ваш `p5r.exe` не тот, под который собран мод, — напишите в issue версию игры. |
| В логе «сигнатура не найдена» | Сборка `p5r.exe` отличается; в этом случае мод вообще ничего не меняет. Напишите версию игры (репак / Steam / Microsoft Store). |
| Очки не умножаются | Проверьте, что `Multiplier` больше 1 и `Enabled` = `true`; включите `VerboseLog` и посмотрите лог лоадера в момент начисления очков. |
| Лоадер не может загрузить мод | Проверьте, что установлен и включён `Reloaded.Memory.SigScan.ReloadedII`, а среда .NET 7, установленная вместе с Reloaded-II, обновлена. |

## Сборка из исходников

Нужен .NET SDK 8 (или новее) и python3:

```bash
bash build.sh              # собирает обе версии, прогоняет тесты, пакует dist/*.zip
DOTNET=/путь/к/dotnet bash build.sh    # если dotnet не в PATH
```

`build.sh` собирает русскую и английскую версии из одного кода (`-p:ModLang=EN` переключает
тексты), прогоняет тестовый проект против обеих DLL и пакует три релизных архива в `dist/`.
Тот же скрипт запускается в GitHub Actions
([.github/workflows/build.yml](.github/workflows/build.yml)): каждый push собирается и
тестируется, а тег вида `v1.1.1` публикует релиз с приложенными архивами.

## Тесты

Тестовый проект (`P5R.SocialStatMultiplier.Tests`) создаёт настоящий объект мода, подсовывает
ему подставной массив очков и играет роль игры: **19 из 19 сценариев проходят** — удвоение и
утроение прироста, повторные начисления, отсутствие саморазгона, загрузка сейва, сброс
значений, смена множителя на ходу, `Enabled = false`, Suspend/Resume, последовательность
вызовов лоадера до старта мода, защита от переполнения 32767, множитель ×1 и корректное
завершение.

Единственное, что нельзя проверить вне настоящей игры, — совпадение сигнатуры памяти в
конкретной сборке `p5r.exe`. Если она не совпадёт, мод напишет в лог
`Сигнатура очков социальных характеристик не найдена` и не изменит ничего вообще.

## Благодарности

* [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) — лоадер и шаблон мода, на
  котором основан проект.
* [Reloaded.Memory.SigScan](https://github.com/Reloaded-Project/Reloaded.Memory) — поиск
  сигнатур в памяти.
* Сигнатура массива социальных очков взята из открытого мода
  [p5rpc.SocialStatTracker](https://github.com/AnimatedSwine37/p5rpc.SocialStatTracker) автора
  **AnimatedSwine37** — спасибо, что выложил её.

## Дисклеймер

Неофициальный мод, сделанный фанатами. Не связан с Atlus и Sega и не одобрен ими. Никакие
ресурсы игры, исполняемые файлы или извлечённые данные не входят ни в этот репозиторий, ни в
релизные архивы — только собственный код. *Persona 5 Royal* и связанные с ней названия
принадлежат их правообладателям.

## Лицензия

[MIT](LICENSE)
