# Health check script for Forth.Core project
# Builds, runs tests, and checks ANS compliance, then suggests next task

param(
    [switch]$SkipDocs
)

Write-Host "Running health check for Forth.Core..." -ForegroundColor Green

# Check submodule status
Write-Host "Checking submodule status..." -ForegroundColor Yellow
$submoduleStatus = git submodule status tests\forth2012-test-suite
if ($submoduleStatus -match "^\+") {
    Write-Host "Warning: Submodule tests\forth2012-test-suite has local modifications!" -ForegroundColor Yellow
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test .\tests\4th.Tests\4th.Tests\4th.Tests.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Run docs generation
if (-not $SkipDocs -and -not $env:GITHUB_ACTIONS) {
    Write-Host "Generating word documentation..." -ForegroundColor Yellow
    dotnet run --project tools/DocsGen -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docs generation failed!" -ForegroundColor Red
        exit 1
    }
}

# Run ANS compliance diff (don't fail on missing for health check)
Write-Host "Running ANS compliance diff..." -ForegroundColor Yellow
$ansReport = Join-Path $env:TEMP "ans-diff-report.md"
dotnet run --project tools/ans-diff -- --sets=all --fail-on-missing=false 2>&1 | Tee-Object -FilePath $ansReport

# List TODO.md contents
Write-Host "Listing TODO.md contents..." -ForegroundColor Yellow
Get-Content TODO.md

# Generate prompt for next task
Write-Host "`nHealth check complete!" -ForegroundColor Green

exit 0exit 0