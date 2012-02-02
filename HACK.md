TODO: Need help filing out this file

In this document, we will do our level best to get you start coding on the
project.  Obviously, read the README.md -- which has some basic info, the 
basic software needed to compile stuff.  This document will attempt to get 
a little more nitty gritty about each major piece and how they flow.

# Installer
...

# Python
Octgn uses IronPython for its in-game scripting engine.

The python definitions go in PythonApi.py. You can call outside code with the following syntax:  _api.<function_name> and it will call the corresponding function in ScriptApi.cs.
For example, the following code found in PythonApi.py is the random function:
```def rnd(min, max):
  return _api.Random(min, max)```

It calls Random with a min and max value that is location in ScriptApi.cs.

# octgnFX/CassiniDev
...

# octgnFX/ConsoleHelper
...

# octgnFX/Graphics
...

# octgnFX/Lib
...

# octgnFX/Octgn
...

# octgnFX/Octgn.Data
...

# octgnFX/Octgn.LobbyServer
...

# octgnFX/Octgn.Server
...

# octgnFX/Octgn.StandAloneServer
...

# octgnFX/Skylabs.Lobby
...

# octgnFX/Skylabs.MultiLogin
...

# octgnFX/Webcontent
...

