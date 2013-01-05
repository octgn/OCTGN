#3.0.1.27
+ Localized and changed chat server endpoints
+ Added v1 of Deck Builder Plugin System
+ Offline games now can see their external IP

#3.0.1.26
+ Fixed 10 crash scenarios
+ Fixed a problem with the updater
+ Fixed webRead no response before timeout error.
+ Fixed https://github.com/kellyelton/OCTGN/issues/508 (Lobby chat missing on first start)

#3.0.1.25
+ Fixed webRead bug.

#3.0.1.24
+ Fixed issue with passwords with special characters not working

#3.0.1.23
+ Added password reset button
+ New login mechanism
+ New registration mechanism
+ Added error submitting
+ Fixed potential deadlock
+ Middle mouse button to pan
+ Added currentGameName() to get current hosted game name
+ { and } in askInteger don't crash anymore
+ Fixed temp cards moving crashing octgn
+ Whispering other players now more distinguishable
+ Sealed deck editor UI update
+ Games can now only grab cards from there own games

#3.0.1.22
+ Removed twitter feed and replaced with an xml file on the website.
+ Fixed a hosting bug.
+ Removed XML protocol from OCTGN
+ Quick fix for settings not saving.
+ Increased the login timeout for people with a bad connection
+ Can launch and connect to StandAloneServer using just the command line.

#3.0.1.21
+ Disabled sandboxing of python scripts.
 
#3.0.1.20
+ Fixed multiple instance of OCTGN message box always showing up.
+ Removed OCTGN Shortcut updater, as it caused crashes issues on weird machines.
 
#3.0.1.19
+ If multiple instances of OCTGN are running, prompts user if they want to kill them
+ If you log in, and another instance of OCTGN is logged in anywhere, it gets booted properly and doesn't create a never ending binding loop
+ Updates OCTGN links on the Desktop, Start Menu, Quick Launch, Pinned Task Bar, and Pinned Start Menu to point to the current install location on startup.
+ Unbroke offline games
 
#3.0.1.18
+ Changed the updater
+ Fixed some url's
+ Added auto build+release stuff for CruiseControl.net
 
#3.0.1.14
+ Fixed autoupdate bug.
+ 
#3.0.1.13
+ Fixed bug that caused load to hang

#3.0.1.12
+ Removed the split screen
+ Added help in file menu
+ Added automatic updating

#3.0.1.11
+ Make gamelist autorefresh
+ Arrange user list in lobby by name
+ Adding friends and chatting works again
+ User status fixed
+ Lobby is now opt-in

#3.0.1.10
+ Fixed another game install bug
+ Added game isolation(games are unaware of each other)
+ Added game uninstallation(select a game to get to the set listing and click remove game)

#3.0.1.9
+ Fixed install game bug

#3.0.1.7:
+ Minor boot changes
+ Lobby Sounds
+ Setup enhancements 

#3.0.1.6:
+ Changed .net framework requirements from full to client profile.
+ New server
+ Proper SQL query escaping for Deck Builder

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
