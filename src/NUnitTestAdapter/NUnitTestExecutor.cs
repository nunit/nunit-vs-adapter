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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Core;
// #define LAUNCHDEBUGGER

namespace NUnit.VisualStudio.TestAdapter
{

    [ExtensionUri(ExecutorUri)]
    public sealed class NUnitTestExecutor : NUnitTestAdapter, ITestExecutor
    {
        ///<summary>
        /// The Uri used to identify the NUnitExecutor
        ///</summary>
        public const string ExecutorUri = "executor://NUnitTestExecutor";

        // The currently executing assembly runner
        private AssemblyRunner currentRunner;

        #region ITestExecutor

        /// <summary>
        /// Called by the Visual Studio IDE to run all tests. Also called by TFS Build
        /// to run either all or selected tests. In the latter case, a filter is provided
        /// as part of the run context.
        /// </summary>
        /// <param name="sources">Sources to be run.</param>
        /// <param name="runContext">Context to use when executing the tests.</param>
        /// <param name="frameworkHandle">Test log to send results and messages through</param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            TestLog.Initialize(frameworkHandle);
            Initialize(runContext);
            if (RegistryFailure)
            {
                TestLog.SendErrorMessage(ErrorMsg);
            }
            Info("executing tests", "started");

            try
            {
                // Ensure any channels registered by other adapters are unregistered
                CleanUpRegisteredChannels();

                var tfsfilter = new TfsTestFilter(runContext);
                TestLog.SendDebugMessage("Keepalive:" + runContext.KeepAlive);
                var enableShutdown = (UseVsKeepEngineRunning) ? !runContext.KeepAlive : true;
                if (!tfsfilter.HasTfsFilterValue)
                {
                    if (!(enableShutdown && !runContext.KeepAlive))  // Otherwise causes exception when run as commandline, illegal to enableshutdown when Keepalive is false, might be only VS2012
                        frameworkHandle.EnableShutdownAfterTestRun = enableShutdown;
                }
                
                foreach (var source in sources)
                {
                    string sourceAssembly = source;
                    if (!Path.IsPathRooted(sourceAssembly))
                        sourceAssembly = Path.Combine(Environment.CurrentDirectory, sourceAssembly);

                    currentRunner = new AssemblyRunner(TestLog, sourceAssembly, tfsfilter, this, CollectSourceInformation);
                    currentRunner.RunAssembly(frameworkHandle);
                }
            }
            catch (Exception ex)
            {
                TestLog.SendErrorMessage("Exception thrown executing tests", ex);
            }
            finally
            {
                Info("executing tests", "finished");
            }

        }

        /// <summary>
        /// Called by the VisualStudio IDE when selected tests are to be run. Never called from TFS Build.
        /// </summary>
        /// <param name="tests">The tests to be run</param>
        /// <param name="runContext">The RunContext</param>
        /// <param name="frameworkHandle">The FrameworkHandle</param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
#if LAUNCHDEBUGGER
            Debugger.Launch();
#endif

            TestLog.Initialize(frameworkHandle);
            Initialize(runContext);
            if (RegistryFailure)
            {
                TestLog.SendErrorMessage(ErrorMsg);
            }
            var enableShutdown = (UseVsKeepEngineRunning) ? !runContext.KeepAlive : true;
            frameworkHandle.EnableShutdownAfterTestRun = enableShutdown;
            Debug("executing tests", "EnableShutdown set to " +enableShutdown);
            Info("executing tests", "started");

            // Ensure any channels registered by other adapters are unregistered
            CleanUpRegisteredChannels();

            var assemblyGroups = tests.GroupBy(tc => tc.Source);
            foreach (var assemblyGroup in assemblyGroups)
            {
                currentRunner = new AssemblyRunner(TestLog, assemblyGroup.Key, assemblyGroup, this, CollectSourceInformation);
                currentRunner.RunAssembly(frameworkHandle);
            }

            Info("executing tests", "finished");

        }

        void ITestExecutor.Cancel()
        {
            if (currentRunner != null)
                currentRunner.CancelRun();
        }

        #endregion
    }
}
