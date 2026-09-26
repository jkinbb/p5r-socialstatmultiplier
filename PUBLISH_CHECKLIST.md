# Что сделать перед публикацией (чек-лист)

Репозиторий полностью готов к заливке: собирается, тестируется и пакует релиз сам.
Ниже — мелочи, которые стоит поправить под себя, и порядок публикации.

## 1. Заменить подставные данные (3 места)

| Файл | Что заменить |
|---|---|
| `LICENSE` | `<ВАШ НИК / YOUR NAME>` → твой ник или имя |
| `P5R.SocialStatMultiplier/ModConfig.json` | `"ModAuthor": "Arena.ai"` → твой ник |
| `P5R.SocialStatMultiplier/ModConfig.en.json` | `"ModAuthor": "Arena.ai"` → твой ник |

## 2. Создать репозиторий и залить код

```bash
cd P5R_SocialStatMultiplier_repo
git init -b main
git add .
git commit -m "Social Stat Multiplier 1.1.0 — Reloaded-II mod for Persona 5 Royal"
git remote add origin https://github.com/ТВОЙ_НИК/p5r-socialstatmultiplier.git
git push -u origin main
```

(Или создай пустой репозиторий на GitHub через веб-интерфейс и загрузи файлы перетаскиванием —
`.github/workflows/build.yml` тоже загрузится и заработает.)

После первого пуша открой вкладку **Actions** — workflow «Build» соберёт обе версии мода, прогонит
19 автотестов и приложит к сборке готовый архив.

## 3. Выпустить релиз

```bash
git tag v1.1.0
git push origin v1.1.0
```

По тегу workflow сам создаст релиз `v1.1.0` и приложит к нему архив
`P5R.SocialStatMultiplier_v1.1.0_RU_EN.zip`. Ссылку на релиз можно давать игрокам.

## 4. (Необязательно) Автообновление мода из лоадера

Чтобы Reloaded-II умел сам предлагать обновления, добавь в оба `ModConfig*.json` ссылку на
репозиторий и блок GitHub-релиза (подставь свой ник и имя репозитория):

```json
  "ReleaseMetadataFileName": "P5R.SocialStatMultiplier.ReleaseMetadata.json",
  "ProjectUrl": "https://github.com/ТВОЙ_НИК/p5r-socialstatmultiplier",
  "PluginData": {
    "GitHubRelease": {
      "UserName": "ТВОЙ_НИК",
      "RepositoryName": "p5r-socialstatmultiplier",
      "UseReleaseTag": true,
      "AssetFileName": "P5R.SocialStatMultiplier_v1.1.0_RU_EN.zip"
    }
  }
```

После этого в лоадере мод подхватит обновления из твоих релизов. Если публикуешь ещё и на
GameBanana — там вместо `GitHubRelease` используется `PluginData.GameBanana` с `ItemType: "Mod"`
и `ItemId` страницы.

## 5. (Необязательно) GameBanana

1. Создать страницу мода (категория Mod), **доступ — приватный**, пока не готово.
2. Взять `ItemId` из адреса страницы и вписать его в `ModConfig.json` (см. пункт 4).
3. В архив добавить пустой файл-маркер `RELOADED` в корень — тогда на сайте появится кнопка
   «1-click install» для Reloaded-II.
4. Загрузить архив в раздел Files и открыть страницу.

## 6. Что НЕ надо выкладывать

* Файлы игры, `.exe`, `.cpk`, извлечённые скрипты, русификатор, арты и логотипы Atlus/Sega.
* Файлы из папки `dist/` в репозиторий коммитить не нужно (она в `.gitignore`) — они попадают
  в Releases как артефакты.
