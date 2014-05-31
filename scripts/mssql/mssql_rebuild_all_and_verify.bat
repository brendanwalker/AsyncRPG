@echo off

call mssql_constants.bat
@set "WEB_SERVER_URL=http://localhost:80/AsyncRPG"

echo "Rebuilding Game DB"
"%TOOL_EXE%" rebuild_database -C "%DB_CONNECTION_STRING%" -M "%MOB_DATA_PATH%" -T "%MAP_TEMPLATE_PATH%"

echo "Running Web Service Verification Script"
"%TOOL_EXE%" run_web_service_test_script  -S "%TEST_SCRIPT_FILE_PATH%" -W "%WEB_SERVER_URL%"

timeout /T -1