@echo off

call build_constants.bat

"%UNITY_EXE%" -quit -batchmode -buildWindowsPlayer "%BUILD_DIR%\client\AsyncRPGClient.exe" -logFile "%BUILD_DIR%\client\AsyncRPGClient.build.log"
copy /Y "%LIB_DIR%\x86\SQLite.Interop.dll" "%BUILD_DIR%\client"