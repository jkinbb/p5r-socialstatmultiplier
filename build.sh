#!/usr/bin/env bash
#
# Сборка мода «Множитель социальных очков» / Social Stat Multiplier:
#   1) русская и английская версии мода,
#   2) прогон автотестов против обеих сборок,
#   3) упаковка релизного архива в dist/.
#
# Требуется .NET SDK 8 (или новее) и python3.
# Тот же скрипт запускается в GitHub Actions (.github/workflows/build.yml).

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

DOTNET="${DOTNET:-dotnet}"
PROJ="$ROOT/P5R.SocialStatMultiplier/P5R.SocialStatMultiplier.csproj"
TESTS="$ROOT/P5R.SocialStatMultiplier.Tests"
STAGE="$ROOT/_stage"

echo "== 1/4 Чистое состояние =="
rm -rf "$STAGE" "$ROOT/P5R.SocialStatMultiplier/obj" "$ROOT/P5R.SocialStatMultiplier/bin" \
       "$ROOT/P5R.SocialStatMultiplier.Tests/obj" "$ROOT/P5R.SocialStatMultiplier.Tests/bin"
mkdir -p "$STAGE"

echo
echo "== 2/4 Сборка русской версии =="
"$DOTNET" build -c Release "$PROJ" -o "$STAGE/RU"
rm -rf "$ROOT/P5R.SocialStatMultiplier/obj"
"$DOTNET" build -c Release -p:ModLang=EN "$PROJ" -o "$STAGE/EN"

echo
echo "== 3/4 Автотесты против обеих сборок =="
"$DOTNET" run -c Release --project "$TESTS" -p:ModDllDir="$STAGE/RU" > "$STAGE/test_ru.txt" 2>&1 || {
    tail -25 "$STAGE/test_ru.txt"; echo "ТЕСТЫ (RU) ПРОВАЛЕНЫ"; exit 1; }
tail -1 "$STAGE/test_ru.txt"

"$DOTNET" run -c Release --project "$TESTS" -p:ModDllDir="$STAGE/EN" > "$STAGE/test_en.txt" 2>&1 || {
    tail -25 "$STAGE/test_en.txt"; echo "ТЕСТЫ (EN) ПРОВАЛЕНЫ"; exit 1; }
tail -1 "$STAGE/test_en.txt"

echo
echo "== 4/4 Упаковка релизного архива =="
python3 "$ROOT/pack_release.py"

echo
echo "Готово. Архив лежит в dist/:"
ls -la "$ROOT/dist"
