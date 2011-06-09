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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlStandardFormatter : Interfaces.ISqlTreeFormatter
    {
        /*
         * TODO:
         *  - Find a way to stack same-indent closing parens (parens buffer??)
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
         *  - Establish a cleaner way to group neighboring DECLARE, SET and PRINT statements together
         *     (current combination of clause-starter-tracking and upcoming-keyword-peeking is 
         *       expensive and ugly)
         */

        public TSqlStandardFormatter() : this("\t", true, false, true, true, true, false) {}

        public TSqlStandardFormatter(string indentString, bool expandCommaLists, bool trailingCommas, bool expandBooleanExpressions, bool expandCaseStatements, bool uppercaseKeywords, bool htmlColoring)
        {
            IndentString = indentString;
            ExpandCommaLists = expandCommaLists;
            TrailingCommas = trailingCommas;
            ExpandBooleanExpressions = expandBooleanExpressions;
            ExpandBetweenConditions = ExpandBooleanExpressions;
            ExpandCaseStatements = expandCaseStatements;
            UppercaseKeywords = uppercaseKeywords;
            HTMLColoring = htmlColoring;
        }

        public string IndentString { get; set; }
        public bool ExpandCommaLists { get; set; }
        public bool TrailingCommas { get; set; }
        public bool ExpandBooleanExpressions { get; set; }
        public bool ExpandCaseStatements { get; set; }
        public bool ExpandBetweenConditions { get; set; }
        public bool UppercaseKeywords { get; set; }
        public bool HTMLColoring { get; set; }
        public bool HTMLFormatted { get { return HTMLColoring; } }

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            StringBuilder outString = new StringBuilder();
            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT, Interfaces.SqlXmlConstants.ANAME_ERRORFOUND)) != null)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING! (formatted SQL could be incorrect / logically different) ");
            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT, Interfaces.SqlXmlConstants.ANAME_DATALOSS)) != null)
                outString.AppendLine("--WARNING! SOME STRUCTURE COULD NOT BE PRESERVED! (formatted SQL will still be logically equivalent) ");

            XmlNodeList rootList = sqlTreeDoc.SelectNodes(string.Format("/{0}/*", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT));
            bool breakExpected = false;
            bool sourceBreakPending = false;
            XmlElement lastClauseStarter = null;
            ProcessSqlNodeList(outString, rootList, 0, ref breakExpected, ref lastClauseStarter, ref sourceBreakPending);
            WhiteSpace_BreakIfExpected(outString, 0, ref breakExpected, ref sourceBreakPending);

            return outString.ToString();
        }

        private void ProcessSqlNodeList(StringBuilder outString, XmlNodeList rootList, int indentLevel, ref bool breakExpected, ref XmlElement firstSemanticElement, ref bool sourceBreakPending)
        {
            foreach (XmlElement contentElement in rootList)
            {
                ProcessSqlNode(outString, contentElement, indentLevel, ref breakExpected, ref firstSemanticElement, ref sourceBreakPending);
            }
        }

        private void ProcessSqlNode(StringBuilder outString, XmlElement contentElement, int indentLevel, ref bool breakExpected, ref XmlElement firstSemanticElement, ref bool sourceBreakPending)
        {
            XmlElement firstSemanticElementDeadEnd = null;

            if (firstSemanticElement == null
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_WHITESPACE)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                && !contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                )
                firstSemanticElement = contentElement;

            switch (contentElement.Name)
            {
                case Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT:
                    WhiteSpace_SeparateStatements(contentElement, outString, indentLevel, ref breakExpected, ref firstSemanticElement, ref sourceBreakPending);
                    firstSemanticElement = null;
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElement, ref sourceBreakPending);
                    breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE:
                    if (contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_EXPRESSION_PARENS))
                        breakExpected = true;
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected, ref firstSemanticElement, ref sourceBreakPending);
                    breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_UNION_CLAUSE:
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword(contentElement.InnerText));
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    outString.Append(Environment.NewLine);
                    outString.Append(FormatKeyword("GO"));
                    breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_DDL_OTHER_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_CURSOR_DECLARATION:
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElement, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_AS_BLOCK:
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("AS"));
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel - 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_TRIGGER_CONDITION:
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK:
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("FOR"));
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel - 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS:
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("FOR"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_RETURNS:
                    outString.Append(Environment.NewLine);
                    outString.Append(FormatKeyword("RETURNS"));
                    breakExpected = false;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CTE_WITH_CLAUSE:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("WITH"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CTE_AS_BLOCK:
                    //newline regardless of whether previous element recommended a break or not.
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("AS"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_CONDITION:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("BETWEEN"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND), indentLevel + 2, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    if (ExpandBetweenConditions)
                        WhiteSpace_BreakToNextLine(outString, indentLevel + 1, ref breakExpected, ref sourceBreakPending);
                    else
                        WhiteSpace_SeparateWords(contentElement, outString, indentLevel + 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("AND"));
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND), indentLevel + 2, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_INPUT:
                case Interfaces.SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION:
                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND:
                case Interfaces.SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS:
                case Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS:
                    //simply process sub-nodes - don't add space or expect any linebreaks (but respect them if necessary)
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("("));
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator(")"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_DDL_PARENS:
                case Interfaces.SqlXmlConstants.ENAME_EXPRESSION_PARENS:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("("));
                    StringBuilder innerStringBuilder = new StringBuilder();
                    ProcessSqlNodeList(innerStringBuilder, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    string innerString = innerStringBuilder.ToString();
                    outString.Append(innerString);
                    //if there was a linebreak in the parens content, then force the closing paren onto a new line.
                    if (Regex.IsMatch(innerString, @"(\r|\n)+"))
                        breakExpected = true;
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator(")"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BEGIN_END_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK:
                case Interfaces.SqlXmlConstants.ENAME_CATCH_BLOCK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("BEGIN"));
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK))
                        outString.Append(FormatKeyword(" TRY"));
                    else if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CATCH_BLOCK))
                        outString.Append(FormatKeyword(" CATCH"));
                    WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("END"));
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK))
                        outString.Append(FormatKeyword(" TRY"));
                    else if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CATCH_BLOCK))
                        outString.Append(FormatKeyword(" CATCH"));
                    // shouldn't this normally be handled by the next statement?
                    //breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_WHILE_LOOP:
                case Interfaces.SqlXmlConstants.ENAME_IF_STATEMENT:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_WHILE_LOOP))
                        outString.Append(FormatKeyword("WHILE"));
                    else
                        outString.Append(FormatKeyword("IF"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    //test for begin end block:
                    XmlNode beginBlock = contentElement.SelectSingleNode(string.Format("{0}/{1}/*[local-name() = '{2}' or local-name() = '{3}']", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT, Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE, Interfaces.SqlXmlConstants.ENAME_BEGIN_END_BLOCK, Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK));
                    if (beginBlock != null)
                    {
                        WhiteSpace_BreakToNextLine(outString, indentLevel - 1, ref breakExpected, ref sourceBreakPending);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)), indentLevel - 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    }
                    else
                    {
                        WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    }
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.SqlXmlConstants.ENAME_ELSE_CLAUSE), indentLevel - 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_ELSE_CLAUSE:
                    WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("ELSE"));
                    //test for begin end block:
                    XmlNode beginBlock2 = contentElement.SelectSingleNode(string.Format("{0}/{1}/*[local-name() = '{2}' or local-name() = '{3}']", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT, Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE, Interfaces.SqlXmlConstants.ENAME_BEGIN_END_BLOCK, Interfaces.SqlXmlConstants.ENAME_TRY_BLOCK));
                    if (beginBlock2 != null)
                    {
                        WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)), indentLevel, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    }
                    else
                    {
                        WhiteSpace_BreakToNextLine(outString, indentLevel + 1, ref breakExpected, ref sourceBreakPending);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)), indentLevel + 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    }
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_STATEMENT:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("CASE"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    if (ExpandCaseStatements)
                        breakExpected = true;
                    WhiteSpace_SeparateWords(null, outString, indentLevel + 1, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword("END"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_CASE_WHEN:
                case Interfaces.SqlXmlConstants.ENAME_CASE_THEN:
                case Interfaces.SqlXmlConstants.ENAME_CASE_ELSE:
                    if (ExpandCaseStatements)
                        breakExpected = true;
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CASE_WHEN))
                        outString.Append(FormatKeyword("WHEN"));
                    else if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_CASE_THEN))
                        outString.Append(FormatKeyword("THEN"));
                    else
                        outString.Append(FormatKeyword("ELSE"));
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected, ref firstSemanticElementDeadEnd, ref sourceBreakPending);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMENT_MULTILINE:
                    WhiteSpace_SeparateComment(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (HTMLColoring)
                        outString.Append(@"<span class=""SQLComment"">");
                    outString.Append("/*");
                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                    else
                        outString.Append(contentElement.InnerText);
                    outString.Append("*/");
                    if (HTMLColoring)
                        outString.Append("</span>");
                    if (contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT))
                        breakExpected = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMENT_SINGLELINE:
                    WhiteSpace_SeparateComment(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (HTMLColoring)
                        outString.Append(@"<span class=""SQLComment"">");
                    outString.Append("--");
                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("\r", "").Replace("\n", "")));
                    else
                        outString.Append(contentElement.InnerText.Replace("\r", "").Replace("\n", ""));
                    if (HTMLColoring)
                        outString.Append("</span>");
                    breakExpected = true;
                    sourceBreakPending = true;
                    break;

                case Interfaces.SqlXmlConstants.ENAME_STRING:
                case Interfaces.SqlXmlConstants.ENAME_NSTRING:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_NSTRING))
                        outString.Append("N");

                    if (HTMLColoring)
                        outString.Append(@"<span class=""SQLString"">");

                    outString.Append("'");

                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("'", "''")));
                    else
                        outString.Append(contentElement.InnerText.Replace("'", "''"));

                    outString.Append("'");
                    
                    if (HTMLColoring)
                        outString.Append("</span>");
                    break;

                case Interfaces.SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append("[");
                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("]", "]]")));
                    else
                        outString.Append(contentElement.InnerText.Replace("]", "]]"));
                    outString.Append("]");
                    break;

                case Interfaces.SqlXmlConstants.ENAME_QUOTED_STRING:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append("\"");
                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText.Replace("\"", "\"\"")));
                    else
                        outString.Append(contentElement.InnerText.Replace("\"", "\"\""));
                    outString.Append("\"");
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMMA:

                    if (TrailingCommas)
                    {
                        outString.Append(FormatOperator(","));

                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS)
                                )
                            )
                            breakExpected = true;
                    }
                    else
                    {
                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_FUNCTION_PARENS)
                                )
                            )
                            WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                        else
                            WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);

                        outString.Append(FormatOperator(","));
                    }
                    break;

                case Interfaces.SqlXmlConstants.ENAME_ASTERISK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("*"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_PERIOD:
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator("."));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_SEMICOLON:
                    WhiteSpace_BreakIfExpected(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator(";"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_AND_OPERATOR:
                case Interfaces.SqlXmlConstants.ENAME_OR_OPERATOR:
                    if (ExpandBooleanExpressions)
                        WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    else
                        WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);

                    if (contentElement.Name.Equals(Interfaces.SqlXmlConstants.ENAME_AND_OPERATOR))
                        outString.Append(FormatOperator("AND"));
                    else
                        outString.Append(FormatOperator("OR"));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHEROPERATOR:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatOperator(contentElement.InnerText));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_COMPOUNDKEYWORD:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword(contentElement.Attributes[Interfaces.SqlXmlConstants.ANAME_SIMPLETEXT].Value));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHERKEYWORD:
                case Interfaces.SqlXmlConstants.ENAME_DATATYPE_KEYWORD:
                case Interfaces.SqlXmlConstants.ENAME_BEGIN_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_COMMIT_TRANSACTION:
                case Interfaces.SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    outString.Append(FormatKeyword(contentElement.InnerText));
                    break;

                case Interfaces.SqlXmlConstants.ENAME_FUNCTION_KEYWORD:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (HTMLColoring)
                    {
                        outString.Append(@"<span class=""SQLFunction"">");
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                        outString.Append("</span>");
                    }
                    else
                        outString.Append(contentElement.InnerText);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_OTHERNODE:
                case Interfaces.SqlXmlConstants.ENAME_NUMBER_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_MONETARY_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_BINARY_VALUE:
                case Interfaces.SqlXmlConstants.ENAME_LABEL:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected, ref sourceBreakPending);
                    if (HTMLColoring)
                        outString.Append(System.Web.HttpUtility.HtmlEncode(contentElement.InnerText));
                    else
                        outString.Append(contentElement.InnerText);
                    break;

                case Interfaces.SqlXmlConstants.ENAME_WHITESPACE:
                    if (Regex.IsMatch(contentElement.InnerText, @"(\r|\n)+"))
                        sourceBreakPending = true;
                    //ignore
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
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

        private void WhiteSpace_SeparateStatements(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected, ref XmlElement previousClauseStarter, ref bool sourceBreakPending)
        {
            if (breakExpected)
            {
                //check whether this is a DECLARE/SET clause with similar precedent, and therefore exempt from double-linebreak.
                XmlElement thisClauseStarter = FirstSemanticElementChild(contentElement);
                if (!(thisClauseStarter != null
                    && previousClauseStarter != null
                    && thisClauseStarter.Name.Equals(Interfaces.SqlXmlConstants.ENAME_OTHERKEYWORD)
                    && previousClauseStarter.Name.Equals(Interfaces.SqlXmlConstants.ENAME_OTHERKEYWORD)
                    && ((thisClauseStarter.InnerXml.ToUpper().Equals("SET")
                            && previousClauseStarter.InnerXml.ToUpper().Equals("SET")
                            )
                        || (thisClauseStarter.InnerXml.ToUpper().Equals("DECLARE")
                            && previousClauseStarter.InnerXml.ToUpper().Equals("DECLARE")
                            )
                        || (thisClauseStarter.InnerXml.ToUpper().Equals("PRINT")
                            && previousClauseStarter.InnerXml.ToUpper().Equals("PRINT")
                            )
                        )
                    ))
                    outString.Append(Environment.NewLine);

                outString.Append(Environment.NewLine);
                Indent(outString, indentLevel);
                breakExpected = false;
            }
            sourceBreakPending = false;
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

        private void WhiteSpace_SeparateWords(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected, ref bool sourceBreakPending)
        {
            if (breakExpected)
                WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
            else if (contentElement == null
                || HasNonTextNonWhitespacePriorSiblingThatIsNotAPeriod(contentElement)
                )
                outString.Append(" ");
            sourceBreakPending = false;
        }

        private void WhiteSpace_SeparateComment(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected, ref bool sourceBreakPending)
        {
            if (breakExpected && sourceBreakPending)
                WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
            else if (contentElement == null
                || HasNonTextNonWhitespacePriorSiblingThatIsNotAPeriod(contentElement)
                )
                outString.Append(" ");
            sourceBreakPending = false;
        }

        private void WhiteSpace_BreakIfExpected(StringBuilder outString, int indentLevel, ref bool breakExpected, ref bool sourceBreakPending)
        {
            if (breakExpected)
                WhiteSpace_BreakToNextLine(outString, indentLevel, ref breakExpected, ref sourceBreakPending);
        }

        private void WhiteSpace_BreakToNextLine(StringBuilder outString, int indentLevel, ref bool breakExpected, ref bool sourceBreakPending)
        {
            outString.Append(Environment.NewLine);
            Indent(outString, indentLevel);
            breakExpected = false;
            sourceBreakPending = false;
        }

        private void Indent(StringBuilder outString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                outString.Append(IndentString);
            }
        }

        private static bool HasNonTextNonWhitespacePriorSiblingThatIsNotAPeriod(XmlNode contentNode)
        {
            XmlNode currentNode = contentNode;

            while (currentNode.PreviousSibling != null)
            {
                if (currentNode.PreviousSibling.NodeType == XmlNodeType.Element
                    && !currentNode.PreviousSibling.Name.Equals(Interfaces.SqlXmlConstants.ENAME_WHITESPACE)
                    )
                {
                    if (currentNode.PreviousSibling.Name.Equals(Interfaces.SqlXmlConstants.ENAME_PERIOD))
                        return false;
                    else
                        return true;
                }
                else
                {
                    currentNode = currentNode.PreviousSibling;
                }
            }

            return false;
        }
    }
}
