#!/usr/bin/env bash
# Builds the library and produces the ZIP you upload to the ODC Portal
# (Settings > External logic > Upload external logic).
#
# ODC expects a ZIP containing the compiled assembly and its dependencies.
# The OutSystems SDK assembly is included; ODC ignores it at runtime.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOTNET="${DOTNET:-dotnet}"
CONFIG="Release"
OUT="$ROOT/src/bin/$CONFIG/net8.0"
DIST="$ROOT/dist"
ZIP="$DIST/Stopwatch_Library.zip"

echo "==> Building ($CONFIG)…"
"$DOTNET" build "$ROOT/src/Stopwatch.csproj" -c "$CONFIG" --nologo

echo "==> Running tests…"
"$DOTNET" test "$ROOT/tests/Stopwatch.Tests.csproj" -c "$CONFIG" --nologo

echo "==> Packaging…"
rm -rf "$DIST"
mkdir -p "$DIST"
# Zip the DLLs (and the .deps.json) from the build output, flat, no parent dirs.
(
  cd "$OUT"
  zip -j -X "$ZIP" ./*.dll ./*.deps.json >/dev/null
)

echo "==> Done: $ZIP"
unzip -l "$ZIP"
