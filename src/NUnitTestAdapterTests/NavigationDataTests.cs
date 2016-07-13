﻿using System.IO;
using NUnit.Framework;

namespace NUnit.VisualStudio.TestAdapter.Tests
{
    public class NavigationDataTests
    {
        private static readonly string ThisAssemblyPath =
            Path.GetFullPath("NUnit.VisualStudio.TestAdapter.Tests.dll");

        NavigationDataProvider _provider;

        const string Prefix = "NUnit.VisualStudio.TestAdapter.Tests.NavigationTestData";

        [SetUp]
        public void SetUp()
        {
            _provider = new NavigationDataProvider(ThisAssemblyPath);
        }

        [TestCase("", "EmptyMethod_OneLine", 9, 9)]
        [TestCase("", "EmptyMethod_TwoLines", 12, 13)]
        [TestCase("", "EmptyMethod_ThreeLines", 16, 17)]
        [TestCase("", "EmptyMethod_LotsOfLines", 20, 23)]
        [TestCase("", "SimpleMethod_Void_NoArgs", 26, 28)]
        [TestCase("", "SimpleMethod_Void_OneArg", 32, 33)]
        [TestCase("", "SimpleMethod_Void_TwoArgs", 38, 39)]
        [TestCase("", "SimpleMethod_ReturnsInt_NoArgs", 44, 46)]
        [TestCase("", "SimpleMethod_ReturnsString_OneArg", 50, 51)]
        // Generic method uses simple name
        [TestCase("", "GenericMethod_ReturnsString_OneArg", 55, 56)]
        [TestCase("", "AsyncMethod_Void", 60, 62)]
        [TestCase("", "AsyncMethod_Task", 67, 69)]
        [TestCase("", "AsyncMethod_ReturnsInt", 74, 76)]
        [TestCase("/NestedClass", "SimpleMethod_Void_NoArgs", 83, 85)]
        [TestCase("/ParameterizedFixture", "SimpleMethod_ReturnsString_OneArg", 101, 102)]
        // Generic Fixture requires ` plus type arg count
        [TestCase("/GenericFixture`2", "Matches", 116, 117)]
        [TestCase("/GenericFixture`2/DoublyNested", "WriteBoth", 132, 133)]
        [TestCase("/GenericFixture`2/DoublyNested`1", "WriteAllThree", 151, 152)]
        [TestCase("/DerivedClass", "EmptyMethod_ThreeLines", 160, 161)]
        public void VerifyNavigationData(string suffix, string methodName, int expectedLineDebug, int expectedLineRelease)
        {
            // Get the navigation data - ensure names are spelled correctly!
            var className = Prefix + suffix;
            var data = _provider.GetNavigationData(className, methodName);
            Assert.NotNull(data, "Unable to retrieve navigation data");

            // Verify the navigation data
            // Note that different line numbers are returned for our
            // debug and release builds, as follows:
            //
            // DEBUG
            //   Min line: Opening curly brace.
            //   Max line: Closing curly brace.
            //
            // RELEASE
            //   Min line: First non-comment code line or closing
            //             curly brace if the method is empty.
            //   Max line: Last code line if that line is an 
            //             unconditional return statement, otherwise
            //             the closing curly brace.
            Assert.That(data.FilePath, Is.StringEnding("NavigationTestData.cs"));
#if DEBUG
            Assert.That(data.LineNumber, Is.EqualTo(expectedLineDebug));
#else
            Assert.That(data.LineNumber, Is.EqualTo(expectedLineRelease));
#endif
        }
    }
}
