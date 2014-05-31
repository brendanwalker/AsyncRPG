@echo off

call sqlite_constants.bat

"%SQL_SERVER_TOOL%" "%DB_FILE%"