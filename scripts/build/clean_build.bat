@echo off

call build_constants.bat

pushd "%BUILD_DIR%"
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
popd

pushd "%UNITY_CLIENT_PLUGINS%"
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
popd