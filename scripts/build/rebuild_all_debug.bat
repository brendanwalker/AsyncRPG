pushd %~dp0
call build_constants.bat
echo "[Clean]"
call clean_build.bat

echo "[Building Server]"
call build_server_debug.bat

echo "[Building Client]"
call build_unity_client.bat
popd