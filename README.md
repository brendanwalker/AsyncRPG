AsyncRPG
========

A multi-player turned based dungeon crawler written in Unity/C#

### What is the goal of AsyncRPG?
AsyncRPG is a my attempt to make a turn based multi-player dungeon crawler that can either be played to completion in a single session with friends or a play by notification style akin to "Let's Draw Something". I really enjoy playing dungeon crawler games with friends, but as we've gotten older it's become difficult to dedicate hours at a time to a single board game. I've wanted to be able to start a campaign with friend, and then "save it to the cloud". Then you could either continue the campaign a move at a time on your smart phone when you have a few moments on the bus, or at home on your PC. If multiple people are signed on at the same time then the game should open up chat and become more real time.

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

### What's can I help with?
A million things. Despite everything done far there is a ton to do. Here's a few ideas:
* Design  
  * What are the player character classes? What abilities do they have?  
  * More fully define the enemy AI types? What abilities do they have?  
  * Power up / loot design?  
  * Bosses?  
  * Hacking puzzles?  
  * Dungeon Layout  
* Art
  * Need a real UI  
  * Using temp sprites, mostly purchased from RPGMaker.  
  * Particle effects  
  * Room Tiles  
* Sound  
  * Need new sounds for everything  
* Code  
  * Get running on the iPhone and Android (sqlite will be the interesting part)  
  * DB should initialize from imported data, rather then from files.  
  * UI work  
  * Effects hookup  
  * Player interaction (attacks, abilities, etc)  
  * New behaviors in the behavior tree  
  * Security (encryption, account info)  
  * Thread safety  
