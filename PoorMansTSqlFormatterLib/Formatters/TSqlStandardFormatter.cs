/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters
{

    public class TSqlStandardFormatter : Interfaces.ISqlTreeFormatter
    {

        public TSqlStandardFormatter() : this(new TSqlStandardFormatterOptions()) { }
        
        public TSqlStandardFormatter(TSqlStandardFormatterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            Options = options;

            if (options.KeywordStandardization)
                KeywordMapping = StandardKeywordRemapping.Instance;
            ErrorOutputPrefix = Interfaces.MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;
        }

        [Obsolete("Use the constructor with the TSqlStandardFormatterOptions parameter")]
        public TSqlStandardFormatter(string indentString, int spacesPerTab, int maxLineWidth, bool expandCommaLists, bool trailingCommas, bool spaceAfterExpandedComma, bool expandBooleanExpressions, bool expandCaseStatements, bool expandBetweenConditions, bool breakJoinOnSections, bool uppercaseKeywords, bool htmlColoring, bool keywordStandardization)
        {
            Options = new TSqlStandardFormatterOptions
                {
                    IndentString = indentString,
                    SpacesPerTab = spacesPerTab,
                    MaxLineWidth = maxLineWidth,
                    ExpandCommaLists = expandCommaLists,
                    TrailingCommas = trailingCommas,
                    SpaceAfterExpandedComma = spaceAfterExpandedComma,
                    ExpandBooleanExpressions = expandBooleanExpressions,
                    ExpandBetweenConditions = expandBetweenConditions,
                    ExpandCaseStatements = expandCaseStatements,
                    UppercaseKeywords = uppercaseKeywords,
                    BreakJoinOnSections = breakJoinOnSections,
                    HTMLColoring = htmlColoring,
                    KeywordStandardization = keywordStandardization
                };

            if (keywordStandardization)
                KeywordMapping = StandardKeywordRemapping.Instance;
            ErrorOutputPrefix = Interfaces.MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;
        }
        
        public TSqlStandardFormatterOptions Options { get; private set; }
        
        public IDictionary<string, string> KeywordMapping = new Dictionary<string, string>();

        [Obsolete("Use Options.IndentString instead")]
        public string IndentString { get { return Options.IndentString; } set { Options.IndentString = value; } }
        [Obsolete("Use Options.SpacesPerTab instead")]
        public int SpacesPerTab { get { return Options.SpacesPerTab; } set { Options.SpacesPerTab = value; } }
        [Obsolete("Use Options.MaxLineWidth instead")]
        public int MaxLineWidth { get { return Options.MaxLineWidth; } set { Options.MaxLineWidth = value; } }
        [Obsolete("Use Options.ExpandCommaLists instead")]
        public bool ExpandCommaLists { get { return Options.ExpandCommaLists; } set { Options.ExpandCommaLists = value; } }
        [Obsolete("Use Options.TrailingCommas instead")]
        public bool TrailingCommas { get { return Options.TrailingCommas; } set { Options.TrailingCommas = value; } }
        [Obsolete("Use Options.SpaceAfterExpandedComma instead")]
        public bool SpaceAfterExpandedComma { get { return Options.SpaceAfterExpandedComma; } set { Options.SpaceAfterExpandedComma = value; } }
        [Obsolete("Use Options.ExpandBooleanExpressions instead")]
        public bool ExpandBooleanExpressions { get { return Options.ExpandBooleanExpressions; } set { Options.ExpandBooleanExpressions = value; } }
        [Obsolete("Use Options.ExpandBetweenConditions instead")]
        public bool ExpandCaseStatements { get { return Options.ExpandCaseStatements; } set { Options.ExpandCaseStatements = value; } }
        [Obsolete("Use Options.ExpandCaseStatements instead")]
        public bool ExpandBetweenConditions { get { return Options.ExpandBetweenConditions; } set { Options.ExpandBetweenConditions = value; } }
        [Obsolete("Use Options.UppercaseKeywords instead")]
        public bool UppercaseKeywords { get { return Options.UppercaseKeywords; } set { Options.UppercaseKeywords = value; } }
        [Obsolete("Use Options.BreakJoinOnSections instead")]
        public bool BreakJoinOnSections { get { return Options.BreakJoinOnSections; } set { Options.BreakJoinOnSections = value; } }
        [Obsolete("Use Options.HTMLColoring instead")]
        public bool HTMLColoring { get { return Options.HTMLColoring; } set { Options.HTMLColoring = value; } }

        public bool HTMLFormatted { get { return Options.HTMLColoring; } }
        public string ErrorOutputPrefix { get; set; }

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            //thread-safe - each call to FormatSQLTree() gets its own independent state object
            TSqlStandardFormattingState state = new TSqlStandardFormattingState(Options.HTMLColoring, Options.IndentString, Options.SpacesPerTab, Options.MaxLineWidth, 0);

            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", SqlXmlConstants.ENAME_SQL_ROOT, SqlXmlConstants.ANAME_ERRORFOUND)) != null)
                state.AddOutputContent(ErrorOutputPrefix);

            XmlNodeList rootList = sqlTreeDoc.SelectNodes(string.Format("/{0}/*", SqlXmlConstants.ENAME_SQL_ROOT));
            ProcessSqlNodeList(rootList, state);
            WhiteSpace_BreakAsExpected(state);

            if (state.SpecialRegionActive == SpecialRegionType.NoFormat)
            {
                XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, sqlTreeDoc.DocumentElement);
                TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter(Options.HTMLColoring);
                state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
            }
            else if (state.SpecialRegionActive == SpecialRegionType.Minify)
            {
                XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, sqlTreeDoc.DocumentElement);
                TSqlObfuscatingFormatter tempFormatter = new TSqlObfuscatingFormatter();
                if (HTMLFormatted)
                    state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter.FormatSQLTree(skippedXml)));
                else
                    state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
            }
            return state.DumpOutput();
        }

        private void ProcessSqlNodeList(XmlNodeList rootList, TSqlStandardFormattingState state)
        {
            foreach (XmlElement contentElement in rootList)
                ProcessSqlNode(contentElement, state);
        }

        private void ProcessSqlNode(XmlElement contentElement, TSqlStandardFormattingState state)
        {
            int initialIndent = state.IndentLevel;

            if (contentElement.GetAttribute(SqlXmlConstants.ANAME_HASERROR) == "1")
                state.OpenClass(SqlHtmlConstants.CLASS_ERRORHIGHLIGHT);

            switch (contentElement.Name)
            {
                case SqlXmlConstants.ENAME_SQL_STATEMENT:
                    WhiteSpace_SeparateStatements(contentElement, state);
                    state.ResetKeywords();
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.StatementBreakExpected = true;
                    break;

                case SqlXmlConstants.ENAME_SQL_CLAUSE:
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.DecrementIndent();
					if (Options.NewClauseLineBreaks > 0)
	                    state.BreakExpected = true;
					if (Options.NewClauseLineBreaks > 1)
						state.AdditionalBreaksExpected = Options.NewClauseLineBreaks - 1;
                    break;

                case SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE:
                    state.DecrementIndent();
                    state.WhiteSpace_BreakToNextLine(); //this is the one already recommended by the start of the clause
                    state.WhiteSpace_BreakToNextLine(); //this is the one we additionally want to apply
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.BreakExpected = true;
                    state.AdditionalBreaksExpected = 1;
                    break;

                case SqlXmlConstants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    state.WhiteSpace_BreakToNextLine();
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.BreakExpected = true;
                    break;

                case SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case SqlXmlConstants.ENAME_DDL_OTHER_BLOCK:
                case SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_DECLARATION:
                case SqlXmlConstants.ENAME_BEGIN_TRANSACTION:
                case SqlXmlConstants.ENAME_SAVE_TRANSACTION:
                case SqlXmlConstants.ENAME_COMMIT_TRANSACTION:
                case SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION:
                case SqlXmlConstants.ENAME_CONTAINER_OPEN:
                case SqlXmlConstants.ENAME_CONTAINER_CLOSE:
                case SqlXmlConstants.ENAME_WHILE_LOOP:
                case SqlXmlConstants.ENAME_IF_STATEMENT:
                case SqlXmlConstants.ENAME_SELECTIONTARGET:
                case SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT:
                case SqlXmlConstants.ENAME_CTE_WITH_CLAUSE:
                case SqlXmlConstants.ENAME_PERMISSIONS_BLOCK:
                case SqlXmlConstants.ENAME_PERMISSIONS_DETAIL:
                case SqlXmlConstants.ENAME_MERGE_CLAUSE:
                case SqlXmlConstants.ENAME_MERGE_TARGET:
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_CASE_INPUT:
                case SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION:
                case SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND:
                case SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND:
                    WhiteSpace_SeparateWords(state);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT:
                case SqlXmlConstants.ENAME_MERGE_ACTION:

					bool singleStatementIsIf = contentElement.SelectSingleNode(SqlXmlConstants.ENAME_SQL_STATEMENT + "/" + SqlXmlConstants.ENAME_SQL_CLAUSE + "/" + SqlXmlConstants.ENAME_IF_STATEMENT) != null;

					if (singleStatementIsIf && contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_ELSE_CLAUSE))
					{
						//artificially decrement indent and skip new statement break for "ELSE IF" constructs
						state.DecrementIndent();
					}
					else
					{
						state.BreakExpected = true;
					}
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
					if (singleStatementIsIf && contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_ELSE_CLAUSE))
					{
						//bring indent bak to symmetrical level
						state.IncrementIndent();
					}

					state.StatementBreakExpected = false; //the responsibility for breaking will be with the OUTER statement; there should be no consequence propagating out from statements in this container;
                    state.UnIndentInitialBreak = false; //if there was no word spacing after the last content statement's clause starter, doesn't mean the unIndent should propagate to the following content!
                    break;

                case SqlXmlConstants.ENAME_PERMISSIONS_TARGET:
                case SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT:
                case SqlXmlConstants.ENAME_DDL_WITH_CLAUSE:
                case SqlXmlConstants.ENAME_MERGE_CONDITION:
                case SqlXmlConstants.ENAME_MERGE_THEN:
                    state.BreakExpected = true;
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.DecrementIndent();
                    break;

                case SqlXmlConstants.ENAME_JOIN_ON_SECTION:
                    if (Options.BreakJoinOnSections)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    if (Options.BreakJoinOnSections)
                        state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state);
                    if (Options.BreakJoinOnSections)
                        state.DecrementIndent();
                    break;

                case SqlXmlConstants.ENAME_CTE_ALIAS:
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_ELSE_CLAUSE:
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT), state.IncrementIndent());
                    break;

                case SqlXmlConstants.ENAME_DDL_AS_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state);
                    state.IncrementIndent();
                    break;

                case SqlXmlConstants.ENAME_TRIGGER_CONDITION:
                    state.DecrementIndent();
                    state.WhiteSpace_BreakToNextLine();
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    break;

                case SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS:
                case SqlXmlConstants.ENAME_CTE_AS_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    break;

                case SqlXmlConstants.ENAME_DDL_RETURNS:
                case SqlXmlConstants.ENAME_MERGE_USING:
                case SqlXmlConstants.ENAME_MERGE_WHEN:
                    state.BreakExpected = true;
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_BETWEEN_CONDITION:
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND), state.IncrementIndent());
                    if (Options.ExpandBetweenConditions)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_CLOSE), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND), state.IncrementIndent());
                    state.DecrementIndent();
                    state.DecrementIndent();
                    break;

                case SqlXmlConstants.ENAME_DDLDETAIL_PARENS:
                case SqlXmlConstants.ENAME_FUNCTION_PARENS:
					//simply process sub-nodes - don't add space or expect any linebreaks (but respect linebreaks if necessary)
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator("("), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.DecrementIndent();
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator(")"), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_DDL_PARENS:
                case SqlXmlConstants.ENAME_EXPRESSION_PARENS:
                case SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS:
				case SqlXmlConstants.ENAME_IN_PARENS:
					WhiteSpace_SeparateWords(state);
					if (contentElement.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS) || contentElement.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS))
                        state.IncrementIndent();
                    state.AddOutputContent(FormatOperator("("), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    TSqlStandardFormattingState innerState = new TSqlStandardFormattingState(state);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), innerState);
                    //if there was a linebreak in the parens content, or if it wanted one to follow, then put linebreaks before and after.
                    if (innerState.BreakExpected || innerState.OutputContainsLineBreak)
                    {
                        if (!innerState.StartsWithBreak)
                            state.WhiteSpace_BreakToNextLine();
                        state.Assimilate(innerState);
                        state.WhiteSpace_BreakToNextLine();
                    }
                    else
                    {
                        state.Assimilate(innerState);
                    }
                    state.AddOutputContent(FormatOperator(")"), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    if (contentElement.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS) || contentElement.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS))
                        state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_BEGIN_END_BLOCK:
                case SqlXmlConstants.ENAME_TRY_BLOCK:
                case SqlXmlConstants.ENAME_CATCH_BLOCK:
                    if (contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && contentElement.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && contentElement.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                        state.DecrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT), state);
                    state.DecrementIndent();
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_CLOSE), state);
                    state.IncrementIndent();
                    if (contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && contentElement.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && contentElement.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                        state.IncrementIndent();
                    break;

                case SqlXmlConstants.ENAME_CASE_STATEMENT:
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CASE_INPUT), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CASE_WHEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CASE_ELSE), state);
                    if (Options.ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_CLOSE), state);
                    state.DecrementIndent();
                    break;

                case SqlXmlConstants.ENAME_CASE_WHEN:
                case SqlXmlConstants.ENAME_CASE_THEN:
                case SqlXmlConstants.ENAME_CASE_ELSE:
                    if (Options.ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_CASE_THEN), state);
                    state.DecrementIndent();
                    break;

                case SqlXmlConstants.ENAME_AND_OPERATOR:
                case SqlXmlConstants.ENAME_OR_OPERATOR:
                    if (Options.ExpandBooleanExpressions)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_COMMENT_MULTILINE:
                    if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.InnerXml.ToUpperInvariant().Contains("[/NOFORMAT]"))
                    {
                        XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, contentElement);
                        TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter(Options.HTMLColoring);
                        state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }
                    else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.InnerXml.ToUpperInvariant().Contains("[/MINIFY]"))
                    {
                        XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, contentElement);
                        TSqlObfuscatingFormatter tempFormatter = new TSqlObfuscatingFormatter();
                        if (HTMLFormatted)
                            state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter.FormatSQLTree(skippedXml)));
                        else
                            state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }

                    WhiteSpace_SeparateComment(contentElement, state);
                    state.AddOutputContent("/*" + contentElement.InnerText + "*/", Interfaces.SqlHtmlConstants.CLASS_COMMENT);
                    if (contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                        || (contentElement.NextSibling != null
                            && contentElement.NextSibling.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                            && Regex.IsMatch(contentElement.NextSibling.InnerText, @"(\r|\n)+")
                            )
                        )
                        //if this block comment is at the start or end of a statement, or if it was followed by a 
                        // linebreak before any following content, then break here.
                        state.BreakExpected = true;
                    else
                    {
                        state.WordSeparatorExpected = true;
                    }

                    if (state.SpecialRegionActive == null && contentElement.InnerXml.ToUpperInvariant().Contains("[NOFORMAT]"))
                    {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.NoFormat;
                        state.RegionStartNode = contentElement;
                    }
                    else if (state.SpecialRegionActive == null && contentElement.InnerXml.ToUpperInvariant().Contains("[MINIFY]"))
                    {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.Minify;
                        state.RegionStartNode = contentElement;
                    }
                    break;

                case SqlXmlConstants.ENAME_COMMENT_SINGLELINE:
                case SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE:
                    if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.InnerXml.ToUpperInvariant().Contains("[/NOFORMAT]"))
                    {
                        XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, contentElement);
                        TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter(Options.HTMLColoring);
                        state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }
                    else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.InnerXml.ToUpperInvariant().Contains("[/MINIFY]"))
                    {
                        XmlNode skippedXml = ExtractXmlBetween(state.RegionStartNode, contentElement);
                        TSqlObfuscatingFormatter tempFormatter = new TSqlObfuscatingFormatter();
                        if (HTMLFormatted)
                            state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter.FormatSQLTree(skippedXml)));
                        else
                            state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }

                    WhiteSpace_SeparateComment(contentElement, state);
                    state.AddOutputContent((contentElement.Name == SqlXmlConstants.ENAME_COMMENT_SINGLELINE ? "--" : "//") + contentElement.InnerText.Replace("\r", "").Replace("\n", ""), Interfaces.SqlHtmlConstants.CLASS_COMMENT);
                    state.BreakExpected = true;
                    state.SourceBreakPending = true;

                    if (state.SpecialRegionActive == null && contentElement.InnerXml.ToUpperInvariant().Contains("[NOFORMAT]"))
                    {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.NoFormat;
                        state.RegionStartNode = contentElement;
                    }
                    else if (state.SpecialRegionActive == null && contentElement.InnerXml.ToUpperInvariant().Contains("[MINIFY]"))
                    {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.Minify;
                        state.RegionStartNode = contentElement;
                    }
                    break;

                case SqlXmlConstants.ENAME_STRING:
                case SqlXmlConstants.ENAME_NSTRING:
                    WhiteSpace_SeparateWords(state);
                    string outValue = null;
                    if (contentElement.Name.Equals(SqlXmlConstants.ENAME_NSTRING))
                        outValue = "N'" + contentElement.InnerText.Replace("'", "''") + "'";
                    else
                        outValue = "'" + contentElement.InnerText.Replace("'", "''") + "'";
                    state.AddOutputContent(outValue, Interfaces.SqlHtmlConstants.CLASS_STRING);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("[" + contentElement.InnerText.Replace("]", "]]") + "]");
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_QUOTED_STRING:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("\"" + contentElement.InnerText.Replace("\"", "\"\"") + "\"");
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_COMMA:
                    //comma always ignores requested word spacing
                    if (Options.TrailingCommas)
                    {
                        WhiteSpace_BreakAsExpected(state);
                        state.AddOutputContent(FormatOperator(","), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);

                        if ((Options.ExpandCommaLists
								&& !(contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
									|| contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_FUNCTION_PARENS)
									|| contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
									)
								)
							|| (Options.ExpandInLists
								&& contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
								)
							)
                            state.BreakExpected = true;
                        else
                            state.WordSeparatorExpected = true;
                    }
                    else
                    {
                        if ((Options.ExpandCommaLists
								&& !(contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
									|| contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_FUNCTION_PARENS)
									|| contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
									)
								)
							|| (Options.ExpandInLists
								&& contentElement.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
								)
							)
                        {
                            state.WhiteSpace_BreakToNextLine();
                            state.AddOutputContent(FormatOperator(","), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                            if (Options.SpaceAfterExpandedComma)
                                state.WordSeparatorExpected = true;
                        }
                        else
                        {
                            WhiteSpace_BreakAsExpected(state);
                            state.AddOutputContent(FormatOperator(","), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                            state.WordSeparatorExpected = true;
                        }

                    }
                    break;

                case SqlXmlConstants.ENAME_PERIOD:
                case SqlXmlConstants.ENAME_SEMICOLON:
                case SqlXmlConstants.ENAME_SCOPERESOLUTIONOPERATOR:
                    //always ignores requested word spacing, and doesn't request a following space either.
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator(contentElement.InnerText), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    break;

                case SqlXmlConstants.ENAME_ASTERISK:
                case SqlXmlConstants.ENAME_EQUALSSIGN:
                case SqlXmlConstants.ENAME_ALPHAOPERATOR:
                case SqlXmlConstants.ENAME_OTHEROPERATOR:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(FormatOperator(contentElement.InnerText), Interfaces.SqlHtmlConstants.CLASS_OPERATOR);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_COMPOUNDKEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.Attributes[SqlXmlConstants.ANAME_SIMPLETEXT].Value);
                    state.AddOutputContent(FormatKeyword(contentElement.Attributes[SqlXmlConstants.ANAME_SIMPLETEXT].Value), Interfaces.SqlHtmlConstants.CLASS_KEYWORD);
                    state.WordSeparatorExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(SqlXmlConstants.ENAME_COMMENT_MULTILINE + " | " + SqlXmlConstants.ENAME_COMMENT_SINGLELINE + " | " + SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE), state.IncrementIndent());
                    state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_OTHERKEYWORD:
                case SqlXmlConstants.ENAME_DATATYPE_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.InnerText);
                    state.AddOutputContent(FormatKeyword(contentElement.InnerText), Interfaces.SqlHtmlConstants.CLASS_KEYWORD);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_PSEUDONAME:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(FormatKeyword(contentElement.InnerText), Interfaces.SqlHtmlConstants.CLASS_KEYWORD);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_FUNCTION_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.InnerText);
                    state.AddOutputContent(contentElement.InnerText, Interfaces.SqlHtmlConstants.CLASS_FUNCTION);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_OTHERNODE:
                case SqlXmlConstants.ENAME_MONETARY_VALUE:
                case SqlXmlConstants.ENAME_LABEL:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(contentElement.InnerText);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_NUMBER_VALUE:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(contentElement.InnerText.ToLowerInvariant());
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_BINARY_VALUE:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("0x");
                    state.AddOutputContent(contentElement.InnerText.Substring(2).ToUpperInvariant());
                    state.WordSeparatorExpected = true;
                    break;

                case SqlXmlConstants.ENAME_WHITESPACE:
                    //take note if it's a line-breaking space, but don't DO anything here
                    if (Regex.IsMatch(contentElement.InnerText, @"(\r|\n)+"))
                        state.SourceBreakPending = true;
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }

            if (contentElement.GetAttribute(SqlXmlConstants.ANAME_HASERROR) == "1")
                state.CloseClass();

            if (initialIndent != state.IndentLevel)
                throw new Exception("Messed up the indenting!! Check code/stack or panic!");
        }

        private XmlNode ExtractXmlBetween(XmlNode startingElement, XmlNode endingElement)
        {
            XmlNode currentNode = startingElement;
            XmlNode previousNode = null;
            XmlNode remainder = null;
            XmlNode remainderPosition = null;

            while (currentNode != null)
            {
                if (currentNode.Equals(endingElement))
                    break;

                if (previousNode != null)
                {
                    XmlNode copyOfThisNode = currentNode.OwnerDocument.CreateNode(currentNode.NodeType, currentNode.Name, currentNode.NamespaceURI);
                    if (currentNode.Value != null)
                        copyOfThisNode.Value = currentNode.Value;
                    if (currentNode.Attributes != null)
                        foreach (XmlAttribute attribute in currentNode.Attributes)
                        {
                            XmlAttribute newAttribute = currentNode.OwnerDocument.CreateAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);
                            newAttribute.Value = attribute.Value;
                            copyOfThisNode.Attributes.Append(newAttribute);
                        }

                    if (remainderPosition == null)
                    {
                        remainderPosition = copyOfThisNode;
                        remainder = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.ParentNode) && remainderPosition.ParentNode != null)
                    {
                        remainderPosition = remainderPosition.ParentNode;
                    }
                    else if (currentNode.Equals(previousNode.ParentNode) && remainderPosition.ParentNode == null)
                    {
                        copyOfThisNode.AppendChild(remainderPosition);
                        remainderPosition = copyOfThisNode;
                        remainder = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.NextSibling) && remainderPosition.ParentNode != null)
                    {
                        remainderPosition.ParentNode.AppendChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.NextSibling) && remainderPosition.ParentNode == null)
                    {
                        XmlNode copyOfThisNodesParent = currentNode.OwnerDocument.CreateNode(currentNode.ParentNode.NodeType, currentNode.ParentNode.Name, currentNode.ParentNode.NamespaceURI);
                        remainder = copyOfThisNodesParent;
                        remainder.AppendChild(remainderPosition);
                        remainder.AppendChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                    else
                    {
                        //we must be a child
                        remainderPosition.AppendChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                }

                XmlNode nextNode = null;
                if (previousNode != null && currentNode.HasChildNodes && !(currentNode.Equals(previousNode.ParentNode)))
                {
                    nextNode = currentNode.FirstChild;
                }
                else if (currentNode.NextSibling != null)
                {
                    nextNode = currentNode.NextSibling;
                }
                else
                {
                    nextNode = currentNode.ParentNode;
                }

                previousNode = currentNode;
                currentNode = nextNode;
            }

            return remainder;
        }


        private string FormatKeyword(string keyword)
        {
            string outputKeyword;
            if (!KeywordMapping.TryGetValue(keyword, out outputKeyword))
                outputKeyword = keyword;

            if (Options.UppercaseKeywords)
                return outputKeyword.ToUpperInvariant();
            else
                return outputKeyword.ToLowerInvariant();
        }

        private string FormatOperator(string operatorValue)
        {
            if (Options.UppercaseKeywords)
                return operatorValue.ToUpperInvariant();
            else
                return operatorValue.ToLowerInvariant();
        }

        private void WhiteSpace_SeparateStatements(XmlElement contentElement, TSqlStandardFormattingState state)
        {
            if (state.StatementBreakExpected)
            {
                //check whether this is a DECLARE/SET clause with similar precedent, and therefore exempt from double-linebreak.
                XmlElement thisClauseStarter = FirstSemanticElementChild(contentElement);
				if (!(thisClauseStarter != null
					&& thisClauseStarter.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
					&& state.GetRecentKeyword() != null
					&& ((thisClauseStarter.InnerXml.ToUpperInvariant().Equals("SET")
							&& state.GetRecentKeyword().Equals("SET")
							)
						|| (thisClauseStarter.InnerXml.ToUpperInvariant().Equals("DECLARE")
							&& state.GetRecentKeyword().Equals("DECLARE")
							)
						|| (thisClauseStarter.InnerXml.ToUpperInvariant().Equals("PRINT")
							&& state.GetRecentKeyword().Equals("PRINT")
							)
						)
					))
				{
					for (int i = Options.NewStatementLineBreaks; i > 0; i--)
						state.AddOutputLineBreak();
				}
				else
				{
					for (int i = Options.NewClauseLineBreaks; i > 0; i--)
						state.AddOutputLineBreak();
				}

                state.Indent(state.IndentLevel);
                state.BreakExpected = false;
				state.AdditionalBreaksExpected = 0;
                state.SourceBreakPending = false;
                state.StatementBreakExpected = false;
                state.WordSeparatorExpected = false;
            }
        }

        private XmlElement FirstSemanticElementChild(XmlElement contentElement)
        {
            XmlElement target = null;
            while (contentElement != null)
            {
                target = (XmlElement)contentElement.SelectSingleNode(string.Format("*[local-name() != '{0}' and local-name() != '{1}' and local-name() != '{2}' and local-name() != '{3}']",
                    SqlXmlConstants.ENAME_WHITESPACE,
                    SqlXmlConstants.ENAME_COMMENT_MULTILINE,
                    SqlXmlConstants.ENAME_COMMENT_SINGLELINE,
                    SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE));

                if (target != null
                    && (target.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                        || target.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                        || target.Name.Equals(SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                        || target.Name.Equals(SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK)
                        )
                    )
                    contentElement = target;
                else
                    contentElement = null;
            }

            return target;
        }

        private void WhiteSpace_SeparateWords(TSqlStandardFormattingState state)
        {
            if (state.BreakExpected || state.AdditionalBreaksExpected > 0)
            {
                bool wasUnIndent = state.UnIndentInitialBreak;
                if (wasUnIndent) state.DecrementIndent();
                WhiteSpace_BreakAsExpected(state);
                if (wasUnIndent) state.IncrementIndent();
            }
            else if (state.WordSeparatorExpected)
            {
                state.AddOutputSpace();
            }
            state.UnIndentInitialBreak = false;
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_SeparateComment(XmlElement contentElement, TSqlStandardFormattingState state)
        {
            if (state.CurrentLineHasContent && state.SourceBreakPending)
            {
                state.BreakExpected = true;
                WhiteSpace_BreakAsExpected(state);
            }
            else if (state.WordSeparatorExpected)
                state.AddOutputSpace();
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_BreakAsExpected(TSqlStandardFormattingState state)
        {
            if (state.BreakExpected)
                state.WhiteSpace_BreakToNextLine();
            while (state.AdditionalBreaksExpected > 0)
            {
                state.WhiteSpace_BreakToNextLine();
                state.AdditionalBreaksExpected--;
            }
        }

        class TSqlStandardFormattingState : BaseFormatterState
        {
            //normal constructor
            public TSqlStandardFormattingState(bool htmlOutput, string indentString, int spacesPerTab, int maxLineWidth, int initialIndentLevel)
                : base(htmlOutput)
            {
                IndentLevel = initialIndentLevel;
                HtmlOutput = htmlOutput;
                IndentString = indentString;
				MaxLineWidth = maxLineWidth;

                int tabCount = indentString.Split('\t').Length - 1;
                int tabExtraCharacters = tabCount * (spacesPerTab - 1);
                IndentLength = indentString.Length + tabExtraCharacters;
            }

            //special "we want isolated state, but inheriting existing conditions" constructor
            public TSqlStandardFormattingState(TSqlStandardFormattingState sourceState)
                : base(sourceState.HtmlOutput)
            {
                IndentLevel = sourceState.IndentLevel;
                HtmlOutput = sourceState.HtmlOutput;
                IndentString = sourceState.IndentString;
                IndentLength = sourceState.IndentLength;
                MaxLineWidth = sourceState.MaxLineWidth;
				//TODO: find a way out of the cross-dependent wrapping maze...
                //CurrentLineLength = sourceState.CurrentLineLength;
                CurrentLineLength = IndentLevel * IndentLength;
                CurrentLineHasContent = sourceState.CurrentLineHasContent;
            }

            private string IndentString { get; set; }
            private int IndentLength { get; set; }
            private int MaxLineWidth { get; set; }

            public bool StatementBreakExpected { get; set; }
            public bool BreakExpected { get; set; }
            public bool WordSeparatorExpected { get; set; }
            public bool SourceBreakPending { get; set; }
            public int AdditionalBreaksExpected { get; set; }

            public bool UnIndentInitialBreak { get; set; }
            public int IndentLevel { get; private set; }
            public int CurrentLineLength { get; private set; }
            public bool CurrentLineHasContent { get; private set; }

            public SpecialRegionType? SpecialRegionActive { get; set; }
            public XmlNode RegionStartNode { get; set; }

            private static Regex _startsWithBreakChecker = new Regex(@"^\s*(\r|\n)", RegexOptions.None);
            public bool StartsWithBreak
            {
                get
                {
                    return _startsWithBreakChecker.IsMatch(_outBuilder.ToString());
                }
            }

            public override void AddOutputContent(string content)
            {
                if (SpecialRegionActive == null)
                    AddOutputContent(content, null);
            }

            public override void AddOutputContent(string content, string htmlClassName)
            {
                if (CurrentLineHasContent && (content.Length + CurrentLineLength > MaxLineWidth))
                    WhiteSpace_BreakToNextLine();

                if (SpecialRegionActive == null)
                    base.AddOutputContent(content, htmlClassName);

                CurrentLineHasContent = true;
                CurrentLineLength += content.Length;
            }

            public override void AddOutputLineBreak()
            {
#if DEBUG
                //hints for debugging line-width issues:
                //_outBuilder.Append(" (" + CurrentLineLength.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
#endif

                //if linebreaks are added directly in the content (eg in comments or strings), they 
                // won't be accounted for here - that's ok.
                if (SpecialRegionActive == null)
                    base.AddOutputLineBreak();
                CurrentLineLength = 0;
                CurrentLineHasContent = false;
            }

            internal void AddOutputSpace()
            {
                if (SpecialRegionActive == null)
                    _outBuilder.Append(" ");
            }

            public void Indent(int indentLevel)
            {
                for (int i = 0; i < indentLevel; i++)
                {
                    if (SpecialRegionActive == null)
                        _outBuilder.Append(IndentString);
                    CurrentLineLength += IndentLength;
                }
            }

            internal void WhiteSpace_BreakToNextLine()
            {
                AddOutputLineBreak();
                Indent(IndentLevel);
                BreakExpected = false;
                SourceBreakPending = false;
                WordSeparatorExpected = false;
            }

            //for linebreak detection, use actual string content rather than counting "AddOutputLineBreak()" calls,
            // because we also want to detect the content of strings and comments.
            private static Regex _lineBreakMatcher = new Regex(@"(\r|\n)+", RegexOptions.Compiled);
            public bool OutputContainsLineBreak { get { return _lineBreakMatcher.IsMatch(_outBuilder.ToString()); } }

            public void Assimilate(TSqlStandardFormattingState partialState)
            {
                //TODO: find a way out of the cross-dependent wrapping maze...
                CurrentLineLength = CurrentLineLength + partialState.CurrentLineLength;
                CurrentLineHasContent = CurrentLineHasContent || partialState.CurrentLineHasContent;
                if (SpecialRegionActive == null)
                    _outBuilder.Append(partialState.DumpOutput());
            }


            private Dictionary<int, string> _mostRecentKeywordsAtEachLevel = new Dictionary<int, string>();

            public TSqlStandardFormattingState IncrementIndent()
            {
                IndentLevel++;
                return this;
            }

            public TSqlStandardFormattingState DecrementIndent()
            {
                IndentLevel--;
                return this;
            }

            public void SetRecentKeyword(string ElementName)
            {
                if (!_mostRecentKeywordsAtEachLevel.ContainsKey(IndentLevel))
                    _mostRecentKeywordsAtEachLevel.Add(IndentLevel, ElementName.ToUpperInvariant());
            }

            public string GetRecentKeyword()
            {
                string keywordFound = null;
                int? keywordFoundAt = null;
                foreach (int key in _mostRecentKeywordsAtEachLevel.Keys)
                {
                    if ((keywordFoundAt == null || keywordFoundAt.Value > key) && key >= IndentLevel)
                    {
                        keywordFoundAt = key;
                        keywordFound = _mostRecentKeywordsAtEachLevel[key];
                    }
                }
                return keywordFound;
            }

            public void ResetKeywords()
            {
                List<int> descendentLevelKeys = new List<int>();
                foreach (int key in _mostRecentKeywordsAtEachLevel.Keys)
                    if (key >= IndentLevel)
                        descendentLevelKeys.Add(key);
                foreach (int key in descendentLevelKeys)
                    _mostRecentKeywordsAtEachLevel.Remove(key);
            }
        }

        public enum SpecialRegionType
        {
            NoFormat = 1,
            Minify = 2
        }
    }
}
