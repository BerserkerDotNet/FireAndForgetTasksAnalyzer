using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Collections;
using System.Runtime.CompilerServices;
using TestHelper;
using System;

namespace FireAndForgetTasksAnalyzer.Tests.Data
{
    public class TestDataObject
    {
        public const string EmptyCodeTree = "";

        public static string NotAwaitedGlobalVariableCode = GetCode(@"
                                Task _x;
                                public void Do()
                                {
                                    _x = Task.FromResult(4);
                                }");

        public static string ExplicitlyTypedLocalVariableCode = GetCode(@"
                                public void Do()
                                {
                                    Task x = Task.FromResult(4);
                                }");

        public static string ImmediatelyAwaitedLocalVariableCode = GetCode(@"
                                public async void Do()
                                {
                                    var x = await Task.FromResult(4);
                                }");

        public static string AwaitedLocalVariableCode = GetCode(@"
                                public async void Do()
                                {
                                    var x = Task.FromResult(4);
                                    await x;
                                }");

        public static string NotAwaitedLocalVariableCode = GetCode(@"
                                public void Do()
                                {
                                    var x = Task.FromResult(4);
                                }");

        public static string NotAwaitedLocalVariableCodeWithAsync = GetCode(@"
                                public async void Do()
                                {
                                    var x = Task.FromResult(4);
                                }");

        public static string NotAwaitedLocalVariableFix = GetCode(@"
                                public async void Do()
                                {
                                    var x = await Task.FromResult(4);
                                }");

        public static string NotAwaitedNestedLocalVariableCode = GetCode(@"
                                public void Do()
                                {
                                    if(true){
                                        if(true){
                                            var x = Task.FromResult(4);
                                        }
                                    }
                                }");

        public static string NotAwaitedNestedLocalVariableFix = GetCode(@"
                                public async void Do()
                                {
                                    if(true){
                                        if(true){
                                            var x = await Task.FromResult(4);
                                        }
                                    }
                                }");

        public static string MultipleNotAwaitedLocalVariablesCode = GetCode(@"
                                public void Do()
                                {
                                    var y = Task.FromResult(3);
                                    if(true){
                                        var x = Task.FromResult(4);
                                    }
                                    Task m = Task.FromResult(5);
                                }

                                public void Bar()
                                {
                                    var z = Task.FromResult(4);
                                }");

        public static string MultipleNotAwaitedLocalVariablesFix = GetCode(@"
                                public async void Do()
                                {
                                    var y = await Task.FromResult(3);
                                    if(true){
                                        var x = await Task.FromResult(4);
                                    }
                                    Task m = Task.FromResult(5);
                                }

                                public async void Bar()
                                {
                                    var z = await Task.FromResult(4);
                                }");

        public static IEnumerable ImmediatelyAwaitedLocalVariable(string callerName) => new[]
        {
            GetTestCaseData(ImmediatelyAwaitedLocalVariableCode, callerName)
        };

        public static IEnumerable AwaitedLocalVariable(string callerName) => new[]
        {
            GetTestCaseData(AwaitedLocalVariableCode, callerName)
        };

        public static IEnumerable ExplicitlyTypedLocalVariable(string callerName) => new[]
        {
            GetTestCaseData(ExplicitlyTypedLocalVariableCode, callerName)
        };

        public static IEnumerable NotAwaitedGlobalVariable(string callerName) => new[]
        {
            GetTestCaseData(NotAwaitedGlobalVariableCode, callerName)
        };

        public static IEnumerable NotAwaitedLocalVariable(string callerName)
        {
            var diagnostics = new[] { GetDiagnostic("x", 16, 41) };
            return new[]
            {
                GetTestCaseDataWithWarnings(NotAwaitedLocalVariableCode, diagnostics, NotAwaitedLocalVariableFix, callerName),
                GetTestCaseDataWithWarnings(NotAwaitedLocalVariableCodeWithAsync, diagnostics, NotAwaitedLocalVariableFix, callerName, $"{nameof(NotAwaitedLocalVariable)}WithAsync"),
            };
        }

        public static IEnumerable NotAwaitedNestedLocalVariable(string callerName)
        {
            var diagnostics = new[] { GetDiagnostic("x", 18, 49) };
            return new[]
            {
                GetTestCaseDataWithWarnings(NotAwaitedNestedLocalVariableCode, diagnostics, NotAwaitedNestedLocalVariableFix, callerName),
            };
        }

        public static IEnumerable MultipleNotAwaitedLocalVariables(string callerName)
        {
            var diagnostics = new[] { GetDiagnostic("y", 16, 41), GetDiagnostic("x", 18, 45), GetDiagnostic("z", 25, 41) };
            return new[]
            {
                GetTestCaseDataWithWarnings(MultipleNotAwaitedLocalVariablesCode, diagnostics, MultipleNotAwaitedLocalVariablesFix, callerName),
            };
        }

        private static DiagnosticResult GetDiagnostic(string variableName, int line, int column)
        {
            return new DiagnosticResult
            {
                Id = "FireAndForgetTasksAnalyzer",
                Message = $"Local variable '{variableName}' is not awaited",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        private static TestCaseData GetTestCaseDataWithWarnings(string code, DiagnosticResult[] warnings, string fix, string callerName, [CallerMemberName]string testPostFix = null)
        {
            return new TestCaseData(code, warnings, fix)
                .SetName($"{callerName}{testPostFix}");
        }

        private static object GetTestCaseData(string code, string callerName, [CallerMemberName]string testPostFix = null)
        {
            return new TestCaseData(code)
                .SetName($"{callerName}{testPostFix}");
        }

        private static string GetCode(string classBody)
        {
            return $@"                        
                        using System;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text;
                        using System.Threading.Tasks;
                        using System.Diagnostics;

                        namespace ConsoleApplication1
                        {{
                            public class Foo
                            {{   
                                {classBody}
                            }}
                        }}";
        }
    }

}
