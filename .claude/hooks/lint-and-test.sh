#!/bin/bash
# Stop hook: runs unit tests for the Electron app before Claude finishes.
# Exit 0 = pass (Claude can stop), Exit 2 = fail (Claude must fix errors).

INPUT=$(cat)

# Prevent infinite loop: check for stop_hook_active without jq
if echo "$INPUT" | grep -q '"stop_hook_active":\s*true'; then
  exit 0
fi

ELECTRON_DIR="F:/Source/OCTGN/octgnFX/Octgn.Electron"
MARKER_FILE="$ELECTRON_DIR/.test-hook-marker"

cd "$ELECTRON_DIR" || exit 0

# Check if any Electron source/test files changed since last successful run
# If the marker file doesn't exist, always run
if [ -f "$MARKER_FILE" ]; then
  CHANGED=$(git diff --name-only "$(cat "$MARKER_FILE")" HEAD -- . 2>/dev/null)
  if [ -z "$CHANGED" ]; then
    # No Electron files changed since last run, skip
    exit 0
  fi
fi

# Record current commit before running tests
CURRENT_SHA=$(git rev-parse HEAD 2>/dev/null)

# Run unit tests (skip integration tests), reporter=dot for minimal output
TEST_OUTPUT=$(npx vitest run --reporter=dot 2>&1)
TEST_EXIT=$?

if [ $TEST_EXIT -ne 0 ]; then
  echo "=== Test failures ===" >&2
  # Only show failing test details, not passing ones
  echo "$TEST_OUTPUT" | grep -A 5 "FAIL\|Error\|AssertionError\|expected\|received" >&2
  echo "" >&2
  echo "Fix the test failures above before finishing." >&2
  exit 2
fi

# Tests passed — update marker
echo "$CURRENT_SHA" > "$MARKER_FILE"
exit 0
