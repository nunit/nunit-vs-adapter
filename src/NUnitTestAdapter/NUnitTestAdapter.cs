﻿// ***********************************************************************
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
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Core;
using NUnit.Util;

namespace NUnit.VisualStudio.TestAdapter
{
    public interface INUnitTestAdapter
    {
        TestPackage CreateTestPackage(string sourceAssembly);
    }

    /// <summary>
    /// NUnitTestAdapter is the common base for the
    /// NUnit discoverer and executor classes.
    /// </summary>
    public abstract class NUnitTestAdapter : INUnitTestAdapter
    {
        // Our logger used to display messages
        protected TestLogger TestLog;
        // The adapter version
        private readonly string adapterVersion;

        protected bool UseVsKeepEngineRunning { get; private set; }
        public bool ShadowCopy { get; private set; }
        public bool CollectSourceInformation { get; private set; }

        public int Verbosity { get; private set; }


        protected bool RegistryFailure { get; set; }
        protected string ErrorMsg
        {
            get; set;
        }

        #region Constructor

        /// <summary>
        /// The common constructor initializes NUnit services 
        /// needed to load and run tests and sets some properties.
        /// </summary>
        protected NUnitTestAdapter()
        {
            ServiceManager.Services.AddService(new DomainManager());
            ServiceManager.Services.AddService(new ProjectService());

            ServiceManager.Services.InitializeServices();
            Verbosity = 0;
            RegistryFailure = false;
            adapterVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                var registry = RegistryCurrentUser.OpenRegistryCurrentUser(@"Software\nunit.org\VSAdapter");
                UseVsKeepEngineRunning = registry.Exist("UseVsKeepEngineRunning") && (registry.Read<int>("UseVsKeepEngineRunning") == 1);
                ShadowCopy = registry.Exist("ShadowCopy") && (registry.Read<int>("ShadowCopy") == 1);
                Verbosity = (registry.Exist("Verbosity")) ? registry.Read<int>("Verbosity") : 0;
            }
            catch (Exception e)
            {
                RegistryFailure = true;
                ErrorMsg = e.ToString();
            }

            TestLog = new TestLogger(Verbosity);
        }

        #endregion

        #region Protected Helper Methods

        // The Adapter is constructed using the default constructor.
        // We don't have any info to initialize it until one of the
        // ITestDiscovery or ITestExecutor methods is called. Each
        // Discover or Execute method must call this method.
        protected void Initialize(IDiscoveryContext context)
        {
            var settingsXml = context?.RunSettings?.SettingsXml;
            if (string.IsNullOrEmpty(settingsXml))
                settingsXml = "<RunSettings />";
            var doc = new XmlDocument();
            doc.LoadXml(settingsXml);
            var runConfiguration = doc.SelectSingleNode("RunSettings/RunConfiguration");
            CollectSourceInformation = GetInnerTextAsBool(runConfiguration, "CollectSourceInformation", true);
        }

        #region Helper Methods

        private string GetInnerText(XmlNode startNode, string xpath, params string[] validValues)
        {
            if (startNode != null)
            {
                var targetNode = startNode.SelectSingleNode(xpath);
                if (targetNode != null)
                {
                    string val = targetNode.InnerText;

                    if (validValues != null && validValues.Length > 0)
                    {
                        foreach (string valid in validValues)
                            if (string.Compare(valid, val, StringComparison.OrdinalIgnoreCase) == 0)
                                return valid;

                        throw new ArgumentException(string.Format(
                            "Invalid value {0} passed for element {1}.", val, xpath));
                    }

                    return val;
                }
            }

            return null;
        }

        private bool GetInnerTextAsBool(XmlNode startNode, string xpath, bool defaultValue)
        {
            string temp = GetInnerText(startNode, xpath);

            if (string.IsNullOrEmpty(temp))
                return defaultValue;

            return bool.Parse(temp);
        }

        #endregion

        private const string Name = "NUnit VS Adapter";

        protected void Info(string method, string function)
        {
            var msg = string.Format("{0} {1} {2} is {3}",Name, adapterVersion, method, function);
            TestLog.SendInformationalMessage(msg);
        }

        protected void Debug(string method, string function)
        {
#if DEBUG
            var msg = string.Format("{0} {1} {2} is {3}", Name, adapterVersion, method, function);
            TestLog.SendDebugMessage(msg);
#endif
        }

        protected static void CleanUpRegisteredChannels()
        {
            foreach (IChannel chan in ChannelServices.RegisteredChannels)
                ChannelServices.UnregisterChannel(chan);
        }

        public TestPackage CreateTestPackage(string sourceAssembly)
        {
             var package = new TestPackage(sourceAssembly);
             package.Settings["ShadowCopyFiles"] = ShadowCopy;
             TestLog.SendDebugMessage("ShadowCopyFiles is set to :" + package.Settings["ShadowCopyFiles"]);
             return package;
        }

        #endregion
    }

    
}
