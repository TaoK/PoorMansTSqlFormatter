/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011 Tao Klerks

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
using System.Reflection;
using NDesk.Options;

namespace PoorMansTSqlFormatterCmdLine
{
    class Program
    {
        static void Main(string[] args)
        {
            string indentString = "\t";
            bool trailingCommas = false;
            bool expandBetweenConditions = true;
            bool expandBooleanExpressions = true;
            bool expandCaseStatements = true;
            bool expandCommaLists = true;
            bool uppercaseKeywords = true;

            bool showUsage = false;
            List<string> extensions = new List<string>();
            bool backups = true;
            bool recursiveSearch = false;

            OptionSet p = new OptionSet()
              .Add("is|indentString=", delegate(string v) { indentString = v; })
              .Add("tc|trailingCommas", delegate(string v) { trailingCommas = v != null; })
              .Add("ebc|expandBetweenConditions", delegate(string v) { expandBetweenConditions = v != null; })
              .Add("ebe|expandBooleanExpressions", delegate(string v) { expandBooleanExpressions = v != null; })
              .Add("ecs|expandCaseStatements", delegate(string v) { expandCaseStatements = v != null; })
              .Add("ecl|expandCommaLists", delegate(string v) { expandCommaLists = v != null; })
              .Add("uk|uppercaseKeywords", delegate(string v) { uppercaseKeywords = v != null; })
              .Add("e|extensions=", delegate(string v) { extensions.Add((v.StartsWith(".") ? "" : ".") + v); })
              .Add("r|recursive", delegate(string v) { recursiveSearch = v != null; })
              .Add("b|backups", delegate(string v) { backups = v != null; })
              .Add("h|?|help", delegate(string v) { showUsage = v != null; })
                  ;

            List<string> remainingArgs = p.Parse(args);
            if (remainingArgs.Count != 1)
            {
                showUsage = true;
                Console.WriteLine("Unrecognized arguments found!");
            }

            if (extensions.Count == 0)
                extensions.Add(".sql");

            if (showUsage)
            {
                Console.WriteLine(@"
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. Distributed under AGPL v3.
Copyright (C) 2011 Tao Klerks");
                Console.WriteLine("v" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                Console.WriteLine(@"
Usage notes: 

SqlFormatter <filename or pattern> <options>

is  indentString (default: \t)
tc  trailingCommas (default: false)
ebc expandBetweenConditions (default: true)
ebe expandBooleanExpressions (default: true)
ecs expandCaseStatements (default: true)
ecl expandCommaLists (default: true)
uk  uppercaseKeywords (default: true)
e   extensions (default: sql)
r   recursive (default: false)
b   backups (default: true)
h ? help

Disable boolean options with a trailing minus, enable by just specifying them or with a trailing plus.

eg:

SqlFormatter TestFiles\* /is:""  "" /tc /uc- 

");
                return;
            }

            var formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(indentString, expandCommaLists, trailingCommas, expandBooleanExpressions, expandCaseStatements, uppercaseKeywords, false);
            var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);

            string baseDirectoryName = System.IO.Path.GetDirectoryName(remainingArgs[0]);
            if (baseDirectoryName.Length == 0)
                baseDirectoryName = ".";
            string searchPattern = System.IO.Path.GetFileName(remainingArgs[0]);
            System.IO.DirectoryInfo baseDirectory = null;
            System.IO.FileSystemInfo[] matchingObjects = null;
            try
            {
                baseDirectory = new System.IO.DirectoryInfo(baseDirectoryName);
                if (recursiveSearch)
                    matchingObjects = baseDirectory.GetFileSystemInfos(searchPattern);
                else
                    matchingObjects = baseDirectory.GetFiles(searchPattern);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error processing requested filename/pattern. Error detail: " + e.Message);
                return;
            }

            if (!ProcessSearchResults(extensions, backups, formattingManager, matchingObjects))
            {
                Console.WriteLine("No files found matching filename/pattern: " + remainingArgs[0]);
                return;
            }
        }

        private static bool ProcessSearchResults(List<string> extensions, bool backups, PoorMansTSqlFormatterLib.SqlFormattingManager formattingManager, System.IO.FileSystemInfo[] matchingObjects)
        {
            bool fileFound = false;

            foreach (var fsEntry in matchingObjects)
            {
                if (fsEntry is System.IO.FileInfo)
                {
                    if (extensions.Contains(fsEntry.Extension))
                    {
                        ReFormatFile((System.IO.FileInfo)fsEntry, formattingManager, backups);
                        fileFound = true;
                    }
                }
                else
                {
                    if (ProcessSearchResults(extensions, backups, formattingManager, ((System.IO.DirectoryInfo)fsEntry).GetFileSystemInfos()))
                        fileFound = true;
                }
            }

            return fileFound;
        }

        private static void ReFormatFile(System.IO.FileInfo fileInfo, PoorMansTSqlFormatterLib.SqlFormattingManager formattingManager, bool backups)
        {
            bool failedBackup = false;
            string oldFileContents = "";
            string newFileContents = "";
            bool parsingError = false;
            Exception parseErrorDetail = null;

            //TODO: play with / test encoding complexities
            //TODO: consider using auto-detection - read binary, autodetect, convert.
            //TODO: consider whether to keep same output encoding as source file, or always use same, and if so whether to make parameter-based.
            try
            {
                oldFileContents = System.IO.File.ReadAllText(fileInfo.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read file contents (aborted): " + fileInfo.FullName);
                Console.WriteLine(" Error detail: " + ex.Message);
            }
            if (oldFileContents.Length > 0)
            {
                try
                {
                    newFileContents = formattingManager.Format(oldFileContents, ref parsingError);
                }
                catch (Exception ex)
                {
                    parsingError = true;
                    parseErrorDetail = ex;
                }

                if (parsingError)
                {
                    Console.WriteLine("Encountered error when parsing or formatting file contents (aborted): " + fileInfo.FullName);
                    if (parseErrorDetail != null)
                        Console.WriteLine(" Error detail: " + parseErrorDetail.Message);
                }
            }
            if (newFileContents.Length > 0 && !parsingError && !oldFileContents.Equals(newFileContents))
            {
                if (backups)
                {
                    try
                    {
                        fileInfo.CopyTo(fileInfo.FullName + ".bak", true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to back up file: " + fileInfo.FullName);
                        Console.WriteLine(" Skipping formatting for this file.");
                        Console.WriteLine(" Error detail: " + ex.Message);
                        failedBackup = true;
                    }
                }
                if (!failedBackup)
                {
                    try
                    {
                        System.IO.File.WriteAllText(fileInfo.FullName, newFileContents);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to write reformatted contents: " + fileInfo.FullName);
                        Console.WriteLine(" Error detail: " + ex.Message);
                        Console.WriteLine(" NOTE: this file may have been overwritten with partial content!");
                    }
                }
            }
        }
    }
}
