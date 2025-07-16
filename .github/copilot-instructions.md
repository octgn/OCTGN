This is a c# repository for the OCTGN gaming platform, specifically the client and related projects.
It is written in .net 4.8 and uses C# as the primary programming language.

## Code Standards

### Required Before Each Commit
- Ensure code compiles without errors
- Run unit tests and ensure they pass

### Development Flow (windows)
- Build: `msbuild /t:Rebuild`s
- Test: `packages\NUnit.ConsoleRunner.3.20.1\tools\nunit3-console.exe --result=Octgn.Online.Test.TestResults.xml octgnFX\Octgn.Online.Test\bin\Debug\Octgn.Online.Test.dll`
- Test: `packages\NUnit.ConsoleRunner.3.20.1\tools\nunit3-console.exe --result=Octgn.Test.TestResults.xml octgnFX\Octgn.Test\bin\Debug\Octgn.Test.dll`

### T4 Template Regeneration (REQUIRED after Protocol.xml changes)
- Regenerate networking files: `.\Run-RegenerateNetworkingFiles.ps1`
- **CRITICAL**: Must be run whenever `octgnFX/Octgn.Server/Protocol.xml` is modified

## Repository Structure
- `octgnFX/`: Main source code directory containing all projects
- `octgnFX/Octgn/`: Frontend client application - main user interface
- `octgnFX/Octgn.JodsEngine/`: Game engine and deck editor launched by main client
- `octgnFX/Octgn.Server/`: Core server logic library for message routing
- `octgnFX/Octgn.Online.*/`: Online service components (GameService, CommunicationService, StandAloneServer)
- `octgnFX/Octgn.Core/`: Higher-level core library with WPF dependencies
- `octgnFX/Octgn.Library/`: Base library with minimal dependencies
- `octgnFX/Octgn.DataNew/`: Game data and definition reading logic
- `octgnFX/o8build/`: Command-line tool for building game packages
- `octgnFX/Octgn.Tools.O8buildgui/`: GUI version of o8build
- `octgnFX/Octgn.ProxyGenerator/`: Card proxy image generation
- `octgnFX/Octgn.Launcher/`: Simple launcher with .NET dependency check
- `packages/`: NuGet packages
- `chocolatey/`: Chocolatey package for developer machine setup
- `nuget/`: NuGet packaging files
- `recentchanges.txt`: Change log entries for current pull request (empty on main branch)
- `Version.cs`: Shared version information across all projects

## Key Guidelines
1. Follow C# best practices and .NET Framework 4.8 patterns
2. Maintain existing code structure and organization
3. Use dependency injection patterns where appropriate
4. Write unit tests for new functionality using NUnit - focus on logical methods only, avoid testing WPF behavior
5. Document public APIs and complex logic
6. Update recentchanges.txt with one line per change for pull requests
7. Ensure all projects use shared Version.cs for consistent versioning

## Networking Architecture
OCTGN uses a client-server networking model where all in-game actions must be synchronized between players. The networking stack is critical for multiplayer functionality.

### Network Flow Pattern
1. User performs action (e.g., moves card) - action executed locally for responsiveness
2. Network message sent to server via IServerCalls
3. Server processes message and broadcasts to all clients via IClientCalls
4. Other clients receive message and execute same action with `notifyServer=false` to prevent network loops

### Protocol Definition
- `octgnFX/Octgn.Server/Protocol.xml`: Defines all network messages and parameters
- `octgnFX/Octgn.Server/protocol.xsd`: Schema validation for Protocol.xml
- **CRITICAL**: Message order in Protocol.xml must NEVER change (messages become numbered by order)
- New messages must be added at the end only
- Most networking files are auto-generated from Protocol.xml via T4 templates
- **NEVER manually edit auto-generated files** - they will be overwritten during regeneration

### Message Attributes
- `name`: Message name
- `server="true"`: Client-to-server message (requires server handler)
- `client="true"`: Server-to-client message (requires client handler)  
- `allowedbyspectator="true"`: Spectators can send this message
- `anonymous="true"`: Unauthenticated users can send (Hello/HelloAgain only)

### Server-Side (octgnFX/Octgn.Server)
- `BinaryParser.cs/.tt`: **AUTO-GENERATED** - parses binary messages and calls Handler.cs
- `Handler.cs`: **MANUAL** - implementation of server message handlers
- `IClientCalls.cs/.tt`: **AUTO-GENERATED** - interface for server-to-client calls
- `BinaryStubs.cs/.tt`: **AUTO-GENERATED** - implementation of IClientCalls
- `Broadcaster.cs/.tt`: **AUTO-GENERATED** - broadcasts messages to all connected clients

### Client-Side (octgnFX/Octgn.JodsEngine/Networking)
- `BinaryParser.cs/.tt`: **AUTO-GENERATED** - parses server messages and calls ClientHandler.cs
- `ClientHandler.cs`: **MANUAL** - implementation of client message handlers
- `IServerCalls.cs/.tt`: **AUTO-GENERATED** - interface for client-to-server calls
- `BinaryStubs.cs/.tt`: **AUTO-GENERATED** - implementation of IServerCalls

### Request/Response Pattern
Some messages use Req/Response pattern (e.g., MoveCardReq → MoveCard):
- Client sends *Req message to server
- Server processes and broadcasts non-Req version to all clients
- Both client and server need handlers for their respective message types

### Implementation Requirements
- All in-game actions MUST send network messages for synchronization
- Use `notifyServer=false` parameter when handling incoming network messages
- Never modify Protocol.xml message order - new messages must be added at the end only
- **ALWAYS run `.\Run-RegenerateNetworkingFiles.ps1` after Protocol.xml changes**
- **NEVER manually edit auto-generated networking files** - they will be overwritten

## Game Development and Scripting
OCTGN uses IronPython for in-game scripting and automation. Game developers create Python scripts that interact with the game engine.

### Scripting Architecture
- `octgnFX/Octgn.JodsEngine/Scripting/`: Python scripting engine implementation
- `PythonAPI.py`: Python definitions that call into ScriptAPI.cs
- `ScriptAPI.cs`: C# implementation of Python API functions
- Scripts call `_api.function_name()` to execute C# code from Python
- Multiple API versions supported for backward compatibility

### Game Data Structure
- Games are distributed as NuGet packages
- Game definitions stored in user's Data Directory
- `Game.xml`: Core game definition (board, phases, players, etc.)
- `Set.xml`: Card set definitions with card data
- `Cards/`: Individual card image files
- `Decks/`: Premade deck files
- `Scripts/`: Python automation scripts

## Project Dependencies
Understanding the dependency hierarchy is crucial for making changes:

### Dependency Levels (Low to High)
1. **Octgn.Library**: Base library, minimal dependencies
2. **Octgn.Core**: Core functionality, references Library + WPF
3. **Octgn.DataNew**: Game data reading, references Library
4. **Octgn.Server**: Server logic, references Library
5. **Octgn.JodsEngine**: Game engine, references Core + DataNew + Server
6. **Octgn**: Main client, references most other projects

### Key Patterns
- **Event-driven architecture**: Extensive use of events for game state changes
- **Command pattern**: Actions implemented as command objects
- **Observer pattern**: UI updates through property change notifications
- **Dependency injection**: Used throughout for loose coupling

## Development Tools and Processes
- **T4 Templates**: Auto-generate networking code from Protocol.xml
- **NUnit**: Unit testing framework - focus on logic, not UI
- **log4net**: Logging framework used throughout
- **WPF**: UI framework for all desktop applications
- **IronPython**: Embedded Python interpreter for game scripting