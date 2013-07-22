#3.1.55.158 - Test
+ Messagebox added to be shown on SSL validation errors giving the option to disable SSL cert validation from there. - Gravecorp
+ Fixed limited game card duplication bug. https://github.com/kellyelton/OCTGN/issues/864 - Gravecorp
+ Hard coded some paths for images etc to make things faster - Kelly
+ Fixed a login bug using custom data directory - Kelly
+ Made settings store actual types instead of just strings - Kelly
+ Added game settings for game developers https://github.com/kellyelton/OCTGN/issues/647 - Kelly
+ Fixed some things beng transparent that shouldn't be(like menu's etc) - Kelly
+ Removed all menu shortcuts from game window - Kelly
+ Added chat auto reconnect - Kelly

#3.1.55.157 - Test
+ Added option in the menu to ignore SSL certificate validity. Should fix the rare cases where SSL traffic certs are not properly validated. - Gravecorp
+ AskMarker dialog now sorts by marker name and no longer by GUID. - Gravecorp
+ fix for: https://github.com/kellyelton/OCTGN/issues/897 -Gravecorp

#3.1.55.156
#3.1.54.156 - Test
+ Fixed some excessive data usage

#3.1.54.155 - Test
+ Can view alternate images to cards in the deck editor https://github.com/kellyelton/OCTGN/issues/898 - Kelly

#3.1.54.154
#3.1.53.153 - Test
+ Fixed a login problem - Kelly
+ Changed marker dialog button - Davitz

#3.1.53.152 - Test
+ Fixed a proxygen issue(developers) https://github.com/kellyelton/OCTGN/issues/928 - Gravecorp

#3.1.53.151
#3.1.52.151 - Test
+ Fixed player tab text color https://github.com/kellyelton/OCTGN/issues/918 - Kelly
+ Fixed /table command https://github.com/kellyelton/OCTGN/issues/901 - Kelly
+ Removed full game log - Kelly

#3.1.52.150 - Test
+ Fixed some login issues - Kelly

#3.1.52.149 - Test
+ Sort games by game name - iGemini
+ Deck tries to resolve missing guids when loading a deck - Gravecorp
+ Update options dialog box - KlKitchens
+ Replaced installed game checkbox with a button for visibitily - KlKitchens
+ Added logging of loaded assembly paths - Kelly

#3.1.52.148
#3.1.51.148 - Test
+ Updated feature funding window - Kelly

#3.1.51.147
#3.1.50.147 - Test
+ Fix for myget change to https - klkitchens
+ Fix windows not having custom backgrounds - Kelly
+ Add message if not subscribed when adding an icon - Kelly

#3.1.50.146 - Test
+ Improve image(o8c) import process - klkitchens
+ Fixed https://github.com/kellyelton/OCTGN/issues/909 - Brine

#3.1.49.144
#3.1.48.144 - Test
+ Migrate card images to new location on start - Kelly
+ Delete old images - Kelly
+ Delete old proxies - Kelly
+ Clear old installer files - Kelly
+ Make update window auto scroll - Kelly
+ Change image install directory so images don't get deleted - Graves

#3.1.47.142
#3.1.46.142 - Test
+ Got rid of Subscription Message timer in place of a 'Close' button - Kelly
+ Moved and enhanced friends list - Kelly
+ Fixed a couple bigger memory leaks - Kelly
+ Added feature funding - Kelly

#3.1.46.141
#3.1.45.141 - Test
+ Fixed sub window showing for subscribers - Kelly
+ Fixed some update bugs - Kelly
+ Added shortcut key verification to o8build - Kelly

#3.1.45.140
#3.1.44.140 - Test
+ Fixed some sub issues - Kelly
+ Different disconnect handling - Kelly

#3.1.44.139
#3.1.43.139 - Test
+ Added sub message about password protected games - Kelly
+ Fixed a lot more bugs in game/deck editor etc - Kelly
+ Fixed local hosting - Kelly
+ Fixed gameserv spam - Kelly

#3.1.43.138
#3.1.42.138 - Test
+ Finished fleshing out password protected games - Kelly
+ Fixed unknown error when joining a game you don't have - Kelly
+ Fixed double clicking to resize columns causing a game join #810 - Kelly
+ Add space in username warning - Kelly

#3.1.42.137 - Test
+ Updated api to fix a crash - Kelly
+ Update to logging - Kelly
+ Updated some wording - Kelly
+ Fixed #873, Issues with direct to table not properly filling variables - Kelly
+ Added background to startup message + made it larger to better handle larger messages - Kelly
+ Added password protected games - Kelly

#3.1.42.136
#3.1.41.136 - Test
+ Fixed crash when not adding a name when adding a feed - Kelly
+ Fixed a deck editor crash - Kelly
+ Fixed common crash in sealed editor - Kelly
+ Fixed many more crashes - Kelly

#3.1.41.135
#3.1.40.135 - Test
+ Added profiles for users - Kelly
+ Added user icon support for subscribers - Kelly

#3.1.40.134 - Test
+ Some more bug/stability fixes - Kelly

#3.1.40.133
#3.1.39.133 - Test
+ Lots of bug fixes, heavy stability updates - Kelly
+ More UI Tuning

#3.1.39.132
#3.1.38.132 - Test
+ Fixed chat history deletion(deletes too much and leaves lines behind(or it used to anyways)) - Kelly
+ Improved chat break for unread messages - Kelly
+ Fixed some chat scrolling and also image scrolling issues - Kelly
+ Fixed image clicking now works - Kelly
+ Octgn gap more integrated into client - Kelly
+ Heavy UI retuning for custom backgrounds and style in general - Kelly

#3.1.38.131 - Test
+ Fixed bug in o8build - Kelly

#3.1.38.130
#3.1.37.130 - Test
+ Fix for duplicated cards in sealed - Brine
+ Marker image validation - Gravecorp
+ Updated subscription window - Kelly
+ Updated subscribe menu - Kelly

#3.1.37.129
#3.1.36.129 - Test
#3.1.36.128
#3.1.35.128 - Test
+ In game sound support
+ Custom window background images(skins)
+ Ability for user to change ingame background

#3.1.35.127
#3.1.34.127 - Test
+ Fixed knock knock not working for subscribers - iGemini
+ Added python call to get not active alternates properties #821 - Kelly
+ Updated to new site - Kelly

#3.1.34.126
#3.1.33.126 - Test
+ Switched icon for game log window
+ Got rid of game player tab transparency

#3.1.33.124
#3.1.32.124 - Test
+ More verbose buttons and labels for Game Management tab 
+ Allows for spaces and carriage returns before and after feed url
+ Added options for sound
+ Sound alert when someone @yourusername's you in chat
+ Sound alert when someone sends you an whisper
+ Command line based game starting for devs - /table /game=f80624da-58d7-4957-acd6-8a9ccb41619d
+ Fixes issue with cut off hover images
+ Hover chat or player area darkens background for easier reading

#3.1.32.124
#3.1.31.124 - Test
+ Usernames with spaces display correctly - Kelly
+ Use comma to seperate / commands instead of spaces - Kelly
+ Right click users in chat list - Kelly
+ In game chat resizing - Kelly
+ Can double click in game chat resize to snap chat to full game table height, or double click to snap back again. - Kelly
+ Game devs can now jump directly into a table after the 'Loading Window' using a command - Kelly
+ Loading window now shows which games are being checked to update, and which are updating - Kelly
+ Added a 'Full Game Log' option to the game menu, and the ability to save that log - Kelly
+ Added indenting on multiline chat in game - Kelly
+ Fixed #863 - Kelly

#3.1.31.123
#3.1.30.123 - Test
+ Proxygen fixed schema and validation on the format attribute. -Gravecorp

#3.1.31.122
#3.1.30.122 - Test
+ Removed white square in userlist when both scrollbars are shown. - iGemini
+ Proxygen conditional element if structure splitted value and contains logic for ease of debugging. - Graves
+ Improved image updating in deck editor Fixes #774 - Soul1355
+ Fixed issue with dragging between deck sections - Soul1355
+ Fix some issues in deck editor - Soul1355
+ Update GraphicUtils for proxy generator - DarkSir23
+ Wordwrap shrink to fit for proxies - Graves
+ Reduce overhead on image load in proxygen - Graves
+ Fixed contains not correctly working in some cases add a check for a null constant so you can check value for not defined/null its named #NULL# - Graves
+ Proxytester maintains image aspect ratio now - Graves
+ Fixed o8c group install - Graves

#3.1.30.121
#3.1.29.121 - Test
+ Fix start game dialog never closing - Kelly 
+ Fix temp cleanup during dns change - Kelly

#3.1.29.120
#3.1.28.120 - Test
+ Fixed broken limited deck editor #847 - Brine
+ Xml validation improvements in the o8build - Graves

#3.1.28.119 - Test
+ Implemented fixed/formated text values #846 - Graves

#3.1.28.118
#3.1.27.118 - Test
+ Simple timer for testing proxy gen speed - Graves
+ Added some logging for proxy gen speeds - Kelly
+ Fixed card clone null crash - Kelly
+ Fixed #817 #815 #791 alternate issues - Kelly

#3.1.27.117 - Test
+ Cleaned up some loading messages - Brine
+ More alternate fixes - Kelly
+ Fixed a typo - Kelly
+ Fixed if/else proxygen structure not breaking on match - Graves

#3.1.27.116 - Test
+ Xsd fix - Graves
+ Ignore undefined properties for cards - Kelly
+ Added some schema changes and some restrictions on attributes for proxygen - Graves
+ Proxygen xsd overhaul - Graves
+ Include xsd files in octgn's folder for devs - Graves
+ Added more loading messages - Brine
+ o8build alternate card validation - Graves

#3.1.27.115 - Test
+ Alternates get skipped for validation - Graves

#3.1.27.114 - Test
+ Changed o8build validation order added duplicated property checking for gamedef. - Graves
+ o8build now checks sets for duplicate properties on cards and on properties not defined in definition.xml - Graves
+ o8buildgui updated with selection of lines and a right click menu to copy to clipboard. - Graves
+ Start of askChoice API for multiple choice dialog boxes(python function askChoice('question string', [python list]) will create a box with radio buttons to select the option you want returns index number of chosen option in list) - Brine

#4.1.27.113 - Test
+ Removed xp warning, always install .net 4.0 - Kelly
+ o8build throws an error when version is not defined in a set while converting o8s files - Graves
+ Fixed big memory leak - Kelly
+ Cleanup proxygen xsd - Graves
+ Added option to o8buildgui to use test version instead of master - Graves
+ Redone switch conditional and updated xsd proxygen - Graves

#3.1.26.113
#3.1.26.112 - Test
+ More memory fix - Kelly
+ Fixed image check for chat images - Kelly
+ Updated changelog - Kelly

#3.1.25.112
+ Fixed chat memory issue
+ Chat images for subscribers(gifs disabled for now)
+ Login fixes
+ Fix of play window being always on top when fullscreen
+ Added html support for game documents
+ DB Fixes

#3.1.24.111
+ Fixed now can't edit username if playing online
+ Fixed some event issues

#3.1.24.110
+ Fixed some potential memory leaks

#3.1.24.109
+ Added image and gif support in chat for subscribers
+ Fixed menu bar hiding in game
+ Fixed chat message bar not re showing up if closed
+ Only highlight chat lines if hovering usernames
+ Only allow up and down in chat if no text
+ Add option to change chat history length
+ Added hint in chat text input to show /? command

#3.1.24.108
#3.1.23.108
+ Allow installing multiple o8c files
+ Fixed self hosted game https://github.com/kellyelton/OCTGN/issues/822
+ Fixed pre game lobby always in front and annoying https://github.com/kellyelton/OCTGN/issues/832
+ Fixed not being able to open 2 OCTGN's https://github.com/kellyelton/OCTGN/issues/831
+ Fix for switch statement in proxygen

#3.1.23.107
+ DESTROYED THE EVIL RIBBON CONTROL IN GAME
+ Less user list refreshes for performance

#3.1.23.106
+ User equality speed boost(just a performance thing)
+ elseif in proxy gen fix
+ More performance updates

#3.1.22.106
#3.1.21.106
+ Fixed some chat slow down
+ Trimmed down some logging
+ Adding some logging
+ Fixed game list not showing up
+ Cleaned out some unrequired junk
+ Addes switch case support for proxygen

#3.1.21.105
#3.1.20.105
+ Fixed user chat list
+ Added elseif to proxygen
+ New message box system https://github.com/kellyelton/OCTGN/issues/807

#3.1.20.104
+ Added some sub stuff
+ A fix for the chat log
+ Made version required in the set.xsd

#3.1.19.104
+ Fixed feed games don't update with some version changes https://github.com/kellyelton/OCTGN/issues/809
+ Updated the new login with some visual changes

#3.1.19.103
#3.1.19.102
+ Fixed potential crash/freeze due to breaking my philosophy that while loops are evil

#3.1.18.102
#3.1.17.102
+ More extensive login logging
+ Added more logging
+ Faster propagation???
+ Fixed where you could accidently resize the window when you resize
+ Contact list cleanup
+ Better user icon usage
+ Fixed a sorting bug in the user list
+ Fixed sub price typo

#3.1.17.101
+ Performance boosting

#3.1.17.100
#3.1.16.100
+ Badges for subbed users

#3.1.16.99
#3.1.15.99
+ Made an installer warning for xp

#3.1.15.98
+ Fixed more chat issues
+ Fixed a null crash

#3.1.15.97
+ More performance boosts
+ Killed the indygogo page
+ Increased chat limit to 1k
+ More chat improvements
+ Fixed slow loading of text files in game documents
+ Added install package option to o8buildgui
+ Added o8buildgui

#3.1.15.96
+ Fixed a chat bug

#3.1.14.96
+ Limit chat messages to 100
+ Fixed crash from nag window
+ Fixed bug talking to lobby while offline

#3.1.13.96 - First release of 3.1.x.x
+ Always start as command line(sas)
+ Removed some unnessisary tracing in lobby server
+ Fixed sub list
+ Fixed message box bug

#3.1.12.96
+ Cut sub nag box time in half
+ Fixed switchTo bug https://github.com/kellyelton/OCTGN/issues/797

#3.1.12.95
+ Fixed game time
+ Fixed o8c cancel not allowed to install another o8c

#3.1.12.94
+ Fixed a hack(nothing about that sounds right)

#3.1.12.93
+ This is the real hack for the test version

```
To come...There are 30 ish more changes that can be found in our commit list https://github.com/kellyelton/OCTGN/commits/master for now
```

#3.0.12.58
+ Fixed issue where people boot into the game and crash https://github.com/kellyelton/OCTGN/issues/723

#3.0.10.55
+ Fixed offline gaming https://github.com/kellyelton/OCTGN/issues/679
+ Fixed deck builder crash when no games installed https://github.com/kellyelton/OCTGN/issues/680

#3.0.9.52
+ Added ability to create cards in arbitrary groups https://github.com/kellyelton/OCTGN/issues/674 
+ Fixed bug that was causing OCTGN to crash when someone would join the pre game lobby.

#3.0.8.52[Test Build]
+ Added ability to create cards in arbitrary groups https://github.com/kellyelton/OCTGN/issues/674 
+ Fixed bug that was causing OCTGN to crash when someone would join the pre game lobby.

#3.0.8.51
+ Fixed multiple lobby chat tabs appearing over time
+ Defaulted HardwareRendering to 'ON'
+ Fixed config files not saving
+ Octgn remembers last game type hosted https://github.com/kellyelton/OCTGN/issues/641

#3.0.7.51[Test Build]
+ Fixed multiple lobby chat tabs appearing over time
+ Defaulted HardwareRendering to 'ON'
+ Fixed config files not saving
+ Octgn remembers last game type hosted https://github.com/kellyelton/OCTGN/issues/641

#3.0.7.50
+ Fixed crash from Game list resize.
+ Some file locking on config file so it doesn't keep breaking.
+ Drag issue in deck fixed

#3.0.6.48
+ Increased login page size
+ Added turnNumber() to python API https://github.com/kellyelton/OCTGN/commit/fbaf0ecd246b72d3a96150ebe457d47c92ab6f71

#3.0.5.47
+ Added missing login links for registration and password recovery etc.
+ Fixed Custom Games resizing issue
+ Removes title bar on fullscreen game
+ Added card.setController(player) api
+ Added card.peek() api
+ Added setActivePlayer() api

#3.0.3.43
+ More performance tuning
+ Added some performance options to the options menu
+ Performance options start on the lowest settings
+ Fixed offline play bug
+ Fixed Pre Game Lobby popping up behind main window.
+ Fixed Ctrl+S Shortcut in game

#3.0.2.42
+ Performance tuning

#3.0.2.41
+ Added options dialog
+ Added light chat option

#3.0.2.40
+ Fixed game hosting and joining.

#3.0.2.39
+ Added isTableBackgroundFlipped() and setTableBackgroundFlipped(flipped) to python api https://github.com/kellyelton/OCTGN/issues/540
+ Can now save a limited deck during play https://github.com/kellyelton/OCTGN/issues/559
+ Fixed deck editor crash https://github.com/kellyelton/OCTGN/issues/577

#3.0.2.38
+ **New Layout**
+ Fix issue with OCTGN window being offscreen
+ Converted most colors(I think)
+ Converted Table window to new style
+ Autorefresh news list
+ Changed contact list(for better or worse)
+ /commands for most user based things
+ Improved offline playability with this new layout
+ Better offline nickname picker
+ Fixed window dragging error(right click)
+ Fixed some race conditions
+ Improved deck editor drag drop

#3.0.1.32
+ Deck editor, most things are draggable(use shift key for some stuff)
+ Shuffling improvement
+ Offline hosting works with no internet connection again.

#3.0.1.31
+ Fixed game menus(missing shortcut keys and scrolling)
+ Shrunk down menu spacing

#3.0.1.30
+ Performance gain in lobby.

#3.0.1.29
+ Dummy change to fix updater.

#3.0.1.28
+ Friendslist is now sorted by online and offline and then alphabetically
+ Reworked how fonts are loaded from game definitions. Developers see [here](https://github.com/kellyelton/OCTGN/pull/621)
+ UI Changes
+ Made reconnecting not be a popup window anymore
+ Changed updating back to installer
+ New chat
+ Fixed offline games
+ fixed deck loading bug
+ In game chat window is now resizable
+ Fixed chat command crash
+ Created basic plugin system for the deck editor(octgn.library on nuget)
+ python: can now draw arrows

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
