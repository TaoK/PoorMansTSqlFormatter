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

using System.Reflection;
using System.Resources;
using PoorMansTSqlFormatterLib;

namespace PoorMansTSqlFormatterPluginShared
{
    public static class Utils
    {
        public static PoorMansTSqlFormatterLib.SqlFormattingManager GetFormattingManager(ISqlSettings settings)
        {
            var options = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions();
            options.IndentString = settings.IndentString;
            options.SpacesPerTab = settings.SpacesPerTab;
            options.MaxLineWidth = settings.MaxLineWidth;
            options.ExpandCommaLists = settings.ExpandCommaLists;
            options.TrailingCommas = settings.TrailingCommas;
            options.SpaceAfterExpandedComma = settings.SpaceAfterExpandedComma;
            options.ExpandBooleanExpressions = settings.ExpandBooleanExpressions;
            options.ExpandCaseStatements = settings.ExpandCaseStatements;
            options.ExpandBetweenConditions = settings.ExpandBetweenConditions;
            options.BreakJoinOnSections = settings.BreakJoinOnSections;
            options.UppercaseKeywords = settings.UppercaseKeywords;
            options.KeywordStandardization = settings.KeywordStandardization;
            
            var formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(options);

            ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterPluginShared.GeneralLanguageContent", Assembly.GetExecutingAssembly());
            formatter.ErrorOutputPrefix = _generalResourceManager.GetString("ParseErrorWarningPrefix") + System.Environment.NewLine;
            var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);
            return formattingManager;
        }
    }
}
