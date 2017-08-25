using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using NUnit.Framework;
using FireAndForgetTasksAnalyzer.Tests.Data;

namespace FireAndForgetTasksAnalyzer.Tests
{
    [TestFixture]
    public class FireAndForgetTasksAnalyzerTests : CodeFixVerifier
    {
        [NamedTestCase(nameof(TestDataObject.EmptyCodeTree), arguments: TestDataObject.EmptyCodeTree)]
        [NamedTestData(nameof(TestDataObject.NotAwaitedGlobalVariable))]
        [NamedTestData(nameof(TestDataObject.ExplicitlyTypedLocalVariable))]
        [NamedTestData(nameof(TestDataObject.ImmediatelyAwaitedLocalVariable))]
        [NamedTestData(nameof(TestDataObject.AwaitedLocalVariable))]
        public void ShouldNotWarnAbout(string code)
        {
            VerifyCSharpDiagnostic(code);
        }

        [NamedTestData(nameof(TestDataObject.NotAwaitedLocalVariable))]
        [NamedTestData(nameof(TestDataObject.NotAwaitedNestedLocalVariable))]
        [NamedTestData(nameof(TestDataObject.MultipleNotAwaitedLocalVariables))]
        public void ShouldWarnAndFix(string code, DiagnosticResult[] expectedWarnings, string fix)
        {
            VerifyCSharpDiagnostic(code, expectedWarnings);
            VerifyCSharpFix(code, fix);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FireAndForgetTasksAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FireAndForgetTasksAnalyzerAnalyzer();
        }
    }
}