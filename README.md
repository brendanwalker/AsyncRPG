AsyncRPG
========

A multi-player turned based dungeon crawler written in Unity/C#

### What is this exactly?
A detailed explaination of this project can be found [here](http://brendanwalker.github.io/AsyncRPG/)

### Project Organization
* \build  
  * \content - copy of map and mob template data used by the server on initialization  
  * \server - standalone game server and server tool  
  * \client - game client executable and dependent data 
* \docs - documentation that mirrors what's on the wiki  
* \lib - 3rd party libraries (DBLinq, LitJson, and System.Data.SQLite)  
* \scripts  
  * \build - batch files used to rebuild the game server and client  
  * \mssql - SQLServer specific setup scripts (IIS only)  
  * \os - Windows specific server setup scripts (IIS only)  
  * \script_data - Auto-verification script data  
  * \sqlite - SQLite server testing scripts  
* \src  
  * \content - map and mob template source data  
  * \server  
    * \AsyncRPGDBTool - Database modification and verification tool  
    * \AsyncRPGSharedLib - The vast majority of the server code  
    * \AsyncRPGWebServer - Wrapper exe for the stand alone web server  
    * \AsyncRPGWebService - Wrapper ASPX pages when hosting in IIS  
  * \unityClient - Game client source and dependant content  
* \tools - Ogmo editor and sqliteadmin
 
### How do I build and run this game?

#### Initial Build
1. Make sure the the paths are set up correctly in scripts\build\build_constants.bat
  * DOT_NET_FRAMEWORK should point to a .NET 3.5 framework install (needed for server build)  
  * UNITY_ROOT should point at the Unity install  
2. Run scripts\build\build_server_debug.bat
  * This builds the server DLLs that the Unity client is dependant on  
3. Launch the AsyncRPG.unity project in src\unityClient\Assets  
  * This will do an initial import of the assets the client depends on  
4. Close the Unity editor once it's done importing  
5. Run the build_unity_client.bat in the scipts\build folder  
  * This generates the game client in the build\client folder   

#### Building the Server 
Do either of the following:
* Build the src\server\AsyncRPGServer.sln in Visual Studio 
* Run scripts\build\build_server_debug.bat

#### Building the Client
Make sure to build the server at lease once, then do either of the following:
* Open and build src\unityClient\Assets\AsyncRPG.unity  
  * Need to do this at least once to import assets  
* Run scripts\build\build_unity_client.bat  

#### Building ALL THE THINGS!
If you have followed the initial build steps, then: 
* Run scripts\build\rebuild_all_debug.bat script
