// ***********************************************************************
// Copyright (c) 2011 Charlie Poole, Terje Sandstrom
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Core;
using NUnit.Framework;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    [Category("TestConverter")]
    public class TestConverterTestsStaticHelpers
    {
        [TestCase(ResultState.Cancelled, Result = TestOutcome.None)]
        [TestCase(ResultState.Error, Result = TestOutcome.Failed)]
        [TestCase(ResultState.Failure, Result = TestOutcome.Failed)]
        [TestCase(ResultState.Ignored, Result = TestOutcome.Skipped)]
        [TestCase(ResultState.Inconclusive, Result = TestOutcome.None)]
        [TestCase(ResultState.NotRunnable, Result = TestOutcome.Failed)]
        [TestCase(ResultState.Skipped, Result = TestOutcome.Skipped)]
        [TestCase(ResultState.Success, Result = TestOutcome.Passed)]
        public TestOutcome ResultStateToTestOutcome(ResultState resultState)
        {
            return TestConverter.ResultStateToTestOutcome(resultState);
        }
    }
}
