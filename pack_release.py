#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Упаковка релизного архива с обеими версиями мода (RU + EN) в dist/.

Запускается из build.sh (локально и в GitHub Actions).
Перед упаковкой проверяет, что сборки действительно различаются языком:
русская dll содержит русские строки и не содержит английских, английская — наоборот,
а ModName в ModConfig.json у них разный.
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


def fail(message: str) -> None:
    print('ОШИБКА: ' + message)
    sys.exit(1)


def check_builds() -> str:
    """Проверяет, что сборки на месте и отличаются языком. Возвращает версию мода."""
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

    ru_cfg = json.loads((STAGE / 'RU' / 'ModConfig.json').read_text(encoding='utf-8'))
    en_cfg = json.loads((STAGE / 'EN' / 'ModConfig.json').read_text(encoding='utf-8'))

    if ru_cfg['ModName'] == en_cfg['ModName']:
        fail('ModName в русском и английском ModConfig.json совпадают')
    if ru_cfg['ModId'] != en_cfg['ModId']:
        fail('ModId у версий должен совпадать (это один и тот же мод)')
    for cfg in (ru_cfg, en_cfg):
        if not re.fullmatch(r'\d+\.\d+\.\d+', cfg['ModVersion']):
            fail(f"версия «{cfg['ModVersion']}» не похожа на X.Y.Z")

    print(f"    сборки на месте: {ru_cfg['ModName']} / {en_cfg['ModName']}, версия {ru_cfg['ModVersion']}")
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


def main() -> None:
    version = check_builds()
    DIST.mkdir(exist_ok=True)
    archive = DIST / f'P5R.SocialStatMultiplier_v{version}_RU_EN.zip'
    report: list = []

    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        # общие тексты в корне архива
        for src, dst in ((DOCS / 'ARCHIVE_INTRO_EN.txt', 'README.md'),
                         (DOCS / 'ARCHIVE_INTRO_RU.txt', 'ЧИТАЙ_МЕНЯ.txt')):
            zf.writestr(dst, src.read_text(encoding='utf-8'))
            report.append((dst, src.stat().st_size))

        # английская версия
        for name in ('P5R.SocialStatMultiplier.dll', 'P5R.SocialStatMultiplier.pdb',
                     'ModConfig.json', 'Preview.png'):
            add(zf, STAGE / 'EN' / name, pathlib.Path(EN_FOLDER) / MOD_SUBFOLDER / name, report)
        add(zf, DOCS / 'README_EN.md', pathlib.Path(EN_FOLDER) / 'README.md', report)
        add(zf, STAGE / 'test_en.txt', pathlib.Path(EN_FOLDER) / 'test_results_EN.txt', report)

        # русская версия
        for name in ('P5R.SocialStatMultiplier.dll', 'P5R.SocialStatMultiplier.pdb',
                     'ModConfig.json', 'Preview.png'):
            add(zf, STAGE / 'RU' / name, pathlib.Path(RU_FOLDER) / MOD_SUBFOLDER / name, report)
        add(zf, DOCS / 'README_RU.md', pathlib.Path(RU_FOLDER) / 'ЧИТАЙ_МЕНЯ.txt', report)
        add(zf, STAGE / 'test_ru.txt', pathlib.Path(RU_FOLDER) / 'тест_результаты.txt', report)

        # исходный код (общий для обеих версий)
        for name in ('Mod.cs', 'Config.cs', 'Strings.cs', 'P5R.SocialStatMultiplier.csproj'):
            add(zf, PROJ / name, pathlib.Path('Исходники/P5R.SocialStatMultiplier') / name, report)
        add(zf, PROJ / 'Template', pathlib.Path('Исходники/P5R.SocialStatMultiplier/Template'), report)
        add(zf, TESTS / 'Program.cs', pathlib.Path('Исходники/P5R.SocialStatMultiplier.Tests/Program.cs'), report)
        add(zf, TESTS / 'P5R.SocialStatMultiplier.Tests.csproj',
            pathlib.Path('Исходники/P5R.SocialStatMultiplier.Tests/P5R.SocialStatMultiplier.Tests.csproj'), report)

    digest = hashlib.md5(archive.read_bytes()).hexdigest()
    print(f'    {archive.name}: {archive.stat().st_size} Б, md5={digest}, файлов={len(report)}')


if __name__ == '__main__':
    main()
