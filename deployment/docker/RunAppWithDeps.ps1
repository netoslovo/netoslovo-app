$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot
try {
    & docker compose -f compose.infra.yaml -f compose.app.yaml -f compose.proxy.docker.yaml up -d --build
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

exit $exitCode
