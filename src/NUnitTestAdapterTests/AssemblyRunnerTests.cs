// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Framework;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    // TODO: Review and convert to tests of the executor as needed

    [TestFixture]
    public class AssemblyRunnerTests
    {
        [SetUp]
        public void SetUp()
        {
            //MethodInfo fakeTestMethod1 = GetType()
            //    .GetMethod("FakeTestMethod1", BindingFlags.Instance | BindingFlags.NonPublic);
            //fakeTest1 = new NUnitTestMethod(fakeTestMethod1);
            //MethodInfo fakeTestMethod2 = GetType()
            //    .GetMethod("FakeTestMethod2", BindingFlags.Instance | BindingFlags.NonPublic);
            //fakeTest2 = new NUnitTestMethod(fakeTestMethod2);
        }

        private static readonly Uri ExecutorUri = new Uri(NUnitTestExecutor.ExecutorUri);

// ReSharper disable once UnusedMember.Local
        private void FakeTestMethod1()
        {
        }

// ReSharper disable once UnusedMember.Local
        private void FakeTestMethod2()
        {
        }

        private ITest fakeTest1;

        private ITest fakeTest2;

        //[Test]
        //public void AddsFilteredCorrectly()
        //{
        //    var t1 = new TestCase(fakeTest1.TestName.FullName, ExecutorUri, "test");
        //    var t2 = new TestCase(fakeTest2.TestName.FullName, ExecutorUri, "test");
        //    var list = new List<TestCase> {t1, t2};
        //    var runner = new AssemblyRunner(new TestLogger(), "test", list,null);
        //    runner.AddTestCases(fakeTest1);
        //    runner.AddTestCases(fakeTest2);
        //    Assert.That(runner.NUnitFilter.IsEmpty, Is.False, "NUnitfilter should not be empty, we have added testcases");
        //    Assert.That(runner.LoadedTestCases.Count, Is.EqualTo(2), "We should have had 2 converted MS test cases here");
        //}

        //[Test]
        //public void AddsNonFilteredCorrectly()
        //{
        //    var runner = new AssemblyRunner(new TestLogger(), "test",null);
        //    runner.AddTestCases(fakeTest1);
        //    runner.AddTestCases(fakeTest2);
        //    Assert.That(runner.NUnitFilter.IsEmpty, Is.True, "NUnitfilter has been touched");
        //    Assert.That(runner.LoadedTestCases.Count, Is.EqualTo(2), "We should have had 2 test cases here");
        //}


        //[Test]
        //public void VerifyConstruction1()
        //{
        //    var runner = new AssemblyRunner(new TestLogger(), "test",null);
        //    Assert.That(runner.NUnitFilter.IsEmpty, Is.True);
        //}

        //[Test]
        //public void VerifyConstruction2()
        //{
        //    var t1 = new TestCase(fakeTest1.TestName.FullName, ExecutorUri, "test");
        //    var t2 = new TestCase(fakeTest2.TestName.FullName, ExecutorUri, "test");
        //    var list = new List<TestCase> {t1, t2};
        //    var runner = new AssemblyRunner(new TestLogger(), "test", list,null);
        //    Assert.False(runner.NUnitFilter.IsEmpty);
        //    Assert.That(runner.NUnitFilter, Is.TypeOf<SimpleNameFilter>());
        //    Assert.True(runner.NUnitFilter.Match(fakeTest1));
        //    Assert.True(runner.NUnitFilter.Match(fakeTest2));
        //}

        // TODO: Instead of using AddTestCases, we should be loading an actual assembly

        //[Test]
        //public void HandleTfsFilterCorrectlyWhenFilterIsEmpty()
        //{
        //    var tfsfilter = new Mock<ITfsTestFilter>();
        //    tfsfilter.Setup(f => f.HasTfsFilterValue).Returns(false);
        //    var runner = new AssemblyRunner(new TestLogger(), "test", tfsfilter.Object);
        //    runner.AddTestCases(fakeTest1);
        //    runner.AddTestCases(fakeTest2);

        //    Assert.That(runner.NUnitFilter.IsEmpty, Is.False, "NUnitfilter should not be empty, we have added testcases");
        //    Assert.That(runner.LoadedTestCases.Count, Is.EqualTo(2), "We should have had 2 converted MS test cases here");
        //}
        //[Test]
        //public void HandleTfsFilterCorrectlyWhenNoFilter()
        //{
        //    var runner = new AssemblyRunner(new TestLogger(), "test", (TFSTestFilter)null);
        //    runner.AddTestCases(fakeTest1);
        //    runner.AddTestCases(fakeTest2);

        //    Assert.That(runner.NUnitFilter.IsEmpty, Is.False, "NUnitfilter should not be empty, we have added testcases");
        //    Assert.That(runner.LoadedTestCases.Count, Is.EqualTo(2), "We should have had 2 converted MS test cases here");
        //}
    }
}