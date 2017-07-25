/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

Additional Contributors:
 * Timothy Klenke, 2012

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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

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
        public const string ERROR_FOUND_WARNING = "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!\r\n";

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

        public static void StripWhiteSpaceFromSqlTree(Node sqlTree)
        {
            StripElementNamesFromXml(sqlTree, new[] { SqlStructureConstants.ENAME_WHITESPACE });
        }

        public static void StripCommentsFromSqlTree(Node sqlTree)
        {
            StripElementNamesFromXml(sqlTree, SqlStructureConstants.ENAMELIST_COMMENT);
        }

        private static void StripElementNamesFromXml(Node sqlTree, IEnumerable<string> elementNames)
        {
            var toRemove = sqlTree.ChildrenByNames(elementNames).ToList();
            foreach (Node childThing in toRemove)
                sqlTree.RemoveChild(childThing);

            foreach (Node childThing in sqlTree.Children)
                StripElementNamesFromXml(childThing, elementNames);
        }

        public static IEnumerable<string> GetInputSqlFileNames()
        {
            return FolderFileNameIterator(GetTestContentFolder("InputSql"));
        }

        public static string GetTestFileContent(string fileName, string testFolderPath)
        {
            return File.ReadAllText(Path.Combine(Utils.GetTestContentFolder(testFolderPath), fileName));
        }

        public static string StripFileConfigString(string fileName)
        {
            int openParens = fileName.IndexOf("(");
            if (openParens >= 0)
            {
                int closeParens = fileName.IndexOf(")", openParens);
                if (closeParens >= 0)
                {
                    return fileName.Substring(0, openParens) + fileName.Substring(closeParens + 1);
                }
                return fileName;
            }
            return fileName;
        }

        public static string GetFileConfigString(string fileName)
        {
            int openParens = fileName.IndexOf("(");
            if (openParens >= 0)
            {
                int closeParens = fileName.IndexOf(")", openParens);
                if (closeParens >= 0)
                {
                    return fileName.Substring(openParens + 1, (closeParens - openParens) - 1);
                }
                return "";
            }
            return "";
        }
    }
}
