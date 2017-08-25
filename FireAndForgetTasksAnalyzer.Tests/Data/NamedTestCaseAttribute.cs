using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;

namespace FireAndForgetTasksAnalyzer.Tests.Data
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class NamedTestCaseAttribute : TestCaseAttribute
    {
        public NamedTestCaseAttribute(string namePostfix, [CallerMemberName]string callerName = null, params object[] arguments)
            : base(arguments)
        {
            TestName = $"{callerName}{namePostfix}";
        }
    }
}
