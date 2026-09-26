#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Упаковка релизных архивов в dist/. Запускается из build.sh (локально и в GitHub Actions).

Делает ТРИ архива, имена у них ПОСТОЯННЫЕ — без версии:

    P5R.SocialStatMultiplier_RU_EN.zip  — обе версии + исходники (для людей, страница релиза)
    P5R.SocialStatMultiplier_RU.zip     — только русская версия (+ маркер RELOADED)
    P5R.SocialStatMultiplier_EN.zip     — только английская версия (+ маркер RELOADED)

Почему без версии: эти имена прописаны в PluginData.GitHubRelease.AssetFileName внутри
мода — по ним Reloaded-II ищет файл обновления в свежем релизе. Если вписать в имя версию
(..._v1.1.1_RU.zip), то на следующем релизе лоадер будет искать несуществующий файл и
автообновление сломается.

Одиночные архивы (RU/EN) — это те, что отдаются в автообновление и в 1-click установку:
внутри ровно один мод и пустой файл RELOADED в корне (маркер для GameBanana).

Перед упаковкой проверяется:
  * сборки на месте и языки в dll не перепутаны;
  * ModName у версий разный, ModId — одинаковый (это один мод в двух языках);
  * в ModConfig.json прописаны ProjectUrl и PluginData.GitHubRelease, и AssetFileName
    совпадает с именем архива, который этот конфиг упаковывает (иначе автообновление
    не найдёт файл).
"""

import hashlib
import json
import pathlib
import re
import sys
import zipfile

ROOT = pathlib.Path(__file__).resolve().parent
STAGE = ROOT / '_stage'
DIST = ROOT / 'dist'
PROJ = ROOT / 'P5R.SocialStatMultiplier'
DOCS = ROOT / 'docs'
TESTS = ROOT / 'P5R.SocialStatMultiplier.Tests'

RU_DLL_MARKER = 'Включить мод'.encode('utf-16-le')
EN_DLL_MARKER = 'Enable mod'.encode('utf-16-le')

RU_FOLDER = 'RU_Русская_версия'
EN_FOLDER = 'EN_English_version'
MOD_SUBFOLDER = 'Mods/P5R.SocialStatMultiplier'

# постоянные имена архивов — их же ждёт автообновление
ZIP_BOTH = 'P5R.SocialStatMultiplier_RU_EN.zip'
ZIP_RU = 'P5R.SocialStatMultiplier_RU.zip'
ZIP_EN = 'P5R.SocialStatMultiplier_EN.zip'

# ожидаемые значения ProjectUrl / PluginData (должны совпадать с репозиторием)
GITHUB_USER = 'jkinbb'
GITHUB_REPO = 'p5r-socialstatmultiplier'


def fail(message: str) -> None:
    print('ОШИБКА: ' + message)
    sys.exit(1)


def load_config(lang: str) -> dict:
    path = STAGE / lang / 'ModConfig.json'
    return json.loads(path.read_text(encoding='utf-8'))


def check_builds() -> str:
    """Проверяет сборки, языки и настройки автообновления. Возвращает версию мода."""
    for lang in ('RU', 'EN'):
        for name in ('P5R.SocialStatMultiplier.dll', 'ModConfig.json', 'Preview.png'):
            if not (STAGE / lang / name).is_file():
                fail(f'нет файла сборки {lang}/{name} — сначала запусти build.sh')

    ru_dll = (STAGE / 'RU' / 'P5R.SocialStatMultiplier.dll').read_bytes()
    en_dll = (STAGE / 'EN' / 'P5R.SocialStatMultiplier.dll').read_bytes()

    if RU_DLL_MARKER not in ru_dll:
        fail('в русской сборке нет русских строк')
    if EN_DLL_MARKER not in en_dll:
        fail('в английской сборке нет английских строк')
    if EN_DLL_MARKER in ru_dll or RU_DLL_MARKER in en_dll:
        fail('языки сборок перепутаны (в одной dll строки обоих языков)')

    ru_cfg = load_config('RU')
    en_cfg = load_config('EN')

    if ru_cfg['ModName'] == en_cfg['ModName']:
        fail('ModName в русском и английском ModConfig.json совпадают')
    if ru_cfg['ModId'] != en_cfg['ModId']:
        fail('ModId у версий должен совпадать (это один и тот же мод)')
    if not re.fullmatch(r'\d+\.\d+\.\d+', ru_cfg['ModVersion']):
        fail(f"версия «{ru_cfg['ModVersion']}» не похожа на X.Y.Z")
    if ru_cfg['ModVersion'] != en_cfg['ModVersion']:
        fail('ModVersion у русской и английской версий различаются')

    # автообновление должно быть настроено в обоих конфигах
    for lang, cfg, expected_asset in (('RU', ru_cfg, ZIP_RU), ('EN', en_cfg, ZIP_EN)):
        url = cfg.get('ProjectUrl', '')
        if url != f'https://github.com/{GITHUB_USER}/{GITHUB_REPO}':
            fail(f'{lang}: ProjectUrl пустой или не тот (сейчас «{url}»)')
        plugin = (cfg.get('PluginData') or {}).get('GitHubRelease')
        if not plugin:
            fail(f'{lang}: в ModConfig.json нет PluginData.GitHubRelease — автообновление работать не будет')
        if plugin.get('UserName') != GITHUB_USER or plugin.get('RepositoryName') != GITHUB_REPO:
            fail(f'{lang}: в PluginData.GitHubRelease указан не тот репозиторий')
        if plugin.get('UseReleaseTag') is not True:
            fail(f'{lang}: UseReleaseTag должен быть true (версия берётся из тега релиза)')
        if plugin.get('AssetFileName') != expected_asset:
            fail(f'{lang}: AssetFileName «{plugin.get("AssetFileName")}» не совпадает с именем '
                 f'архива «{expected_asset}» — автообновление не найдёт файл')
        if plugin.get('AssetFileName', '').count(ru_cfg['ModVersion']) or '_v' in plugin.get('AssetFileName', ''):
            fail(f'{lang}: в имени файла автообновления не должно быть версии')

    print(f"    сборки на месте: {ru_cfg['ModName']} / {en_cfg['ModName']}, версия {ru_cfg['ModVersion']}")
    print(f"    автообновление: RU → {ZIP_RU}, EN → {ZIP_EN}")
    return ru_cfg['ModVersion']


def add(zf: zipfile.ZipFile, src: pathlib.Path, dst: pathlib.Path, report: list) -> None:
    src = pathlib.Path(src)
    if src.is_dir():
        for path in sorted(src.rglob('*')):
            if path.is_file() and not any(x in path.parts for x in ('bin', 'obj', '__pycache__')):
                add(zf, path, dst / path.relative_to(src), report)
    else:
        zf.write(src, str(dst))
        report.append((str(dst), src.stat().st_size))


def mod_files(zf: zipfile.ZipFile, lang: str, report: list) -> None:
    """Кладёт папку мода Mods/P5R.SocialStatMultiplier (сам мод, без обвязки)."""
    for name in ('P5R.SocialStatMultiplier.dll', 'P5R.SocialStatMultiplier.pdb',
                 'ModConfig.json', 'Preview.png'):
        add(zf, STAGE / lang / name, pathlib.Path(MOD_SUBFOLDER) / name, report)


def build_both() -> pathlib.Path:
    """Обе версии + исходники — архив для страницы релиза."""
    archive = DIST / ZIP_BOTH
    report: list = []
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for src, dst in ((DOCS / 'ARCHIVE_INTRO_EN.txt', 'README.md'),
                         (DOCS / 'ARCHIVE_INTRO_RU.txt', 'ЧИТАЙ_МЕНЯ.txt')):
            zf.writestr(dst, src.read_text(encoding='utf-8'))
            report.append((dst, src.stat().st_size))

        for name in ('P5R.SocialStatMultiplier.dll', 'P5R.SocialStatMultiplier.pdb',
                     'ModConfig.json', 'Preview.png'):
            add(zf, STAGE / 'EN' / name, pathlib.Path(EN_FOLDER) / MOD_SUBFOLDER / name, report)
        add(zf, DOCS / 'README_EN.md', pathlib.Path(EN_FOLDER) / 'README.md', report)
        add(zf, STAGE / 'test_en.txt', pathlib.Path(EN_FOLDER) / 'test_results_EN.txt', report)

        for name in ('P5R.SocialStatMultiplier.dll', 'P5R.SocialStatMultiplier.pdb',
                     'ModConfig.json', 'Preview.png'):
            add(zf, STAGE / 'RU' / name, pathlib.Path(RU_FOLDER) / MOD_SUBFOLDER / name, report)
        add(zf, DOCS / 'README_RU.md', pathlib.Path(RU_FOLDER) / 'ЧИТАЙ_МЕНЯ.txt', report)
        add(zf, STAGE / 'test_ru.txt', pathlib.Path(RU_FOLDER) / 'тест_результаты.txt', report)

        for name in ('Mod.cs', 'Config.cs', 'Strings.cs', 'P5R.SocialStatMultiplier.csproj'):
            add(zf, PROJ / name, pathlib.Path('Исходники/P5R.SocialStatMultiplier') / name, report)
        add(zf, PROJ / 'Template', pathlib.Path('Исходники/P5R.SocialStatMultiplier/Template'), report)
        add(zf, TESTS / 'Program.cs', pathlib.Path('Исходники/P5R.SocialStatMultiplier.Tests/Program.cs'), report)
        add(zf, TESTS / 'P5R.SocialStatMultiplier.Tests.csproj',
            pathlib.Path('Исходники/P5R.SocialStatMultiplier.Tests/P5R.SocialStatMultiplier.Tests.csproj'), report)
    return archive


def build_single(lang: str) -> pathlib.Path:
    """Один язык + маркер RELOADED — это то, что тянет автообновление и 1-click."""
    archive = DIST / (ZIP_RU if lang == 'RU' else ZIP_EN)
    report: list = []
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        mod_files(zf, lang, report)

        readme = DOCS / ('README_RU.md' if lang == 'RU' else 'README_EN.md')
        zf.writestr('ЧИТАЙ_МЕНЯ.txt' if lang == 'RU' else 'README.md',
                    readme.read_text(encoding='utf-8'))

        # маркер 1-click установки для GameBanana (пустой файл в корне архива)
        zf.writestr('RELOADED', b'')
        report.append(('RELOADED', 0))

        # инструкция «как обновляться» — для тех, кто ставит одиночный архив
        zf.writestr('КАК_ОБНОВЛЯТЬ.txt' if lang == 'RU' else 'HOW_TO_UPDATE.txt',
                    UPDATE_NOTE_RU if lang == 'RU' else UPDATE_NOTE_EN)
    return archive


UPDATE_NOTE_RU = """КАК ОБНОВЛЯТЬ ЭТОТ МОД
=======================

Auto (ничего делать не надо): мод сам сообщает лоадеру, что вышла новая версия —
в списке модов рядом появится кнопка обновления. Скачивается архив с таким же набором
файлов, настройки (Config.json) при обновлении сохраняются.

Вручную: скачать свежий P5R.SocialStatMultiplier_RU.zip со страницы релизов и заменить
папку P5R.SocialStatMultiplier в <Reloaded-II>\\Mods\\ на новую.

Проверить установленную версию: в лоадере в списке модов, либо в файле
<Reloaded-II>\\Mods\\P5R.SocialStatMultiplier\\ModConfig.json ("ModVersion").
"""

UPDATE_NOTE_EN = """HOW TO UPDATE THIS MOD
======================

Automatic (do nothing): the mod tells the launcher about new releases, so an update button
appears next to it in the mod list. The archive downloaded has exactly the same file layout,
and your settings (Config.json) are kept.

Manual: download the fresh P5R.SocialStatMultiplier_EN.zip from the Releases page and replace
the folder P5R.SocialStatMultiplier in <Reloaded-II>\\Mods\\.

Installed version: shown in the launcher's mod list, or in
<Reloaded-II>\\Mods\\P5R.SocialStatMultiplier\\ModConfig.json ("ModVersion").
"""


def main() -> None:
    version = check_builds()
    DIST.mkdir(exist_ok=True)
    for old in DIST.glob('*.zip'):
        old.unlink()

    archives = [build_both(), build_single('RU'), build_single('EN')]

    lines = [f'P5R.SocialStatMultiplier {version} — контрольные суммы архивов', '']
    for archive in archives:
        digest = hashlib.md5(archive.read_bytes()).hexdigest()
        print(f'    {archive.name}: {archive.stat().st_size} Б, md5={digest}')
        lines.append(f'{digest}  {archive.name}')
    (DIST / 'md5.txt').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    print(f'    {DIST.name}/md5.txt — контрольные суммы (вставь их в описание релиза, если хочешь)')


if __name__ == '__main__':
    main()
