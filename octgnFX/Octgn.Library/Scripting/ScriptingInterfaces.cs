using System;

namespace Octgn.Library.Scripting
{
    /// <summary>
    /// Interface for the scripting engine to support dependency injection
    /// </summary>
    public interface IScriptingEngine : IDisposable
    {
        void ExecuteFunctionSecureNoFormat(string function, string args);
    }

    /// <summary>
    /// Interface for validating function availability
    /// </summary>
    public interface IFunctionValidator
    {
        bool IsFunctionAvailable(string functionName);
    }

    /// <summary>
    /// Interface for executing code
    /// </summary>
    public interface ICodeExecutor
    {
        void ExecuteFunction(string functionName, string arguments);
    }

    /// <summary>
    /// Default implementation of IFunctionValidator that checks actual scope
    /// </summary>
    public class DefaultFunctionValidator : IFunctionValidator
    {
        private readonly Func<string, bool> _checkFunction;

        public DefaultFunctionValidator(Func<string, bool> checkFunction)
        {
            _checkFunction = checkFunction ?? throw new ArgumentNullException(nameof(checkFunction));
        }

        public bool IsFunctionAvailable(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
                return false;

            return _checkFunction(functionName);
        }
    }

    /// <summary>
    /// Default implementation of ICodeExecutor that executes actual code
    /// </summary>
    public class DefaultCodeExecutor : ICodeExecutor
    {
        private readonly Action<string, string> _executeFunction;

        public DefaultCodeExecutor(Action<string, string> executeFunction)
        {
            _executeFunction = executeFunction ?? throw new ArgumentNullException(nameof(executeFunction));
        }

        public void ExecuteFunction(string functionName, string arguments)
        {
            _executeFunction(functionName, arguments);
        }
    }

    /// <summary>
    /// Mock implementation of IFunctionValidator for testing
    /// </summary>
    public class MockFunctionValidator : IFunctionValidator
    {
        private readonly Func<string, bool> _isAvailableFunc;

        public MockFunctionValidator(Func<string, bool> isAvailableFunc = null)
        {
            _isAvailableFunc = isAvailableFunc ?? (_ => true); // Default to allowing all functions
        }

        public bool IsFunctionAvailable(string functionName)
        {
            return _isAvailableFunc(functionName);
        }
    }

    /// <summary>
    /// Mock implementation of ICodeExecutor for testing
    /// </summary>
    public class MockCodeExecutor : ICodeExecutor
    {
        private readonly Action<string, string> _executeAction;

        public MockCodeExecutor(Action<string, string> executeAction = null)
        {
            _executeAction = executeAction ?? ((f, a) => { }); // Default to no-op
        }

        public void ExecuteFunction(string functionName, string arguments)
        {
            _executeAction(functionName, arguments);
        }
    }
}
