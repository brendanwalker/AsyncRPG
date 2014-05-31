@echo off

call mssql_constants.bat

echo "Running Unit Tests"
"%TOOL_EXE%" run_unit_tests