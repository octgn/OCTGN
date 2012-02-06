Get Involved!
-------------------------------------------------
* Read the https://github.com/kellyelton/OCTGN/blob/master/README.md -- IRC, Mailing List, Issues, etc.
* *HELP US CODE:*
  * Create a github account (https://github.com/signup/free)
  * Setup git locally (http://help.github.com/win-set-up-git/)
  * Fork the repository (http://help.github.com/fork-a-repo/)
  * Make your awesome change! (by following this file)
  * Request we pull it in (http://help.github.com/send-pull-requests/)

Your own awesome changes
-------------------------------------------------
* Get Visual Studio Express C# (http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express)
* Open the main project file under **OCTGN/octgnFX/Octgn/OCTGN.sln** with Visual Studio Express C#
* Do the greatness!

To work on the web component
-------------------------------------------------
**Note:** This project is not included in the main project, because having it included makes it unable to be opened from 
express editions, and we want to keep the development process open to all.  It requires the main project to be built so 
it can include the required assemblies.

* Get Visual Web Developer Express (http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-web-developer-express)
* Open the subproject file under **Webcontent/Webcontent.csproj** with the web developer version of Visual Studio Express
* **Important** This project references two components from the main project, you will have to add them as references
  * OCTGN\octgnFX\Skylabs.Lobby\bin\Debug\Skylabs.Lobby.dll 
  * OCTGN\octgnFX\Octgn\bin\Debug\Skylabs.LobbyServer.exe
* Do the greatness! 

Components
-------------------------------------------------
**TODO:** Installer, octgnFX/CassiniDev, octgnFX/ConsoleHelper, octgnFX/Graphics, octgnFX/Lib, octgnFX/Octgn.Data, 
octgnFX/Octgn.LobbyServer, octgnFX/Octgn.Server,  octgnFX/Octgn.StandAloneServer, octgnFX/Skylabs.Lobby, octgnFX/Skylabs.MultiLogin, 
octgnFX/Webcontent

# Python
Octgn uses IronPython for its in-game scripting engine.

The python definitions go in [PythonApi.py](https://github.com/kellyelton/OCTGN/blob/master/octgnFX/Octgn/Scripting/PythonAPI.py). You can call outside code with the following syntax:  _api.function_name and it will call the corresponding function in [ScriptApi.cs](https://github.com/kellyelton/OCTGN/blob/master/octgnFX/Octgn/Scripting/ScriptAPI.cs).
For example, the following code found in [PythonApi.py](https://github.com/kellyelton/OCTGN/blob/master/octgnFX/Octgn/Scripting/PythonAPI.py) is the random function:
```def rnd(min, max):
  return _api.Random(min, max)```

It calls Random with a min and max value that is located in [ScriptApi.cs](https://github.com/kellyelton/OCTGN/blob/master/octgnFX/Octgn/Scripting/ScriptAPI.cs).

# octgnFX/Octgn
Most of the work in this project is shouldered by Game.cs. Inside Game.cs is a IVistaDBDatabase that has all card data installed in it. Card data is read and dropped into a CardModel format, which is stored in a Dictionary<Guid, CardModel> cardModelCache. CardModels are wrapped in a CardIdentity class (apparently for anti-cheating), which is wrapped in a Card class. Cards are placed into Piles and manipulated according to the gamedef.
