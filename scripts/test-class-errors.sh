#!/usr/bin/env bash

if [ -z "$1" ]; then
  echo "Usage: ./test-class-errors.sh <TestClassName>"
  echo ""
  echo "Example: ./test-class-errors.sh TodoItemApiTests"
  echo ""
  echo "This script runs tests and shows only:"
  echo "  - Failures with error messages and stack traces"
  echo "  - File paths to failed test locations"
  echo "  - Console output lines prefixed with 'debug: '"
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
dotnet test --filter "FullyQualifiedName~$TEST_CLASS" --logger "console;verbosity=detailed" 2>&1 | awk -v red="$RED" -v green="$GREEN" -v yellow="$YELLOW" -v blue="$BLUE" -v cyan="$CYAN" -v nc="$NC" '
BEGIN {
    in_failure = 0
    in_error_msg = 0
    current_file_path = ""
}

# Capture xUnit.net formatted file paths BEFORE the failure message appears
/\[xUnit\.net.*\].*\/[^(]+\([0-9]+,[0-9]+\): at / {
    # Extract and store the file path - it will be used with the next failure
    gsub(/.*\[xUnit\.net[^]]*\][[:space:]]*/, "")
    gsub(/: at .*/, "")
    # Convert format from /path/file.cs(484,0) to /path/file.cs:line 484
    gsub(/\([0-9]+,[0-9]+\)/, "")
    current_file_path = $0
    next
}

# Match "Failed" section headers
/Failed RememberAllBackend\.Tests/ {
    if (in_failure) {
        print ""
    }
    in_failure = 1
    in_error_msg = 0
    print red "FAILED: " nc $0
    # Print the file path right after the failed test name
    if (current_file_path != "") {
        print blue "  → " current_file_path nc
        current_file_path = ""
    }
    next
}

# Match "Error Message:" and capture everything until we see "Stack Trace:"
/^[[:space:]]*Error Message:/ {
    in_error_msg = 1
    next
}

# Capture error message content (non-indented lines after Error Message:)
in_error_msg && /^[[:space:]]{3,}[^ ]/ && !/Stack Trace:/ {
    print yellow "  " $0 nc
}

# Match "Stack Trace:" line
/^[[:space:]]*Stack Trace:/ {
    in_error_msg = 0
    next
}

# Capture stack trace lines (indented lines starting with "at ")
/^[[:space:]]{5,}at / {
    # Check if this line contains a file path (from the summary section)
    if ($0 ~ /in \//) {
        # Highlight file paths in blue and line numbers in cyan
        line_copy = $0
        gsub(/in \/[^:]+/, blue "&" nc, line_copy)
        gsub(/:line [0-9]+/, cyan "&" nc, line_copy)
        print "  " line_copy
    } else {
        print "  " $0
    }
}

# Handle "--- End of stack trace" marker
/--- End of stack trace/ {
    # Skip this line, it does not add value
    next
}

# Match console debug output (lines containing "debug: ")
/debug: / {
    print green "DEBUG: " nc $0
}

# Match final summary line
/Failed!/ {
    print ""
    print cyan "==================================================" nc
    print red $0 nc
    print cyan "==================================================" nc
    next
}

# Match passed summary line for context
/Passed!/ {
    print ""
    print cyan "==================================================" nc
    print green $0 nc
    print cyan "==================================================" nc
    next
}

END {
}
'
