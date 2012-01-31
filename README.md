OCTGN 3.0
=================================================
This is the new home for the OCTGN open source project.  The project's license is in LICENSE.md


Get Involved! 
-------------------------------------------------
*REPORT BUGS:* https://github.com/kellyelton/OCTGN/issues
*VOTE ON FEATURES:* https://trello.com/board/octgn-v3-0-0-x/4efffbacce6a25c1792b8ee2
*HELP US CODE:*

* Create a github account (https://github.com/signup/free)
* Setup git locally (http://help.github.com/win-set-up-git/)
* Fork the repository (http://help.github.com/fork-a-repo/)
* Make your awesome change! (see PLAN.md to see future plans)
* Request we pull it in (http://help.github.com/send-pull-requests/)


Your own awesome changes
-------------------------------------------------
* Get Visual Studio Express C# (http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express)
* Open the main project file under **OCTGN/octgnFX/Octgn/OCTGN.sln** with Visual Studio Express C#
* Do the greatness!


Other places to get involved
-------------------------------------------------
* Forums
* Mailing List
* Trello


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
