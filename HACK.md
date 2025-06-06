Contribute!
-------------------------------------------------
* To get started, Join us on <a href="https://discord.gg/Yn3Jrpj">Discord</a>
* Sign the Contributor License Agreement https://www.clahub.com/agreements/octgn/OCTGN
* *HELP US CODE:*
  * Create a github account (https://github.com/signup/free)
  * Setup git locally (https://help.github.com/win-set-up-git/)
  * Fork the repository (https://help.github.com/fork-a-repo/)
  * Install the latest version of Visual Studio https://www.visualstudio.com/vs/
  * Make your awesome change!
  * Request we pull it in (https://help.github.com/send-pull-requests/)

Dev Environment Details
------------------------------------------------
* The main OCTGN repo contains a subrepo/submodule OCTGN/Octgn.Communication. Keep this in mind when cloning to your local machine
* Install Wix toolset (https://wixtoolset.org/) to remove wix errors or disable relevant projects
* Install Newtonsoft.json NuGet package. It is a reference for OCTGN.Core
  
Components
-------------------------------------------------
**TODO:** Installer, octgnFX/Graphics, octgnFX/Lib, octgnFX/Octgn.Data, octgnFX/Octgn.LobbyServer, 
octgnFX/Octgn.Server,  octgnFX/Octgn.StandAloneServer, octgnFX/Skylabs.Lobby

# Python
Octgn uses IronPython for its in-game scripting engine.

The python definitions go in [PythonApi.py](https://github.com/octgn/OCTGN/blob/master/octgnFX/Octgn/Scripting/PythonAPI.py). You can call outside code with the following syntax:  _api.function_name and it will call the corresponding function in [ScriptApi.cs](https://github.com/kellyelton/OCTGN/blob/master/octgnFX/Octgn/Scripting/ScriptAPI.cs).
For example, the following code found in [PythonApi.py](https://github.com/octgn/OCTGN/blob/master/octgnFX/Octgn/Scripting/PythonAPI.py) is the random function:
```
def rnd(min, max):
  return _api.Random(min, max)
```

It calls Random with a min and max value that is located in [ScriptApi.cs](https://github.com/octgn/OCTGN/blob/master/octgnFX/Octgn/Scripting/ScriptAPI.cs).

## Pile Protection API
The pile protection system allows controlling who can view pile contents. Available functions:

### PileGetProtectionState(id)
Gets the current protection state of a pile.
- **Parameters:** `id` (int) - The pile ID
- **Returns:** String - `"false"`, `"true"`, or `"ask"`

### PileSetProtectionState(id, state)
Sets the protection state of a pile.
- **Parameters:** 
  - `id` (int) - The pile ID
  - `state` (string) - `"false"` (allow), `"true"` (block), or `"ask"` (request permission)

**Example usage:**
```python
# Protect player deck from viewing
myt.PileSetProtectionState(me.piles['Deck'].Id, "true")

# Set hand to require permission
myt.PileSetProtectionState(me.piles['Hand'].Id, "ask")

# Check current protection state
state = myt.PileGetProtectionState(me.piles['Hand'].Id)
```

# octgnFX/Octgn.Data
Octgn.Data is the access to the database (mostly). This project provides access to that data in meaningful formats. Most of the items in this project are relatively self-explanatory, but keep in mind that many of them are only the data counterparts of octgnFX/Octgn classes.

Some of the classes do not (as of yet) have direct connections to the database. Deck.cs is a prime example. Currently all decks are stored/accessed as individual files with the user determining name and location.
