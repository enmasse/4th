# Health check script for Forth.Core project
# Builds, runs tests, and checks ANS compliance, then suggests next task

Write-Host "Running health check for Forth.Core..." -ForegroundColor Green

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test 4th.Tests
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Run ANS compliance diff (don't fail on missing for health check)
Write-Host "Running ANS compliance diff..." -ForegroundColor Yellow
dotnet run --project tools/ans-diff -- --sets=all --fail-on-missing=false

# Generate prompt for next task
Write-Host "`nHealth check complete!" -ForegroundColor Green

exit 0