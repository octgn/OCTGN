using System;

namespace Octgn.Scripting
{
    /// <summary>
    /// Interface for the scripting engine to enable testing
    /// </summary>
    public interface IScriptingEngine : IDisposable
    {
        /// <summary>
        /// Executes a function securely with validation
        /// </summary>
        void ExecuteFunctionSecureNoFormat(string function, string args);

        /// <summary>
        /// Executes a function without security validation
        /// </summary>
        void ExecuteFunctionNoFormat(string function, string args);
    }

    /// <summary>
    /// Interface for function existence validation - allows mocking in tests
    /// </summary>
    public interface IFunctionValidator
    {
        /// <summary>
        /// Checks if a function exists and is callable in the current scope
        /// </summary>
        bool IsFunctionAvailable(string functionName, out object functionObject);
    }

    /// <summary>
    /// Interface for code execution - allows mocking in tests
    /// </summary>
    public interface ICodeExecutor
    {
        /// <summary>
        /// Executes the function with the given arguments
        /// </summary>
        void ExecuteFunction(string function, string args);
    }
}
