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
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Core;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Runtime.Remoting;

namespace NUnit.VisualStudio.TestAdapter
{
    /// <summary>
    /// NUnitEventListener implements the EventListener interface and
    /// translates each event into a message for the VS test platform.
    /// </summary>
    public class NUnitEventListener : MarshalByRefObject, EventListener, IDisposable // Public for testing
    {
        private readonly ITestExecutionRecorder testLog;
        private readonly TestConverter testConverter;

        public override object InitializeLifetimeService()
        {
            // Give the listener an infinite lease lifetime by returning null
            // http://msdn.microsoft.com/en-us/magazine/cc300474.aspx#edupdate
            // This also means RemotingServices.Disconnect() must be called to prevent memory leaks
            // http://nbevans.wordpress.com/2011/04/17/memory-leaks-with-an-infinite-lifetime-instance-of-marshalbyrefobject/
            return null;
        }

        public NUnitEventListener(ITestExecutionRecorder testLog, TestConverter testConverter)
        {
            this.testLog = testLog;
            this.testConverter = testConverter;
        }

        public void RunStarted(string name, int testCount)
        {
            testLog.SendMessage(TestMessageLevel.Informational, "Run started: " + name);
        }

        public void RunFinished(Exception exception)
        {
        }

        public void RunFinished(NUnit.Core.TestResult result)
        {
        }

        public string Output { get; private set; }

        public void SuiteStarted(TestName testName)
        {

        }

        public void SuiteFinished(NUnit.Core.TestResult result)
        {
            if ((result.IsError || result.IsFailure) &&
                (result.FailureSite == FailureSite.SetUp || result.FailureSite == FailureSite.TearDown))
            {
                testLog.SendMessage(
                    TestMessageLevel.Error,
                    string.Format("{0} failed for test fixture {1}", result.FailureSite, result.FullName));
                if (result.Message != null)
                    testLog.SendMessage(TestMessageLevel.Error, result.Message);
                if (result.StackTrace != null)
                    testLog.SendMessage(TestMessageLevel.Error, result.StackTrace);
            }
        }

        public void TestStarted(TestName testName)
        {
            TestCase ourCase = testConverter.GetCachedTestCase(testName.UniqueName);

            // Simply ignore any TestName not found in the cache
            if (ourCase != null)
            {
                this.testLog.RecordStart(ourCase);
                // Output = testName.FullName + "\r";
            }

        }

        public void TestFinished(NUnit.Core.TestResult result)
        {
            TestResult ourResult = testConverter.ConvertTestResult(result);
            ourResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, Output));
            this.testLog.RecordEnd(ourResult.TestCase, ourResult.Outcome);
            this.testLog.RecordResult(ourResult);
            Output = "";
        }

        public void TestOutput(TestOutput testOutput)
        {
            string message = testOutput.Text;
            int length = message.Length;
            int drop = message.EndsWith(Environment.NewLine)
                ? Environment.NewLine.Length
                : message[length - 1] == '\n' || message[length - 1] == '\r'
                    ? 1
                    : 0;
            if (drop > 0)
                message = message.Substring(0, length - drop);
            if (!string.IsNullOrEmpty(message.Trim()))
                testLog.SendMessage(TestMessageLevel.Informational, message);
            string type="";
            // Consider adding this later, as an option.
            //switch (testOutput.Type)
            //{
            //    case TestOutputType.Trace:
            //        type ="Debug: ";
            //        break;
            //    case TestOutputType.Out:
            //        type ="Console: ";
            //        break;
            //    case TestOutputType.Log:
            //        type="Log: ";
            //        break;
            //    case TestOutputType.Error:
            //        type="Error: ";
            //        break;
            //}
            this.Output += (type+message+'\r');
        }

        public void UnhandledException(Exception exception)
        {
        }

        #region IDisposable
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    RemotingServices.Disconnect(this);
                }
            }
            disposed = true;
        }

        ~NUnitEventListener()
        {
            Dispose(false);
        }
        #endregion
    }
}
