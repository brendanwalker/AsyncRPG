@echo off

call build_constants.bat

mkdir "%UNITY_CLIENT_PLUGINS%"
copy /Y "%BUILD_DIR%\server\AsyncRPGSharedLib.dll" "%UNITY_CLIENT_PLUGINS%"
copy /Y "%LIB_DIR%\System.Data.SQLite.dll.config" "%UNITY_CLIENT_PLUGINS%"
xcopy "%LIB_DIR%\*.dll" "%UNITY_CLIENT_PLUGINS%" /s /y /q