#!/usr/bin/env sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
cd "$script_dir"

exec docker compose -f compose.infra.yaml -f compose.app.yaml -f compose.proxy.docker.yaml up -d --build
