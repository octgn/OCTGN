#3.0.1.6:
+ Changed .net framework requirements from full to client profile.

#3.0.1.5:
+ Fixed font permission issue with UAC
+ New database rebuild table
+ Supports help.txt documents
+ Fixed deck editor display empty fields on a deleted game

#3.0.1.4:

+ Fixed hosted games not showing up
+ Fixed bug when switching filter
+ Optional custom font support

#3.0.1.3:

+ Problem with two OCTGN's open at once, booting eachother off fixed
+ Trim debug window so it doesn't overflow
+ Fixed up SimpleConfig
+ Chat topic can now be set by chat owners, and exists in Lobby chat
+ Remembers if you checked Two-Sided Table
+ Default game name, and remembers last game name
+ Announcements from server is now possible
+ Save password feature fixed
+ Game run time fixed
+ Games not refreshing fixed

#3.0.1.2:

+ Added option in File menu to disable installing sets/defs on startup
+ Fixed problems with installer(SQLite and others)
+ Fixed issue with pregame lobby not changing size

#3.0.1.1:

+ Catches errors when installing/checking defs/sets on startup
+ Fixed crash when OCTGN can't access news feed
+ Fixed memory leak in deck editor
+ Fixed deck editor crashes 
+ Fixed midgame crashes due to SimpleDataTableCache
+ Fixed ram usage issues


#3.0.1.0:

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
+ Clicking the preview image in DeckEditor flips cards
+ Hovering previews of cards with alternate images (hold alt key)
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
