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
using System.Collections.Generic;
using NUnit.Core;

namespace NUnit.VisualStudio.TestAdapter
{
    /// <summary>
    /// ------------------ IMPORTANT !! -----------------------
    /// This type needs to be serializable as it will be passed over to different Appdomain.
    /// So please make sure all the types you use inside this class serialiable, eg. you 
    /// can't use the type ObjectModel.ValidateArg which is not serializable.
    /// </summary>
    [Serializable]
    class TestRunFilter : TestFilter
    {
        private readonly List<string> map;

        public TestRunFilter(List<string> map)
        {
            this.map = map;
        }

        public override bool Match(ITest test)
        {
            if (test != null)
            {
                return test.TestType == "TestMethod" && map.Contains(test.TestName.FullName);
            }

            return false;
        }
    }
}
