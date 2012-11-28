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
using System.Text.RegularExpressions;
using System.Xml;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlStandardFormatterOptions
    {
        public TSqlStandardFormatterOptions()
        {
            IndentString = "\t";
            SpacesPerTab = 4;
            MaxLineWidth = 999;
            ExpandCommaLists = true;
            TrailingCommas = false;
            SpaceAfterExpandedComma = false;
            ExpandBooleanExpressions = true;
            ExpandBetweenConditions = true;
            ExpandCaseStatements = true;
            UppercaseKeywords = true;
            BreakJoinOnSections = false;
            HTMLColoring = false;
            KeywordStandardization = false;
        }

        private string _indentString;
        public string IndentString
        {
            get
            {
                return _indentString;
            }
            set
            {
                _indentString = value.Replace("\\t", "\t").Replace("\\s", " ");
            }
        }

        public int SpacesPerTab { get; set; }
        public int MaxLineWidth { get; set; }
        public bool ExpandCommaLists { get; set; }
        public bool TrailingCommas { get; set; }
        public bool SpaceAfterExpandedComma { get; set; }
        public bool ExpandBooleanExpressions { get; set; }
        public bool ExpandCaseStatements { get; set; }
        public bool ExpandBetweenConditions { get; set; }
        public bool UppercaseKeywords { get; set; }
        public bool BreakJoinOnSections { get; set; }
        public bool HTMLColoring { get; set; }
        public bool KeywordStandardization { get; set; }

    }
}
