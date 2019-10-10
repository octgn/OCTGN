#3.4.209.0
+ Removed Version and GameVersion requirements from set XMLs
+ Add optional attributes ShortName, Description, ReleaseDate to set XML

#3.4.208.0
+ fixed decks being falsely flagged as unsaved when moving the cursor over the deck - Ben
+ removed card counts from player/global editor tabs if they have no sections - Ben
+ disabled editor deck tabs when no deck is loaded - Ben
+ Fixed crashes related to shared deck sections in the editor - Ben

#3.4.207.0

#3.4.206.0
+ Added more features to the installer - Kelly

#3.4.203.0
+ Add ability to change DataDirectory and ImageDirectory inside of OCTGN - Kelly

#3.4.202.0
+ New Installer - Kelly E
+ New Data Directory Config - Kelly E

#3.3.141.0
+ Fixed a few bugs - Kelly

#3.3.140.0
+ Phase bar and deck stats don't overlap the limited deck editor
+ Fixed slowdown when resizing deck editor in some cases - Ben
+ Added card count to player/global tabs - Ben
+ o8build: Don't throw an unexpeted error when a game has no scripts. (allows for basic modules with no scripts to be built)
+ Fix game replay ordering - Ben
+ Moved extended chat behind zoomed card - Ben
+ Allow launching the deck editor directly with no deck specified via -e - Ben

#3.3.132.0
+ Fixed bug blocking Pass Control and Take Control actions

#3.3.131.0
+ Fix periodic lockups in game - Kelly
+ Fix not being able to connect to an offline game - Kelly
+ Fixed 'Skip' button showing up when joining games - Kelly
+ Fixed issue sometimes causing OCTGN to show a 'Disconnected' dialog when we're actually connected - Kelly

#3.3.130.0

#3.3.129.0

#3.3.127.0
+ Fix bug with Global Player - Kelly

#3.3.126.0
+ Add replay support - Kelly
+ Update some buttons and tab bar styles - Kelly

#3.3.125.0
+ Fix issue causing network calls to go out of order - Kelly
+ Fix built in sleeves not being included in the installer - Kelly
+ Fix Phase Control text being cut off if too long - Kelly
+ Fix issue with lobby popping up behind the main window. - Kelly
+ Fixed some issues where hovering/clicking was only working on certain parts of list items (games list etc) - Kelly

#3.3.124.0

#3.3.123.0

#3.3.122.0

#3.3.120.0

#3.3.118.0
+ Disable custom sleeves in online play for now - Kelly

#3.3.117.0
+ Custom Sleeves - Kelly

#3.3.116.0
+ Don't categorize deck stats. - Kelly

#3.3.115.0
+ Fix game reset not working - Kelly
+ Add Card List menu item in game - Kelly

#3.3.114.0

#3.3.113.0

#3.3.112.0
+ Made deck editor window more consistent with available hotkeys - Ben
+ Disable 'instant' searches for 1-2 characters, should dramatically improve performance - Ben

#3.3.111.0
+ Fixed bolding for active deck section - Ben
+ Made New Deck Hotkey respect unsaved decks - Ben
+ Deck editor improvments, new hotkeys, better keyboard navigation. see wiki - Ben
+ Made host game window select game if only one installed - Ben
+ Other minor usability improvements in deck editor - Ben

#3.3.110.0
+ Updated EULA - Kelly
+ Include o8build, Log Exporter, and Octide in installer - Kelly
+ Better uninstall instructions - Kelly

#3.3.108.0
+ made main window close respect deckeditor cancel - Ben

#3.3.107.0

#3.3.106.0
+ Windows 7 fixes - Kellye

#3.3.105.0

#3.3.104.0
+ Fixed OR searches for everything - BenMatteson

#3.3.103.0
+ Fixed bug where multi-property matching was being ignored in the booster pack generator
+ Fixed searching multiple sets in the deck editor - BenMatteson

#3.3.102.0

#3.3.101.0

#3.3.100.0

#3.3.99.0

#3.2.98.0

#3.2.97.0

#3.2.96.0
+ New installer - Kelly

#3.2.92.0
+ Fix bug where adding another filter to a loaded saved search crashes OCTGN - Kelly

#3.2.91.0

#3.2.90.0
+ Fixed zoom bug where zooming out could invert table and cause crash - Kelly
+ Made mouse wheel zoom and keyboard zoom consistent regardless of zoom level - Kelly

#3.2.89.0
+ Fixed offline joining game complaining of game service being unavailable - Kelly

#3.2.88.0
+ Fix selection dialog being stuck on game table - Kelly

#3.2.87.0
+ Fixed reconnecting - Kelly

#3.2.85.0
+ spectators cant activate hotkeys (fixed a crash)
+ only you and the host can modify your spectator or playerside setting

#3.2.84.0

#3.2.83.0
+ Fixed a networking bug involving card alt switching

#3.2.82.0
+ Use a better RNG for generating packs; fixes bug with bad RNG seeds preventing certain combinations of options in pack generator.

#3.2.81.0
+ Font size attribute in game def now optional, uses OCTGN's default for that font if not specified
+ o8build catches font sizes of 0 or less
+ Preferences added for Note, Deck Editor, Context, and Chat font sizes; groundwork for adding adjustable default font sizes to settings.

#3.2.80.0
+ Fixed crash in deck editor when filtering on 'Does Not Equal' issue #1695 - Soul1355

#3.2.79.0
+ game document icons now appear in the play window
+ added 'changetwosidedtable' boolean attribute in game def to disable the host from changing two-sided table status
+ o8build - game document icon attribute is now optional
+ o8build - game font src attribute is now optional, uses OCTGNs default font if omitted
+ o8build - will validate that the gameId value matches the game's GUID
+ o8build - height/width attribute removed from hand and groups; required on table
+ gamedatabase - backend code to load a gamedatabase from a custom directory

#3.2.78.0
+ Fixed a bug where python wasn't returning the correct card property values, especially when alternates were involved and the property was supposed to default to the base card's property.

#3.2.77.0
+ Added "Load Pre-Built deck" in play window game menu, which opens the dialog window into the game install's Decks folder.
+ Installing/updating a game will no longer copy the game's bundled decks into OCTGN's decks folder.

#3.2.75.0
+ fixed bug where players were never assigned to the inverted side of the two-sided table by default

#3.2.74.0

#3.2.73.0

#3.2.70.0

#3.2.69.0
+ Minor code cleanup
+ F10 hotkey (also in Options menu) to reset game table zoom and position.
+ Fix crash when closing an offline Connect to Game dialog
+ Fix issue where two-sided table wasn't consistent between players

#3.2.65.0

#3.2.63.0

#3.2.62.0

#3.2.61.0

#3.2.60.0

#3.2.59.0

#3.2.58.0

#3.2.57.0

#3.2.56.0

#3.2.55.0

#3.2.54.0

#3.2.53.0
+ Fix limited deck editor showing for spectators and crashing the game.

#3.2.52.0
+ Fixed log sharing.

#3.2.51.0
+ Fixed sorted deck section sometimes showing wrong card when clicked - Soul1355

#3.2.50.0
+ Added api call rndArray - Kelly

#3.2.49.0

#3.2.48.0

#3.2.47.0

#3.2.46.0

#3.2.45.0

#3.2.44.0

#3.2.43.0

#3.2.42.0

#3.2.41.0

#3.2.40.0

#3.2.39.0

#3.2.38.0

#3.2.37.0

#3.2.36.0

#3.2.35.0

#3.2.33.0

#3.2.32.0

#3.2.31.0

#3.2.30.0

#3.2.29.0

#3.2.26.0

#3.2.25.0

#3.2.24.0

#3.2.21.0

#3.2.20.0

#3.2.18.0

#3.2.17.0

#3.2.15.0

#3.2.14.0

#3.2.13.0

#3.2.12.0

#3.1.303.0
+ Fix scroll positon in Limited Deck Builder, #1675

#3.1.302.0

#3.1.301.0

#3.1.300.0

#3.1.299.0
+ added 'alternates' and 'highlights' arguments for 3.1.0.2 CardsMoved events

#3.1.298.0
+ Added the 'RichText' card property Type, rich text properties must now declare this type.

#3.1.297.0

#3.1.296.0
+ Major changes to the Phase system
+ Phases now cycle sequentially, active player will set stops on phases instead of jumping between them
+ Phase stops will persist between turns
+ Clicking the "Pass turn" green arrow button on the player tab will sequentially scan through phases and pause on the first phase that has a stop set.  If there are no stopped phases, it will pass the turn to the next player (old functionality)
+ Added OverridePhaseClicked and OverrideTurnPassed override events to control the functionality of the Pass Turn and phase buttons
+ Added several python API calls to provide greater control over changing turns, changing phases, changing active player, toggling stops
+ Fixed a visual bug involving the play/pause turn buttons on the player tab

#3.1.295.0
+ OverrideCardsMoved event now passes faceups argument indicating the faceup end-status of the card (if the user held shift to play the card facedown, etc)
+ queryCard python 3.1.0.2 API will return a list of GUIDs matching card filter parameters (same parameters as askCard)
+ fixed being unable to get GUIDs of facedown cards in 3.1.0.2 API
+ added discord link to login page and cleaned up twitter link
+ fixed an issue where the inverted table A/B indicators in the pre-game lobby weren't toggling properly
+ fixed a gameserializer crash when a game has no gameboard children defined
+ Support for advanced text formatting for set XML property values
+ - Added Symbol definition to Game XML for icons (replaces need to use custom fonts for icons)
+ - Added in-line support for bold/italic/colored fonts, and symbol icons, in card property values (only appears in deck editor)

#3.1.294.0
+ Game Installer copies plugins folder to OCTGN's plugin folder

#3.1.293.0
+ Added a checkbox to hide alternates from deck editor searches

#3.1.292.0
+ Updated some packages - Kelly

#3.1.291.0
+ First fix update - Kelly

#3.1.290.0

#3.1.289.0

#3.1.288.0

#3.1.287.0

#3.1.286.0

#3.1.285.0

#3.1.284.0
+ Allow logging in with email - Kelly

#3.1.283.0
+ showIf and getName action tags now properly pass group and table coordinates to python

#3.1.282.0
+ Fixed issue causing updates of games to not be installed - Kelly

#3.1.281.0
+ Don't refresh the game list so much - Kelly

#3.1.280.0
+ Created an All list in the Game Manager - Kelly

#3.1.279.0
+ Removed more subscription stuff - Kelly

#3.1.278.0
+ Removed Special Offer bar - Kelly
+ Changed subscribe message to new model - Kelly
+ Fixed adding feeds being broken - Kelly

#3.1.277.0
+ Fixed issue where a user leaving before the game start would cause the game to break - Kelly

#3.1.276.0
+ player.setActive() and setPhase() python API can now pass an optional 'force' bool parameter, True will ignore players pausing the turn/phase and force to target
+ Event overrides will prevent OCTGN from handling certain functions and pass the relevant parameters to python instead
+  - OverrideCardsMoved event for drag/drop card(s) movements, and the various 'Send To' default actions in context menus
+  - OverrideTurnPassed event for players clicking the 'Next Turn' arrows
+  - OverrideGameReset event for resetting the game via the menu option
+  - OverridePhasePassed event for players changing phases

#3.1.275.0
+ Brought back Twitch.tv support - Kelly

#3.1.274.0
+ Brought back(fixed) sleeve support - Kelly

#3.1.273.0
+ Tightened up Game Service - Kelly

#3.1.272.0
+ Removed matchmaking - Kelly
+ Fixed chat times in lobby - Kelly

#3.1.271.0
+ added some new loading messages, no biggie

#3.1.270.0
+ choosePack() API now returns a tuple (setname, packname, packID) instead of just the ID

#3.1.269.0
+ Fixed chat not working - Kelly

#3.1.268.0
+ generatePack(packId) API uses OCTGN's card pack generator to return a list of card GUIDS comprising a pack of cards.

#3.1.267.0
+ interactive Game Phases toolbar added to game window, with python and event hooks

#3.1.266.0
+ Added a bulk card image importer option to deck editor (sub only)

#3.1.265.0
+ Undid MyGet hack fix since they updated their site - Kelly

#3.1.264.0
+ fixed a silly bug that broke game right-click menus
+ renamed showName tag in action definition to getName

#3.1.263.0
+ Fixed missing titles - Kelly

#3.1.262.0

#3.1.261.0
+ Added 'showName' tag to group and card actions, can be used to rename the action in the menu via python
+ Expanded showIf functionality to groups (passes empty list as parameter to python function)

#3.1.260.0
+ Disabled matchmaking tab.

#3.1.259.0
+ fixed an issue where card alternate flags became case-sensitive (they shouldnt be)

#3.1.258.0
+ Invalid integer values on integer custom properties now default to null values in deck editor (makes card filters more effective)

#3.1.257.0
+ Alternate card images load properly when the base card image is missing

#3.1.256.0
+ Added a zoom slider to the SelectMultiCard dialog window

#3.1.255.0
+ Fixed some bugs with web_read and web_post - Kelly
+ Cleaned up some code - Kelly
+ Wrote some unit tests - Kelly

#3.1.254.0

#3.1.253.0
+ Hello 4.5 - Kelly

#3.1.252.0
+ can zoom the table with CTRL+ and CTRL-

#3.1.251.0
+ loading a limited deck will announce a different load message to prevent cheats

#3.1.250.0
+ added OnCardControllerChanged to 3.1.0.2 API game events

#3.1.249.0
+ - Can define additional properties to match with for card pack generator
+ - Can include cards from other set defs within specific card packs
+ - Can define alternative property values to cards included from other sets

#3.1.248.0
+ card alternates can define their own card sizes

#3.1.247.0
+ #1510 sets can be hidden from the deck editor

#3.1.246.0
+ The API version immediately before the most recent one won't get flagged for incompatibility.

#3.1.245.0
+ Fixed an issue with multiple screens support introduced with window decorators

#3.1.244.0
+ reverted commit that disabled game feeds

#3.1.243.0

#3.1.242.0
+ octide wont error out anymore :)

#3.1.241.0
+ added option to use native os window borders, also added an updated octgn window border - BoykaTheMad

#3.1.240.0
+ fixed issue where new card select dialogs were larger than the screen size on small resolutions

#3.1.239.0
+ Allow player color to be set through python - Gemini

#3.1.238.0
+ enhanced cardDlg to allow drag/drop reordering between one or two lists of cards
+ Red highlight on selected cards in the dialog window

#3.1.237.0
+ Multi-touch support to pinch-zoom and move the table

#3.1.236.0

#3.1.235.0
+ Updated the login page for updated login system - Kelly

#3.1.234.0
+ alternates will show up in deck editor grid now - brine

#3.1.233.0
+ Added option to change sound effect used when players join a game
+ Glorious KnockKnock makes a triumphant return

#3.1.232.0
+ Added workaround to mitigate custom font issues introduced in Windows 10 update 1511- Kelly

#3.1.231.0
+ Fixed The Spoils ad - Kelly

#3.1.230.0
+ Updated The Spoils images - Kelly

#3.1.229.0
+ Fixed issue: #1523 o8build validation on deck sections needing valid group targets.

#3.1.228.0
+ Activated 3.1.0.2 python API to live

#3.1.227.0
+ Make game chat obey font size setting - Soul1355

#3.1.226.0
+ Don't crash if deleting garbage fails - Kelly

#3.1.225.0
+ More Deck Editor tweaks - Soul1355

#3.1.224.0
+ Another attempt to fix issues launching Deck Editor - Soul1355

#3.1.223.0
+ Refactor sort button to hopefully resolve deck editor not launching for some - Soul1355

#3.1.222.0
+ Fix a networking bug - Kelly

#3.1.221.0
+ Fixed some exceptions - Kelly

#3.1.220.0
+ fixed a game install bug because some bugs don't have a boardposition defined

#3.1.219.0
+ changed player.hasInvertedTable() to player.isInverted

#3.1.218.0
+ got rid of the dumb warning messages

#3.1.217.0
+ fixed a bug with swapping gameboards where the width/height wasn't updating visually - brine

#3.1.216.0
+ fixed bug where you cant install/play games that have no board defined - brine

#3.1.215.0
+ Add ability to sort deck sections (finally) - Soul1355
+ Other miscelaneous editor improvements - Soul1355

#3.1.214.0
+ Added support for multiple game boards
+ 3.2.0.2 API for changing game boards is now table.board (get/set)
+ changing game boards networks to all players
+ game boards aren't associated with the table group anymore in the XML.
+ Made sure compatibility isn't broken with games that still use the old method of declaring boards inside the <table> tag.

#3.1.213.0
+ Fix Crops folder being allowed by o8build - Gravecorp
+ o8build verify valid card size names in set.xml - Gravecorp

#3.1.212.0
+ Added SetName property to proxygen to expose the cards set name to the templates ticket #1498 - Gravecorp

#3.1.211.0
+ added "moveto" to <group> element, False will hide the group from the Move To submenu
+ player names are colored correctly when one player's name contains another player's name (I.E. brine vs brine42)
+ scripted argument returns to OnMarkerChanged 3.1.0.2 event
+ marker name and GUID are now passed as separate arguments in 3.1.0.2 API
+ removed some 3.1.0.2 event trigger restrictions
+ selectCard API changed to a "cardDlg" class object, use cardDlg.show() to display the dialog window
+ cardDlg returns the proper card object list for single-choice mode

#3.1.210.0
+ Remove lag window for stability while we investigate better feedback for lag - Soul1355

#3.1.209.0
+ Fix issue installing image packs when offline - Kelly

#3.1.208.0
+ Updated some lag window stuff to make it cleaner - Kelly
+ Ignore strange lag window error - Kelly

#3.1.207.0
+ Players leaving the game when its their turn will reset the active player to null (initial)

#3.1.206.0
+ Hide Debug and Hide Error checkboxes will affect all previous chat log as well

#3.1.205.0
+ Improved performance - Soul1355

#3.1.204.0
+ Events trigger after their chat notifications (counter, targets, turn passing)
+ moving an anchored card to another group will remove the anchor
+ Added a "Hide Debug" toggle to hide the debug messages that print to the chat log in dev mode

#3.1.203.0
+ Updated lag window so it isn't cloasable - Kelly
+ Changed some text in the window - Kelly

#3.1.202.0
+ Fixed cards face up being messed up after reconnect - Kelly

#3.1.201.0
+ Fixed bug with winforms conversion when the script line was commented out

#3.1.200.0
+ Fixed issue with winforms - Kelly
+ Added api call showWinForm - Kelly

#3.1.199.0
+ upgrades to the selectCard() API calls

#3.1.198.0
+ Fixed all build - Kelly

#3.1.197.0
+ Temp fix for people using WinForms - Kelly

#3.1.196.0
+ Fixed issue where if you disconnect spectators become players - Kelly
+ Fixed issue where cards are face up that shouldn't be after reconnect - Kelly
+ Fixed issue where the Waiting For Players windows doesn't go away properly - Kelly

#3.1.195.0
+ Fixed a marker bug - Kelly

#3.1.194.0
+ 3.1.0.2 API - reverted player.isActive setter back to player.setActive()

#3.1.193.0
+ Fixed non default marker naming #446 - Kelly

#3.1.192.0
+ Fixed o8build error in card size path validation - Gravecorp

#3.1.191.0
+ OnCounterChanged, OnCardTargeted, OnCardArrowTargeted now include 'scripted' arguments
+ fixed bug where dynamic properties commit made all property names case-sensitive.

#3.1.190.0
+ Fixes spectator checkbox infinite loop - Kelly

#3.1.189.0

#3.1.188.0
+ Added the 3.1.0.2 Python API
+ Significant changes to the names and functionality of many API functions
+ Changes to several game events
+ Added card.filter to the 3.1.0.1 and 3.1.0.2 APIs, which adds a transparent colored overlay to the card image
+ Added card.set API to return the name of the set the card comes from
+ Split apart askCard's two modes, the LIST mode is now named chooseCard
+ card.height and card.width now return the correct values for front and back of custom sized cards
+ moved offset and isInverted API functions from the table to the Card class
+ card.offset will default to the card's position if no parameters are passed
+ card.isInverted will default to the card's y coordinate if no parameters are passed
+ added player.isSubscriber API to tell if a player is a subscriber
+ chooseCards 3.1.0.2 API now supports multi-card selections, can pass min and max selection counts, will return list of card selected objects
+ fixed some bugs with the onMarkerChanged event not passing the proper values
+ Use objects instead of argument lists in events for ScriptAPI v 3.1.0.2 and
+ greater - Kelly

#3.1.187.0
+ Fixed a bug - Kelly

#3.1.186.0
+ Fixed octgn not shutting down cleanly - Kelly

#3.1.185.0
+ Fix the UI Lag Window so it doesn't stay up after the game closes - Kelly
+ Make the UI Lag Window take more time to pop up - Kelly

#3.1.184.0

#3.1.183.0
+ Improve interaction between resizing chat and player tabs - Soul1355

#3.1.182.0
+ Added the ability to change card properties dynamically - Kelly

#3.1.181.0
+ Fix some image loading stuff - Gemini

#3.1.180.0
+ Added GUI option to change over to test releases.

#3.1.179.0
+ O8buildgui file name change to something more sensible - Gravecorp
+ Removed test o8build option from O8buildGUI - Gravecorp
+ Added path checks for card sizes in o8build - Gravecorp

#3.1.178.0
+ Added window to show if UI is lagging - Kelly

#3.1.177.0
+ Make sure that when upsidedown it figures out proper center - Kelly

#3.1.176.0
+ Fixed some issues with card inversion and custom sizes - Kelly

#3.1.175.0
+ Fixed the transparent click through of images - Kelly

#3.1.174.0
+ Fixed a few spam errors - Kelly

#3.1.173.0
+ Can now click through transparent areas of Cards - Kelly

#3.1.172.0

#3.1.171.0

#3.1.170.0

#3.1.165.381

#3.1.164.381 - Test

#3.1.164.362
+ 

#3.1.163.362 - Test
+ 

#3.1.164.380 - Test

#3.1.164.376 - Test

#3.1.164.377 - Test

#3.1.164.374 - Test

#3.1.16.374 - Test

#3.1.13.377

#3.1.12.369 - Test

#3.1.9.369

#3.1.163.361

#3.1.162.361 - Test

#3.1.162.360

#3.1.161.360 - Test

#3.1.161.359 - Test

#3.1.161.358 - Test

#3.1.161.357 - Test

#3.1.161.356
+ Proxygen if contains and else conflict fix. - Gravecorp
+ Don't display network share error if fix in place - Gravecorp
+ Add seperate size and corner radius for upside down cards - Kelly
+ Updated and removed some sounds - Kelly
+ Allow copying of errors from game text - Kelly
+ Fix a bug where spectators are upsidedown - Kelly
+ fixes #1371 anchor lock returns to left side of card - brine
+ Check for _id attribute in __cmp__ methods - Seurimas
+ Increased readability for card amounts in the Pick Cards Dialog - Celludriel
+ Enable nicer font rendering mode - Rstarkov
+ Fixed up some sizes of some UI items for users with larger fonts - Rstarkov
+ Added script APi version deprecating - Kelly
+ Make player area resizable - Soul1322
+ Updated Challenge Board to use full search query - Kelly
+ Fix some items in lists where clicking in certain areas didn't do anything - Kelly
+ Fixed some various code formatting issues - Rstarkov
+ Card movement scripting updates - Brine

#3.1.160.356 - Test
+ Proxygen if contains and else conflict fix. - Gravecorp
+ Don't display network share error if fix in place - Gravecorp
+ Add seperate size and corner radius for upside down cards - Kelly
+ Updated and removed some sounds - Kelly
+ Allow copying of errors from game text - Kelly
+ Fix a bug where spectators are upsidedown - Kelly
+ fixes #1371 anchor lock returns to left side of card - brine
+ Check for _id attribute in __cmp__ methods - Seurimas
+ Increased readability for card amounts in the Pick Cards Dialog - Celludriel
+ Enable nicer font rendering mode - Rstarkov
+ Fixed up some sizes of some UI items for users with larger fonts - Rstarkov
+ Added script APi version deprecating - Kelly
+ Make player area resizable - Soul1322
+ Updated Challenge Board to use full search query - Kelly
+ Fix some items in lists where clicking in certain areas didn't do anything - Kelly
+ Fixed some various code formatting issues - Rstarkov
+ Card movement scripting updates - Brine

#3.1.160.355 - Test
+ Proxygen if contains and else conflict fix. - Gravecorp
+ Don't display network share error if fix in place - Gravecorp
+ Add seperate size and corner radius for upside down cards - Kelly
+ Updated and removed some sounds - Kelly
+ Allow copying of errors from game text - Kelly
+ Fix a bug where spectators are upsidedown - Kelly

#3.1.160.354 - Test
+ Proxygen if contains and else conflict fix. - Gravecorp
+ Don't display network share error if fix in place - Gravecorp
+ Add seperate size and corner radius for upside down cards - Kelly

#3.1.160.353
+ Fixed some card size bugs - Kelly
+ Fix some card sizing pythong issues - Brine

#3.1.159.353 - Test
+ Fixed some card size bugs - Kelly
+ Fix some card sizing pythong issues - Brine

#3.1.159.352
+ Removed need for any kind of hack for reading card data/local or remote - Kelly
+ Don't change the interface to not subscribed if the user is subscribed but there was web call failure. - Kelly

#3.1.158.352 - Test
+ Removed need for any kind of hack for reading card data/local or remote - Kelly
+ Don't change the interface to not subscribed if the user is subscribed but there was web call failure. - Kelly

#3.1.158.351 - Test
+ Removed need for any kind of hack for reading card data/local or remote - Kelly

#3.1.158.350
+ Made it so that anchor icon is only visible if hovered - Brine
+ Fixed some table menu items showing up in the hand - Kelly
+ Fixed some anchor error messages - Kelly
+ Fixed #1359 Break in script engine when calling certain python calls in OnGameStart and OnTableLoad - Kelly
+ Fixed #1348 Added the ability for a developer to trigger on all cards that moved in an operation, instead of just one - Kelly
+ Made the networking batch together all move operations - Kelly
+ Added OnMoveCards, OnScriptedMoveCards events - Kelly
+ Fixed #1344 If group width/height == 0, it sets it to 1. O8build will also show a relevant warning about it - Kelly
+ Fixed #1347 Getting disconnecting durring a random call can cause the script engine to become unrepsonsive = Kelly
+ Fixed #416 #1227 can get the results of a peek on a card if it's your own immediately - Kelly

#3.1.157.350 - Test
+ Made it so that anchor icon is only visible if hovered - Brine
+ Fixed some table menu items showing up in the hand - Kelly
+ Fixed some anchor error messages - Kelly
+ Fixed #1359 Break in script engine when calling certain python calls in OnGameStart and OnTableLoad - Kelly
+ Fixed #1348 Added the ability for a developer to trigger on all cards that moved in an operation, instead of just one - Kelly
+ Made the networking batch together all move operations - Kelly
+ Added OnMoveCards, OnScriptedMoveCards events - Kelly
+ Fixed #1344 If group width/height == 0, it sets it to 1. O8build will also show a relevant warning about it - Kelly
+ Fixed #1347 Getting disconnecting durring a random call can cause the script engine to become unrepsonsive = Kelly
+ Fixed #416 #1227 can get the results of a peek on a card if it's your own immediately - Kelly

#3.1.157.349 - Test
+ Made it so that anchor icon is only visible if hovered - Brine
+ Fixed some table menu items showing up in the hand - Kelly
+ Fixed some anchor error messages - Kelly
+ Fixed #1359 Break in script engine when calling certain python calls in OnGameStart and OnTableLoad - Kelly
+ Fixed #1348 Added the ability for a developer to trigger on all cards that moved in an operation, instead of just one - Kelly
+ Made the networking batch together all move operations - Kelly
+ Added OnMoveCards, OnScriptedMoveCards events - kelly

#3.1.157.348

#3.1.156.348 - Test

#3.1.157.345
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly
+ Fixes bug that causes flipped cards to not flip over again - Kelly

#3.1.156.347 - Test
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly
+ Fixes bug that causes flipped cards to not flip over again - Kelly

#3.1.156.346 - Test
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly
+ Fixes bug that causes flipped cards to not flip over again - Kelly

#3.1.156.345 - Test
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly
+ Fixes bug that causes flipped cards to not flip over again - Kelly

#3.1.156.344 - Test
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly

#3.1.156.343 - Test
+ Added ability to anchor/unanchor cards via right click menu or through scripting(card.setAnchored card.anchored) - Kelly

#3.1.156.342
+ fix pile-view window for small play window https://github.com/kellyelton/OCTGN/issues/1320 - Soul1355
+ Fixes card sleeve error spam https://github.com/kellyelton/OCTGN/issues/1339 - Kelly
+ Fix crash from user error when starting directly to table https://github.com/kellyelton/OCTGN/issues/1299 - Kelly
+ Fixed menu save deck button not working https://github.com/kellyelton/OCTGN/issues/1341 - Kelly
+ Added game count, total game time, average game time, and levels to the profile page - Kelly

#3.1.155.342 - Test
+ fix pile-view window for small play window https://github.com/kellyelton/OCTGN/issues/1320 - Soul1355
+ Fixes card sleeve error spam https://github.com/kellyelton/OCTGN/issues/1339 - Kelly
+ Fix crash from user error when starting directly to table https://github.com/kellyelton/OCTGN/issues/1299 - Kelly
+ Fixed menu save deck button not working https://github.com/kellyelton/OCTGN/issues/1341 - Kelly
+ Added game count, total game time, average game time, and levels to the profile page - Kelly

#3.1.155.341
+ fix pile-view window for small play window https://github.com/kellyelton/OCTGN/issues/1320 - Soul1355
+ Fixes card sleeve error spam https://github.com/kellyelton/OCTGN/issues/1339 - Kelly
+ Fix crash from user error when starting directly to table https://github.com/kellyelton/OCTGN/issues/1299 - Kelly
+ Fixed menu save deck button not working https://github.com/kellyelton/OCTGN/issues/1341 - Kelly
+ Added game count, total game time, average game time, and levels to the profile page - Kelly

#3.1.154.341 - Test
+ fix pile-view window for small play window https://github.com/kellyelton/OCTGN/issues/1320 - Soul1355
+ Fixes card sleeve error spam https://github.com/kellyelton/OCTGN/issues/1339 - Kelly
+ Fix crash from user error when starting directly to table https://github.com/kellyelton/OCTGN/issues/1299 - Kelly
+ Fixed menu save deck button not working https://github.com/kellyelton/OCTGN/issues/1341 - Kelly
+ Added game count, total game time, average game time, and levels to the profile page - Kelly

#3.1.154.340
+ make game doc. window non-modal https://github.com/kellyelton/OCTGN/issues/1319 - Soul1355
+ add drag&drop deck sorting https://github.com/kellyelton/OCTGN/issues/654 - Soul1355
+ add option to cancel additional instances https://github.com/kellyelton/OCTGN/issues/1309 - Soul1355
+ Added push notifications for hosted games - Kelly E
+ Have partial system for language localization in place - Kelly E

#3.1.153.340 - Test
+ make game doc. window non-modal https://github.com/kellyelton/OCTGN/issues/1319 - Soul1355
+ add drag&drop deck sorting https://github.com/kellyelton/OCTGN/issues/654 - Soul1355
+ add option to cancel additional instances https://github.com/kellyelton/OCTGN/issues/1309 - Soul1355
+ Added push notifications for hosted games - Kelly E
+ Have partial system for language localization in place - Kelly E

#3.1.153.339 - Test
+ make game doc. window non-modal https://github.com/kellyelton/OCTGN/issues/1319 - Soul1355
+ add drag&drop deck sorting https://github.com/kellyelton/OCTGN/issues/654 - Soul1355
+ add option to cancel additional instances https://github.com/kellyelton/OCTGN/issues/1309 - Soul1355

#3.1.153.338
+ Fix spectate/play slider to be correct - Kelly
+ Auto refresh game list - Kelly

#3.1.152.338 - Test
+ Fix spectate/play slider to be correct - Kelly
+ Auto refresh game list - Kelly

#3.1.152.337

#3.1.151.337 - Test

#3.1.151.336
+ python cannot see alternates while the card is not visible to you - brine
+ Submit games icon url when hosting games - Kelly

#3.1.150.336 - Test
+ python cannot see alternates while the card is not visible to you - brine
+ Submit games icon url when hosting games - Kelly

#3.1.150.335
+ Removed right bar in place of a popup window - Kelly
+ Add menu item to show link to open source and source code - Kelly

#3.1.149.335 - Test
+ Removed right bar in place of a popup window - Kelly
+ Add menu item to show link to open source and source code - Kelly

#3.1.149.334

#3.1.148.334 - Test

#3.1.148.333
+ Changed how DC% is calculated - Kelly

#3.1.147.333 - Test
+ Changed how DC% is calculated - Kelly

#3.1.147.332

#3.1.146.332 - Test

#3.1.146.331

#3.1.145.331 - Test

#3.1.145.330

#3.1.144.330 - Test

#3.1.144.329
+ Added dynamic menu items to games - Zack G

#3.1.143.329 - Test
+ Added dynamic menu items to games - Zack G

#3.1.143.328
+ Added disconnect % tracking - Kelly
+ Fixed a crash on update https://github.com/kellyelton/OCTGN/issues/1299 - Brine

#3.1.142.328 - Test
+ Added disconnect % tracking - Kelly
+ Fixed a crash on update https://github.com/kellyelton/OCTGN/issues/1299 - Brine

#3.1.142.327
+ Added disconnect % tracking - Kelly
+ Fixed a crash on update https://github.com/kellyelton/OCTGN/issues/1299 - Brine

#3.1.141.327 - Test
+ Added disconnect % tracking - Kelly
+ Fixed a crash on update https://github.com/kellyelton/OCTGN/issues/1299 - Brine

#3.1.141.326
+ Fixed log sharing - Kelly
+ Increased deck sharing count to 30 from 15 - Kelly
+ Fixed some startup problems where no(or bad) network connection would cause OCTGN to hang - Kelly
+ Fixed some excessive logging issues - Kelly
+ Added webPost(url, data, timeout=0) - Kelly
+ Added export to text file function https://github.com/kellyelton/OCTGN/issues/1241 - Kelly
+ Added separators between card and group actions respectively - zachgomez
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly
+ Fixed some bugs - Kelly

#3.1.140.326 - Test
+ Fixed log sharing - Kelly
+ Increased deck sharing count to 30 from 15 - Kelly
+ Fixed some startup problems where no(or bad) network connection would cause OCTGN to hang - Kelly
+ Fixed some excessive logging issues - Kelly
+ Added webPost(url, data, timeout=0) - Kelly
+ Added export to text file function https://github.com/kellyelton/OCTGN/issues/1241 - Kelly
+ Added separators between card and group actions respectively - zachgomez
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly
+ Fixed some bugs - Kelly

#3.1.140.325
+ Fixed log sharing - Kelly
+ Increased deck sharing count to 30 from 15 - Kelly
+ Fixed some startup problems where no(or bad) network connection would cause OCTGN to hang - Kelly
+ Fixed some excessive logging issues - Kelly
+ Added webPost(url, data, timeout=0) - Kelly
+ Added export to text file function https://github.com/kellyelton/OCTGN/issues/1241 - Kelly
+ Added separators between card and group actions respectively - zachgomez
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly

#3.1.139.325 - Test
+ Fixed log sharing - Kelly
+ Increased deck sharing count to 30 from 15 - Kelly
+ Fixed some startup problems where no(or bad) network connection would cause OCTGN to hang - Kelly
+ Fixed some excessive logging issues - Kelly
+ Added webPost(url, data, timeout=0) - Kelly
+ Added export to text file function https://github.com/kellyelton/OCTGN/issues/1241 - Kelly
+ Added separators between card and group actions respectively - zachgomez
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly

#3.1.139.324
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly

#3.1.138.324 - Test
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly
+ Made api 3.1.0.1 live - Kelly

#3.1.138.323 - Test
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly

#3.1.138.322 - Test
+ Added sleeve support - Kelly
+ Always save password - Kelly
+ Fixed a Kellyism - Brine
+ Made community chat honor bans - Gemini
+ Few matchmaking fixes - Kelly

#3.1.138.321 - Test

#3.1.138.320 - Test

#3.1.138.319
+ Finished matchmaking - Kelly

#3.1.137.319 - Test
+ Finished matchmaking - Kelly

#3.1.137.318 - Test

#3.1.137.317 - Test

#3.1.137.316

#3.1.136.316 - Test

#3.1.136.315

#3.1.135.315 - Test

#3.1.135.314
+ Added user ignoring - Gemini
+ Fixed #1249 - spectators and reconnects firing OnTableLoad and OnGameStart - Kelly
+ Added events OnPlayerConnect and OnPlayerRageQuit - Kelly
+ Hopefully Fixed - stack/52fe718f78415508c8f5adbb - Kelly
+ Fixed #632 - Improper input breaks dialog. - Kelly
+ Fixed #1257 - Table no longer requires a board to be set - Kelly.
+ Fixed #1253 - OnGlobalVariableChanged and OnPlayerGlobalVariableChanged both correctly reflect the old value now. - Kelly

#3.1.134.314 - Test
+ Added user ignoring - Gemini
+ Fixed #1249 - spectators and reconnects firing OnTableLoad and OnGameStart - Kelly
+ Added events OnPlayerConnect and OnPlayerRageQuit - Kelly
+ Hopefully Fixed - stack/52fe718f78415508c8f5adbb - Kelly
+ Fixed #632 - Improper input breaks dialog. - Kelly
+ Fixed #1257 - Table no longer requires a board to be set - Kelly.
+ Fixed #1253 - OnGlobalVariableChanged and OnPlayerGlobalVariableChanged both correctly reflect the old value now. - Kelly

#3.1.134.313
+ Added user ignoring - Gemini
+ Fixed #1249 - spectators and reconnects firing OnTableLoad and OnGameStart - Kelly
+ Added events OnPlayerConnect and OnPlayerRageQuit - Kelly
+ Hopefully Fixed - stack/52fe718f78415508c8f5adbb - Kelly
+ Fixed #632 - Improper input breaks dialog. - Kelly
+ Fixed #1257 - Table no longer requires a board to be set - Kelly.
+ Fixed #1253 - OnGlobalVariableChanged and OnPlayerGlobalVariableChanged both correctly reflect the old value now. - Kelly

#3.1.133.313 - Test
+ Added user ignoring - Gemini
+ Fixed #1249 - spectators and reconnects firing OnTableLoad and OnGameStart - Kelly
+ Added events OnPlayerConnect and OnPlayerRageQuit - Kelly
+ Hopefully Fixed - stack/52fe718f78415508c8f5adbb - Kelly
+ Fixed #632 - Improper input breaks dialog. - Kelly
+ Fixed #1257 - Table no longer requires a board to be set - Kelly.
+ Fixed #1253 - OnGlobalVariableChanged and OnPlayerGlobalVariableChanged both correctly reflect the old value now. - Kelly

#3.1.133.312 - Test

#3.1.133.311 - Test

#3.1.133.310 - Test

#3.1.133.309 - Test

#3.1.133.308

#3.1.132.308 - Test

#3.1.132.307
+ Added warning for users with Desktop Experience shut off - Kelly
+ Disabled matchmaking tab for now - Kelly

#3.1.131.307 - Test
+ Added warning for users with Desktop Experience shut off - Kelly
+ Disabled matchmaking tab for now - Kelly

#3.1.131.306
+ Allow username/passwords in feeds - Kelly

#3.1.130.306 - Test
+ Allow username/passwords in feeds - Kelly

#3.1.130.305 - Test
+ Allow username/passwords in feeds - Kelly

#3.1.130.304 - Test
+ Allow username/passwords in feeds - Kelly

#3.1.130.303 - Test
+ Allow username/passwords in feeds - Kelly

#3.1.130.302
+ Fixed some errors spectators were causing - Kelly
+ Replaced feed list with a drop down - Kelly
+ Allow for screen sizes down to 800x600 - Kelly
+ Added box to ask if using wine to fix mac/linux issues - Kelly
+ Added check to see if running on network drive with potential fixes - Kelly
+ Added links to octgn status page and twitter account - Kelly
+ Added some more descriptive errors in o8build - Kelly

#3.1.129.302 - Test
+ Fixed some errors spectators were causing - Kelly
+ Replaced feed list with a drop down - Kelly
+ Allow for screen sizes down to 800x600 - Kelly
+ Added box to ask if using wine to fix mac/linux issues - Kelly
+ Added check to see if running on network drive with potential fixes - Kelly
+ Added links to octgn status page and twitter account - Kelly
+ Added some more descriptive errors in o8build - Kelly

#3.1.129.301 - Test
+ Fixed some errors spectators were causing - Kelly
+ Replaced feed list with a drop down - Kelly
+ Allow for screen sizes down to 800x600 - Kelly
+ Added box to ask if using wine to fix mac/linux issues - Kelly
+ Added check to see if running on network drive with potential fixes - Kelly
+ Added links to octgn status page and twitter account - Kelly
+ Added some more descriptive errors in o8build - Kelly

#3.1.129.300 - Test
+ Fixed some errors spectators were causing - Kelly
+ Replaced feed list with a drop down - Kelly
+ Allow for screen sizes down to 800x600 - Kelly
+ Added box to ask if using wine to fix mac/linux issues - Kelly
+ Added check to see if running on network drive with potential fixes - Kelly
+ Added links to octgn status page and twitter account - Kelly

#3.1.129.299 - Test
+ Fixed some errors spectators were causing - Kelly

#3.1.129.298
+ Made hand density user configurable - Soul1355

#3.1.128.298 - Test
+ Made hand density user configurable - Soul1355

#3.1.128.297 - Test
+ Made hand density user configurable - Soul1355

#3.1.128.296
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly
+ Fixed card names not showing up when cards are moved between visible and non-visible groups. - Jason

#3.1.127.296 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly
+ Fixed card names not showing up when cards are moved between visible and non-visible groups. - Jason

#3.1.127.295
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.295 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.294 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.293 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.292 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.291 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.290 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason
+ Add spectator support - Kelly
+ Removed the 'Zoom' text in game - Kelly
+ Allow host to mute spectators - Kelly
+ Allow host to kick players/spectators - Kelly

#3.1.126.289 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp
+ Added resetGame() to the Python API - Jason
+ Changed Pile.lookAtTop() to Pile.lookAt(<count>, <isTop> = True) - see Wiki - Jason

#3.1.126.288 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason
+ Fixed a small #NULL# check error thanks to Jason for pointing it out - Gravecorp

#3.1.126.287 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason

#3.1.126.286 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason

#3.1.126.285 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason

#3.1.126.284 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ Added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex. - Gemini
+ Fixed: Show the erroneous file in TestSetXml - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Only actual card moves trigger card pulse animation - Jason
+ Only actual card moves trigger OnMoveCard and OnScriptedMoveCard (3.1.0.1) - Jason
+ Added Group.viewers (returns a list of players) - Jason
+ Added Group.addViewer(player) and Group.removeViewer(player) - Jason

#3.1.126.283 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex.

#3.1.126.282 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex.

#3.1.126.281 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex.

#3.1.126.280 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason
+ Fixed URL parsing regex.

#3.1.126.279 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason
+ Fixed #1210 - Jason

#3.1.126.278 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason
+ Added base of Script Engine 3.1.0.1 - Jason
+ Added Pile.collapsed (readable, settable) to Python API (3.1.0.1) - Jason
+ Added Pile.lookAtTop(<count>, default 0 or All) to Python API (3.1.0.1) - Jason

#3.1.126.277 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1103 - Jason

#3.1.126.276 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason
+ added Group.visibility (readonly) and Group.setVisibility(<string>) to the Python API - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1192 - Kelly

#3.1.126.275 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ OnLoadDeck now reveals to the event the actual cards in the groups if it can - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason

#3.1.126.274 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ OnLoadDeck now reveals to the event the actual cards in the groups if it can - Kelly
+ Manually refresh to refresh game list - Kelly
+ Fixed more crashes - Kelly
+ Added OnMarkerChanged events - Jason
+ Added highlight and markers paramenter to OnMoveCard event - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/1199 - Jason
+ Fixed https://github.com/kellyelton/OCTGN/issues/914 - Jason 
+ Fixed https://github.com/kellyelton/OCTGN/issues/1083 - Jason
+ Added ability to minimize or disable "Card Pulse" - Jason
+ Reorganized Options window - Jason

#3.1.126.273 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ OnLoadDeck now reveals to the event the actual cards in the groups if it can - Kelly
+ Manually refresh to refresh game list - Kelly

#3.1.126.272 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ OnLoadDeck now reveals to the event the actual cards in the groups if it can - Kelly
+ Manually refresh to refresh game list - Kelly

#3.1.126.271 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly
+ OnLoadDeck now reveals to the event the actual cards in the groups if it can - Kelly
+ Manually refresh to refresh game list - Kelly

#3.1.126.270 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1194 Python wd() changed - Kelly
+ Fixed more crashes - Kelly
+ Added the ability to disable/enable game scripts - Kelly

#3.1.126.269

#3.1.125.269 - Test

#3.1.125.268 - Test

#3.1.125.267
+ Fixed wrong disconnected message - Kelly
+ Fixed more crashes - Kelly
+ Rehauled ingame messages and chat - Kelly
+ Merged pre game lobby into game window - Kelly
+ No more pay for sounds - Kelly
+ Fixed up some chat issues - Kelly
+ Added new python call notifyBar(color, message) - Kelly

#3.1.124.267 - Test
+ Fixed wrong disconnected message - Kelly
+ Fixed more crashes - Kelly
+ Rehauled ingame messages and chat - Kelly
+ Merged pre game lobby into game window - Kelly
+ No more pay for sounds - Kelly
+ Fixed up some chat issues - Kelly
+ Added new python call notifyBar(color, message) - Kelly

#3.1.124.266
+ Fixed up some chat issues - Kelly
+ Added new python call notifyBar(color, message) - Kelly

#3.1.123.266 - Test
+ Fixed up some chat issues - Kelly
+ Added new python call notifyBar(color, message) - Kelly

#3.1.123.265
+ Fixed wrong disconnected message - Kelly
+ Fixed more crashes - Kelly
+ Rehauled ingame messages and chat - Kelly
+ Merged pre game lobby into game window - Kelly
+ No more pay for sounds - Kelly

#3.1.122.265 - Test
+ Fixed wrong disconnected message - Kelly
+ Fixed more crashes - Kelly
+ Rehauled ingame messages and chat - Kelly
+ Merged pre game lobby into game window - Kelly
+ No more pay for sounds - Kelly

#3.1.122.264 - Test
+ Fixed wrong disconnected message - Kelly
+ Fixed more crashes - Kelly
+ Rehauled ingame messages and chat - Kelly
+ Merged pre game lobby into game window - Kelly

#3.1.122.263
+ fix inconsistent behavior of moveto https://github.com/kellyelton/OCTGN/issues/1184 - Soul1355
+ OCTGN doesn't crash anymore if it loads up a bad script, it instead shows you the error - Kelly
+ Messages from the pre game lobby get put into the game chat window when it starts - Kelly
+ Fixed some crashes - Kelly
+ Fixed some crashes - Soul1355
+ Fixed data directory only able to be located on c:\ - Gemini

#3.1.121.263 - Test
+ fix inconsistent behavior of moveto https://github.com/kellyelton/OCTGN/issues/1184 - Soul1355
+ OCTGN doesn't crash anymore if it loads up a bad script, it instead shows you the error - Kelly
+ Messages from the pre game lobby get put into the game chat window when it starts - Kelly
+ Fixed some crashes - Kelly
+ Fixed some crashes - Soul1355
+ Fixed data directory only able to be located on c:\ - Gemini

#3.1.121.262
+ Add local handling for shuffle - Soul1355
+ Re-implimented player tab scrolling - Soul1355
+ Make player sides consistent; fix https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Fixed bugs in Counter changes https://github.com/kellyelton/OCTGN/issues/1153 - DarkSir23
+ Fixed bugs in Marker removal https://github.com/kellyelton/OCTGN/issues/1162 - DarkSir23
+ Added the ability to add extra packs and lands in Limited (Sealed) - DarkSir23
+ Improved some reconnect behavior - DarkSir23
+ Fixed a bug causing large images width image importing in the deck editor - Gemini
+ Got some bugs fixed causing ghost players - Soul1355
+ Reversed a checkbox - DarkSir23
+ Fixed some spam errors - Kelly

#3.1.120.262 - Test
+ Add local handling for shuffle - Soul1355
+ Re-implimented player tab scrolling - Soul1355
+ Make player sides consistent; fix https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Fixed bugs in Counter changes https://github.com/kellyelton/OCTGN/issues/1153 - DarkSir23
+ Fixed bugs in Marker removal https://github.com/kellyelton/OCTGN/issues/1162 - DarkSir23
+ Added the ability to add extra packs and lands in Limited (Sealed) - DarkSir23
+ Improved some reconnect behavior - DarkSir23
+ Fixed a bug causing large images width image importing in the deck editor - Gemini
+ Got some bugs fixed causing ghost players - Soul1355
+ Reversed a checkbox - DarkSir23
+ Fixed some spam errors - Kelly

#3.1.120.261 - Test
+ Add local handling for shuffle - Soul1355

#3.1.120.260

#3.1.119.260 - Test

#3.1.119.259
+ Fixed lots of crashes - Kelly
+ Fixed lots of crashes - Soul
+ Fixed lots of crashes - Gemini

#3.1.118.259 - Test
+ Fixed lots of crashes - Kelly
+ Fixed lots of crashes - Soul
+ Fixed lots of crashes - Gemini

#3.1.118.258

#3.1.117.258 - Test

#3.1.117.257
+ Fix issue with crashes on profile page - Kelly

#3.1.116.257 - Test
+ Fix issue with crashes on profile page - Kelly

#3.1.116.256
+ Cleaned up global piles tab - Soul1355
+ Allow scrolling player tabs with mouse wheel - Soul1355
+ Fixed OCTGN not closing properly https://github.com/kellyelton/OCTGN/issues/1154 - Kelly
+ Better progress bars on game update on start - Kelly
+ Make game updates unable to timeout on start(led to corrupt game installs) - Kelly
+ Added a subscription price to sub messages - Kelly
+ If you disconnect and reconnect to chat it won't pop open another subscription message - Kelly

#3.1.115.256 - Test
+ Cleaned up global piles tab - Soul1355
+ Allow scrolling player tabs with mouse wheel - Soul1355
+ Fixed OCTGN not closing properly https://github.com/kellyelton/OCTGN/issues/1154 - Kelly
+ Better progress bars on game update on start - Kelly
+ Make game updates unable to timeout on start(led to corrupt game installs) - Kelly
+ Added a subscription price to sub messages - Kelly
+ If you disconnect and reconnect to chat it won't pop open another subscription message - Kelly

#3.1.115.255
+ Added the ability to delete shared decks - Kelly
+ Added the ability to view shared decks on a users profile - Kelly
+ Show game author in the correct place - Gemini
+ Fix problem where people with proxies couldn't connect https://github.com/kellyelton/OCTGN/issues/415 - Kelly
+ Make sure sealed only produces 1 copy of card per pick https://github.com/kellyelton/OCTGN/issues/1018 - Gemini
+ Make remoteCall() honor script mute settings - DarkSir23
+ Fixed lag from changing counters/markers quickly https://github.com/kellyelton/OCTGN/issues/114 - Kelly
+ Add local handling for Notify fix https://github.com/kellyelton/OCTGN/issues/1112 - Soul1355
+ Added ability to right click user and invite to game https://github.com/kellyelton/OCTGN/issues/72 - Kelly
+ Should resolve https://github.com/kellyelton/OCTGN/issues/1024 https://github.com/kellyelton/OCTGN/issues/994 https://github.com/kellyelton/OCTGN/issues/1015 - Kelly

#3.1.114.255 - Test
+ Added the ability to delete shared decks - Kelly
+ Added the ability to view shared decks on a users profile - Kelly
+ Show game author in the correct place - Gemini
+ Fix problem where people with proxies couldn't connect https://github.com/kellyelton/OCTGN/issues/415 - Kelly
+ Make sure sealed only produces 1 copy of card per pick https://github.com/kellyelton/OCTGN/issues/1018 - Gemini
+ Make remoteCall() honor script mute settings - DarkSir23
+ Fixed lag from changing counters/markers quickly https://github.com/kellyelton/OCTGN/issues/114 - Kelly
+ Add local handling for Notify fix https://github.com/kellyelton/OCTGN/issues/1112 - Soul1355
+ Added ability to right click user and invite to game https://github.com/kellyelton/OCTGN/issues/72 - Kelly
+ Should resolve https://github.com/kellyelton/OCTGN/issues/1024 https://github.com/kellyelton/OCTGN/issues/994 https://github.com/kellyelton/OCTGN/issues/1015 - Kelly

#3.1.114.254 - Test
+ Should resolve https://github.com/kellyelton/OCTGN/issues/1024 https://github.com/kellyelton/OCTGN/issues/994 https://github.com/kellyelton/OCTGN/issues/1015 - Kelly

#3.1.114.253
+ Added the ability to delete shared decks - Kelly
+ Added the ability to view shared decks on a users profile - Kelly
+ Show game author in the correct place - Gemini
+ Fix problem where people with proxies couldn't connect https://github.com/kellyelton/OCTGN/issues/415 - Kelly
+ Make sure sealed only produces 1 copy of card per pick https://github.com/kellyelton/OCTGN/issues/1018 - Gemini
+ Make remoteCall() honor script mute settings - DarkSir23
+ Fixed lag from changing counters/markers quickly https://github.com/kellyelton/OCTGN/issues/114 - Kelly 
+ Add local handling for Notify fix https://github.com/kellyelton/OCTGN/issues/1112 - Soul1355
+ Added ability to right click user and invite to game https://github.com/kellyelton/OCTGN/issues/72 - Kelly

#3.1.113.253 - Test
+ Added the ability to delete shared decks - Kelly
+ Added the ability to view shared decks on a users profile - Kelly
+ Show game author in the correct place - Gemini
+ Fix problem where people with proxies couldn't connect https://github.com/kellyelton/OCTGN/issues/415 - Kelly
+ Make sure sealed only produces 1 copy of card per pick https://github.com/kellyelton/OCTGN/issues/1018 - Gemini
+ Make remoteCall() honor script mute settings - DarkSir23
+ Fixed lag from changing counters/markers quickly https://github.com/kellyelton/OCTGN/issues/114 - Kelly 
+ Add local handling for Notify fix https://github.com/kellyelton/OCTGN/issues/1112 - Soul1355
+ Added ability to right click user and invite to game https://github.com/kellyelton/OCTGN/issues/72 - Kelly

#3.1.113.252
+ Fixed problem adding/removing feeds https://github.com/kellyelton/OCTGN/issues/1130 - Kelly
+ Removed the need to pick a game when laoding a deck in the deck editor https://github.com/kellyelton/OCTGN/issues/1119 - Kelly
+ Fixed card.delete issue https://github.com/kellyelton/OCTGN/issues/1134 - Kelly
+ Removed seconds from hosted games https://github.com/kellyelton/OCTGN/issues/1132 - Kelly
+ Fixed some bugs when loading search filters - Kelly
+ Fixed problem with alternates being wrong https://github.com/kellyelton/OCTGN/issues/817 - Kelly
+ Fixed sets being wrong when loading search filters https://github.com/kellyelton/OCTGN/issues/1095 - Kelly
+ Fixed all loaded filters in deck editor being open when loading deck filter - Kelly
+ Fixed subscriber's custom table image not loading. - Gemini

#3.1.112.252 - Test
+ Fixed problem adding/removing feeds https://github.com/kellyelton/OCTGN/issues/1130 - Kelly
+ Removed the need to pick a game when laoding a deck in the deck editor https://github.com/kellyelton/OCTGN/issues/1119 - Kelly
+ Fixed card.delete issue https://github.com/kellyelton/OCTGN/issues/1134 - Kelly
+ Removed seconds from hosted games https://github.com/kellyelton/OCTGN/issues/1132 - Kelly
+ Fixed some bugs when loading search filters - Kelly
+ Fixed problem with alternates being wrong https://github.com/kellyelton/OCTGN/issues/817 - Kelly
+ Fixed sets being wrong when loading search filters https://github.com/kellyelton/OCTGN/issues/1095 - Kelly
+ Fixed all loaded filters in deck editor being open when loading deck filter - Kelly
+ Fixed subscriber's custom table image not loading. - Gemini

#3.1.112.251
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355
+ Add scrollbar to PlayerControl https://github.com/kellyelton/OCTGN/issues/1085 - Soul1355
+ Significant performance increase(huge lag reduction) - Kelly
+ Potentially fixed https://github.com/kellyelton/OCTGN/issues/1123 - Kelly
+ Fixed player sides flipping back and forth - Kelly
+ Reopened https://github.com/kellyelton/OCTGN/issues/1049 - Kelly

#3.1.111.251 - Test
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355
+ Add scrollbar to PlayerControl https://github.com/kellyelton/OCTGN/issues/1085 - Soul1355
+ Significant performance increase(huge lag reduction) - Kelly
+ Potentially fixed https://github.com/kellyelton/OCTGN/issues/1123 - Kelly
+ Fixed player sides flipping back and forth - Kelly
+ Reopened https://github.com/kellyelton/OCTGN/issues/1049 - Kelly

#3.1.111.250
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355
+ Add scrollbar to PlayerControl https://github.com/kellyelton/OCTGN/issues/1085 - Soul1355
+ Significant performance increase(huge lag reduction) - Kelly
+ Potentially fixed https://github.com/kellyelton/OCTGN/issues/1123 - Kelly

#3.1.110.250 - Test
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355
+ Add scrollbar to PlayerControl https://github.com/kellyelton/OCTGN/issues/1085 - Soul1355
+ Significant performance increase(huge lag reduction) - Kelly
+ Potentially fixed https://github.com/kellyelton/OCTGN/issues/1123 - Kelly

#3.1.110.249 - Test
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355
+ Add scrollbar to PlayerControl https://github.com/kellyelton/OCTGN/issues/1085 - Soul1355

#3.1.110.248 - Test
+ Fix player side inconsistency https://github.com/kellyelton/OCTGN/issues/1049 - Soul1355
+ Improve player color readability https://github.com/kellyelton/OCTGN/issues/1101 - Soul1355

#3.1.110.247
+ Fix hidden pile issue https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Created getPlayers() api call to replace the players variable - Brine
+ Added visibility of running games - Kelly
+ Updated game broadcasting - Kelly
+ Removed the LobbyServer and replace it with a GameService that's more loosly coupled with the stand alone servers- Kelly
+ Updated lots of libraries - Kelly
+ Made GameService more easily replacable than the old LobbyServer - Kelly
+ Tossed out a whole bunch of junk - Kelly
+ Fixed some issues with the SaS's not closing properly - Kelly
+ Fixed the config file issues breaking OCTGN - Kelly
+ Added some slidy buttons - Kelly

#3.1.109.247 - Test
+ Fix hidden pile issue https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Created getPlayers() api call to replace the players variable - Brine
+ Added visibility of running games - Kelly
+ Updated game broadcasting - Kelly
+ Removed the LobbyServer and replace it with a GameService that's more loosly coupled with the stand alone servers- Kelly
+ Updated lots of libraries - Kelly
+ Made GameService more easily replacable than the old LobbyServer - Kelly
+ Tossed out a whole bunch of junk - Kelly
+ Fixed some issues with the SaS's not closing properly - Kelly
+ Fixed the config file issues breaking OCTGN - Kelly
+ Added some slidy buttons - Kelly

#3.1.109.246 - Test
+ Fix hidden pile issue https://github.com/kellyelton/OCTGN/issues/1110 - Soul1355
+ Created getPlayers() api call to replace the players variable - Brine
+ Added visibility of running games - Kelly
+ Updated game broadcasting - Kelly
+ Removed the LobbyServer and replace it with a GameService that's more loosly coupled with the stand alone servers- Kelly
+ Updated lots of libraries - Kelly
+ Made GameService more easily replacable than the old LobbyServer - Kelly
+ Tossed out a whole bunch of junk - Kelly
+ Fixed some issues with the SaS's not closing properly - Kelly
+ Fixed the config file issues breaking OCTGN - Kelly

#3.1.109.245 - Test

#3.1.109.244
+ Added opt in game fonts https://github.com/kellyelton/OCTGN/issues/970 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/945 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/893 - Kelly
+ Fixed card visibility issue https://github.com/kellyelton/OCTGN/issues/1110 - Kelly
+ Fixed reconnect right click issues https://github.com/kellyelton/OCTGN/issues/1093 - Kelly
+ Increased reconnect timeout - Kelly
+ Increased reconnect window to 2 minutes - Kelly
+ Fixed issues with multiple of the same user showing up in pregame lobby - Kelly

#3.1.108.244 - Test
+ Added opt in game fonts https://github.com/kellyelton/OCTGN/issues/970 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/945 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/893 - Kelly
+ Fixed card visibility issue https://github.com/kellyelton/OCTGN/issues/1110 - Kelly
+ Fixed reconnect right click issues https://github.com/kellyelton/OCTGN/issues/1093 - Kelly
+ Increased reconnect timeout - Kelly
+ Increased reconnect window to 2 minutes - Kelly
+ Fixed issues with multiple of the same user showing up in pregame lobby - Kelly

#3.1.108.243 - Test
+ Added opt in game fonts https://github.com/kellyelton/OCTGN/issues/970 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/945 - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/893 - Kelly

#3.1.108.242
+ Fix for https://github.com/kellyelton/OCTGN/issues/1116 - Kelly
+ Fix some users can't host games - Kelly
+ Fixed frequest settings file error message box - Kelly

#3.1.107.242 - Test
+ Fix for https://github.com/kellyelton/OCTGN/issues/1116 - Kelly
+ Fix some users can't host games - Kelly
+ Fixed frequest settings file error message box - Kelly

#3.1.107.241
+ Fix for https://github.com/kellyelton/OCTGN/issues/1116 - Kelly
+ Fix some users can't host games - Kelly
+ Fixed frequest settings file error message box - Kelly

#3.1.106.241 - Test
+ Fix for https://github.com/kellyelton/OCTGN/issues/1116 - Kelly
+ Fix some users can't host games - Kelly
+ Fixed frequest settings file error message box - Kelly

#3.1.106.240

#3.1.105.240 - Test

#3.1.105.239
+ Added back local games, but you must enable 'Advanced Options' in the options window - Kelly
+ Replaced 'Start Time' with 'Run Time' in the Play tab - Kelly
+ Fixed a bug causing crashes on log in - Kelly
+ Refresh the game list faster - Kelly
+ Modified some logging - Kelly
+ Fixed file downloader crash - Kelly
+ Fixed window crash - Kelly
+ Fixed game def crash - Kelly
+ Better handling of a crash when starting a game - Kelly
+ Better handling of a game loading crash - Kelly
+ Fixed target arrow - Soul1355

#3.1.104.239 - Test
+ Added back local games, but you must enable 'Advanced Options' in the options window - Kelly
+ Replaced 'Start Time' with 'Run Time' in the Play tab - Kelly
+ Fixed a bug causing crashes on log in - Kelly
+ Refresh the game list faster - Kelly
+ Modified some logging - Kelly
+ Fixed file downloader crash - Kelly
+ Fixed window crash - Kelly
+ Fixed game def crash - Kelly
+ Better handling of a crash when starting a game - Kelly
+ Better handling of a game loading crash - Kelly
+ Fixed target arrow - Soul1355

#3.1.104.238
+ Fixed https://github.com/kellyelton/OCTGN/issues/1014 - Soul1355
+ Fixed additional ghost card issues - Soul1355
+ Added LAN and local games to 'Play' tab - Kelly
+ Fixed deck sharing crashing on some systems - Kelly
+ Fixed issue joining online games - Kelly

#3.1.103.238 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1014 - Soul1355
+ Fixed additional ghost card issues - Soul1355
+ Added LAN and local games to 'Play' tab - Kelly
+ Fixed deck sharing crashing on some systems - Kelly
+ Fixed issue joining online games - Kelly

#3.1.103.237
+ Fixed https://github.com/kellyelton/OCTGN/issues/1014 - Soul1355
+ Fixed additional ghost card issues - Soul1355
+ Added LAN and local games to 'Play' tab - Kelly
+ Fixed deck sharing crashing on some systems - Kelly

#3.1.102.237 - Test
+ Fixed https://github.com/kellyelton/OCTGN/issues/1014 - Soul1355
+ Fixed additional ghost card issues - Soul1355
+ Added LAN and local games to 'Play' tab - Kelly
+ Fixed deck sharing crashing on some systems - Kelly

#3.1.102.236
+ Fixed some issues with password protected games - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1104 - Kelly
+ Fixed can't play offline games https://github.com/kellyelton/OCTGN/issues/1096 - Kelly
+ Allow joining offline games to select username in join dialog, instead of receiving another popup window asking for username - Kelly

#3.1.101.236 - Test
+ Fixed some issues with password protected games - Gemini
+ Fixed https://github.com/kellyelton/OCTGN/issues/1104 - Kelly
+ Fixed can't play offline games https://github.com/kellyelton/OCTGN/issues/1096 - Kelly
+ Allow joining offline games to select username in join dialog, instead of receiving another popup window asking for username - Kelly

#3.1.101.235 - Test

#3.1.101.234 - Test

#3.1.101.233

#3.1.100.233 - Test

#3.1.100.232
+ Fixed some reconnect issues - Kelly

#3.1.99.232 - Test
+ Fixed some reconnect issues - Kelly

#3.1.99.231 - Test

#3.1.99.230
+ fix Ghost cards and align doubleclick time for cards with host system - Soul1355
+ tweak filter behavior in deck editor - Soul1355
+ Fixed shuffle logic error - soul1355
+ Added o8d file extension handler - Kelly
+ Added Deck Sharing - Kelly

#3.1.98.230 - Test
+ fix Ghost cards and align doubleclick time for cards with host system - Soul1355
+ tweak filter behavior in deck editor - Soul1355
+ Fixed shuffle logic error - soul1355
+ Added o8d file extension handler - Kelly
+ Added Deck Sharing - Kelly

#3.1.98.229 - Test
+ fix Ghost cards and align doubleclick time for cards with host system - Soul1355
+ tweak filter behavior in deck editor - Soul1355
+ Fixed shuffle logic error - soul1355
+ Added o8d file extension handler - Kelly
+ Added Deck Sharing - Kelly

#3.1.98.228

#3.1.97.228 - Test

#3.1.97.227

#3.1.96.227

#3.1.95.227
+ Made filter popup easier to dismiss and auto-focus text field - Soul1355
+ clear peeking players for all cards in a shuffle; fixes #1064 - soul1355

#3.1.94.227 - Test
+ Made filter popup easier to dismiss and auto-focus text field - Soul1355
+ clear peeking players for all cards in a shuffle; fixes #1064 - soul1355

#3.1.94.226
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown
+ Fixed https://github.com/kellyelton/OCTGN/issues/1065 Deck editor crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1060 Shift in game crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1054 Cards visible after shuffle on window close - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1067 Anti social connection - Kelly
+ Updated deck editor filter style a bit - Soul1355

#3.1.93.226 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown
+ Fixed https://github.com/kellyelton/OCTGN/issues/1065 Deck editor crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1060 Shift in game crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1054 Cards visible after shuffle on window close - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1067 Anti social connection - Kelly
+ Updated deck editor filter style a bit - Soul1355

#3.1.93.225
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown
+ Fixed https://github.com/kellyelton/OCTGN/issues/1065 Deck editor crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1060 Shift in game crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1054 Cards visible after shuffle on window close - Kelly

#3.1.92.225 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown
+ Fixed https://github.com/kellyelton/OCTGN/issues/1065 Deck editor crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1060 Shift in game crash - Kelly
+ Fixed https://github.com/kellyelton/OCTGN/issues/1054 Cards visible after shuffle on window close - Kelly

#3.1.92.224
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown

#3.1.91.224 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown

#3.1.91.223 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown

#3.1.91.222 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown

#3.1.91.221 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly
+ Fixed remoteCall crash when passing player type - Unknown
+ Implemented group controller functions https://github.com/kellyelton/OCTGN/issues/1041 - Unknown

#3.1.91.220 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown
+ Fixed blurry news https://github.com/kellyelton/OCTGN/issues/1046 - Kelly
+ Increased the host game timeout - Kelly
+ Made search filters not so bulky https://github.com/kellyelton/OCTGN/issues/969 - Kelly

#3.1.91.219 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly
+ Added reconnects - Kelly
+ Added most of the code for spectators - Kelly
+ Fixed up card.delete api - Unknown
+ Mark deck changes from plugins as unsaved - Unknown
+ Added search count in the deck editor - Unknown
+ Fixed some proxy generator bugs #928 #1057 - Unknown

#3.1.91.218 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly

#3.1.91.217 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly

#3.1.91.216 - Test
+ Fixed users disconnecting breaking the game - Kelly
+ Significantly sped up shuffling - Kelly

#3.1.91.215

#3.1.90.215 - Test

#3.1.90.214
+ Fixed card action bug that would make game unrecoverable - Kelly
+ Fixed diagnostics window bug - Kelly

#3.1.89.214 - Test
+ Fixed card action bug that would make game unrecoverable - Kelly
+ Fixed diagnostics window bug - Kelly

#3.1.89.213

#3.1.88.213 - Test

#3.1.88.212

#3.1.87.212 - Test

#3.1.87.211
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly
+ Added remote calling for game devs - Kelly
+ Can see the name of games that you don't have - Kelly

#3.1.86.211 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly
+ Added remote calling for game devs - Kelly
+ Can see the name of games that you don't have - Kelly

#3.1.86.210 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly
+ Added remote calling for game devs - Kelly
+ Can see the name of games that you don't have - Kelly

#3.1.86.209 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly
+ Added remote calling for game devs - Kelly

#3.1.86.208 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly

#3.1.86.207 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly

#3.1.86.206 - Test
+ Super fast deck loads - Kelly
+ Fixed looking at library slowness and shuffling slowness https://github.com/kellyelton/OCTGN/issues/820 https://github.com/kellyelton/OCTGN/issues/843- Kelly

#3.1.86.205
+ Enable mouse wheel scroll in deck editor - Soul1355
+ Fix some errors in the deck editor - Soul1355
+ Fixed bad files breaking install o8c - Kelly

#3.1.85.205 - Test
+ Enable mouse wheel scroll in deck editor - Soul1355
+ Fix some errors in the deck editor - Soul1355
+ Fixed bad files breaking install o8c - Kelly

#3.1.85.204
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly
+ Updated some UI to be more consistent, colorful, and touch friendly - Kelly
+ Added diagnostics panel - Kelly

#3.1.84.204 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly
+ Updated some UI to be more consistent, colorful, and touch friendly - Kelly
+ Added diagnostics panel - Kelly

#3.1.84.203
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly
+ Updated some UI to be more consistent, colorful, and touch friendly - Kelly
+ Added diagnostics panel - Kelly

#3.1.83.203 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly
+ Updated some UI to be more consistent, colorful, and touch friendly - Kelly
+ Added diagnostics panel - Kelly

#3.1.83.202
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly
+ Updated some UI to be more consistent, colorful, and touch friendly - Kelly

#3.1.82.202 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.201 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.200 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.199 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.198 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.197 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly
+ Fixed global ownership https://github.com/kellyelton/OCTGN/issues/959 - Kelly
+ Allow notes in game to change font size by ctrl+scroll wheel up and down - Kelly

#3.1.82.196 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ [removed]Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly
+ Better more flexible shared deck control in deck editor and game in general https://github.com/kellyelton/OCTGN/issues/326 https://github.com/kellyelton/OCTGN/issues/755 https://github.com/kellyelton/OCTGN/issues/1005- Kelly

#3.1.82.195 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly
+ Added the ability to set a deck as 'Shared' in the deck editor https://github.com/kellyelton/OCTGN/issues/326 - Kelly
+ Added the ability to add notes to a deck https://github.com/kellyelton/OCTGN/issues/426 - Kelly
+ On loading a deck in game, if the deck has a note, it will show it in game - Kelly

#3.1.82.194 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly
+ Fixed a bug when pressing 2, and other auto complete annoyances - Kelly
+ Improved auto search https://github.com/kellyelton/OCTGN/issues/1008 - Soul1355
+ Added the ability to move remaining cards in the sealed editor to a specific deck https://github.com/kellyelton/OCTGN/issues/27- Kelly

#3.1.82.193 - Test
+ Implemented table notes https://github.com/kellyelton/OCTGN/issues/25 - Kelly

#3.1.82.192
+ Temp fix for shuffle bug - Kelly

#3.1.81.192 - Test
+ Temp fix for shuffle bug - Kelly

#3.1.81.191
+ Fixed problem with two sided table not propagating properly - Kelly

#3.1.80.191 - Test
+ Fixed problem with two sided table not propagating properly - Kelly

#3.1.80.190
+ Fixed missing two sided table option - Kelly
+ Fix playing mp3 sounds freezing client - Kelly

#3.1.79.190 - Test
+ Fixed missing two sided table option - Kelly
+ Fix playing mp3 sounds freezing client - Kelly

#3.1.79.189
+ Fixed ? cards when a player leaves - Kelly
+ Don't have to pick two sided table anymore - Kelly
+ Make installer install as a user instead of admin - Kelly
+ Disabled silent installer for now - Kelly
+ Fixed UAC Disabled or run as administrator auto close - Kelly

#3.1.78.189 - Test
+ Fixed ? cards when a player leaves - Kelly
+ Don't have to pick two sided table anymore - Kelly
+ Make installer install as a user instead of admin - Kelly
+ Disabled silent installer for now - Kelly
+ Fixed UAC Disabled or run as administrator auto close - Kelly

#3.1.78.188
+ Fixed ? cards when a player leaves - Kelly
+ Don't have to pick two sided table anymore - Kelly
+ Make installer install as a user instead of admin - Kelly
+ Disabled silent installer for now - Kelly

#3.1.77.188 - Test
+ Fixed ? cards when a player leaves - Kelly
+ Don't have to pick two sided table anymore - Kelly
+ Make installer install as a user instead of admin - Kelly
+ Disabled silent installer for now - Kelly

#3.1.77.187
+ Removed mp3 support until bug is fixed, or error can be caught - Kelly

#3.1.76.187 - Test
+ Removed mp3 support until bug is fixed, or error can be caught - Kelly

#3.1.76.186

#3.1.75.186 - Test

#3.1.75.185
+ Added some event logging - Kelly
+ Added option to disable game sounds - Kelly

#3.1.74.185 - Test
+ Added some event logging - Kelly
+ Added option to disable game sounds - Kelly

#3.1.74.184
+ Add spell checking to the chat - Kelly
+ Add auto complete to the chat - Kelly
+ Made autoscroll much better - Kelly

#3.1.73.184 - Test
+ Add spell checking to the chat - Kelly
+ Add auto complete to the chat - Kelly
+ Made autoscroll much better - Kelly

#3.1.73.183

#3.1.72.183 - Test

#3.1.72.182
+ Fixed some sub users getting sub messages - Kelly

#3.1.71.182 - Test
+ Fixed some sub users getting sub messages - Kelly

#3.1.71.181
+ Added the ability to hide errors in game - Kelly

#3.1.70.181 - Test
+ Added the ability to hide errors in game - Kelly

#3.1.70.180
+ Made -t command load faster - Kelly
+ Fixed watch list not populating because of date formatting issue - Gemini
+ Fixed attaching multiple game events to one event handler https://github.com/kellyelton/OCTGN/issues/981 - Kelly
+ Fix OnChangeCounter old value bug https://github.com/kellyelton/OCTGN/issues/983 - Kelly
+ MoveTo now tracks if it was induced by script or not https://github.com/kellyelton/OCTGN/issues/984 - Kelly

#3.1.69.180 - Test
+ Made -t command load faster - Kelly
+ Fixed watch list not populating because of date formatting issue - Gemini
+ Fixed attaching multiple game events to one event handler https://github.com/kellyelton/OCTGN/issues/981 - Kelly
+ Fix OnChangeCounter old value bug https://github.com/kellyelton/OCTGN/issues/983 - Kelly
+ MoveTo now tracks if it was induced by script or not https://github.com/kellyelton/OCTGN/issues/984 - Kelly

#3.1.69.179
+ Allow game board image to be changed by calling table.setBoardImage(path) https://github.com/kellyelton/OCTGN/issues/540 - Kelly
+ Added OnPlayerGlobalVariableChanged and OnGlobalVariableChanged events - Kelly

#3.1.68.179 - Test
+ Allow game board image to be changed by calling table.setBoardImage(path) https://github.com/kellyelton/OCTGN/issues/540 - Kelly
+ Added OnPlayerGlobalVariableChanged and OnGlobalVariableChanged events - Kelly

#3.1.68.178

#3.1.67.178 - Test

#3.1.67.177
+ Add BGG GenCon Live Coverage Stream - Kelly

#3.1.66.177 - Test
+ Add BGG GenCon Live Coverage Stream - Kelly

#3.1.66.176
+ Fixed subscription issue - Kelly

#3.1.65.176 - Test
+ Fixed subscription issue - Kelly

#3.1.65.175

#3.1.64.175 - Test

#3.1.64.174
+ Updated some release automation tasks - Kelly
+ Made game sounds async - Kelly
+ Add mp3 support for sound playback - Kelly
+ New windows created load in the same monitor as OCTGN #977 - Kelly

#3.1.63.174 - Test
+ Updated some release automation tasks - Kelly
+ Made game sounds async - Kelly
+ Add mp3 support for sound playback - Kelly
+ New windows created load in the same monitor as OCTGN #977 - Kelly

#3.1.63.173 - Test
+ Updated some release automation tasks - Kelly
+ Made game sounds async - Kelly
+ Add mp3 support for sound playback - Kelly
+ New windows created load in the same monitor as OCTGN #977 - Kelly

#3.1.58.168 - Test
+ Add Instant Search to Deck Editor, Addresses https://github.com/kellyelton/OCTGN/issues/458 - Soul1355

#3.1.56.165 - Test
+ changes fallback behavior of cards in piles, addresses https://github.com/kellyelton/OCTGN/issues/937 - Soul1355
+ Add the ability to save/load searches in the deck editor https://github.com/kellyelton/OCTGN/issues/961 - Kelly

#3.1.56.164
+ 10% Feature Funding goals complete

#3.1.56.164 - Test
+ Fixed issues with some games starting due to waiting for global - Kelly
+ Fixed chat undocking not sizing - Kelly
+ Fixing missing chat messages - Kelly

#3.1.55.163 - Test
+ Fixed a UI issue

#3.1.55.162 - Test
+ Add custom chat font sizes - Kelly

#3.1.55.160 - Test
+ When starting a game, wait for all players to be ready before allowing any actions - Kelly
+ Add option to use window or tab for chats. You can also dock/undock a chat from OCTGN - Kelly

#3.1.55.158 - Test
+ Messagebox added to be shown on SSL validation errors giving the option to disable SSL cert validation from there. - Gravecorp
+ Fixed limited game card duplication bug. https://github.com/kellyelton/OCTGN/issues/864 - Gravecorp
+ Hard coded some paths for images etc to make things faster - Kelly
+ Fixed a login bug using custom data directory - Kelly
+ Made settings store actual types instead of just strings - Kelly
+ Added game settings for game developers https://github.com/kellyelton/OCTGN/issues/647 - Kelly
+ Fixed some things being transparent that shouldn't be(like menu's etc) - Kelly
+ Removed all menu shortcuts from game window - Kelly
+ Added chat auto reconnect - Kelly
+ Modified some feature funding icons - Kelly
+ Added some getting started helpful automations + Spoils Plug - Kelly

#3.1.55.157 - Test
+ Added option in the menu to ignore SSL certificate validity. Should fix the rare cases where SSL traffic certs are not properly validated. - Gravecorp
+ Changes made to the limited play dialog. It will no longer show sets in the list that have no boosters in them. - Gravecorp
+ Finally fixed the booster box not selecting the first in the list on changing the set. - Gravecorp
+ Limited play no longer shows in the menu for games which have no boosters defined in any of the sets. - Gravecorp
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

























































































































































































































































