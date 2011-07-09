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
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlIdentityFormatter : Interfaces.ISqlTokenFormatter, Interfaces.ISqlTreeFormatter
    {

        public bool HTMLFormatted { get { return false; } }

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            string rootElement = SqlXmlConstants.ENAME_SQL_ROOT;
            StringBuilder outString = new StringBuilder();
            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", rootElement, SqlXmlConstants.ANAME_ERRORFOUND)) != null)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING!");

            XmlNodeList rootList = sqlTreeDoc.SelectNodes(string.Format("/{0}/*", rootElement));
            ProcessSqlNodeList(outString, rootList);

            return outString.ToString();
        }

        private static void ProcessSqlNodeList(StringBuilder outString, XmlNodeList rootList)
        {
            foreach (XmlElement contentElement in rootList)
            {
                ProcessSqlNode(outString, contentElement);
            }
        }

        private static void ProcessSqlNode(StringBuilder outString, XmlElement contentElement)
        {
            switch (contentElement.Name)
            {
                case SqlXmlConstants.ENAME_DDLDETAIL_PARENS:
                case SqlXmlConstants.ENAME_DDL_PARENS:
                case SqlXmlConstants.ENAME_FUNCTION_PARENS:
                case SqlXmlConstants.ENAME_EXPRESSION_PARENS:
                    outString.Append("(");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"));
                    outString.Append(")");
                    break;

                case SqlXmlConstants.ENAME_SQL_STATEMENT:
                case SqlXmlConstants.ENAME_SQL_CLAUSE:
                case SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION:
                case SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case SqlXmlConstants.ENAME_DDL_OTHER_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_DECLARATION:
                case SqlXmlConstants.ENAME_BEGIN_END_BLOCK:
                case SqlXmlConstants.ENAME_TRY_BLOCK:
                case SqlXmlConstants.ENAME_CATCH_BLOCK:
                case SqlXmlConstants.ENAME_CASE_STATEMENT:
                case SqlXmlConstants.ENAME_CASE_INPUT:
                case SqlXmlConstants.ENAME_CASE_WHEN:
                case SqlXmlConstants.ENAME_CASE_THEN:
                case SqlXmlConstants.ENAME_CASE_ELSE:
                case SqlXmlConstants.ENAME_IF_STATEMENT:
                case SqlXmlConstants.ENAME_ELSE_CLAUSE:
                case SqlXmlConstants.ENAME_WHILE_LOOP:
                case SqlXmlConstants.ENAME_DDL_AS_BLOCK:
                case SqlXmlConstants.ENAME_BETWEEN_CONDITION:
                case SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND:
                case SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND:
                case SqlXmlConstants.ENAME_CTE_WITH_CLAUSE:
                case SqlXmlConstants.ENAME_CTE_AS_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS:
                case SqlXmlConstants.ENAME_TRIGGER_CONDITION:
                case SqlXmlConstants.ENAME_COMPOUNDKEYWORD:
                case SqlXmlConstants.ENAME_BEGIN_TRANSACTION:
                case SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION:
                case SqlXmlConstants.ENAME_SAVE_TRANSACTION:
                case SqlXmlConstants.ENAME_COMMIT_TRANSACTION:
                case SqlXmlConstants.ENAME_BATCH_SEPARATOR:
                case SqlXmlConstants.ENAME_UNION_CLAUSE:
                case SqlXmlConstants.ENAME_CONTAINER_OPEN:
                case SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT:
                case SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case SqlXmlConstants.ENAME_CONTAINER_CLOSE:
                    foreach (XmlNode childNode in contentElement.ChildNodes)
                    {
                        switch (childNode.NodeType)
                        {
                            case XmlNodeType.Text:
                                outString.Append(childNode.InnerText);
                                break;
                            case XmlNodeType.Element:
                                ProcessSqlNode(outString, (XmlElement)childNode);
                                break;
                            case XmlNodeType.Comment:
                                //ignore; actual displayable T-SQL comments are elements.
                                break;
                            default:
                                throw new Exception("Unexpected xml node type encountered!");
                        }
                    }
                    break;

                case SqlXmlConstants.ENAME_COMMENT_MULTILINE:
                    outString.Append("/*");
                    outString.Append(contentElement.InnerText);
                    outString.Append("*/");
                    break;
                case SqlXmlConstants.ENAME_COMMENT_SINGLELINE:
                    outString.Append("--");
                    outString.Append(contentElement.InnerText);
                    break;
                case SqlXmlConstants.ENAME_STRING:
                    outString.Append("'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case SqlXmlConstants.ENAME_NSTRING:
                    outString.Append("N'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case SqlXmlConstants.ENAME_QUOTED_STRING:
                    outString.Append("\"");
                    outString.Append(contentElement.InnerText.Replace("\"", "\"\""));
                    outString.Append("\"");
                    break;
                case SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME:
                    outString.Append("[");
                    outString.Append(contentElement.InnerText.Replace("]", "]]"));
                    outString.Append("]");
                    break;

                case SqlXmlConstants.ENAME_COMMA:
                    outString.Append(",");
                    break;

                case SqlXmlConstants.ENAME_PERIOD:
                    outString.Append(".");
                    break;

                case SqlXmlConstants.ENAME_SEMICOLON:
                    outString.Append(";");
                    break;

                case SqlXmlConstants.ENAME_ASTERISK:
                    outString.Append("*");
                    break;

                case SqlXmlConstants.ENAME_AND_OPERATOR:
                case SqlXmlConstants.ENAME_OR_OPERATOR:
                case SqlXmlConstants.ENAME_FUNCTION_KEYWORD:
                case SqlXmlConstants.ENAME_DATATYPE_KEYWORD:
                case SqlXmlConstants.ENAME_DDL_RETURNS:
                case SqlXmlConstants.ENAME_OTHERNODE:
                case SqlXmlConstants.ENAME_WHITESPACE:
                case SqlXmlConstants.ENAME_OTHEROPERATOR:
                case SqlXmlConstants.ENAME_OTHERKEYWORD:
                case SqlXmlConstants.ENAME_NUMBER_VALUE:
                case SqlXmlConstants.ENAME_MONETARY_VALUE:
                case SqlXmlConstants.ENAME_BINARY_VALUE:
                case SqlXmlConstants.ENAME_LABEL:
                    outString.Append(contentElement.InnerText);
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
        }


        public string FormatSQLTokens(Interfaces.ITokenList sqlTokenList)
        {
            StringBuilder outString = new StringBuilder();

            if (sqlTokenList.HasErrors)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING TOKENIZING!");

            foreach (var entry in sqlTokenList)
            {
                switch (entry.Type)
                {
                    case SqlTokenType.MultiLineComment:
                        outString.Append("/*");
                        outString.Append(entry.Value);
                        outString.Append("*/");
                        break;
                    case SqlTokenType.SingleLineComment:
                        outString.Append("--");
                        outString.Append(entry.Value);
                        break;
                    case SqlTokenType.String:
                        outString.Append("'");
                        outString.Append(entry.Value.Replace("'", "''"));
                        outString.Append("'");
                        break;
                    case SqlTokenType.NationalString:
                        outString.Append("N'");
                        outString.Append(entry.Value.Replace("'", "''"));
                        outString.Append("'");
                        break;
                    case SqlTokenType.QuotedString:
                        outString.Append("\"");
                        outString.Append(entry.Value.Replace("\"", "\"\""));
                        outString.Append("\"");
                        break;
                    case SqlTokenType.BracketQuotedName:
                        outString.Append("[");
                        outString.Append(entry.Value.Replace("]", "]]"));
                        outString.Append("]");
                        break;
                    case SqlTokenType.OpenParens:
                        outString.Append("(");
                        break;
                    case SqlTokenType.CloseParens:
                        outString.Append(")");
                        break;

                    case SqlTokenType.Comma:
                        outString.Append(",");
                        break;

                    case SqlTokenType.Period:
                        outString.Append(".");
                        break;

                    case SqlTokenType.Semicolon:
                        outString.Append(";");
                        break;

                    case SqlTokenType.Asterisk:
                        outString.Append("*");
                        break;

                    case SqlTokenType.OtherNode:
                    case SqlTokenType.WhiteSpace:
                    case SqlTokenType.OtherOperator:
                    case SqlTokenType.Number:
                    case SqlTokenType.BinaryValue:
                    case SqlTokenType.MonetaryValue:
                        outString.Append(entry.Value);
                        break;
                    default:
                        throw new Exception("Unrecognized Token Type in Token List!");
                }
            }

            return outString.ToString();
        }
    }
}
