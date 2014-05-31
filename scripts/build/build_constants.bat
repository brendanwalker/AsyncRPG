@echo off

@set "DOT_NET_FRAMEWORK=C:\Windows\Microsoft.NET\Framework\v3.5"
@set "MSBUILD_EXE=%DOT_NET_FRAMEWORK%\msbuild.exe"

@set "UNITY_ROOT=C:\Program Files (x86)\Unity"
@set "UNITY_EXE=%UNITY_ROOT%\Editor\Unity.exe"
@set "MONO_DEV_ROOT=%UNITY_ROOT%\MonoDevelop\bin"
@set "MDTOOL_EXE=%MONO_DEV_ROOT%\mdtool.exe"

@set "PROJECT_ROOT=..\.."
@set "UNITY_CLIENT=%PROJECT_ROOT%\src\unityClient"
@set "UNITY_CLIENT_PLUGINS=%UNITY_CLIENT%\Assets\Plugins"
@set "SERVER_SRC_DIR=%PROJECT_ROOT%\src\server"
@set "SERVER_SLN=%SERVER_SRC_DIR%\AsyncRPGServer.sln"
@set "SHAREDLIB_SRC_DIR=%SERVER_SRC_DIR%\AsyncRPGSharedLib"
@set "BUILD_DIR=%PROJECT_ROOT%\build"
@set "LIB_DIR=%PROJECT_ROOT%\lib"
@set "CONTENT_DIR=%PROJECT_ROOT%\src\content"
@set "DB_METAL_EXE=%LIB_DIR%\dbmetal.exe"