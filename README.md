AsyncRPG
========

A multi-player turned based dungeon crawler written in Unity/C#

Initial Build
=============
* Make sure the the paths are set up correctly in scripts\build\build_constants.bat
 - DOT_NET_FRAMEWORK should point to a .NET 3.5 framework install (needed for server build)
 - UNITY_ROOT should point at the Unity install
* Run scripts\build\build_server_debug.bat
 - This builds the server DLLs that the Unity client is dependant on
* Launch the AsyncRPG.unity project in src\unityClient\Assets
 - This will do an initial import of the assets the client depends on
 - Close the Unity editor once it's done importing
* Run the build_unity_client.bat in the scipts\build folder
 - This generates the game client in the build\client folder
* From this point forward you should be able to build both the server and client
code together using the scripts\build\rebuild_all_debug.bat script
