// ***********************************************************************
// Copyright (c) 2013 Charlie Poole
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

using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace NUnit.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    using Core;
    using Core.Filters;
    using Util;
    using System.Runtime.Remoting;

    /// <summary>
    /// The AssemblyRunner class executes tests in a single assembly
    /// </summary>
    public class AssemblyRunner
    {
        private readonly TestRunner runner = new TestDomain();
        private readonly TestLogger logger;
        private readonly string assemblyName;

        private TestFilter nunitFilter;
        private readonly List<TestCase> loadedTestCases;
        private readonly TestConverter testConverter;
        private INUnitTestAdapter NUnitTestAdapter { get; set; }

        #region Constructors

        // This constructor is called by the others and is used directly for testing
        public AssemblyRunner(TestLogger logger, string assemblyName, INUnitTestAdapter nunitTestAdapter)
        {
            this.logger = logger;
            this.assemblyName = assemblyName;
            testConverter = new TestConverter(logger, assemblyName);
            loadedTestCases = new List<TestCase>();
            nunitFilter = TestFilter.Empty;
            NUnitTestAdapter = nunitTestAdapter;
        }

        // This constructor is used when the executor is called with a list of test cases
        public AssemblyRunner(TestLogger logger, string assemblyName, IEnumerable<TestCase> selectedTestCases, INUnitTestAdapter nunitTestAdapter)
            : this(logger, assemblyName, nunitTestAdapter)
        {
            nunitFilter = MakeTestFilter(selectedTestCases);
        }

        private readonly ITfsTestFilter tfsFilter;

        // This constructor is used when the executor is called with a list of assemblies
        public AssemblyRunner(TestLogger logger, string assemblyName, ITfsTestFilter tfsFilter, INUnitTestAdapter nunitTestAdapter)
            : this(logger, assemblyName, nunitTestAdapter)
        {
            this.tfsFilter = tfsFilter;
        }

        private static SimpleNameFilter MakeTestFilter(IEnumerable<TestCase> ptestCases)
        {
            var filter = new SimpleNameFilter();
            foreach (TestCase testCase in ptestCases)
            {
                filter.Add(testCase.FullyQualifiedName);
            }
            return filter;
        }

        #endregion

        #region Properties

        // TODO: Revise tests and remove
        public TestFilter NUnitFilter 
        {
            get { return nunitFilter; }
        }

        // TODO: Revise tests and remove
        public IList<TestCase> LoadedTestCases 
        {
            get { return loadedTestCases; }
        }

        // TODO: Revise tests and remove
        public TestConverter TestConverter
        { 
            get { return testConverter; }
        }

        #endregion

        #region Public Methods

        public void RunAssembly(IFrameworkHandle testLog)
        {
            try
            {
#if LAUNCHDEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif
                if (TryLoadAssembly())
                {
                    using (NUnitEventListener listener = new NUnitEventListener(testLog, TestConverter))
                    {
                        try
                        {
                            runner.Run(listener, NUnitFilter, true, LoggingThreshold.Off);
                        }
                        catch (NullReferenceException)
                        {
                            // this happens during the run when CancelRun is called.
                            logger.SendDebugMessage("Nullref caught");
                        }
                        finally
                        {
                            runner.Unload();
                        }
                    }
                }
                else
                {
                    logger.NoNUnit2TestsFoundIn(assemblyName);
                }
            }
            catch (BadImageFormatException)
            {
                // we skip the native c++ binaries that we don't support.
                logger.AssemblyNotSupportedWarning(assemblyName);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                // Probably from the GetExportedTypes in NUnit.core, attempting to find an assembly, not a problem if it is not NUnit here
                logger.DependentAssemblyNotFoundWarning(ex.FileName, assemblyName);
            }
            catch (UnsupportedFrameworkException ex)
            {
                //The UnsupportedFrameworkException is thrown by nunit, if the assembly references the NUnit v3 framework.
                logger.SendInformationalMessage("Attempt to load assembly with unsupported test framework in " +  assemblyName);
            }
            catch (Exception ex)
            {
                logger.SendErrorMessage("Exception thrown executing tests in " + assemblyName, ex);
            }
        }

        public void CancelRun()
        {
            if (runner != null && runner.Running)
                runner.CancelRun();
        }

        // Try to load the assembly and, if successful, populate
        // the list of all loaded assemblies. As a side effect
        // of calling TestConverter.ConvertTestCase, the converter's
        // cache of all test cases is populated as well. All
        // future calls to convert a test case may now use the cache.
        private bool TryLoadAssembly()
        {
            var package = NUnitTestAdapter.CreateTestPackage(assemblyName);
            if (!runner.Load(package))
                return false;
            logger.SendMessage(TestMessageLevel.Informational,string.Format("Loading tests from {0}",package.FullName));
            AddTestCases(runner.Test);
            if (tfsFilter==null || !tfsFilter.HasTfsFilterValue) 
                return true;
            var filteredTestCases = tfsFilter.CheckFilter(LoadedTestCases);
            var ptestCases = filteredTestCases as TestCase[] ?? filteredTestCases.ToArray();
            logger.SendMessage(TestMessageLevel.Informational, string.Format("TFS Filter detected: LoadedTestCases {0}, Filterered Test Cases {1}", LoadedTestCases.Count, ptestCases.Count()));
            nunitFilter = MakeTestFilter(ptestCases);
            return true;
        }

        // This method is public for testing purposes.
        // TODO: Test by actually loading an assembly and make it private
        public void AddTestCases(ITest test)
        {
            if (test.IsSuite)
                foreach (ITest child in test.Tests) AddTestCases(child);
            else
                LoadedTestCases.Add(TestConverter.ConvertTestCase(test));
        }

        #endregion
    }
}
