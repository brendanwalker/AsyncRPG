@echo off

call sqlite_constants.bat

echo "Running Web Service Verification Script"
"%TOOL_EXE%" run_web_service_test_script  -S "%TEST_SCRIPT_FILE_PATH%" -W "%WEB_SERVER_URL%"

timeout /T -1