CassiniDev
Cassini for Developers and Testers:  http://cassinidev.codeplex.com
------------------------------------------------------------------------------------------

The goal of the CassiniDev project is to provide an open platform for developing a robust 
ASP.Net web server implementation that addresses many of the limitations and difficulties 
encountered when using Cassini and/or Visual Studio Development Server.

CassiniDev is suitable for use as a standalone WinForms GUI application, a console 
application, self hosting and in automated testing scenarios including continuous 
integration and as a 100% compatible drop-in replacement for the Visual Studio development 
server.


Highlights
------------------------------------------------------------------------------------------
* Full support for any IP address. Not limited to localhost.
** NOTE: Due to an intentional limitation in SimpleWorkerRequest, WCF content is not 
   servable on other than the loopback (localhost)
* HostName support with option to temporarily add hosts file entry.
* Port scan option. Dynamically assign an available port when specific port is not required 
  or known to be available.
* WaitOnPort: Length of time, in ms, to wait for specific port to become available.
* TimeOut: Length of time, in ms, to sit idle before stopping server.
* NTLM authentication support.
* Single file GUI and Console applications and a library assembly for in-process hosting.
* Painless self hosting of a full ASP.Net server implementation for applications and testing
  frameworks.
* Unlike Cassini and Visual Studio Development Server, CassiniDev supports a full compliment 
  of content types.
* Integrated request/response log and viewer to support debugging.
* Visual Studio 2008/2010 Development server drop-in replacement with all CassiniDev 
  enhancements.
* [insert your improvements here]



Please see http://cassinidev.codeplex.com/documentation for the latest documentation

------------------------------------------------------------------------------------------
System Requirements:
------------------------------------------------------------------------------------------
Users:
* .Net Framework 3.5sp1 or 4.0

Developers:
* .Net Framework 3.5sp1 or 4.0
* Visual Studio 2008/2010
 
------------------------------------------------------------------------------------------
New in CassiniDev v3.5.1.4/v4.0.1.4 beta 3
------------------------------------------------------------------------------------------
* CassiniDev is now 100% compatible with WebDeb.WebServer.exe and can be
  used as a drop-in debugging replacement for the Visual Studio development servers.
* Integrated traffic logging with viewer. If System.Data.SQLite is present, logs can be 
  persisted. Otherwise events are only captured when the log window is active.

 
System.Data.SQLite is required to enable persistent logging. You may obtain the 
latest version here:
http://sourceforge.net/projects/sqlite-dotnet2/files/SQLite%20for%20ADO.NET%202.0/

 
New in CassiniDev v3.5.1.0/v4.0.1.0 beta
------------------------------------------------------------------------------------------
* Added .Net 4 / VS10 build. 
* Simplified test fixtures. 
* Un-Refactored the not-so-simple MVP pattern to reduce code bloat and 
  complexity. 
* Added content-type support for a wide variety of files previously not 
  supported by Cassini. Thanks Zippy. 
 
New in CassiniDev v3.5.0.5
------------------------------------------------------------------------------------------
* Reintroduced the Lib project and signed all 
* Implemented the CassiniSqlFixture*  works great, as far as i can tell, just 
  whipped it up to support a test case and I like it, build a disposable sql 
  database, spin up a web server and shut it all down disposed. What's not to 
  like? 
* Fixed bug in Fixture: IPMode, PortMode, Timeout and WaitForPort were not being 
  set properly. 
* Reintroduced library project, set build events to build a binary release 
  directory for use as external for Salient.WebTest 
* Removed some faulty debug code and cleaned up a stupid last minute mistake 
  r.e. path 
* Fixed typo in readme. Quoted paths are allowed on command line, just be sure 
  to omit trailing slash or it will be interpreted as an escape. 
* Refactored to a simple MVP pattern with a simple Service Locator/Abstract 
  Factory class to facilitate testing. 

New in CassiniDev v3.5.0.4
------------------------------------------------------------------------------------------
* Added Cassini hosting Fixture and supporting classes to facilitate use of 
* CassiniDev in testing scenarios 
    While CassiniDev and CassiniDev-console may be referenced as a library it is 
    not recommended for testing scenarios.
    A fixture class has been provided (CassiniDev.Testing.Fixture) that reliably 
    hosts the console application in a separate process. 
* Include test projects demonstrating some possible scenarios for use of 
  CassiniDev in integration/interaction/smoke testing of web based resources. 

New in CassiniDev v3.5.0.3
------------------------------------------------------------------------------------------
* Improved command line parsing. 
* Console version added for use in headless processes 
    The console application can be run in a non-interactive session and requires 
    that all supplied arguments are valid for the process to start. 
    The GUI application will reject invalid arguments with a dialog notification 
    and present the UI for modification of arguments. 
* Both versions are standalone and require no GAC assembly. 
* Implemented arbitrary IP use including both IPv4 and IPv6 Any and Loopback. 
* Added port scanning to allow dynamic allocation of first available port in 
  specified range. 
* Added hosts file utility. CassiniDev can dynamically add a temporary hosts 
  file entry to allow dns resolution of application specific domains. 
* Implemented support for relative paths. 

Branched from Cassini v3.5.0.2
* -----------------------------------------------------------------------------------------
New in Cassini v3.5.0.2
------------------------------------------------------------------------------------------
* Fix for the default documents. 

New in Cassini v3.5.0.1
------------------------------------------------------------------------------------------
* Support for MVC friendly URLs (directory listing only overrides 404 responses 
  for directories) 

New in Cassini v3.5
------------------------------------------------------------------------------------------
* Runs as a single EXE* - does not require an assembly in GAC 
* Supported IPv6-only configurations 
* Upgraded to support .NET Framework 3.5 
* Includes VS project file 
* License changed to Ms-PL 

To Do:
------------------------------------------------------------------------------------------ 
* Test IPv6 functionality thoroughly. 
* Application virtual path and single app hosting limitations:
    I would like to see a scenario in which a virtual web directory can be 
    described as the hosting environment allowing pointers to shared resources 
    and perhaps multiple web applications. 
* Disabling directory browsing appears to be broken but I think it relates to the MVC
  friendly URL fix by Dmitry.