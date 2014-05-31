@echo off

call sqlite_constants.bat

echo "Running Dungeon Layout Validator"
"%TOOL_EXE%" validate_dungeons -C "%DB_CONNECTION_STRING%"

timeout /T -1