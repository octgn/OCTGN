using System;

namespace Octgn.Scripting
{
    /// <summary>
    /// Exception thrown when a script execution is blocked for security reasons
    /// </summary>
    public class ScriptSecurityException : Exception
    {
        public string FunctionName { get; }
        public string Arguments { get; }
        public string GeneratedCode { get; }
        public string SecurityReason { get; }

        public ScriptSecurityException(string functionName, string arguments, string securityReason)
            : base($"Script execution blocked for security reasons: {securityReason}")
        {
            FunctionName = functionName;
            Arguments = arguments;
            SecurityReason = securityReason;
        }

        public ScriptSecurityException(string functionName, string arguments, string generatedCode, string securityReason)
            : base($"Script execution blocked for security reasons: {securityReason}")
        {
            FunctionName = functionName;
            Arguments = arguments;
            GeneratedCode = generatedCode;
            SecurityReason = securityReason;
        }
    }
}
