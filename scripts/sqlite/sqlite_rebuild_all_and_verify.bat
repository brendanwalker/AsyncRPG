@echo off

call sqlite_constants.bat

echo "Rebuilding Game DB"
"%TOOL_EXE%" rebuild_database -C "%DB_CONNECTION_STRING%" -M "%MOB_DATA_PATH%" -T "%MAP_TEMPLATE_PATH%"

echo "Launching Web Server"
start "AsyncRPGWebServer" "%WEB_SERVER_EXE%"

echo "Running Web Service Verification Script"
"%TOOL_EXE%" run_web_service_test_script  -S "%TEST_SCRIPT_FILE_PATH%" -W "%WEB_SERVER_URL%"

timeout /T -1