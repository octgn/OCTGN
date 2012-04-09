#3.0.1.0 Running List:

+ Chatting service seperated from game hosting service
+ Offline play added
+ More info about hosted games
+ Python error checking/catching in multiple places
+ Removed lots of dead code
+ New database using Sqlite
+ Added Game Rules window to play
+ Moved user settings to a settings file
+ No longer uses Google to authenticate
+ Huge amount of code cleaning/refactoring
+ GlobalVariables for game devs.
+ Headlines on login page
+ Offline messages
+ Fixed several UI bugs
+ Added About window and links on login page
+ Lobby reconnects on disconnect
+ Sub directory for sets
+ Can set custom data directory for octgn
+ Can now filter game list by game name
+ Fixed too many bugs to count.  Take a look at https://github.com/kellyelton/OCTGN/issues?sort=updated&direction=desc&state=closed

#3.0.0.3:

+ Moved user settings to the registry
+ Added "/developer" command while in game for python console
+ Added python console button in game
+ Changed some window titles
+ When exiting deck editor, it now asks you if you want to save your changes, if changes were made
+ When making a new deck, prompts you to save current deck if not saved
+ Added server messages, and timed server restart functions
+ Added status messages when logging in
+ Added server status to login page

**Fixed**:

+ Lobby Chat no user issue/can't host game
+ Issue where Octgn will sometimes run in background when closed
+ Lots of instabilities and random crash problems.
+ Username not carried through to game
+ Better checking of finished games
+ Problems selecting sets in the set list
+ Multiple windows issues
+ Fixed Log Off/Quit issues with windows
+ Removed AboutWindow in game

**Python**:

+ Added me.isActivePlayer
+ Added openUrl(url)