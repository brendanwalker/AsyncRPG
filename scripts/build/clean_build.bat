@echo on

call build_constants.bat

echo "Cleaning %BUILD_DIR%"
del "%BUILD_DIR%\*.*" /s /q
rmdir "%BUILD_DIR%" /s /q
mkdir "%BUILD_DIR%"

echo "Cleaning %UNITY_CLIENT_PLUGINS%"
del "%UNITY_CLIENT%\Assets\Plugins.meta"
del "%UNITY_CLIENT_PLUGINS%\*.*" /s /q
rmdir "%UNITY_CLIENT_PLUGINS%" /s /q
mkdir "%UNITY_CLIENT_PLUGINS%"