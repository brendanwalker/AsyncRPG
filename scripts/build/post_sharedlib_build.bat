pushd %~dp0
call copy_server_dlls_to_build.bat
call copy_server_dlls_to_unity.bat
popd