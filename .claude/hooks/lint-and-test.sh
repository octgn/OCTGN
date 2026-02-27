#!/bin/bash
# Stop hook: runs unit tests for the Electron app before Claude finishes.
# Exit 0 = pass (Claude can stop), Exit 2 = fail (Claude must fix errors).

INPUT=$(cat)

# Prevent infinite loop: check for stop_hook_active without jq
if echo "$INPUT" | grep -q '"stop_hook_active":\s*true'; then
  exit 0
fi

cd "F:/Source/OCTGN/octgnFX/Octgn.Electron" || exit 0

FAILED=0

# Run unit tests (skip integration tests)
TEST_OUTPUT=$(npx vitest run 2>&1)
TEST_EXIT=$?

if [ $TEST_EXIT -ne 0 ]; then
  echo "=== Test failures ===" >&2
  echo "$TEST_OUTPUT" >&2
  FAILED=1
fi

if [ $FAILED -ne 0 ]; then
  echo "" >&2
  echo "Fix the test failures above before finishing." >&2
  exit 2
fi

exit 0
