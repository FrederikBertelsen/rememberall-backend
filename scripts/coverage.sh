#!/usr/bin/env bash
dotnet test -- --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
ReportGenerator -reports:RememberAllBackend.Tests/bin/Debug/net8.0/TestResults/coverage.cobertura.xml -targetdir:CoverageReport
open CoverageReport/index.html