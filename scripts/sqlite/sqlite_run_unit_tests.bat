@echo off

call sqlite_constants.bat

echo "Running Unit Tests"
"%TOOL_EXE%" run_unit_tests


timeout /T -1