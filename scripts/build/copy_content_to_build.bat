@echo off

call build_constants.bat

echo "%CONTENT_DIR%"
xcopy "%CONTENT_DIR%\maps\*.oel" "%BUILD_DIR%\content\maps\" /s /y /q
xcopy "%CONTENT_DIR%\mobs\*.json" "%BUILD_DIR%\content\mobs\" /s /y /q