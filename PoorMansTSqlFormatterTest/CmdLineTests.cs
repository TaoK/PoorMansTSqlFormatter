/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using NUnit.Framework;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    public class CmdLineTests
    {
        [Test]
        public void TestCmdLineSwitches()
        {
            //TODO: Test all the cmdline switches, on and off, ideally using a single test input file.
        }

        [Test]
        public void TestCmdLineIO()
        {
            //TODO: test with one file, multiple files, stdin/out
        }

        [Test]
        public void TestCmdLineErrorAbort()
        {
            //TODO: test invalid parsing and confirm does not overwrite unless requested
        }
    }
}
