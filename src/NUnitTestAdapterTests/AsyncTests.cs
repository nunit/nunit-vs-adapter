using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    public class AsyncTests
    {
        [Test]
        public async void VoidTestSuccess()
        {
            var result = await ReturnOne();

            Assert.AreEqual(1, result);
        }

        [Test]
        public async void VoidTestFailure()
        {
            var result = await ReturnOne();

            Assert.Throws<AssertionException>(() => Assert.AreEqual(2, result));
        }

        [Test]
        public async void VoidTestExpectedException()
        {
            Assert.Throws<InvalidOperationException>(async () => await ThrowException());
        }

        [Test]
        public async Task TaskTestSuccess()
        {
            var result = await ReturnOne();

            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task TaskTestFailure()
        {
            var result = await ReturnOne();

            Assert.Throws<AssertionException>(() => Assert.AreEqual(2, result));
        }

        [Test]
        public async Task TaskTestExpectedException()
        {
            Assert.Throws<InvalidOperationException>(async () => await ThrowException());
        }

        [TestCase(ExpectedResult = 1 )]
        public async Task<int> TaskTTestCaseWithResultCheckSuccess()
        {
            return await ReturnOne();
        }

        [TestCase(ExpectedResult=TestOutcome.Failed)]
        public async Task<int> TaskTTestCaseWithResultCheckFailure()
        {
            int result = 0;

            Assert.Throws<AssertionException>(async () =>
            {
                result = await ReturnOne();
            });

            return result;
        }

        private static Task<int> ReturnOne()
        {
            return Task.Run(() => 1);
        }

        private static Task ThrowException()
        {
            return Task.Run(() =>
            {
                throw new InvalidOperationException();
            });
        }
    }
}
