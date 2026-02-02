#!/usr/bin/env bash

echo ""
echo "=================================================="
echo "Generating code coverage report..."
echo "=================================================="
echo ""
rm -fr CoverageReport
rm -fr RememberAllBackend.Tests/TestResults/
rm -fr RememberAllBackend.Tests/obj/
rm -fr RememberAllBackend.Tests/bin/
rm -fr RememberAllBackend/bin/
rm -fr RememberAllBackend/obj/

echo ""
echo "=================================================="
echo "Building project..."
echo "=================================================="
echo ""
dotnet clean
dotnet restore
dotnet build

echo ""
echo "=================================================="
echo "Running tests with coverage..."
echo "=================================================="
echo ""
dotnet test --collect:"Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

echo ""
echo "=================================================="
echo "Generating HTML report..."
echo "=================================================="
echo ""
COVERAGE_FILE=$(find RememberAllBackend.Tests/TestResults -name "*.cobertura.xml" 2>/dev/null | head -1)

if [ -z "$COVERAGE_FILE" ]; then
  echo "Error: Coverage file not found"
  exit 1
fi

ReportGenerator -reports:"$COVERAGE_FILE" -targetdir:CoverageReport -assemblyfilters:"-*Tests"

echo ""
echo "=================================================="
echo "Opening coverage report..."
echo "=================================================="
echo ""
open CoverageReport/index.html