# API response regression tests

This small NUnit project links the production API client, HTTP extensions,
exception factory, and API models. Only unrelated hosting types are substituted,
so it runs without the legacy WPF test dependency graph or communication submodule.
The client tests use a local loopback HTTP server; they never contact OCTGN services.

From the repository root on Windows with .NET 8 and the .NET Framework 4.8 targeting pack installed:

```powershell
dotnet test octgnFX/Octgn.Online.Api.Test/Octgn.Online.Api.Test.csproj -m:1
```

Coverage includes every existing `ApiClientException` HTTP failure path, the
session-validation 404 behavior, JSON and HTML content, missing response context,
charset decoding, bounded stream reads, deferred read failures, and the synchronous
factory under a synchronization context that cannot process continuations.
The project is included in OCTGN.sln and targets both net8.0 and the shipping
net48 runtime. Solution inclusion builds the tests; the command above executes
both target frameworks. No CI execution is claimed.

`ResponseBody` and `ResponseBodyReadException` contain opt-in server diagnostics and
may include sensitive data. They are excluded from `Message` and `ToString()`.
This applies to diagnostics created by the HTTP-failure factory; the unchanged
ValidateSession HTTP-200/non-ok validation path still includes its server value.
Request URI sanitization removes user information, query strings, and fragments;
path segments, including user/device identifiers, remain in diagnostics.
Seekable content streams retain their original position. Nonseekable streams
are consumed to capture the excerpt. Invalid charset labels fall back to UTF-8.
The excerpt is limited to 4,096 characters; `ResponseBodyTruncated` identifies a
longer body. Existing `HttpClient` requests still use their usual buffering behavior.

To demonstrate the original regression, export the baseline `ApiClient.cs` and
`ApiClientException.cs` into a temporary directory, then pass
`-p:Baseline=true -p:ProductionSourceRoot=<temporary-directory>` to the test command.
This runs the synchronous response-body property regression and the two
baseline-compatible HTTP failure-message cases, plus control cases. Revision
`eb8bb590` lacks ResponseBody and emits literal interpolation placeholders instead
of status502 for GetUsersSharedDecks and WebhookQueuePublish. Expected baseline
assertion failures demonstrate those specific defects; setup/binding failures do
not constitute regression proof.
