@echo off

call sqlite_constants.bat

echo "Rebuilding Game DB"
"%TOOL_EXE%" rebuild_database -C "%DB_CONNECTION_STRING%" -M "%MOB_DATA_PATH%" -T "%MAP_TEMPLATE_PATH%"

if not "%1" == "nowait" timeout /T -1