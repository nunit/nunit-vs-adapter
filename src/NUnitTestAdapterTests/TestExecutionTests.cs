﻿// ****************************************************************
// Copyright (c) 2012 NUnit Software. All rights reserved.
// ****************************************************************

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Framework;
using NUnit.Tests.Assemblies;
using NUnit.VisualStudio.TestAdapter.Tests.Fakes;

namespace NUnit.VisualStudio.TestAdapter.Tests
{

    [Category("TestExecution")]
    public class TestExecutionTests
    {
        static readonly string MockAssemblyPath = Path.GetFullPath("mock-assembly.dll");
        static readonly IRunContext Context = new FakeRunContext();

        private List<TestResult> testResults;
        private FakeFrameworkHandle testLog;
        private static ITestExecutor executor;
        ResultSummary Summary { get;  set; }    


        [OneTimeSetUp]
        public void LoadMockassembly()
        {
            // Sanity check to be sure we have the correct version of mock-assembly.dll
            Assert.That(MockAssembly.Tests, Is.EqualTo(31),
                "The reference to mock-assembly.dll appears to be the wrong version");
            new List<TestCase>();
            testResults = new List<TestResult>();
            testLog = new FakeFrameworkHandle();

            // Load the NUnit mock-assembly.dll once for this test, saving
            // the list of test cases sent to the discovery sink
            executor = ((ITestExecutor) new NUnitTestExecutor());
            executor.RunTests(new[] { MockAssemblyPath }, Context, testLog);
            this.Summary = new ResultSummary(testResults);
        }

        [Test]
        public void CorrectNumberOfTestCasesWereStarted()
        {
            const FakeFrameworkHandle.EventType eventType = FakeFrameworkHandle.EventType.RecordStart;
            Assert.That(
                testLog.Events.FindAll(e => e.EventType == eventType).Count,
                Is.EqualTo(MockAssembly.ResultCount));
        }

        [Test]
        public void CorrectNumberOfTestCasesWereEnded()
        {
            const FakeFrameworkHandle.EventType eventType = FakeFrameworkHandle.EventType.RecordEnd;
            Assert.That(
                testLog.Events.FindAll(e => e.EventType == eventType).Count,
                Is.EqualTo(MockAssembly.ResultCount));
        }

        [Test]
        public void CorrectNumberOfResultsWereReceived()
        {
            const FakeFrameworkHandle.EventType eventType = FakeFrameworkHandle.EventType.RecordResult;
            Assert.That(
                testLog.Events.FindAll(e => e.EventType == eventType).Count,
                Is.EqualTo(MockAssembly.ResultCount));
        }

        readonly TestCaseData[] outcomes =
        {
            // NOTE: One inconclusive test is reported as None
            new TestCaseData(TestOutcome.Passed).Returns(MockAssembly.TestsRun - MockAssembly.Errors - MockAssembly.Failures - 1),
            new TestCaseData(TestOutcome.Failed).Returns(MockAssembly.Errors + MockAssembly.Failures + MockAssembly.NotRunnable),
            new TestCaseData(TestOutcome.Skipped).Returns(MockAssembly.NotRun - MockAssembly.Explicit - MockAssembly.NotRunnable),
            new TestCaseData(TestOutcome.None).Returns(1),
            new TestCaseData(TestOutcome.NotFound).Returns(0)
        };

        [TestCaseSource("outcomes")]
        public int TestOutcomeTotalsAreCorrect(TestOutcome outcome)
        {
            return testLog.Events
                .FindAll(e => e.EventType == FakeFrameworkHandle.EventType.RecordResult)
                .ConvertAll(e => e.TestResult)
                .FindAll(r => r.Outcome == outcome)
                .Count;
        }

        [TestCase("MockTest3", TestOutcome.Passed, "Succeeded!", true)]
        [TestCase("FailingTest", TestOutcome.Failed, "Intentional failure", true)]
        [TestCase("TestWithException", TestOutcome.Failed, "System.ApplicationException : Intentional Exception", true)]
        // NOTE: Should Inconclusive be reported as TestOutcome.None?
        [TestCase("InconclusiveTest", TestOutcome.None, "No valid data", false)]
        [TestCase("MockTest4", TestOutcome.Skipped, "ignoring this test method for now", false)]
        // NOTE: Should this be failed?
        [TestCase("NotRunnableTest", TestOutcome.Failed, "No arguments were provided", false)]
        public void TestResultIsReportedCorrectly(string name, TestOutcome outcome, string message, bool hasStackTrace)
        {
            var testResult = testLog.Events
                .FindAll(e => e.EventType == FakeFrameworkHandle.EventType.RecordResult)
                .ConvertAll(e => e.TestResult)
                .Find(r => r.TestCase.DisplayName == name);

            Assert.NotNull(testResult, "Unable to find result for method: " + name);
            Assert.That(testResult.Outcome, Is.EqualTo(outcome));
            Assert.That(testResult.ErrorMessage, Is.EqualTo(message));
            if (hasStackTrace)
                Assert.NotNull(testResult.ErrorStackTrace);
        }

        [Test]
        public void ExplicitTestDoesNotShowUpInResults()
        {
            Assert.Null(testResults.Find(r => r.TestCase.DisplayName == "ExplicitlyRunTest"));
        }

        #region Nested ResultSummary Helper Class

        private class ResultSummary
        {
            private readonly Dictionary<TestOutcome, int> summary;

            public ResultSummary(IEnumerable<TestResult> results)
            {
                summary = new Dictionary<TestOutcome, int>();
                
                foreach(TestResult result in results)
                {
                    var outcome = result.Outcome;
                    summary[outcome] = GetCount(outcome) + 1;
                }
            }

            private int GetCount(TestOutcome outcome)
            {
                return summary.ContainsKey(outcome)
                    ? summary[outcome]
                    : 0;
            }
        }

        #endregion
    }
}
