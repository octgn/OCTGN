using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Octgn.Networking;
using Octgn.Play;
using System.Reflection;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;

using Microsoft.Scripting.Utils;

using Octgn.Core;
using Octgn.Core.DataExtensionMethods;

using log4net;
using Octgn.Library.Scripting;
using Octgn.Library;

namespace Octgn.Scripting
{

    [Export]
    public class Engine : IScriptingEngine, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ScriptScope ActionsScope;
        private ScriptEngine _engine;
        private readonly Queue<ScriptJobBase> _executionQueue = new Queue<ScriptJobBase>(4);
        private readonly MemoryStream _outputStream = new MemoryStream();
        private StreamWriter _outputWriter;
        private readonly bool _isForTesting;
        
        // Dependency injection for testing
        private IFunctionValidator _functionValidator;
        private ICodeExecutor _codeExecutor;
        
        // This is a hack. The sponsor object is used to keep the remote side of the Dialog API alive.
        // I would like to make this cleaner but it really seems to be an impass at the moment.
        // Combining Scripting + Remoting + Lifetime management + Garbage Collection + Partial trust
        // is an aweful and ugly mess.
        //private Sponsor _sponsor;

        public Engine()
            : this(false)
        {
        }

        public Engine(bool forTesting)
            : this(forTesting, null, null)
        {
        }

        public Engine(bool forTesting, IFunctionValidator functionValidator, ICodeExecutor codeExecutor)
        {
            _isForTesting = forTesting;
            _functionValidator = functionValidator;
            _codeExecutor = codeExecutor;
            
            if (!forTesting && Program.GameEngine != null)
            {
                Program.GameEngine.ScriptEngine = this;
                Program.GameEngine.EventProxy = new GameEventProxy(this, Program.GameEngine);
            }
            
            // If not in testing mode and no dependencies injected, set up defaults after scope is ready
            if (!forTesting && _functionValidator == null && _codeExecutor == null)
            {
                // Note: Default implementations will be set up in LoadScript when ActionsScope is available
            }
        }

        public void SetupEngine(bool testing)
        {
            Log.DebugFormat("Creating scripting engine: forTesting={0}", testing);
            //AppDomain sandbox = CreateSandbox(testing);
            _engine = Python.CreateEngine();
            //_engine.SetTrace(OnTraceback);
            _outputWriter = new StreamWriter(_outputStream);
            _engine.Runtime.IO.SetOutput(_outputStream, _outputWriter);
            _engine.SetSearchPaths(new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Scripting\Lib") });

            var workingDirectory = Directory.GetCurrentDirectory();
            Log.DebugFormat("Setting working directory: {0}", workingDirectory);
            if (Program.GameEngine != null)
            {
                workingDirectory = Path.Combine(Config.Instance.Paths.DatabasePath, Program.GameEngine.Definition.Id.ToString());
                var search = _engine.GetSearchPaths();
                search.Add(workingDirectory);
                _engine.SetSearchPaths(search);
            }
            //var workingDirectory = Directory.GetCurrentDirectory();
            if (Program.GameEngine != null)
            {
                workingDirectory = Path.Combine(Config.Instance.Paths.DatabasePath, Program.GameEngine.Definition.Id.ToString());
            }

            ActionsScope = CreateScope(workingDirectory);
            
            // Set up default implementations if in production mode and none were injected
            if (!_isForTesting && _functionValidator == null && _codeExecutor == null)
            {
                _functionValidator = new DefaultFunctionValidator(functionName => 
                    ActionsScope.TryGetVariable(functionName, out var functionObject) && functionObject != null);
                _codeExecutor = new DefaultCodeExecutor(ExecuteFunctionNoFormat);
            }
            
            if (Program.GameEngine == null || testing) return;
            
            // Show sandboxing warning if disabled
            if (!Core.Prefs.EnableGameSandboxing && !testing)
            {
                Log.WarnFormat("Game sandboxing is DISABLED - remote script calls will not be validated");
                Program.GameMess.Warning("⚠️ SECURITY WARNING: Game sandboxing is disabled. Remote script calls from other players will not be validated. Continue at your own risk.");
                Program.GameMess.Warning("You can enable sandboxing in OCTGN Options > Advanced tab for improved security.");
            }
            
            Log.Debug("Loading Scripts...");
            foreach (var script in Program.GameEngine.Definition.GetScripts().ToArray())
            {
                try
                {
                    Log.DebugFormat("Loading Script {0}", script.Path);
                    var str = File.ReadAllText(script.Path);
                    str = ConvertWinforms(str);
                    var src = _engine.CreateScriptSourceFromString(str, script.Path);
                    //var src = _engine.CreateScriptSourceFromFile(script.Path);
                    src.Execute(ActionsScope);
                    Log.DebugFormat("Script Loaded");
                }
                catch (Exception e)
                {
                    var gs = script ?? new Octgn.DataNew.Entities.GameScript()
                    {
                        Path = "Unknown"
                    };
                    var eo = _engine.GetService<ExceptionOperations>();
                    var error = eo.FormatException(e);
                    Program.GameMess.Warning("Could not load script " + gs.Path + Environment.NewLine + error);
                }
            }
            Log.Debug("Scripts Loaded.");
        }

        public TracebackDelegate OnTraceback(TraceBackFrame frame, string result, object payload)
        {

            var code = (FunctionCode)frame.f_code;
            if (result == "call")
            {
                Program.GameMess.GameDebug("[{0}:{1}]{2}", code.co_filename, (int)frame.f_lineno, code.co_name);
            }
            return this.OnTraceback;
        }

        public void ReloadScripts()
        {
            this.SetupEngine(false);
        }

        internal ScriptJobBase CurrentJob
        {
            get { return _executionQueue.Peek(); }
        }

        public String[] TestScripts(GameEngine game)
        {
            var errors = new List<string>();
            foreach (var s in game.Definition.GetScripts())
            {
                try
                {
                    ScriptSource src = _engine.CreateScriptSourceFromString(s.Script, SourceCodeKind.Statements);
                    src.Execute(ActionsScope);
                }
                catch (Exception e)
                {
                    var eo = _engine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(e);
                    errors.Add(String.Format("[{2}:{0}]: Python Error:\n{1}", game.Definition.Name, error, s.Path));
                }
            }
            return errors.ToArray();
        }

        public ScriptScope CreateScope(string workingDirectory)
        {
            ScriptScope scope = _engine.CreateScope();
            InjectOctgnIntoScope(scope, workingDirectory);
            return scope;
        }

        public bool TryExecuteInteractiveCode(string code, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            ScriptSource src = _engine.CreateScriptSourceFromString(code, SourceCodeKind.InteractiveCode);
            switch (src.GetCodeProperties())
            {
                case ScriptCodeParseResult.IncompleteToken:
                    return false;
                case ScriptCodeParseResult.IncompleteStatement:
                    // An empty line ends the statement
                    if (!code.TrimEnd(' ', '\t').EndsWith("\n"))
                        return false;
                    break;
            }
            StartExecution(src, scope, continuation);
            return true;
        }

        public void RegisterFunction(string function, PythonFunction derp)
        {
            //efcache[function] = derp;
        }

        //private readonly Dictionary<string, PythonFunction> efcache = new Dictionary<string, PythonFunction>();
        //private readonly Version ev = new Version("3.1.0.2");
        //private bool didMsg = false;
        public void ExecuteFunction(string function, params object[] args)
        {
            //if (Program.GameEngine.Definition.ScriptVersion < ev)
            //{
            //    if (!didMsg)
            //    {
            //        didMsg = true;
            //        Program.Print(Player.LocalPlayer, "Using old event system");
            //    }
            var sb = new StringBuilder();

            if (args.Length == 1 && args[0] is System.Dynamic.ExpandoObject)
            {
                sb.Append("EventArgument({");
                IDictionary<string, object> propertyValues = (IDictionary<string, object>)args[0];
                var i = 0;
                foreach (var prop in propertyValues)
                {
                    var isLast = i == propertyValues.Count - 1;
                    sb.Append("\"" + prop.Key + "\":");
                    var a = prop.Value;
                    if (a is Array)
                    {
                        var arr = a as Array;
                        sb.Append("[");
                        var argStrings = new List<string>();
                        foreach (var o in arr)
                        {
                            argStrings.Add(FormatObject(o));
                        }
                        sb.Append(string.Join(",", argStrings));
                        sb.Append("]");
                    }
                    else sb.Append(FormatObject(a));
                    if (!isLast) sb.Append(", ");
                    i++;
                }
                sb.Append("})");
            }
            else
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var isLast = i == args.Length - 1;
                    var a = args[i];
                    if (a is Array)
                    {
                        var arr = a as Array;
                        sb.Append("[");
                        var argStrings = new List<string>();
                        foreach (var o in arr)
                        {
                            argStrings.Add(FormatObject(o));
                        }
                        sb.Append(string.Join(",", argStrings));
                        sb.Append("]");
                    }
                    else sb.Append(FormatObject(a));

                    if (!isLast) sb.Append(", ");

                }
            }
            ExecuteFunctionNoFormat(function, sb.ToString());
            //}
            //else
            //{
            //    if (!didMsg)
            //    {
            //        didMsg = true;
            //        Program.Print(Player.LocalPlayer, "Using new event system");
            //    }
            //    if (efcache.ContainsKey(function) == false)
            //    {
            //        const string format = @"_api.RegisterEvent(""{0}"", {0})";
            //        var str = string.Format(format, function);

            //        var src = _engine.CreateScriptSourceFromString(str, SourceCodeKind.Statements);
            //        StartExecution(src, ActionsScope, (x) =>
            //        {
            //            if (efcache.ContainsKey(function))
            //            {
            //                ExecuteFunction(function, args);
            //                return;
            //            }
            //            Log.Error("The function should have been registered... " + function);
            //        });
            //        return;
            //    }

            //    if (_executionQueue.Count == 0)
            //    {
            //        var jerb = new InvokedScriptJob(() => ExecuteFunction(function, args));

            //        //ExecuteFunction(function, args);
            //        StartExecution(jerb);
            //        return;
            //    }

            //    var fun = efcache[function];
            //    //var con = HostingHelpers.GetLanguageContext(_engine);
            //    try
            //    {
            //        // Get the args
            //        var newArgList = new List<object>();
            //        foreach (var arg in args)
            //        {
            //            var na = ConvertArgs(arg);
            //            newArgList.Add(na);
            //        }
            //        _engine.Operations.Invoke(fun, newArgList.ToArray());

            //    }
            //    catch (Exception e)
            //    {
            //        Log.Error(e);
            //    }
            //}
        }

        public void ExecuteFunctionNoFormat(string function, string args)
        {
            const string Template = @"{0}({1})";
            var stringSource = string.Format(Template, function, args);
            var src = _engine.CreateScriptSourceFromString(stringSource, SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        public void ExecuteFunctionSecureNoFormat(string function, string args)
        {
            // Check if sandboxing is enabled - if not, skip all security checks
            if (!Core.Prefs.EnableGameSandboxing)
            {
                Log.DebugFormat("Game sandboxing disabled - executing function '{0}' without security checks", function);
                ExecuteFunctionNoFormat(function, args);
                return;
            }

            Log.DebugFormat("Executing secure remote function call: {0}({1})", function, args);

            // 1. Validate function name - must be a valid Python identifier
            if (!IsValidPythonIdentifier(function))
            {
                Log.WarnFormat("Security violation: Invalid function name '{0}'. Function names must be valid Python identifiers.", function);
                throw new ScriptSecurityException(function, args, "Invalid function name - must be valid Python identifier");
            }

            // 2. Check for dangerous function names
            if (IsDangerousFunction(function))
            {
                Log.WarnFormat("Security violation: Function '{0}' is not allowed for security reasons.", function);
                throw new ScriptSecurityException(function, args, "Dangerous function name not allowed");
            }

            // 3. Check if the function exists in the scope before executing it
            if (_functionValidator != null)
            {
                // Use dependency-injected validator for testing
                if (!_functionValidator.IsFunctionAvailable(function))
                {
                    Log.WarnFormat("Security violation: Function '{0}' does not exist in the current scope.", function);
                    throw new ScriptSecurityException(function, args, "Function does not exist in current scope");
                }
            }
            else
            {
                // Use actual scope checking for production
                if (!ActionsScope.TryGetVariable(function, out var functionObject))
                {
                    Log.WarnFormat("Security violation: Function '{0}' does not exist in the current scope.", function);
                    throw new ScriptSecurityException(function, args, "Function does not exist in current scope");
                }

                // 4. Additional check to ensure it's actually callable
                if (functionObject == null)
                {
                    Log.WarnFormat("Security violation: Function '{0}' exists but is null.", function);
                    throw new ScriptSecurityException(function, args, "Function exists but is null");
                }
            }

            // 5. Parse and validate arguments
            try
            {
                ValidateArguments(args);
                Log.DebugFormat("Arguments validation passed for function '{0}': {1}", function, args);
            }
            catch (ScriptSecurityException)
            {
                // Re-throw security exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Security violation: Failed to validate arguments for function '{0}'. Args: '{1}', Error: {2}", function, args, ex.Message);
                throw new ScriptSecurityException(function, args, $"Failed to validate arguments: {ex.Message}");
            }

            // 6. All security checks passed - execute with original arguments
            Log.DebugFormat("Security checks passed, executing: {0}({1})", function, args);
            
            if (_codeExecutor != null)
            {
                // Use dependency-injected executor for testing
                _codeExecutor.ExecuteFunction(function, args);
            }
            else
            {
                // Use actual execution for production
                ExecuteFunctionNoFormat(function, args);
            }
        }

        public string FormatObject(object o)
        {
            if (o == null)
            {
                return string.Format("None");
            }
            if (o is Array)
            {
                var o2 = o as Array;
                return string.Format("[{0}]", string.Join(",", o2.Select(this.FormatObject)));
            }
            if (o is Player)
            {
                return string.Format("Player({0})", (o as Player).Id);
            }
            if (o is Group)
            {
                var h = o as Group;
                return PythonConverter.GroupCtor(h);
            }
            if (o is Card)
            {
                var h = o as Card;
                return string.Format("Card({0})", h.Id);
            }
            if (o is Counter)
            {
                var h = o as Counter;
                var player = Player.All.FirstOrDefault(x => x.Counters.Any(y => y.Id == h.Id));
                return string.Format("Counter({0},{1},{2})", h.Id, FormatObject(h.Name), FormatObject(player));
            }
            if (o is string)
            {
                return string.Format("\"{0}\"", o);
            }
            return o.ToString();
        }

        /// <summary>
        /// Validates that a string is a valid Python identifier
        /// </summary>
        private static bool IsValidPythonIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // Must start with letter or underscore
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                return false;

            // Rest must be letters, digits, or underscores
            for (int i = 1; i < identifier.Length; i++)
            {
                if (!char.IsLetterOrDigit(identifier[i]) && identifier[i] != '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a function name is potentially dangerous
        /// </summary>
        private static bool IsDangerousFunction(string function)
        {
            var dangerousFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "exec", "eval", "compile", "__import__", "open", "file", "input", "raw_input",
                "reload", "execfile", "apply", "getattr", "setattr", "delattr", "hasattr",
                "globals", "locals", "vars", "dir", "exit", "quit"
            };

            return dangerousFunctions.Contains(function) || IsWebFunction(function);
        }

        /// <summary>
        /// Checks if a function name is a web-related function that should not be allowed via remoteCall
        /// </summary>
        private static bool IsWebFunction(string function)
        {
            // Block all web-related functions to prevent remote users from making arbitrary web requests
            return function.StartsWith("web", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates arguments by parsing them and ensuring each one is a safe type
        /// </summary>
        private void ValidateArguments(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return;

            try
            {
                // Parse the arguments string as individual arguments
                var parsedArgs = ParseArgumentList(args);

                foreach (var arg in parsedArgs)
                {
                    if (!IsValidArgument(arg.Trim()))
                    {
                        Log.WarnFormat("Security violation: Invalid argument detected: '{0}'", arg);
                        throw new ScriptSecurityException("ExecuteFunctionSecureNoFormat", args, $"Invalid argument detected: {arg}");
                    }
                }
            }
            catch (ScriptSecurityException)
            {
                // Re-throw security exceptions
                throw;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Failed to validate arguments '{0}': {1}", args, ex.Message);
                throw new ScriptSecurityException("ExecuteFunctionSecureNoFormat", args, $"Failed to validate arguments: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a comma-separated argument list, respecting quoted strings and nested brackets
        /// </summary>
        private List<string> ParseArgumentList(string args)
        {
            var arguments = new List<string>();
            var current = new StringBuilder();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var bracketDepth = 0;
            var parenDepth = 0;
            var braceDepth = 0;

            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    current.Append(c);
                }
                else if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    current.Append(c);
                }
                else if (!inSingleQuote && !inDoubleQuote)
                {
                    if (c == '[')
                    {
                        bracketDepth++;
                        current.Append(c);
                    }
                    else if (c == ']')
                    {
                        bracketDepth--;
                        current.Append(c);
                    }
                    else if (c == '(')
                    {
                        parenDepth++;
                        current.Append(c);
                    }
                    else if (c == ')')
                    {
                        parenDepth--;
                        current.Append(c);
                    }
                    else if (c == '{')
                    {
                        braceDepth++;
                        current.Append(c);
                    }
                    else if (c == '}')
                    {
                        braceDepth--;
                        current.Append(c);
                    }
                    else if (c == ',' && bracketDepth == 0 && parenDepth == 0 && braceDepth == 0)
                    {
                        // This is a top-level comma separator
                        arguments.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            // Add the last argument
            if (current.Length > 0)
            {
                arguments.Add(current.ToString());
            }

            return arguments;
        }

        /// <summary>
        /// Parses dictionary pairs from dictionary content, respecting nesting
        /// </summary>
        private List<string> ParseDictionaryPairs(string content)
        {
            var pairs = new List<string>();
            var current = new StringBuilder();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var bracketDepth = 0;
            var parenDepth = 0;
            var braceDepth = 0;

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    current.Append(c);
                }
                else if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    current.Append(c);
                }
                else if (!inSingleQuote && !inDoubleQuote)
                {
                    if (c == '[')
                    {
                        bracketDepth++;
                        current.Append(c);
                    }
                    else if (c == ']')
                    {
                        bracketDepth--;
                        current.Append(c);
                    }
                    else if (c == '(')
                    {
                        parenDepth++;
                        current.Append(c);
                    }
                    else if (c == ')')
                    {
                        parenDepth--;
                        current.Append(c);
                    }
                    else if (c == '{')
                    {
                        braceDepth++;
                        current.Append(c);
                    }
                    else if (c == '}')
                    {
                        braceDepth--;
                        current.Append(c);
                    }
                    else if (c == ',' && bracketDepth == 0 && parenDepth == 0 && braceDepth == 0)
                    {
                        // This is a top-level comma separator for dictionary pairs
                        pairs.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            // Add the last pair
            if (current.Length > 0)
            {
                pairs.Add(current.ToString());
            }

            return pairs;
        }

        /// <summary>
        /// Parses a single key-value pair from dictionary content
        /// </summary>
        private Tuple<string, string> ParseKeyValuePair(string pair)
        {
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var bracketDepth = 0;
            var parenDepth = 0;
            var braceDepth = 0;
            var colonIndex = -1;

            // Find the colon separator that's not inside quotes or nested structures
            for (int i = 0; i < pair.Length; i++)
            {
                char c = pair[i];

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                }
                else if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                }
                else if (!inSingleQuote && !inDoubleQuote)
                {
                    if (c == '[')
                    {
                        bracketDepth++;
                    }
                    else if (c == ']')
                    {
                        bracketDepth--;
                    }
                    else if (c == '(')
                    {
                        parenDepth++;
                    }
                    else if (c == ')')
                    {
                        parenDepth--;
                    }
                    else if (c == '{')
                    {
                        braceDepth++;
                    }
                    else if (c == '}')
                    {
                        braceDepth--;
                    }
                    else if (c == ':' && bracketDepth == 0 && parenDepth == 0 && braceDepth == 0)
                    {
                        colonIndex = i;
                        break;
                    }
                }
            }

            if (colonIndex == -1)
            {
                return null; // No valid colon separator found
            }

            var key = pair.Substring(0, colonIndex).Trim();
            var value = pair.Substring(colonIndex + 1).Trim();

            return new Tuple<string, string>(key, value);
        }

        /// <summary>
        /// Validates a single argument, returning true if it's safe to use
        /// </summary>
        private bool IsValidArgument(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
                return true;

            arg = arg.Trim();

            // First check if it contains any obvious expression indicators
            if (ContainsExpressionIndicators(arg))
            {
                Log.WarnFormat("Rejecting argument with expression indicators: '{0}'", arg);
                return false;
            }

            // Block Python dunder variables (double underscore variables) for security
            if (IsPythonDunderVariable(arg))
            {
                Log.WarnFormat("Rejecting Python dunder variable for security: '{0}'", arg);
                return false;
            }

            // Allow None/null
            if (arg.Equals("None", StringComparison.OrdinalIgnoreCase) || 
                arg.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow numbers (integers and floats)
            if (IsNumber(arg))
            {
                return true;
            }

            // Allow boolean values - only exact Python boolean literals
            if (arg.Equals("True", StringComparison.Ordinal) || 
                arg.Equals("False", StringComparison.Ordinal))
            {
                return true;
            }

            // Allow quoted strings
            if (IsQuotedString(arg))
            {
                // Additional check for quote escape exploits
                if (ContainsQuoteBreakoutExploit(arg))
                {
                    Log.WarnFormat("Rejecting argument with quote breakout exploit: '{0}'", arg);
                    return false;
                }
                return true;
            }

            // Allow OCTGN object constructors
            if (IsValidOctgnObjectConstructor(arg))
            {
                return true;
            }

            // Allow lists of valid arguments
            if (arg.StartsWith("[") && arg.EndsWith("]"))
            {
                return IsValidList(arg);
            }

            // Allow dictionaries of valid arguments
            if (arg.StartsWith("{") && arg.EndsWith("}"))
            {
                return IsValidDictionary(arg);
            }

            // Reject everything else
            Log.WarnFormat("Rejecting unsafe argument: '{0}'", arg);
            return false;
        }
        
        /// <summary>
        /// Checks if an argument contains indicators that it's an expression rather than a simple value
        /// </summary>
        private bool ContainsExpressionIndicators(string arg)
        {
            // Skip this check for arguments that start and end with brackets (lists), braces (dictionaries), or quotes (strings)
            // as they have their own validation
            if ((arg.StartsWith("[") && arg.EndsWith("]")) ||
                (arg.StartsWith("{") && arg.EndsWith("}")) ||
                (arg.StartsWith("\"") && arg.EndsWith("\"")) ||
                (arg.StartsWith("'") && arg.EndsWith("'")))
            {
                return false;
            }
            
            // If it could be a number (including negative), let the IsNumber method handle it
            if (CouldBeNumber(arg))
            {
                return false;
            }
            
            // Look for characters that typically indicate expressions or operations
            // outside of quoted strings and constructors
            var suspiciousChars = new[] { '+', '-', '*', '/', '%', '&', '|', '^', '~', '<', '>', '=' };
            
            bool inQuotes = false;
            char quoteChar = '\0';
            bool inEscape = false;
            int parenDepth = 0;
            
            for (int i = 0; i < arg.Length; i++)
            {
                char c = arg[i];
                
                if (inEscape)
                {
                    inEscape = false;
                    continue;
                }
                
                if (c == '\\')
                {
                    inEscape = true;
                    continue;
                }
                
                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                    continue;
                }
                
                if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    quoteChar = '\0';
                    continue;
                }
                
                if (!inQuotes)
                {
                    if (c == '(')
                    {
                        parenDepth++;
                    }
                    else if (c == ')')
                    {
                        parenDepth--;
                    }
                    else if (parenDepth == 0)
                    {
                        // Outside of any constructor parentheses and quotes, check for suspicious characters
                        if (Array.IndexOf(suspiciousChars, c) >= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Quick check to see if a string could potentially be a number (including negative numbers)
        /// This is used to avoid false positives in expression detection
        /// </summary>
        private bool CouldBeNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            value = value.Trim();
            
            // Quick heuristic: if it starts with a digit, +, or -, and contains only number-like characters,
            // it's probably a number and should be validated by IsNumber instead
            if (value.Length == 0)
                return false;
                
            char first = value[0];
            
            // Check for Python number formats
            if (first == '-' && value.Length > 1)
            {
                // Could be negative Python number format
                if (value.Length > 3 && value[1] == '0' && (value[2] == 'b' || value[2] == 'B' || value[2] == 'x' || value[2] == 'X' || value[2] == 'o' || value[2] == 'O'))
                    return true;
            }
            
            if (first == '0' && value.Length > 2)
            {
                char second = char.ToLowerInvariant(value[1]);
                // Python binary, hex, or octal
                if (second == 'b' || second == 'x' || second == 'o')
                    return true;
            }
            
            if (!char.IsDigit(first) && first != '+' && first != '-' && first != '.')
                return false;
                
            // Check if it contains only characters that could be in a number
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!char.IsDigit(c) && c != '.' && c != '+' && c != '-' && c != 'e' && c != 'E')
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Checks if a string represents a single valid number (not an expression)
        /// </summary>
        private bool IsNumber(string value)
        {
            // Must be a single number literal, not an expression
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            value = value.Trim();
            
            // Check for Python-style number formats first
            if (IsPythonNumberFormat(value))
                return true;
            
            // Check standard decimal numbers
            // Allow: digits, decimal point, minus sign (at start or after e/E), plus sign (at start or after e/E), 'e'/'E' for scientific notation
            bool hasDecimal = false;
            bool hasExponent = false;
            
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                
                if (char.IsDigit(c))
                    continue;
                    
                if (c == '.' && !hasDecimal && !hasExponent)
                {
                    hasDecimal = true;
                    continue;
                }
                
                if ((c == 'e' || c == 'E') && !hasExponent && i > 0)
                {
                    hasExponent = true;
                    continue;
                }
                
                // Allow + or - at the start of the number or immediately after e/E for scientific notation
                if (c == '+' || c == '-')
                {
                    if (i == 0) // At the start of the number (negative/positive numbers)
                    {
                        continue;
                    }
                    if (hasExponent && i > 0 && (value[i-1] == 'e' || value[i-1] == 'E')) // After e/E in scientific notation
                    {
                        continue;
                    }
                }
                
                // Any other character makes this not a simple number
                return false;
            }
            
            // Now verify it actually parses as a number
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        /// <summary>
        /// Checks if a string is a valid Python number format (binary, hex, octal)
        /// </summary>
        private bool IsPythonNumberFormat(string value)
        {
            if (value.Length < 3) // Minimum for 0x1, 0b1, 0o1
                return false;

            // Handle negative numbers
            bool isNegative = value.StartsWith("-");
            if (isNegative)
            {
                value = value.Substring(1);
                if (value.Length < 3)
                    return false;
            }

            if (!value.StartsWith("0"))
                return false;

            char prefix = value.Length > 1 ? char.ToLowerInvariant(value[1]) : '\0';
            
            switch (prefix)
            {
                case 'b': // Binary: 0b1010
                    return IsBinaryNumber(value);
                case 'x': // Hexadecimal: 0xFF
                    return IsHexNumber(value);
                case 'o': // Octal: 0o755
                    return IsOctalNumber(value);
                default:
                    return false;
            }
        }

        private bool IsBinaryNumber(string value)
        {
            if (!value.StartsWith("0b") && !value.StartsWith("0B"))
                return false;

            for (int i = 2; i < value.Length; i++)
            {
                if (value[i] != '0' && value[i] != '1')
                    return false;
            }
            
            return value.Length > 2; // Must have at least one binary digit
        }

        private bool IsHexNumber(string value)
        {
            if (!value.StartsWith("0x") && !value.StartsWith("0X"))
                return false;

            for (int i = 2; i < value.Length; i++)
            {
                char c = char.ToLowerInvariant(value[i]);
                if (!char.IsDigit(c) && (c < 'a' || c > 'f'))
                    return false;
            }
            
            return value.Length > 2; // Must have at least one hex digit
        }

        private bool IsOctalNumber(string value)
        {
            if (!value.StartsWith("0o") && !value.StartsWith("0O"))
                return false;

            for (int i = 2; i < value.Length; i++)
            {
                char c = value[i];
                if (c < '0' || c > '7')
                    return false;
            }
            
            return value.Length > 2; // Must have at least one octal digit
        }

        /// <summary>
        /// Checks if a string is properly quoted and contains only a single string literal
        /// </summary>
        private bool IsQuotedString(string value)
        {
            // Must be a single quoted string, not an expression
            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
            {
                // Ensure it's a single string literal - no unescaped quotes inside
                // and no expressions that could combine strings
                return IsSingleStringLiteral(value, '"');
            }
            
            if (value.StartsWith("'") && value.EndsWith("'") && value.Length >= 2)
            {
                // Ensure it's a single string literal - no unescaped quotes inside
                // and no expressions that could combine strings
                return IsSingleStringLiteral(value, '\'');
            }
            
            return false;
        }
        
        /// <summary>
        /// Validates that a value is a single string literal without expressions
        /// </summary>
        private bool IsSingleStringLiteral(string value, char quoteChar)
        {
            // Block hex and unicode escape sequences for security
            // These could be used to encode dangerous strings to bypass filtering
            if (value.Contains("\\x") || value.Contains("\\u"))
            {
                Log.WarnFormat("Rejecting string with hex/unicode escape sequences for security: '{0}'", value);
                return false;
            }
            
            // Block triple quotes for security (can be used for quote injection)
            if (value.Contains("'''") || value.Contains("\"\"\""))
            {
                Log.WarnFormat("Rejecting string with triple quotes for security: '{0}'", value);
                return false;
            }
            
            // Find the closing quote and validate there's nothing after it except whitespace
            int quoteCount = 0;
            bool inEscape = false;
            int closingQuoteIndex = -1;
            
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                
                if (inEscape)
                {
                    inEscape = false;
                    continue;
                }
                
                if (c == '\\')
                {
                    inEscape = true;
                    continue;
                }
                
                if (c == quoteChar)
                {
                    quoteCount++;
                    if (quoteCount == 2) // This is the closing quote
                    {
                        closingQuoteIndex = i;
                        break;
                    }
                }
            }
            
            // Should have exactly 2 quotes (start and end) for a single string literal
            if (quoteCount != 2 || closingQuoteIndex == -1)
            {
                return false;
            }
            
            // Check that everything after the closing quote is just whitespace
            for (int i = closingQuoteIndex + 1; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    Log.WarnFormat("Rejecting string with content after closing quote: '{0}'", value);
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Validates a list argument
        /// </summary>
        private bool IsValidList(string listArg)
        {
            // Remove the outer brackets
            var content = listArg.Substring(1, listArg.Length - 2).Trim();
            
            if (string.IsNullOrEmpty(content))
            {
                return true; // Empty list is valid
            }

            // Parse the list elements
            var elements = ParseArgumentList(content);

            foreach (var element in elements)
            {
                if (!IsValidArgument(element.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates a dictionary argument
        /// </summary>
        private bool IsValidDictionary(string dictArg)
        {
            // Remove the outer braces
            var content = dictArg.Substring(1, dictArg.Length - 2).Trim();
            
            if (string.IsNullOrEmpty(content))
            {
                return true; // Empty dictionary is valid
            }

            // Parse the dictionary key-value pairs
            var pairs = ParseDictionaryPairs(content);

            foreach (var pair in pairs)
            {
                var keyValue = ParseKeyValuePair(pair.Trim());
                if (keyValue == null)
                {
                    Log.WarnFormat("Failed to parse dictionary key-value pair: '{0}'", pair);
                    return false;
                }

                // Validate both key and value
                if (!IsValidArgument(keyValue.Item1.Trim()) || !IsValidArgument(keyValue.Item2.Trim()))
                {
                    Log.WarnFormat("Invalid dictionary key or value: key='{0}', value='{1}'", keyValue.Item1, keyValue.Item2);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if an argument is a valid OCTGN object constructor
        /// </summary>
        private bool IsValidOctgnObjectConstructor(string arg)
        {
            // Match patterns like Player(123), Card(456), etc.
            // Note: [a-zA-Z_][a-zA-Z0-9_]* matches valid Python identifiers (variables)
            var validConstructors = new[]
            {
                @"^Player\(\d+\)$",
                @"^Card\(\d+\)$", 
                @"^Counter\(\d+\s*,\s*""[^""]*""\s*,\s*(Player\(\d+\)|[a-zA-Z_][a-zA-Z0-9_]*)\)$",
                @"^Counter\(\d+\s*,\s*'[^']*'\s*,\s*(Player\(\d+\)|[a-zA-Z_][a-zA-Z0-9_]*)\)$",
                @"^Pile\(\d+\s*,\s*""[^""]*""\s*,\s*(Player\(\d+\)|[a-zA-Z_][a-zA-Z0-9_]*)\)$",
                @"^Pile\(\d+\s*,\s*'[^']*'\s*,\s*(Player\(\d+\)|[a-zA-Z_][a-zA-Z0-9_]*)\)$",
                @"^table$"
            };

            foreach (var pattern in validConstructors)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(arg, pattern))
                {
                    return true;
                }
            }
            
            // Check if it's a simple Python identifier (variable name)
            if (IsSimplePythonIdentifier(arg))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Validates that a string is a simple Python identifier without expressions
        /// </summary>
        private bool IsSimplePythonIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            identifier = identifier.Trim();

            // Must start with letter or underscore
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                return false;

            // Rest must be letters, digits, or underscores only
            // No spaces, operators, parentheses, brackets, or other characters
            for (int i = 1; i < identifier.Length; i++)
            {
                char c = identifier[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a string is a Python dunder variable (starts and ends with double underscores)
        /// These are blocked for security as they can provide access to dangerous functionality
        /// </summary>
        private bool IsPythonDunderVariable(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            identifier = identifier.Trim();

            // Must be at least 4 characters to have __ at both ends
            if (identifier.Length < 4)
                return false;

            // Check if it starts and ends with double underscores
            return identifier.StartsWith("__") && identifier.EndsWith("__");
        }

        /// <summary>
        /// Detects attempts to break out of string context using quote escapes or multiple string segments
        /// </summary>
        private bool ContainsQuoteBreakoutExploit(string arg)
        {
            // Look for patterns that indicate attempts to break out of string context
            
            // Check for semicolons outside of properly quoted strings (statement separators)
            if (ContainsUnquotedCharacter(arg, ';'))
                return true;
                
            // Check for multiple string segments (indicates concatenation or breakout)
            if (ContainsMultipleStringSegments(arg))
                return true;
                
            // Check for hex/unicode escapes that could create quotes
            if (ContainsEscapeSequencesToQuotes(arg))
                return true;
                
            // Check for newlines/carriage returns that could be used for statement separation
            if (ContainsUnquotedCharacter(arg, '\n') || ContainsUnquotedCharacter(arg, '\r'))
                return true;
                
            return false;
        }

        /// <summary>
        /// Checks if a character appears outside of properly quoted strings
        /// </summary>
        private bool ContainsUnquotedCharacter(string arg, char targetChar)
        {
            bool inQuotes = false;
            char quoteChar = '\0';
            bool inEscape = false;
            
            for (int i = 0; i < arg.Length; i++)
            {
                char c = arg[i];
                
                if (inEscape)
                {
                    inEscape = false;
                    continue;
                }
                
                if (c == '\\')
                {
                    inEscape = true;
                    continue;
                }
                
                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                    continue;
                }
                
                if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    quoteChar = '\0';
                    continue;
                }
                
                // If we find the target character outside of quotes, it's suspicious
                if (!inQuotes && c == targetChar)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Detects multiple string segments that could indicate string concatenation or breakout
        /// </summary>
        private bool ContainsMultipleStringSegments(string arg)
        {
            int stringCount = 0;
            bool inQuotes = false;
            char quoteChar = '\0';
            bool inEscape = false;
            
            for (int i = 0; i < arg.Length; i++)
            {
                char c = arg[i];
                
                if (inEscape)
                {
                    inEscape = false;
                    continue;
                }
                
                if (c == '\\')
                {
                    inEscape = true;
                    continue;
                }
                
                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                    stringCount++;
                    continue;
                }
                
                if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    quoteChar = '\0';
                    continue;
                }
            }
            
            // More than one string segment is suspicious
            return stringCount > 1;
        }

        /// <summary>
        /// Checks for hex or unicode escape sequences that could be used to create quote characters
        /// </summary>
        private bool ContainsEscapeSequencesToQuotes(string arg)
        {
            // Look for hex escapes that create quotes: \x22 ("), \x27 (')
            if (arg.Contains("\\x22") || arg.Contains("\\x27"))
                return true;
                
            // Look for unicode escapes that create quotes: \u0022 ("), \u0027 (')
            if (arg.Contains("\\u0022") || arg.Contains("\\u0027"))
                return true;
                
            return false;
        }

        public void ExecuteOnGroup(string function, Group group, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string pythonGroup = PythonConverter.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(string.Format("{0}({1})", function, pythonGroup),
                                                                    SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnGroup(string function, Group group, Point position, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string pythonGroup = PythonConverter.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(
                string.Format(CultureInfo.InvariantCulture,
                              "result = {0}({1}, {2:F3}, {3:F3})",
                              function, pythonGroup, position.X, position.Y),
                SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnCards(string function, IEnumerable<Card> cards, Point? position = null, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string posArguments = position == null
                                      ? ""
                                      : string.Format(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}",
                                                      position.Value.X, position.Value.Y);
            var sb = new StringBuilder();
            foreach (Card card in cards)
                sb.AppendFormat(CultureInfo.InvariantCulture,
                                "result = {0}(Card({1}){2})\n",
                                function, card.Id, posArguments);
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnBatch(string function, IEnumerable<Card> cards, Point? position = null, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            var sb = new StringBuilder();
            sb.Append("result = ").Append(function).Append("([");
            foreach (Card c in cards)
                sb.Append("Card(").Append(c.Id.ToString(CultureInfo.InvariantCulture)).Append("),");
            sb.Append("]");
            if (position != null)
                sb.AppendFormat(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}", position.Value.X, position.Value.Y);
            sb.Append(")\n");
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        private void StartExecution(ScriptSource src, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            var job = new ScriptJob(src, scope, continuation);
            StartExecution(job);
        }

        private void StartExecution(ScriptJobBase job)
        {
            if (Prefs.EnableGameScripts == false) return;
            _executionQueue.Enqueue(job);
            if (_executionQueue.Count == 1) // Other scripts may be hung. Scripts are executed in order.
                ProcessExecutionQueue();
        }

        private void ProcessExecutionQueue()
        {
            do
            {
                ScriptJobBase job = _executionQueue.Peek();
                var scriptjob = job as ScriptJob;
                if (scriptjob != null)
                    Program.GameMess.GameDebug(scriptjob.Source.GetCode());
                // Because some scripts have to be suspended during asynchronous operations (e.g. shuffle, reveal or random),
                // their evaluation is done on another thread.
                // The process still looks synchronous (no concurrency is allowed when manipulating the game model),
                // which is why a ManualResetEvent is used to synchronise the work of both threads
                if (job.Suspended)
                {
                    job.Suspended = false;
                    job.WorkerSignal.Set();
                }
                else
                {
                    job.DispatcherSignal = new AutoResetEvent(false);
                    job.WorkerSignal = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(Execute, job);
                }

                job.DispatcherSignal.WaitOne();
                while (job.InvokedOperation != null)
                {
                    using (new Mute(job.Muted))
                        job.InvokeResult = job.InvokedOperation.DynamicInvoke();
                    job.InvokedOperation = null;
                    job.WorkerSignal.Set();
                    job.DispatcherSignal.WaitOne();
                }
                if (job.Result != null && !String.IsNullOrWhiteSpace(job.Result.Error))
                {
                    Program.GameMess.Warning("{0}", job.Result.Error.Trim());
                }
                if (job.Suspended) return;
                job.DispatcherSignal.Dispose();
                job.WorkerSignal.Dispose();
                _executionQueue.Dequeue();

                if (job.Continuation != null)
                    job.Continuation(job.Result);
            } while (_executionQueue.Count > 0);
        }

        private void Execute(Object state)
        {
            var job = (ScriptJobBase)state;
            var result = new ExecutionResult();
            try
            {
                //if (job is ScriptJob)
                //{
                var sj = job as ScriptJob;
                var scriptResult = sj.Source.Execute(sj.Scope);
                var hasResult = sj.Scope.TryGetVariable("result", out result.ReturnValue);
                result.Output = Encoding.UTF8.GetString(_outputStream.ToArray(), 0, (int)_outputStream.Length);
                // It looks like Python adds some \r in front of \n, which sometimes
                // (depending on the string source) results in doubled \r\r
                result.Output = result.Output.Replace("\r\r", "\r");
                _outputStream.SetLength(0);
                //}
                //else if (job is InvokedScriptJob)
                //{
                //    var ij = job as InvokedScriptJob;
                //    ij.ExecuteAction();
                //}
            }
            catch (Exception ex)
            {
                var eo = _engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(ex);
                var source = (job is ScriptJob) ? (job as ScriptJob).Source.GetCode() : "";
                result.Error = error + Environment.NewLine + source;
                //result.Error = String.Format("{0}\n{1}",ex.Message,ex.StackTrace);
                //Program.TraceWarning("----Python Error----\n{0}\n----End Error----\n", result.Error);
            }
            job.Result = result;
            job.DispatcherSignal.Set();
        }

        internal void Suspend()
        {
            var job = CurrentJob;
            job.Suspended = true;
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
        }

        internal void Resume()
        {
            ProcessExecutionQueue();
        }

        internal void Invoke(Action action)
        {
            ScriptJobBase job = CurrentJob;
            job.InvokedOperation = action;
            //job.invokedOperation = () =>
            //                           {
            //                               action();
            //                               return null;
            //                           };
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
        }

        internal T Invoke<T>(Func<T> func)
        {
            ScriptJobBase job = _executionQueue.Peek();
            job.InvokedOperation = func;
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
            return (T)job.InvokeResult;
        }

        private void InjectOctgnIntoScope(ScriptScope scope, string workingDirectory)
        {
            // Skip injection during testing
            if (_isForTesting)
            {
                // For testing, only set the working directory
                scope.SetVariable("_wd", workingDirectory);
                
                // Skip CaseInsensitiveDict during testing as it requires collections module
                // which may not be available in the test environment
                return;
            }
            
            scope.SetVariable("_api", Program.GameEngine.ScriptApi);
            scope.SetVariable("_wd", workingDirectory);

            // For convenience reason, the definition of Python API objects is in a seperate file: PythonAPI.py
            _engine.Execute(Properties.Resources.CaseInsensitiveDict, scope);

//            _engine.Execute(
//                @"
//import clr
//clr.AddReference(""mscorlib"")
//",
//                scope
            //);

            //_engine.Runtime.LoadAssembly(typeof(Directory).Assembly);

            var file = Versioned.GetFile("PythonApi", Program.GameEngine.Definition.ScriptVersion);
            using (var str = Application.GetResourceStream(new Uri(file.Path)).Stream)
            using (var sr = new StreamReader(str))
            {
                var script = sr.ReadToEnd();
                _engine.Execute(script, scope);
            }

            // See comment on sponsor declaration
            // Note: this has to be done after api has been activated at least once remotely,
            // that's why the code is here rather than in the c'tor
            //if (_sponsor != null) return;
            //_sponsor = new Sponsor();
            //var life = (ILease)RemotingServices.GetLifetimeService(_api);
            //life.Register(_sponsor);
            //life = (ILease)RemotingServices.GetLifetimeService(_outputWriter);
            //life.Register(_sponsor);
        }

        private string ConvertWinforms(string str)
        {
            var ret = new StringBuilder();
            using (var sr = new StringReader(str))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Contains("ShowDialog()") == false || line.Contains("#") == true)
                    {
                        ret.AppendLine(line);
                        line = sr.ReadLine();
                        continue;
                    }
                    var isb = new StringBuilder();
                    string name = null, padding = null;
                    foreach (char t in line)
                    {
                        if (padding == null)
                        {
                            if (char.IsWhiteSpace(t))
                            {
                                isb.Append(t);
                            }
                            else
                            {
                                padding = isb.ToString();
                                isb.Clear();
                                isb.Append(t);
                            }
                            continue;
                        }
                        if (t != '.')
                        {
                            isb.Append(t);
                        }
                        else
                        {
                            name = isb.ToString();
                            isb.Clear();
                            break;
                        }
                    }
                    var newLine = string.Format("{0}showWinForm({1})", padding, name);
                    ret.AppendLine(newLine);
                    line = sr.ReadLine();
                }
            }
            return ret.ToString();
        }

        #region IDisposable

        public void Dispose()
        {
            //if (_sponsor == null) return;
            // See comment on sponsor declaration
            //var life = (ILease)RemotingServices.GetLifetimeService(_api);
            //life.Unregister(_sponsor);
            //life = (ILease)RemotingServices.GetLifetimeService(_outputWriter);
            //life.Unregister(_sponsor);
        }

        #endregion
    }
}