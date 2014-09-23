// ****************************************************************
// Copyright (c) 2011 NUnit Software. All rights reserved.
// ****************************************************************

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Core;
using NUnit.Framework;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    [Category("TestConverter")]
    public class TestConverterTestsStaticHelpers
    {
        [TestCase(ResultState.Cancelled, ExpectedResult = TestOutcome.None)]
        [TestCase(ResultState.Error, ExpectedResult = TestOutcome.Failed)]
        [TestCase(ResultState.Failure, ExpectedResult = TestOutcome.Failed)]
        [TestCase(ResultState.Ignored, ExpectedResult = TestOutcome.Skipped)]
        [TestCase(ResultState.Inconclusive, ExpectedResult = TestOutcome.None)]
        [TestCase(ResultState.NotRunnable, ExpectedResult = TestOutcome.Failed)]
        [TestCase(ResultState.Skipped, ExpectedResult = TestOutcome.Skipped)]
        [TestCase(ResultState.Success, ExpectedResult = TestOutcome.Passed)]
        public TestOutcome ResultStateToTestOutcome(ResultState resultState)
        {
            return TestConverter.ResultStateToTestOutcome(resultState);
        }
    }
}
