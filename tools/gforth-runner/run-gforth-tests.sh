#!/usr/bin/env bash
# Minimal runner: download Gforth repo archive, run all .4th files with installed gforth, then cleanup.
set -euo pipefail
tmpdir=$(mktemp -d)
archive="$tmpdir/gforth.zip"
repo_dir="$tmpdir/gforth-master"

echo "Using temp dir: $tmpdir"

if ! command -v gforth >/dev/null 2>&1; then
  echo "gforth not found in PATH. Install gforth first (apt/brew/choco)." >&2
  exit 3
fi

echo "Downloading Gforth archive..."
curl -L -s -o "$archive" https://github.com/gforth/gforth/archive/refs/heads/master.zip

echo "Extracting..."
unzip -q "$archive" -d "$tmpdir"

if [ ! -d "$repo_dir" ]; then
  echo "Failed to extract repository archive" >&2
  exit 4
fi

fail_count=0
fail_list=()

echo "Running .4th tests (this may be slow)..."
# Run each .4th file. Some upstream tests may expect helper scripts; this runner simply INCLUDES each file.
while IFS= read -r -d '' file; do
  echo "---- $file"
  if ! gforth -e "INCLUDE \"$file\"" -e "BYE"; then
    echo "FAILED: $file" >&2
    fail_list+=("$file")
    fail_count=$((fail_count+1))
    # stop early on first failure? comment out the next line to continue running all tests
    # break
  fi
done < <(find "$repo_dir" -type f -name "*.4th" -print0)

# Cleanup
rm -rf "$tmpdir"

if [ "$fail_count" -ne 0 ]; then
  echo "Gforth tests completed: $fail_count failures." >&2
  for f in "${fail_list[@]}"; do echo "  $f" >&2; done
  exit 2
fi

echo "Gforth tests executed: no failures detected (exit 0)."
exit 0
