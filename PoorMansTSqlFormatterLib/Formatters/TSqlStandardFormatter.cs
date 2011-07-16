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

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlStandardFormatter : Interfaces.ISqlTreeFormatter
    {
        /*
         * TODO:
         *  - (optionally?) perform some syntantical consistency fixes:
         *    - eliminate redundant "Outer" keyword from "left", "right" and "full" join clauses
         *    - change plain "join" to "inner join"
         *    - make this work despite any join hints (loop, hash, merge, remote)
         *    - Insert -> Insert Into
         *    - TRAN -> TRANSACTION
         *    - PROC -> PROCEDURE
         *    - DECLARE @Test AS INT -> DECLARE @Test INT
         *    
         *  - Implement text width-based line breaking
         *    - Provide preference option for width and tab spaces?
         *  
         */

        public TSqlStandardFormatter() : this("\t", true, false, false, true, true, true, true, false) {}

        public TSqlStandardFormatter(string indentString, bool expandCommaLists, bool trailingCommas, bool spaceAfterExpandedComma, bool expandBooleanExpressions, bool expandCaseStatements, bool expandBetweenConditions, bool uppercaseKeywords, bool htmlColoring)
        {
            IndentString = indentString;
            ExpandCommaLists = expandCommaLists;
            TrailingCommas = trailingCommas;
            SpaceAfterExpandedComma = spaceAfterExpandedComma;
            ExpandBooleanExpressions = expandBooleanExpressions;
            ExpandBetweenConditions = expandBetweenConditions;
            ExpandCaseStatements = expandCaseStatements;
            UppercaseKeywords = uppercaseKeywords;
            HTMLColoring = htmlColoring;
        }

        public string IndentString { get; set; }
        public bool ExpandCommaLists { get; set; }
        public bool TrailingCommas { get; set; }
        public bool SpaceAfterExpandedComma { get; set; }
        public bool ExpandBooleanExpressions { get; set; }
        public bool ExpandCaseStatements { get; set; }
        public bool ExpandBetweenConditions { get; set; }
        public bool UppercaseKeywords { get; set; }
        public bool HTMLColoring { get; set; }
        public bool HTMLFormatted { get { return HTMLColoring; } }

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            //thread-safe - each call to FormatSQLTree() gets its own independent state object
            TSqlFormattingState state = new TSqlFormattingState();
            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT, Interfaces.SqlXmlConstants.ANAME_ERRORFOUND)) != null)
                state.OutBuilder.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING! (formatted SQL could be incorrect / logically different) ");

            XmlNodeList rootList = sqlTreeDoc.SelectNodes(string.Format("/{0}/*", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT));
            ProcessSqlNodeList(rootList, state);
            WhiteSpace_BreakAsExpected(state);

            return state.OutBuilder.ToString();
        }

        private void ProcessSqlNodeList(XmlNodeList rootList, TSqlFormattingState state)
        {
            foreach (XmlElement contentElement in rootList)
            {
                ProcessSqlNode(contentElement, state);
            }
        }

        private void ProcessSqlNode(XmlElement contentElement, TSqlFormattingState state)
        {
            int initialIndent = state.IndentLevel;

            switch (contentElement.Name)
            {
                case Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT:
                    WhiteSpace_SeparateStatements(contentElement, state);
                    state.ResetKeywords();
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.StatementBreakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE:
                    state.IsStartOfClause = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.DecrementIndent();
                    state.BreakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_UNION_CLAUSE:
                    WhiteSpace_BreakToNextLine(state.DecrementIndent()); //this is the one already recommended by the start of the clause
                    WhiteSpace_BreakToNextLine(state); //this is the one we additionally want to apply
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.BreakExpected = true;
                    state.AdditionalBreakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    WhiteSpace_BreakToNextLine(state);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.BreakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_DDL_OTHER_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_CURSOR_DECLARATION:
                case Interfaces.SqlXmlConstants.ENAME_BEGIN_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_SAVE_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_COMMIT_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN:
                case Interfaces.SqlXmlConstants.ENAME_CONTAINER_CLOSE:
                case Interfaces.SqlXmlConstants.ENAME_WHILE_LOOP:
                case Interfaces.SqlXmlConstants.ENAME_IF_STATEMENT:
                case Interfaces.SqlXmlConstants.ENAME_SELECTIONTARGET:
                case Interfaces.SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT:
                case Interfaces.SqlXmlConstants.ENAME_CTE_WITH_CLAUSE:
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_INPUT:
                case Interfaces.SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION:
                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND:
                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND:
                    WhiteSpace_SeparateWords(state);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case Interfaces.SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.StatementBreakExpected = false; //the responsibility for breaking will be with the OUTER statement; there should be no consequence propagating out from statements in this container;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_ELSE_CLAUSE:
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT), state.IncrementIndent());
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_AS_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state);
                    state.IncrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_TRIGGER_CONDITION:
                    WhiteSpace_BreakToNextLine(state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS:
                case Interfaces.SqlXmlConstants.ENAME_CTE_AS_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_RETURNS:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.DecrementIndent());
                    state.IncrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_CONDITION:
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND), state.IncrementIndent());
                    if (ExpandBetweenConditions)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_CLOSE), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND), state.IncrementIndent());
                    state.DecrementIndent();
                    state.DecrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS:
                case Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS:
                    //simply process sub-nodes - don't add space or expect any linebreaks (but respect linebreaks if necessary)
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.OutBuilder.Append(FormatOperator("("));
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state.IncrementIndent());
                    state.DecrementIndent();
                    WhiteSpace_BreakAsExpected(state);
                    state.OutBuilder.Append(FormatOperator(")"));
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_PARENS:
                case Interfaces.SqlXmlConstants.ENAME_EXPRESSION_PARENS:
                case Interfaces.SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS:
                    WhiteSpace_SeparateWords(state);
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_EXPRESSION_PARENS))
                        state.IncrementIndent();
                    state.OutBuilder.Append(FormatOperator("("));
                    TSqlFormattingState innerState = new TSqlFormattingState(state.IndentLevel);
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), innerState);
                    string innerString = innerState.OutBuilder.ToString();
                    //if there was a linebreak in the parens content, or if it wanted one to follow, then put linebreaks before and after.
                    if (innerState.BreakExpected || Regex.IsMatch(innerString, @"(\r|\n)+"))
                    {
                        WhiteSpace_BreakToNextLine(state);
                        state.OutBuilder.Append(innerString);
                        WhiteSpace_BreakToNextLine(state);
                    }
                    else
                    {
                        state.OutBuilder.Append(innerString);
                    }
                    state.OutBuilder.Append(FormatOperator(")"));
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_EXPRESSION_PARENS))
                        state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BEGIN_END_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_CATCH_BLOCK:
                    if (contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && contentElement.ParentNode.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && contentElement.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                        state.DecrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT), state);
                    state.DecrementIndent();
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_CLOSE), state);
                    state.IncrementIndent();
                    if (contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && contentElement.ParentNode.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && contentElement.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                        state.IncrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_STATEMENT:
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CASE_INPUT), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CASE_WHEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CASE_ELSE), state);
                    if (ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_CLOSE), state);
                    state.DecrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_WHEN:
                case Interfaces.SqlXmlConstants.ENAME_CASE_THEN:
                case Interfaces.SqlXmlConstants.ENAME_CASE_ELSE:
                    if (ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_CASE_THEN), state);
                    state.DecrementIndent();
                    break;

                case Interfaces.SqlXmlConstants.ENAME_AND_OPERATOR:
                case Interfaces.SqlXmlConstants.ENAME_OR_OPERATOR:
                    if (ExpandBooleanExpressions)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMENT_MULTILINE:
                    WhiteSpace_SeparateComment(contentElement, state);
                    if (HTMLColoring)
                        state.OutBuilder.Append(@"<span class=""SQLComment"">");
                    state.OutBuilder.Append("/*");
                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                    else
                        state.OutBuilder.Append(contentElement.InnerText);
                    state.OutBuilder.Append("*/");
                    if (HTMLColoring)
                        state.OutBuilder.Append("</span>");
                    if (contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT))
                        state.BreakExpected = true;
                    else
                        state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMENT_SINGLELINE:
                    WhiteSpace_SeparateComment(contentElement, state);
                    if (HTMLColoring)
                        state.OutBuilder.Append(@"<span class=""SQLComment"">");
                    state.OutBuilder.Append("--");
                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("\r", "").Replace("\n", "")));
                    else
                        state.OutBuilder.Append(contentElement.InnerText.Replace("\r", "").Replace("\n", ""));
                    if (HTMLColoring)
                        state.OutBuilder.Append("</span>");
                    state.BreakExpected = true;
                    state.SourceBreakPending = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_STRING:
                case Interfaces.SqlXmlConstants.ENAME_NSTRING:
                    WhiteSpace_SeparateWords(state);
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_NSTRING))
                        state.OutBuilder.Append("N");

                    if (HTMLColoring)
                        state.OutBuilder.Append(@"<span class=""SQLString"">");

                    state.OutBuilder.Append("'");

                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("'", "''")));
                    else
                        state.OutBuilder.Append(contentElement.InnerText.Replace("'", "''"));

                    state.OutBuilder.Append("'");
                    
                    if (HTMLColoring)
                        state.OutBuilder.Append("</span>");

                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME:
                    WhiteSpace_SeparateWords(state);
                    state.OutBuilder.Append("[");
                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("]", "]]")));
                    else
                        state.OutBuilder.Append(contentElement.InnerText.Replace("]", "]]"));
                    state.OutBuilder.Append("]");
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_QUOTED_STRING:
                    WhiteSpace_SeparateWords(state);
                    state.OutBuilder.Append("\"");
                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("\"", "\"\"")));
                    else
                        state.OutBuilder.Append(contentElement.InnerText.Replace("\"", "\"\""));
                    state.OutBuilder.Append("\"");
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMA:
                    //comma always ignores requested word spacing
                    if (TrailingCommas)
                    {
                        state.OutBuilder.Append(FormatOperator(","));

                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS)
                                )
                            )
                            state.BreakExpected = true;
                        else
                            state.WordSeparatorExpected = true;
                    }
                    else
                    {
                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS)
                                )
                            )
                        {
                            WhiteSpace_BreakToNextLine(state);
                            state.OutBuilder.Append(FormatOperator(","));
                            if (SpaceAfterExpandedComma)
                                state.WordSeparatorExpected = true;
                        }
                        else
                        {
                            WhiteSpace_BreakAsExpected(state);
                            state.OutBuilder.Append(FormatOperator(","));
                            state.WordSeparatorExpected = true;
                        }

                    }
                    break;

                case Interfaces.SqlXmlConstants.ENAME_ASTERISK:
                    WhiteSpace_SeparateWords(state);
                    state.OutBuilder.Append(FormatOperator("*"));
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_PERIOD:
                    //always ignores requested word spacing, and doesn't request a following space either.
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.OutBuilder.Append(FormatOperator("."));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_SEMICOLON:
                    WhiteSpace_BreakAsExpected(state);
                    state.OutBuilder.Append(FormatOperator(";"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHEROPERATOR:
                    WhiteSpace_SeparateWords(state);
                    state.OutBuilder.Append(FormatOperator(contentElement.InnerText));
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMPOUNDKEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.Attributes[Interfaces.SqlXmlConstants.ANAME_SIMPLETEXT].Value);
                    state.OutBuilder.Append(FormatKeyword(contentElement.Attributes[Interfaces.SqlXmlConstants.ANAME_SIMPLETEXT].Value));
                    state.WordSeparatorExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_COMMENT_MULTILINE + " | " + Interfaces.SqlXmlConstants.ENAME_COMMENT_SINGLELINE), state.IncrementIndent());
                    state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHERKEYWORD:
                case Interfaces.SqlXmlConstants.ENAME_DATATYPE_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.InnerText);
                    state.OutBuilder.Append(FormatKeyword(contentElement.InnerText));
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_FUNCTION_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.InnerText);
                    if (HTMLColoring)
                    {
                        state.OutBuilder.Append(@"<span class=""SQLFunction"">");
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                        state.OutBuilder.Append("</span>");
                    }
                    else
                        state.OutBuilder.Append(contentElement.InnerText);
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHERNODE:
                case Interfaces.SqlXmlConstants.ENAME_NUMBER_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_MONETARY_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_BINARY_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_LABEL:
                    WhiteSpace_SeparateWords(state);
                    if (HTMLColoring)
                        state.OutBuilder.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                    else
                        state.OutBuilder.Append(contentElement.InnerText);
                    state.WordSeparatorExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_WHITESPACE:
                    //take note if it's a line-breaking space, but don't DO anything here
                    if (Regex.IsMatch(contentElement.InnerText, @"(\r|\n)+"))
                        state.SourceBreakPending = true;
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }

            if (initialIndent != state.IndentLevel)
                throw new Exception("Messed up the indenting!! Check code/stack or panic!");
        }

        private string FormatKeyword(string keyword)
        {
            string keywordOut = null;

            if (UppercaseKeywords)
                keywordOut = keyword.ToUpper();
            else
                keywordOut = keyword.ToLower();

            if (HTMLColoring)
                return @"<span class=""SQLKeyword"">" + keywordOut + "</span>"; //no need for HTMLencoding... keywords are never bad
            else
                return keywordOut;
        }

        private string FormatOperator(string operatorValue)
        {
            string operatorOut = null;

            if (UppercaseKeywords)
                operatorOut = operatorValue.ToUpper();
            else
                operatorOut = operatorValue.ToLower();

            if (HTMLColoring)
                return @"<span class=""SQLOperator"">" + System.Web.HttpUtility.HtmlEncode(operatorOut) + "</span>";
            else
                return operatorOut;
        }

        private void WhiteSpace_SeparateStatements(XmlElement contentElement, TSqlFormattingState state)
        {
            if (state.StatementBreakExpected)
            {
                //check whether this is a DECLARE/SET clause with similar precedent, and therefore exempt from double-linebreak.
                XmlElement thisClauseStarter = FirstSemanticElementChild(contentElement);
                if (!(thisClauseStarter != null
                    && thisClauseStarter.Name.Equals(Interfaces.SqlXmlConstants.ENAME_OTHERKEYWORD)
                    && state.GetRecentKeyword() != null
                    && ((thisClauseStarter.InnerXml.ToUpper().Equals("SET")
                            && state.GetRecentKeyword().Equals("SET")
                            )
                        || (thisClauseStarter.InnerXml.ToUpper().Equals("DECLARE")
                            && state.GetRecentKeyword().Equals("DECLARE")
                            )
                        || (thisClauseStarter.InnerXml.ToUpper().Equals("PRINT")
                            && state.GetRecentKeyword().Equals("PRINT")
                            )
                        )
                    ))
                    state.OutBuilder.Append(Environment.NewLine);

                state.OutBuilder.Append(Environment.NewLine);
                Indent(state.OutBuilder, state.IndentLevel);
                state.BreakExpected = false;
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
                target = (XmlElement)contentElement.SelectSingleNode(string.Format("*[local-name() != '{0}' and local-name() != '{1}' and local-name() != '{2}']",
                    Interfaces.SqlXmlConstants.ENAME_WHITESPACE,
                    Interfaces.SqlXmlConstants.ENAME_COMMENT_MULTILINE,
                    Interfaces.SqlXmlConstants.ENAME_COMMENT_SINGLELINE));

                if (target != null 
                    && (target.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE)
                        || target.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                        || target.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                        )
                    )
                    contentElement = target;
                else
                    contentElement = null;
            }

            return target;
        }

        private void WhiteSpace_SeparateWords(TSqlFormattingState state)
        {
            if (state.BreakExpected || state.AdditionalBreakExpected)
            {
                bool wasStartOfClause = state.IsStartOfClause;
                if (wasStartOfClause) state.DecrementIndent();
                WhiteSpace_BreakAsExpected(state);
                if (wasStartOfClause) state.IncrementIndent();
                state.IsStartOfClause = false;
            }
            else if (state.WordSeparatorExpected)
            {
                state.OutBuilder.Append(" ");
                state.IsStartOfClause = false;
            }
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_SeparateComment(XmlElement contentElement, TSqlFormattingState state)
        {
            if (state.BreakExpected && state.SourceBreakPending)
                WhiteSpace_BreakAsExpected(state);
            else if (state.WordSeparatorExpected)
                state.OutBuilder.Append(" ");
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_BreakAsExpected(TSqlFormattingState state)
        {
            if (state.BreakExpected)
                WhiteSpace_BreakToNextLine(state);
            if (state.AdditionalBreakExpected)
            {
                WhiteSpace_BreakToNextLine(state);
                state.AdditionalBreakExpected = false;
            }
        }

        private void WhiteSpace_BreakToNextLine(TSqlFormattingState state)
        {
            state.OutBuilder.Append(Environment.NewLine);
            Indent(state.OutBuilder, state.IndentLevel);
            state.BreakExpected = false;
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void Indent(StringBuilder outString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                outString.Append(IndentString);
            }
        }

        class TSqlFormattingState
        {
            public TSqlFormattingState() : this(0) { }
            public TSqlFormattingState(int initialIndentLevel) 
            {
                IndentLevel = initialIndentLevel;
            }

            public bool StatementBreakExpected { get; set; }
            public bool BreakExpected { get; set; }
            public bool WordSeparatorExpected { get; set; }
            public bool SourceBreakPending { get; set; }
            public bool AdditionalBreakExpected { get; set; }

            public bool IsStartOfClause { get; set; }
            public int IndentLevel { get; private set; }

            StringBuilder _outBuilder = new StringBuilder();
            public StringBuilder OutBuilder { get { return _outBuilder; } }

            private Dictionary<int, string> _mostRecentKeywordsAtEachLevel = new Dictionary<int, string>();

            public TSqlFormattingState IncrementIndent()
            {
                IndentLevel++;
                return this;
            }

            public TSqlFormattingState DecrementIndent()
            {
                IndentLevel--;
                return this;
            }

            public void SetRecentKeyword(string ElementName)
            {
                if (!_mostRecentKeywordsAtEachLevel.ContainsKey(IndentLevel))
                    _mostRecentKeywordsAtEachLevel.Add(IndentLevel, ElementName.ToUpper());
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
    }
}
