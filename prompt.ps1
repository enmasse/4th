# Prompt script for Forth.Core project
# Generates a prompt to pick the next implementation task, ensuring regression tests are included

Write-Host "Suggested next action: run ans-diff, read todo, pick something to do." -ForegroundColor Cyan
Write-Host "Remember to include regression tests for any new implementation." -ForegroundColor Yellow