# API response regression tests

This small NUnit project links the production API client, HTTP extensions,
exception factory, and API models. Only unrelated hosting types are substituted,
so it runs without the legacy WPF test dependency graph or communication submodule.
The client tests use a local loopback HTTP server; they never contact OCTGN services.

From the repository root with .NET 8 or newer installed:

```powershell
dotnet test octgnFX/Octgn.Online.Api.Test/Octgn.Online.Api.Test.csproj -m:1
```

Coverage includes every existing `ApiClientException` HTTP failure path, the
session-validation 404 behavior, JSON and HTML content, missing response context,
charset decoding, bounded stream reads, deferred read failures, and the synchronous
factory under a synchronization context that cannot process continuations.

`ResponseBody` and `ResponseBodyReadException` contain opt-in server diagnostics and
may include sensitive data. They are excluded from `Message` and `ToString()`.
The excerpt is limited to 4,096 characters; `ResponseBodyTruncated` identifies a
longer body. Existing `HttpClient` requests still use their usual buffering behavior.

To demonstrate the original regression, export the baseline `ApiClient.cs` and
`ApiClientException.cs` into a temporary directory, then pass
`-p:Baseline=true -p:ProductionSourceRoot=<temporary-directory>` to the test command.
This runs the same synchronous response-body regression against those source files
and is expected to fail for revision `eb8bb590` because `ResponseBody` is missing.
