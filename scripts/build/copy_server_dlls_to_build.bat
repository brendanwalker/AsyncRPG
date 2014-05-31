@echo off

call build_constants.bat

xcopy "%LIB_DIR%\SQLite.Interop.*" "%BUILD_DIR%\server" /s /y /q
copy /Y "%LIB_DIR%\System.Data.SQLite.dll.config" "%BUILD_DIR%\server"