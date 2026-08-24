#!/usr/bin/env bash
# Frees ports 7196 (https) and 5136 (http) before starting the API,
# so a leftover background instance never blocks a fresh run.
# On Ctrl+C, force-kills anything still bound to those ports afterward too —
# `dotnet watch` in particular can leave a child MSBuild/apphost process behind.
# Usage: ./scripts/run-api.sh [watch]
set -e

cleanup() {
  echo ""
  echo "Stopping API and freeing ports..."
  fuser -k 7196/tcp 2>/dev/null || true
  fuser -k 5136/tcp 2>/dev/null || true
  exit 0
}
trap cleanup INT TERM

fuser -k 7196/tcp 2>/dev/null || true
fuser -k 5136/tcp 2>/dev/null || true

cd "$(dirname "$0")/../src/server/Vendora.Api"

if [ "$1" = "watch" ]; then
  dotnet watch run --launch-profile https &
else
  dotnet run --launch-profile https &
fi

wait $!
