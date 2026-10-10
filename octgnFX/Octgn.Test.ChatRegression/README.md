# Chat handler regression checks

Build on Windows with `msbuild octgnFX/Octgn.Test.ChatRegression/Octgn.Test.ChatRegression.csproj /p:Configuration=Release /m:1 /nr:false`, then run `packages/NUnit.ConsoleRunner.3.20.1/tools/nunit3-console.exe octgnFX/Octgn.Test.ChatRegression/bin/Release/Octgn.Test.ChatRegression.dll`. The project targets .NET Framework 4.8, uses the repository's existing NUnit 3.13.3 assembly, and needs no package restore. `NUnitFrameworkPath` can point to an existing assembly in another checkout.

It compiles the production handler, broadcaster, binary parser, context/state, configuration, player settings and API message model directly. Test doubles replace socket players, timers, localization, logging and unrelated hosting/API dependencies. This verifies chat opcode dispatch, exact sender/payload (Unicode, empty and direct-call null), mute behavior for local/hosted games, connected/registered recipient filtering, and exception propagation. Unexpected API calls throw.

`HandlerSource` can select an earlier version of Handler.cs to run the same checks against a baseline. These checks cover chat control flow; they do not replace the legacy .NET Framework solution build, transport serialization tests, or real socket integration.
