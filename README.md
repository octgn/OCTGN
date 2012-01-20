OCTGN 3.0
=================================================
This is the new home for the OCTGN open source project.  It is released under a **GPL3** license.


Get Involved! 
-------------------------------------------------
* Create a github account (https://github.com/signup/free)
* Setup git locally (http://help.github.com/win-set-up-git/)
* Fork the repository (http://help.github.com/fork-a-repo/)
* Make your awesome change! (more details below)
* Request we pull it in (http://help.github.com/send-pull-requests/)


Your own awesome changes
-------------------------------------------------
* Get Visual Studio Express C# (http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express)
* Open the main project file under OCTGN/octgnFX/Octgn/OCTGN.sln
* Do the greatness!


To work on the web component
-------------------------------------------------
**Note:** This project is not included in the main project, because having it included makes it unable to be opened from 
express editions, and we want to keep the development process open to all.
* Get Visual Web Developer Express (http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-web-developer-express)
* Open the subproject file under Webcontent/Webcontent.csproj
* **Important** This project references two components from the main project, if you rebuild the main project, you have to 
  update these, are they are copied into the local project space.
- OCTGN\octgnFX\Skylabs.Lobby\bin\Debug\Skylabs.Lobby.dll
- OCTGN\octgnFX\Octgn\bin\Debug\Skylabs.LobbyServer.exe
* Do the greatness! 
