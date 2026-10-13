#!/bin/sh
# Runs the esper test suite with coverage and builds an HTML report.
# Usage: scripts/coverage.sh
# Output: Tests/TestResults/coverage/index.html
# Note: env-gated master sweeps stay skipped unless ESPER_RUN_MASTER_TESTS=1.
set -e
cd "$(dirname "$0")/.."
rm -rf Tests/TestResults/coverage
dotnet test Tests/Tests.csproj \
    --settings coverlet.runsettings \
    --collect:"XPlat Code Coverage" \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=results.trx"
COV=$(ls -t Tests/TestResults/*/coverage.cobertura.xml | head -1)
reportgenerator -reports:"$COV" -targetdir:Tests/TestResults/coverage
echo "Report: Tests/TestResults/coverage/index.html"
