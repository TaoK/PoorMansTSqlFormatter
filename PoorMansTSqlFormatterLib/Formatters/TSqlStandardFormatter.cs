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
         *  - UNION clauses get special formatting?
         *  - Implement text width-based line breaking
         *    - Provide preference option for width and tab spaces?
         *  - Implement keyword casing
         *    - Provide preference option for keyword casing (uppercase/lowercase/titlecase)?
         */

        public TSqlStandardFormatter() : this("\t", false, false, false) {}

        public TSqlStandardFormatter(string indentString, bool expandCommaLists, bool trailingCommas, bool expandBooleanExpressions)
        {
            IndentString = indentString;
            ExpandCommaLists = expandCommaLists;
            TrailingCommas = trailingCommas;
            ExpandBooleanExpressions = expandBooleanExpressions;
        }

        public string IndentString { get; set; }
        public bool ExpandCommaLists { get; set; }
        public bool TrailingCommas { get; set; }
        public bool ExpandBooleanExpressions { get; set; }

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            return FormatSQLDoc(sqlTreeDoc, Interfaces.Constants.ENAME_SQL_ROOT);
        }

        private string FormatSQLDoc(XmlDocument sqlTokenOrTreeDoc, string rootElement)
        {
            StringBuilder outString = new StringBuilder();
            if (sqlTokenOrTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.Constants.ENAME_SQL_ROOT, Interfaces.Constants.ANAME_ERRORFOUND)) != null)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING! (formatted SQL could be incorrect / logically different) ");
            if (sqlTokenOrTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.Constants.ENAME_SQL_ROOT, Interfaces.Constants.ANAME_DATALOSS)) != null)
                outString.AppendLine("--WARNING! SOME STRUCTURE COULD NOT BE PRESERVED! (formatted SQL will still be logically equivalent) ");

            XmlNodeList rootList = sqlTokenOrTreeDoc.SelectNodes(string.Format("/{0}/*", rootElement));
            bool breakExpected = false;
            ProcessSqlNodeList(outString, rootList, 0, ref breakExpected);

            return outString.ToString();
        }

        private void ProcessSqlNodeList(StringBuilder outString, XmlNodeList rootList, int indentLevel, ref bool breakExpected)
        {
            foreach (XmlElement contentElement in rootList)
            {
                ProcessSqlNode(outString, contentElement, indentLevel, ref breakExpected);
            }
        }

        private void ProcessSqlNode(StringBuilder outString, XmlElement contentElement, int indentLevel, ref bool breakExpected)
        {

            switch (contentElement.Name)
            {
                case Interfaces.Constants.ENAME_SQL_STATEMENT:
                    WhiteSpace_SeparateStatements(contentElement, outString, indentLevel, ref breakExpected);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected);
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_SQL_CLAUSE:
                    if (contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_EXPRESSION_PARENS))
                        breakExpected = true;
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected);
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    outString.Append(Environment.NewLine);
                    outString.Append("GO");
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_DDL_BLOCK:
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected);
                    break;

                case Interfaces.Constants.ENAME_DDL_AS_BLOCK:
                    //newline regardless of whether previous element recommended a break or not.
                    outString.Append(Environment.NewLine);
                    outString.Append("AS");
                    breakExpected = true;
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel-1, ref breakExpected);
                    break;

                case Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected);
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_DDLDETAIL_PARENS:
                case Interfaces.Constants.ENAME_FUNCTION_PARENS:
                    //simply process sub-nodes - don't add space or expect any linebreaks (but respect them if necessary)
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("(");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected);
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(")");
                    break;

                case Interfaces.Constants.ENAME_DDL_PARENS:
                case Interfaces.Constants.ENAME_EXPRESSION_PARENS:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("(");
                    StringBuilder innerStringBuilder = new StringBuilder();
                    ProcessSqlNodeList(innerStringBuilder, contentElement.SelectNodes("*"), indentLevel, ref breakExpected);
                    string innerString = innerStringBuilder.ToString();
                    outString.Append(innerString);
                    //if there was a linebreak in the parens content, then force the closing paren onto a new line.
                    if (Regex.IsMatch(innerString, @"(\r|\n)+"))
                        breakExpected = true;
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(")");
                    break;

                case Interfaces.Constants.ENAME_BEGIN_END_BLOCK:
                case Interfaces.Constants.ENAME_TRY_BLOCK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("BEGIN");
                    if (contentElement.Name.Equals(Interfaces.Constants.ENAME_TRY_BLOCK))
                        outString.Append(" TRY");
                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);

                    foreach (XmlNode childNode in contentElement.ChildNodes)
                    {
                        switch (childNode.NodeType)
                        {
                            case XmlNodeType.Element:
                                ProcessSqlNode(outString, (XmlElement)childNode, indentLevel, ref breakExpected);
                                break;
                            case XmlNodeType.Text:
                            case XmlNodeType.Comment:
                                //ignore
                                break;
                            default:
                                throw new Exception("Unexpected xml node type encountered!");
                        }
                    }

                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel - 1, ref breakExpected);
                    outString.Append("END");
                    if (contentElement.Name.Equals(Interfaces.Constants.ENAME_TRY_BLOCK))
                        outString.Append(" TRY");
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_WHILE_LOOP:
                case Interfaces.Constants.ENAME_IF_STATEMENT:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    if (contentElement.Name.Equals(Interfaces.Constants.ENAME_WHILE_LOOP))
                        outString.Append("WHILE");
                    else
                        outString.Append("IF");
                    outString.Append(" ");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION), indentLevel, ref breakExpected);
                    //test for begin end block:
                    XmlNode beginBlock = contentElement.SelectSingleNode(string.Format("{0}/{1}/*[local-name() = '{2}' or local-name() = '{3}']", Interfaces.Constants.ENAME_SQL_STATEMENT, Interfaces.Constants.ENAME_SQL_CLAUSE, Interfaces.Constants.ENAME_BEGIN_END_BLOCK, Interfaces.Constants.ENAME_TRY_BLOCK));
                    if (beginBlock != null)
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel - 1, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel - 1, ref breakExpected);
                    }
                    else
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel, ref breakExpected);
                    }
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.Constants.ENAME_ELSE_CLAUSE), indentLevel - 1, ref breakExpected);
                    break;

                case Interfaces.Constants.ENAME_ELSE_CLAUSE:
                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("ELSE");
                    //test for begin end block:
                    XmlNode beginBlock2 = contentElement.SelectSingleNode(string.Format("{0}/{1}/*[local-name() = '{2}' or local-name() = '{3}']", Interfaces.Constants.ENAME_SQL_STATEMENT, Interfaces.Constants.ENAME_SQL_CLAUSE, Interfaces.Constants.ENAME_BEGIN_END_BLOCK, Interfaces.Constants.ENAME_TRY_BLOCK));
                    if (beginBlock2 != null)
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel, ref breakExpected);
                    }
                    else
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel + 1, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel + 1, ref breakExpected);
                    }
                    break;

                case Interfaces.Constants.ENAME_CASE_STATEMENT:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("CASE");
                    outString.Append(" ");

                    foreach (XmlNode childNode in contentElement.ChildNodes)
                    {
                        switch (childNode.NodeType)
                        {
                            case XmlNodeType.Element:
                                ProcessSqlNode(outString, (XmlElement)childNode, indentLevel + 1, ref breakExpected);
                                break;
                            case XmlNodeType.Text:
                            case XmlNodeType.Comment:
                                //ignore
                                break;
                            default:
                                throw new Exception("Unexpected xml node type encountered!");
                        }
                    }

                    WhiteSpace_SeparateWords(null, outString, indentLevel, ref breakExpected);
                    outString.Append("END");
                    break;

                case Interfaces.Constants.ENAME_COMMENT_MULTILINE:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("/*");
                    outString.Append(contentElement.InnerText);
                    outString.Append("*/");
                    if (contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT))
                        breakExpected = true;
                    break;
                case Interfaces.Constants.ENAME_COMMENT_SINGLELINE:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("--");
                    outString.Append(contentElement.InnerText.Replace("\r", "").Replace("\n", ""));
                    breakExpected = true;
                    break;
                case Interfaces.Constants.ENAME_STRING:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case Interfaces.Constants.ENAME_NSTRING:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("N'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case Interfaces.Constants.ENAME_QUOTED_IDENTIFIER:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("[");
                    outString.Append(contentElement.InnerText.Replace("]", "]]"));
                    outString.Append("]");
                    break;

                case Interfaces.Constants.ENAME_COMMA:

                    if (TrailingCommas)
                    {
                        outString.Append(",");

                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_FUNCTION_PARENS)
                                )
                            )
                            breakExpected = true;
                    }
                    else
                    {
                        if (ExpandCommaLists
                            && !(contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_DDLDETAIL_PARENS)
                                || contentElement.ParentNode.Name.Equals(Interfaces.Constants.ENAME_FUNCTION_PARENS)
                                )
                            )
                            WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                        else
                            WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);

                        outString.Append(",");
                    }
                    break;

                case Interfaces.Constants.ENAME_ASTERISK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("*");
                    break;

                case Interfaces.Constants.ENAME_PERIOD:
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(".");
                    break;

                case Interfaces.Constants.ENAME_SEMICOLON:
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(";");
                    break;

                case Interfaces.Constants.ENAME_AND_OPERATOR:
                case Interfaces.Constants.ENAME_OR_OPERATOR:
                    if (ExpandBooleanExpressions)
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                    else
                        WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);

                    if (contentElement.Name.Equals(Interfaces.Constants.ENAME_AND_OPERATOR))
                        outString.Append("AND");
                    else
                        outString.Append("OR");
                    break;

                case Interfaces.Constants.ENAME_BEGIN_TRANSACTION:
                case Interfaces.Constants.ENAME_COMMIT_TRANSACTION:
                case Interfaces.Constants.ENAME_ROLLBACK_TRANSACTION:
                case Interfaces.Constants.ENAME_OTHERNODE:
                case Interfaces.Constants.ENAME_OTHEROPERATOR:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(contentElement.InnerText);
                    break;
                case Interfaces.Constants.ENAME_WHITESPACE:
                    //ignore
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
        }

        private void WhiteSpace_SeparateStatements(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected)
        {
            if (breakExpected)
            {
                outString.Append(Environment.NewLine);
                outString.Append(Environment.NewLine);
                Indent(outString, indentLevel);
                breakExpected = false;
            }
        }

        private void WhiteSpace_SeparateWords(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected)
        {
            if (breakExpected)
                WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
            else if (contentElement == null 
                || HasNonTextNonWhitespacePriorSiblingThatIsNotAPeriod(contentElement)
                )
                outString.Append(" ");
        }

        private void WhiteSpace_BreakIfExpected(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected)
        {
            if (breakExpected)
                WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
        }

        private void WhiteSpace_BreakToNextLine(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected)
        {
            outString.Append(Environment.NewLine);
            Indent(outString, indentLevel);
            breakExpected = false;
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
                    && !currentNode.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE)
                    )
                {
                    if (currentNode.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_PERIOD))
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
