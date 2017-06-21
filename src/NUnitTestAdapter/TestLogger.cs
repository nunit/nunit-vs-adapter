// ***********************************************************************
// Copyright (c) 2013 Charlie Poole, Terje Sandstrom
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

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace NUnit.VisualStudio.TestAdapter
{
    /// <summary>
    /// TestLogger wraps an IMessageLogger and adds various
    /// utility methods for sending messages. Since the
    /// IMessageLogger is only provided when the discovery
    /// and execution objects are called, we use two-phase
    /// construction. Until Initialize is called, the logger
    /// simply swallows all messages without sending them
    /// anywhere.
    /// </summary>
    public class TestLogger : IMessageLogger
    {
        private IMessageLogger messageLogger;

        int Verbosity { get; set; }

        public TestLogger()
        {
            Verbosity = 0;
        }

        public TestLogger(int verbosity)
        {
            Verbosity = verbosity;
        }

        public void Initialize(IMessageLogger messageLoggerParam)
        {
            messageLogger = messageLoggerParam;
        }

        public void AssemblyNotSupportedWarning(string sourceAssembly)
        {
            SendWarningMessage("Assembly not supported: " + sourceAssembly);
        }

        public void DependentAssemblyNotFoundWarning(string dependentAssembly, string sourceAssembly)
        {
            SendWarningMessage("Dependent Assembly " + dependentAssembly + " of " + sourceAssembly + " not found. Can be ignored if not a NUnit project.");
        }

        public void UnsupportedFrameworkWarning(string assembly)
        {
            SendInformationalMessage("Assembly contains no NUnit 2 tests:  " + assembly);
        }

        public void LoadingAssemblyFailedWarning(string dependentAssembly, string sourceAssembly)
        {
            SendWarningMessage("Assembly " + dependentAssembly + " loaded through " + sourceAssembly + " failed. Assembly is ignored. Correct deployment of dependencies if this is an error.");
        }

        public void NoNUnit2TestsFoundIn(string sourceAssembly)
        {
            SendInformationalMessage("No NUnit 2 tests found in " + sourceAssembly);
        }

        public void SendErrorMessage(string message)
        {
            SendMessage(TestMessageLevel.Error, message);
        }

        public void SendErrorMessage(string message, Exception ex)
        {
            
            switch (Verbosity)
            {
                case 0:
                    var type = ex.GetType();
                    SendErrorMessage(string.Format("Exception {0}, {1}",type, message));
                    break;
                default:
                    SendMessage(TestMessageLevel.Error, message);
                    SendErrorMessage(ex.ToString());
                    break;
            }
        }

        public void SendWarningMessage(string message)
        {
            SendMessage(TestMessageLevel.Warning, message);
        }

        public void SendWarningMessage(string message,Exception ex)
        {
            switch (Verbosity)
            {
                case 0:
                    var type = ex.GetType();
                    SendMessage(TestMessageLevel.Warning,string.Format("Exception {0}, {1}", type, message));
                    break;
                default:
                    SendMessage(TestMessageLevel.Warning, message);
                    SendMessage(TestMessageLevel.Warning,ex.ToString());
                    break;
            }
            SendMessage(TestMessageLevel.Warning, message);
        }

        public void SendInformationalMessage(string message)
        {
            SendMessage(TestMessageLevel.Informational, message);
        }

        public void SendDebugMessage(string message)
        {
#if DEBUG
            SendMessage(TestMessageLevel.Informational, message);
#endif
        }

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            if (messageLogger != null)
                messageLogger.SendMessage(testMessageLevel, message);
        }
    }
}
