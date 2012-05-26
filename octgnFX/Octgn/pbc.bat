@ECHO off
CLS
ECHO ========================================================================
ECHO Have you updated the dbversion in Octgn.Data.UpdateDatabaseQueries.xml ?
SET /P M=y/n.
ECHO Have you updated the SQL code in Octgn.Data.UpdateDatabaseQueries.xml ?
SET /P M=y/n.
ECHO Have you updated the dbversion in MakeDatabase.sql ?
SET /P M=y/n.
ECHO Have yo updated the SQL code in MakeDatabase.sql?
SET /P M=y/n.
ECHO.
ECHO.
ECHO Have you incremented the version in Octgn?
SET /P M=y/n.
echo Have you incremented the version in Octgn.Data?
SET /P M=y/n.
echo Have you incremented the version in Octgn.Server?
SET /P M=y/n.
echo Have you incremented the version in Octgn.StandAloneServer?
SET /P M=y/n.
echo Have you incremented the version in Skylabs.Lobby?
SET /P M=y/n.
echo Have you incremented the version in Skylabs.LobbyServer?
SET /P M=y/n.
echo Have you incremented the version in Install.nsi?
SET /P M=y/n.
echo Have you incremented the version in currentversion.xml?
SET /P M=y/n.
echo Have you incremented the version in the branch gh-pages index.html?
SET /P M=y/n.
echo Are all of the above assembly's version numbers the same?
SET /P M=y/n.
echo.
echo.
echo Have you marked any changes made in CHANGELOG.md?
SET /P M=y/n.
echo.
echo.
echo Have you run Installer.nsi?
SET /P M=y/n.
echo Have you uploaded the install file to the GitHub Downloads page?
SET /P M=y/n.
echo.
echo.
echo Have you updated the index.html to point to the new setup file?
SET /P M=y/n.
echo.
echo.
echo.
echo.
echo If you have done all of this, then you can be confident you haven't fucked up.
echo ========================================================================
SET /P M=y/n.
SET /P M=y/n.
exit