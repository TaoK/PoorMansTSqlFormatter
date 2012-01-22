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
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace PoorMansTSqlFormatterTests
{
    static class Utils
    {
        public const string DATAFOLDER = "Data";
        public const string INPUTSQLFOLDER = "InputSql";
        public const string PARSEDSQLFOLDER = "ParsedSql";
        public const string STANDARDFORMATSQLFOLDER = "StandardFormatSql";

        public const string INVALID_SQL_WARNING = "THIS TEST FILE IS NOT VALID SQL";
        public const string REFORMATTING_INCONSISTENCY_WARNING = "KNOWN SQL REFORMATTING INCONSISTENCY";

        public static string GetTestContentFolder(string folderName)
        {
            DirectoryInfo thisDirectory = new DirectoryInfo(".");
            return Path.Combine(Path.Combine(thisDirectory.Parent.Parent.FullName, DATAFOLDER), folderName);
        }

        public static IEnumerable<string> FolderFileNameIterator(string path)
        {
            DirectoryInfo textFileFolder = new DirectoryInfo(path);
            foreach (FileInfo sampleFile in textFileFolder.GetFiles())
            {
                yield return sampleFile.Name;
            }
        }

        public static void StripWhiteSpaceFromSqlTree(XmlDocument sqlTree)
        {
            XmlNodeList deletionCandidates = sqlTree.SelectNodes(string.Format("//*[local-name() = '{0}']", PoorMansTSqlFormatterLib.Interfaces.SqlXmlConstants.ENAME_WHITESPACE));
            foreach (XmlElement deletionCandidate in deletionCandidates)
                deletionCandidate.ParentNode.RemoveChild(deletionCandidate);
        }

        public static IEnumerable<string> GetInputSqlFileNames()
        {
            return FolderFileNameIterator(GetTestContentFolder("InputSql"));
        }

        public static string GetTestFileContent(string FileName, string TestFolder)
        {
            return File.ReadAllText(Path.Combine(Utils.GetTestContentFolder(TestFolder), FileName));
        }
    }
}
