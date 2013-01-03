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
using System.Linq;

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

        public TSqlStandardFormatterOptions(string serializedString) : this() {

            if (string.IsNullOrEmpty(serializedString)) return;
                        
            foreach (string kvp in serializedString.Split(','))
            {
                string[] splitPair = kvp.Split('=');
                string key = splitPair[0];
                string value = splitPair[1];

                if (key == "IndentString") IndentString = value;
                else if (key == "SpacesPerTab") SpacesPerTab = Convert.ToInt32(value);
                else if (key == "MaxLineWidth") MaxLineWidth = Convert.ToInt32(value);
                else if (key == "ExpandCommaLists") ExpandCommaLists = Convert.ToBoolean(value);
                else if (key == "TrailingCommas") TrailingCommas = Convert.ToBoolean(value);
                else if (key == "SpaceAfterExpandedComma") SpaceAfterExpandedComma = Convert.ToBoolean(value);
                else if (key == "ExpandBooleanExpressions") ExpandBooleanExpressions = Convert.ToBoolean(value);
                else if (key == "ExpandBetweenConditions") ExpandBetweenConditions = Convert.ToBoolean(value);
                else if (key == "ExpandCaseStatements") ExpandCaseStatements = Convert.ToBoolean(value);
                else if (key == "UppercaseKeywords") UppercaseKeywords = Convert.ToBoolean(value);
                else if (key == "BreakJoinOnSections") BreakJoinOnSections = Convert.ToBoolean(value);
                else if (key == "HTMLColoring") HTMLColoring = Convert.ToBoolean(value);
                else if (key == "KeywordStandardization") KeywordStandardization = Convert.ToBoolean(value);
                else throw new ArgumentException("Unknown option: " + key);
            }

        }

        public string ToSerializedString() { 
            var overrides = new Dictionary<string, string>();

            var defaultOptions = new TSqlStandardFormatterOptions();

            if (IndentString != defaultOptions.IndentString) overrides.Add("IndentString", IndentString);
            if (SpacesPerTab != defaultOptions.SpacesPerTab) overrides.Add("SpacesPerTab", SpacesPerTab.ToString());
            if (MaxLineWidth != defaultOptions.MaxLineWidth) overrides.Add("MaxLineWidth", MaxLineWidth.ToString());
            if (ExpandCommaLists != defaultOptions.ExpandCommaLists) overrides.Add("ExpandCommaLists", ExpandCommaLists.ToString());
            if (TrailingCommas != defaultOptions.TrailingCommas) overrides.Add("TrailingCommas", TrailingCommas.ToString());
            if (SpaceAfterExpandedComma != defaultOptions.SpaceAfterExpandedComma) overrides.Add("SpaceAfterExpandedComma", SpaceAfterExpandedComma.ToString());
            if (ExpandBooleanExpressions != defaultOptions.ExpandBooleanExpressions) overrides.Add("ExpandBooleanExpressions", ExpandBooleanExpressions.ToString());
            if (ExpandBetweenConditions != defaultOptions.ExpandBetweenConditions) overrides.Add("ExpandBetweenConditions", ExpandBetweenConditions.ToString());
            if (ExpandCaseStatements != defaultOptions.ExpandCaseStatements) overrides.Add("ExpandCaseStatements", ExpandCaseStatements.ToString());
            if (UppercaseKeywords != defaultOptions.UppercaseKeywords) overrides.Add("UppercaseKeywords", UppercaseKeywords.ToString());
            if (BreakJoinOnSections != defaultOptions.BreakJoinOnSections) overrides.Add("BreakJoinOnSections", BreakJoinOnSections.ToString());
            if (HTMLColoring != defaultOptions.HTMLColoring) overrides.Add("HTMLColoring", HTMLColoring.ToString());
            if (KeywordStandardization != defaultOptions.KeywordStandardization) overrides.Add("KeywordStandardization", KeywordStandardization.ToString());
    
            if (overrides.Count == 0) return string.Empty;
            return string.Join(",", overrides.Select((kvp) => kvp.Key + "=" + kvp.Value).ToArray());
           
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
