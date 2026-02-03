#!/usr/bin/env bash

if [ -z "$1" ]; then
  echo "Usage: ./test-class.sh <TestClassName>"
  echo ""
  echo "Example: ./test-class.sh TodoItemServiceTests"
  exit 1
fi

TEST_CLASS=$1

# ANSI color codes for macOS terminal
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}Running tests from: $TEST_CLASS${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""

# Run tests and capture output
dotnet test --filter "FullyQualifiedName~$TEST_CLASS" 2>&1 | awk -v red="$RED" -v green="$GREEN" -v yellow="$YELLOW" -v blue="$BLUE" -v cyan="$CYAN" -v nc="$NC" '
# Color failed tests in red
/\[FAIL\]/ {
    gsub(/\[FAIL\]/, red "[FAIL]" nc)
}

# Color passed tests in green
/\[PASS\]/ {
    gsub(/\[PASS\]/, green "[PASS]" nc)
}

# Color the summary line
/^(Failed!|Passed!)/ {
    if (/^Failed!/) {
        print ""
        print cyan "==================================================" nc
        print red $0 nc
        print cyan "==================================================" nc
        next
    } else if (/^Passed!/) {
        print ""
        print cyan "==================================================" nc
        print green $0 nc
        print cyan "==================================================" nc
        next
    }
}

# Print everything else as-is
{ print }
'
