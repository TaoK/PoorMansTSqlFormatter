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
using System.Xml;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlStandardFormatter : Interfaces.ISqlTreeFormatter
    {
        /*
         * TODO:
         *  - Handle clauses (when implemented)
         *  - Handle line breaking and indenting on complex logical expressions (AND/OR)
         *  - Handle line breaking and indenting on comma-lists (params, select clauses etc)
         *  - Implement keyword casing
         *  - Provide params for indenting character/sequence
         *  - Provide params for comma-list-preference
         *  - Provide preference option for keyword casing (uppercase/lowercase/titlecase)?
         *  - Handle preferred max width, option and implementation
         */

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            return FormatSQLDoc(sqlTreeDoc, Interfaces.Constants.ENAME_SQL_ROOT);
        }

        private string FormatSQLDoc(XmlDocument sqlTokenOrTreeDoc, string rootElement)
        {
            StringBuilder outString = new StringBuilder();
            if (sqlTokenOrTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.Constants.ENAME_SQL_ROOT, Interfaces.Constants.ANAME_ERRORFOUND)) != null)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING!");

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

                case Interfaces.Constants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    outString.Append(Environment.NewLine);
                    outString.Append("GO");
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel, ref breakExpected);
                    breakExpected = true;
                    break;

                case Interfaces.Constants.ENAME_PARENS:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("(");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"), indentLevel + 1, ref breakExpected);
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(")");
                    break;

                case Interfaces.Constants.ENAME_BEGIN_END_BLOCK:
                case Interfaces.Constants.ENAME_TRY_BLOCK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("BEGIN");
                    if (contentElement.Name.Equals(Interfaces.Constants.ENAME_TRY_BLOCK))
                        outString.Append(" TRY");
                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel + 1, ref breakExpected);

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

                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
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
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION), indentLevel + 1, ref breakExpected);
                    //test for begin end block:
                    XmlNode beginBlock = contentElement.SelectSingleNode(string.Format("{0}/*[local-name() = '{1}' or local-name() = '{2}']", Interfaces.Constants.ENAME_SQL_STATEMENT, Interfaces.Constants.ENAME_BEGIN_END_BLOCK, Interfaces.Constants.ENAME_TRY_BLOCK));
                    if (beginBlock != null)
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel, ref breakExpected);
                    }
                    else
                    {
                        WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel + 1, ref breakExpected);
                        ProcessSqlNodeList(outString, contentElement.SelectNodes(string.Format("{0}", Interfaces.Constants.ENAME_SQL_STATEMENT)), indentLevel + 1, ref breakExpected);
                    }
                    ProcessSqlNodeList(outString, contentElement.SelectNodes(Interfaces.Constants.ENAME_ELSE_CLAUSE), indentLevel, ref breakExpected);
                    break;

                case Interfaces.Constants.ENAME_ELSE_CLAUSE:

                    WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("ELSE");

                    //test for begin end block:
                    XmlNode beginBlock2 = contentElement.SelectSingleNode(string.Format("{0}/*[local-name() = '{1}' or local-name() = '{2}']", Interfaces.Constants.ENAME_SQL_STATEMENT, Interfaces.Constants.ENAME_BEGIN_END_BLOCK, Interfaces.Constants.ENAME_TRY_BLOCK));
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

                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("END");
                    break;

                case Interfaces.Constants.ENAME_COMMENT_MULTILINE:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("/*");
                    outString.Append(contentElement.InnerText);
                    outString.Append("*/");
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
                    WhiteSpace_BreakIfExpected(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append(",");
                    break;
                case Interfaces.Constants.ENAME_ASTERISK:
                    WhiteSpace_SeparateWords(contentElement, outString, indentLevel, ref breakExpected);
                    outString.Append("*");
                    break;
                case Interfaces.Constants.ENAME_BEGIN_TRANSACTION:
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
                outString.Append(Indent(indentLevel));
                breakExpected = false;
            }
        }

        private void WhiteSpace_SeparateWords(XmlElement contentElement, StringBuilder outString, int indentLevel, ref bool breakExpected)
        {
            if (breakExpected)
                WhiteSpace_BreakToNextLine(contentElement, outString, indentLevel, ref breakExpected);
            else if (HasNonTextNonWhitespacePriorSibling(contentElement))
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
            outString.Append(Indent(indentLevel));
            breakExpected = false;
        }

        private string Indent(int indentLevel)
        {
            char[] indentedArray = new char[indentLevel];
            for (int i = 0; i < indentLevel; i++)
            {
                indentedArray[i] = '\t';
            }
            return new string(indentedArray);
        }

        private static bool HasNonTextNonWhitespacePriorSibling(XmlNode contentNode)
        {
            XmlNode currentNode = contentNode;

            while (currentNode.PreviousSibling != null)
            {
                if (currentNode.PreviousSibling.NodeType == XmlNodeType.Element
                    && !currentNode.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE)
                    )
                    return true;
                else
                    currentNode = currentNode.PreviousSibling;
            }

            return false;
        }
    }
}
