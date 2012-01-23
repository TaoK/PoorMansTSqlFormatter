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

using System.Configuration;

namespace PoorMansTSqlFormatterPluginShared
{
    public interface ISqlSettings
    {
        SettingsPropertyCollection Properties { get; }

        bool ExpandCommaLists { get; set; }
        bool TrailingCommas { get; set; }
        bool ExpandBooleanExpressions { get; set; }
        bool ExpandCaseStatements { get; set; }
        bool ExpandBetweenConditions { get; set; }
        bool UppercaseKeywords { get; set; }
        string IndentString { get; set; }
        bool SpaceAfterExpandedComma { get; set; }
        int SpacesPerTab { get; set; }
        int MaxLineWidth { get; set; }
        bool KeywordStandardization { get; set; }
        bool BreakJoinOnSections { get; set; }

        object this[string propertyName] { get; set; }
        void Save();
        void Reset();
    }
}
