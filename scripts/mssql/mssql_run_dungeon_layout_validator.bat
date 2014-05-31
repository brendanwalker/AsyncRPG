@echo off

call mssql_constants.bat

echo "Running Dungeon Layout Validator"
"%TOOL_EXE%" validate_dungeons -C "%DB_CONNECTION_STRING%"