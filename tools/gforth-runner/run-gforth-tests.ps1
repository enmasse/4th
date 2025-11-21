<#
PowerShell port of run-gforth-tests.sh
Downloads Gforth master ZIP into temp, extracts, runs all .4th files with installed gforth,
then cleans up. Exits non-zero if any test fails.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Cleanup {
    param($Path)
    try { if (Test-Path $Path) { Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction SilentlyContinue } } catch { }
}

if (-not (Get-Command gforth -ErrorAction SilentlyContinue)) {
    Write-Error "gforth not found in PATH. Install gforth first (apt/brew/choco) and ensure it's on PATH."
    exit 3
}

$tmp = Join-Path $env:TEMP ("gforth-runner-{0}" -f ([guid]::NewGuid().ToString()))
$zip = Join-Path $tmp 'gforth.zip'
$repoDir = Join-Path $tmp 'gforth-master'

New-Item -ItemType Directory -Path $tmp | Out-Null

try {
    Write-Host "Downloading Gforth archive..."
    Invoke-WebRequest -Uri 'https://github.com/gforth/gforth/archive/refs/heads/master.zip' -OutFile $zip -UseBasicParsing

    Write-Host "Extracting..."
    Expand-Archive -LiteralPath $zip -DestinationPath $tmp -Force

    if (-not (Test-Path $repoDir)) {
        Write-Error "Failed to extract repository archive"
        Cleanup $tmp
        exit 4
    }

    $failCount = 0
    $failList = @()

    Write-Host "Running .4th tests (this may be slow)..."

    $files = Get-ChildItem -Path $repoDir -Recurse -Filter '*.4th' -File
    foreach ($f in $files) {
        $path = $f.FullName
        Write-Host "---- $path"
        & gforth -e "INCLUDE `"$path`"" -e "BYE"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "FAILED: $path"
            $failList += $path
            $failCount++
            # Stop on first failure to save time; comment the next line to continue all tests
            break
        }
    }

    Cleanup $tmp

    if ($failCount -ne 0) {
        Write-Error "Gforth tests completed: $failCount failures."
        foreach ($ff in $failList) { Write-Error "  $ff" }
        exit 2
    }

    Write-Host "Gforth tests executed: no failures detected (exit 0)."
    exit 0
}
catch {
    Write-Error "Error running gforth tests: $_"
    Cleanup $tmp
    exit 5
}
