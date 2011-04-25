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
    public class TSqlIdentityFormatter : Interfaces.ISqlTokenFormatter, Interfaces.ISqlTreeFormatter
    {
        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            return FormatSQLDoc(sqlTreeDoc, Interfaces.Constants.ENAME_SQL_ROOT);
        }

        public string FormatSQLTokens(XmlDocument sqlTokenDoc)
        {
            return FormatSQLDoc(sqlTokenDoc, Interfaces.Constants.ENAME_SQLTOKENS_ROOT);
        }

        private string FormatSQLDoc(XmlDocument sqlTokenOrTreeDoc, string rootElement)
        {
            StringBuilder outString = new StringBuilder();
            if (sqlTokenOrTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", rootElement, Interfaces.Constants.ANAME_ERRORFOUND)) != null)
                outString.AppendLine("--WARNING! ERRORS ENCOUNTERED DURING PARSING!");

            XmlNodeList rootList = sqlTokenOrTreeDoc.SelectNodes(string.Format("/{0}/*", rootElement));
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
                case Interfaces.Constants.ENAME_SQL_STATEMENT:
                case Interfaces.Constants.ENAME_SQL_CLAUSE:
                case Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION:
                case Interfaces.Constants.ENAME_DDL_BLOCK:
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"));
                    break;

                case Interfaces.Constants.ENAME_DDLDETAIL_PARENS:
                case Interfaces.Constants.ENAME_DDL_PARENS:
                case Interfaces.Constants.ENAME_FUNCTION_PARENS:
                case Interfaces.Constants.ENAME_EXPRESSION_PARENS:
                    outString.Append("(");
                    ProcessSqlNodeList(outString, contentElement.SelectNodes("*"));
                    outString.Append(")");
                    break;

                case Interfaces.Constants.ENAME_BEGIN_END_BLOCK:
                case Interfaces.Constants.ENAME_TRY_BLOCK:
                case Interfaces.Constants.ENAME_CASE_STATEMENT:
                case Interfaces.Constants.ENAME_IF_STATEMENT:
                case Interfaces.Constants.ENAME_ELSE_CLAUSE:
                case Interfaces.Constants.ENAME_WHILE_LOOP:
                case Interfaces.Constants.ENAME_DDL_AS_BLOCK:
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

                case Interfaces.Constants.ENAME_COMMENT_MULTILINE:
                    outString.Append("/*");
                    outString.Append(contentElement.InnerText);
                    outString.Append("*/");
                    break;
                case Interfaces.Constants.ENAME_COMMENT_SINGLELINE:
                    outString.Append("--");
                    outString.Append(contentElement.InnerText);
                    break;
                case Interfaces.Constants.ENAME_STRING:
                    outString.Append("'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case Interfaces.Constants.ENAME_NSTRING:
                    outString.Append("N'");
                    outString.Append(contentElement.InnerText.Replace("'", "''"));
                    outString.Append("'");
                    break;
                case Interfaces.Constants.ENAME_QUOTED_IDENTIFIER:
                    outString.Append("[");
                    outString.Append(contentElement.InnerText.Replace("]", "]]"));
                    outString.Append("]");
                    break;
                case Interfaces.Constants.ENAME_PARENS_OPEN:
                    outString.Append("(");
                    break;
                case Interfaces.Constants.ENAME_PARENS_CLOSE:
                    outString.Append(")");
                    break;

                case Interfaces.Constants.ENAME_COMMA:
                    outString.Append(",");
                    break;
                case Interfaces.Constants.ENAME_ASTERISK:
                    outString.Append("*");
                    break;
                case Interfaces.Constants.ENAME_BEGIN_TRANSACTION:
                case Interfaces.Constants.ENAME_OTHERNODE:
                case Interfaces.Constants.ENAME_WHITESPACE:
                case Interfaces.Constants.ENAME_OTHEROPERATOR:
                case Interfaces.Constants.ENAME_BATCH_SEPARATOR:
                case Interfaces.Constants.ENAME_AND_OPERATOR:
                case Interfaces.Constants.ENAME_OR_OPERATOR:
                    outString.Append(contentElement.InnerText);
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
        }

    }
}
