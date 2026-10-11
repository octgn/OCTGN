#!/bin/bash
# Polls a PR for new comments from the repo owner.
# Usage: ./wait-for-pr-response.sh <PR_NUMBER> <LAST_KNOWN_COMMENT_ID>
# Returns the new comment body when found.

PR_NUMBER="${1:?Usage: $0 <PR_NUMBER> [LAST_KNOWN_COMMENT_ID]}"
LAST_ID="${2:-0}"
POLL_INTERVAL=30

echo "Watching PR #${PR_NUMBER} for new comments (after comment ID ${LAST_ID})..."

while true; do
  LATEST=$(gh api "repos/{owner}/{repo}/issues/${PR_NUMBER}/comments" \
    --jq 'map(select(.author_association == "OWNER" or .author_association == "MEMBER")) | last')

  if [ -n "$LATEST" ] && [ "$LATEST" != "null" ]; then
    COMMENT_ID=$(echo "$LATEST" | jq -r '.id')
    if [ "$COMMENT_ID" != "$LAST_ID" ] && [ "$COMMENT_ID" -gt "$LAST_ID" ] 2>/dev/null; then
      echo "--- New comment (ID: ${COMMENT_ID}) ---"
      echo "$LATEST" | jq -r '.body'
      echo "---"
      exit 0
    fi
  fi

  sleep "$POLL_INTERVAL"
done
