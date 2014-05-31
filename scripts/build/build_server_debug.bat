@echo off

call build_constants.bat
call copy_content_to_build.bat

REM "%MDTOOL_EXE%" build --configuration:Debug "%SERVER_SLN%"
"%MSBUILD_EXE%" "%SERVER_SLN%" /p:Configuration=Debug /tv:3.5