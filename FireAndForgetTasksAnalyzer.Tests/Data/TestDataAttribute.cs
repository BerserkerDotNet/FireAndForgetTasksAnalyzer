using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;

namespace FireAndForgetTasksAnalyzer.Tests.Data
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class NamedTestDataAttribute : TestCaseSourceAttribute
    {
        public NamedTestDataAttribute(string sourceName, [CallerMemberName]string callerName = null)
            : base(typeof(TestDataObject), sourceName, new[] { callerName })
        {
        }
    }
}
