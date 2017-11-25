/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

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
using NUnit.Framework;
using System.Diagnostics;
using System.Text;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    public class CmdLineTests
    {
        #if DEBUG
            private const string FORMATTER_EXECUTABLE = "..\\..\\..\\PoorMansTSqlFormatterCmdLine\\bin\\Debug\\SqlFormatter.exe";
        #else
            private const string FORMATTER_EXECUTABLE = "..\\..\\..\\PoorMansTSqlFormatterCmdLine\\bin\\Release\\SqlFormatter.exe";
        #endif
        
        [Test]
        public void TestCmdLineFormattingSwitches()
        {
            TestFormattingFlags("SELECT 1, 2", "SELECT 1\r\n\t,2", "");
            TestFormattingFlags("SELECT 1, 2", "SELECT 1, 2", "/ecl-");
            TestFormattingFlags("SELECT 1, 2", "SELECT 1\r\n ,2", "/is:\" \"");
            TestFormattingFlags("SELECT 1, 2", "SELECT 1\r\n\t, 2", "/sac");
            TestFormattingFlags("SELECT 1, 2", "SELECT 1,\r\n\t2", "/tc");
            TestFormattingFlags("SELECT BETWEEN 1 and 2", "SELECT BETWEEN 1\r\n\t\tAND 2", "");
            TestFormattingFlags("SELECT BETWEEN 1 and 2", "SELECT BETWEEN 1 AND 2", "/ebc-");
            TestFormattingFlags("SELECT SELECT", "SELECT\r\n\r\nSELECT", "");
            TestFormattingFlags("SELECT SELECT", "SELECTSELECT", "/sb:0");
            TestFormattingFlags("SELECT FROM", "SELECT FROM", "/cb:0", "\r\n");
            TestFormattingFlags("SELECT 1 and 2", "SELECT 1\r\n\tAND 2", "");
            TestFormattingFlags("SELECT 1 and 2", "SELECT 1 AND 2", "/ebe-");
            TestFormattingFlags("SELECT case 1 when 2 then 3 end", "SELECT CASE 1\r\n\t\tWHEN 2\r\n\t\t\tTHEN 3\r\n\t\tEND", "");
            TestFormattingFlags("SELECT case 1 when 2 then 3 end", "SELECT CASE 1 WHEN 2 THEN 3 END", "/ecs-");
            TestFormattingFlags("SELECT in (1,2)", "SELECT IN (\r\n\t\t1\r\n\t\t,2\r\n\t\t)", "");
            TestFormattingFlags("SELECT in (1,2)", "SELECT IN (1, 2)", "/eil-");
            TestFormattingFlags("SELECT in (1,2)", "select in (1, 2)", "/eil- /uk-");
            TestFormattingFlags("SELECT NATIONAL CHARACTER VARYING", "SELECT NVARCHAR", "");
            TestFormattingFlags("SELECT NATIONAL CHARACTER VARYING", "SELECT NATIONAL CHARACTER VARYING", "/sk-");
        }

        [Test]
        public void TestCmdLineEncoding()
        {
            //without worrying too much about which encoding it is, let's check we can input and output funky stuff.
            TestFormattingFlags("SELECT 'João played with l''Orange Amère, and they discovered that ãΕΙΒοςπ looked like nonsensical greek while ڡڣڲۆۏ was clearly intended to be some sort of arabic...'",
                "SELECT 'João played with l''Orange Amère, and they discovered that ãΕΙΒοςπ looked like nonsensical greek while ڡڣڲۆۏ was clearly intended to be some sort of arabic...'", 
                "");
        }

        [Test]
        public void TestCmdLineIO()
        {
            //TODO: test with one file, multiple files, stdin/out
            //needs temp folder work...
        }

        [Test]
        public void TestCmdLineErrorAbort()
        {
            //TODO: test invalid parsing and confirm does not overwrite unless requested
            //needs temp folder work...
        }

        private Process StartFormatterProcess(string arguments)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = FORMATTER_EXECUTABLE;
            myProcess.StartInfo.Arguments = arguments;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            myProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            myProcess.Start();
            return myProcess;
        }

        private void TestFormattingFlags(string inputString, string expectedOutputString, string arguments)
        {
            TestFormattingFlags(inputString, expectedOutputString, arguments, "\r\n\r\n");
        }

        private void TestFormattingFlags(string inputString, string expectedOutputString, string arguments, string outputSuffix)
        {
            var formatterProcess = StartFormatterProcess(arguments);
            //TODO: There *must* be a cleaner way to write to another process' STDIN in UTF-8. I'm stuck on a plane so I will need to bear the suspense.
            var wrappingWriter = new System.IO.StreamWriter(formatterProcess.StandardInput.BaseStream, Encoding.UTF8);
            wrappingWriter.Write(inputString);
            wrappingWriter.Flush();
            wrappingWriter.Close();
            var outputString = formatterProcess.StandardOutput.ReadToEnd();
            formatterProcess.WaitForExit();
            if (formatterProcess.ExitCode != 0)
                throw new Exception("Formatter reported error: " + formatterProcess.StandardError.ReadToEnd());
            Assert.AreEqual(expectedOutputString + outputSuffix, outputString, "Output did not match expected");
        }
    }
}
