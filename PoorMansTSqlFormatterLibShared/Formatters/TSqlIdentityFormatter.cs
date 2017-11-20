/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

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
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.ParseStructure;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters
{
    /// <summary>
    /// This formatter is intended to output *exactly the same content as initially parsed*, unless the 
    /// "HtmlColoring" option is enabled (then it should look the same in HTML, except for the coloring).
    /// </summary>
    public class TSqlIdentityFormatter : ISqlTokenFormatter, ISqlTreeFormatter
    {
        public TSqlIdentityFormatter() : this(false) { }
        public TSqlIdentityFormatter(bool htmlColoring)
        {
            HTMLColoring = htmlColoring;
            ErrorOutputPrefix = MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;
        }

        public bool HTMLColoring { get; set; }
        public bool HTMLFormatted { get { return HTMLColoring; } }
        public string ErrorOutputPrefix { get; set; }

        public string FormatSQLTree(Node sqlTreeDoc)
        {
            BaseFormatterState state = new BaseFormatterState(HTMLColoring);

            if (sqlTreeDoc.Name == SqlStructureConstants.ENAME_SQL_ROOT && sqlTreeDoc.GetAttributeValue(SqlStructureConstants.ANAME_ERRORFOUND) == "1")
                state.AddOutputContent(ErrorOutputPrefix);

            //pass "doc" itself into process: useful/necessary when formatting NOFORMAT sub-regions from standard formatter
            ProcessSqlNodeList(new[] { sqlTreeDoc }, state);
            return state.DumpOutput();
        }

        private static void ProcessSqlNodeList(IEnumerable<Node> rootList, BaseFormatterState state)
        {
            foreach (Node contentElement in rootList)
                ProcessSqlNode(state, contentElement);
        }

        private static void ProcessSqlNode(BaseFormatterState state, Node contentElement)
        {
            if (contentElement.GetAttributeValue(SqlStructureConstants.ANAME_HASERROR) == "1")
                state.OpenClass(SqlHtmlConstants.CLASS_ERRORHIGHLIGHT);

            switch (contentElement.Name)
            {
                case SqlStructureConstants.ENAME_DDLDETAIL_PARENS:
                case SqlStructureConstants.ENAME_DDL_PARENS:
				case SqlStructureConstants.ENAME_FUNCTION_PARENS:
				case SqlStructureConstants.ENAME_IN_PARENS:
				case SqlStructureConstants.ENAME_EXPRESSION_PARENS:
                case SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS:
                    state.AddOutputContent("(");
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.AddOutputContent(")");
                    break;

                case SqlStructureConstants.ENAME_SQL_ROOT:
                case SqlStructureConstants.ENAME_SQL_STATEMENT:
                case SqlStructureConstants.ENAME_SQL_CLAUSE:
                case SqlStructureConstants.ENAME_BOOLEAN_EXPRESSION:
                case SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case SqlStructureConstants.ENAME_DDL_OTHER_BLOCK:
                case SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK:
                case SqlStructureConstants.ENAME_CURSOR_DECLARATION:
                case SqlStructureConstants.ENAME_BEGIN_END_BLOCK:
                case SqlStructureConstants.ENAME_TRY_BLOCK:
                case SqlStructureConstants.ENAME_CATCH_BLOCK:
                case SqlStructureConstants.ENAME_CASE_STATEMENT:
                case SqlStructureConstants.ENAME_CASE_INPUT:
                case SqlStructureConstants.ENAME_CASE_WHEN:
                case SqlStructureConstants.ENAME_CASE_THEN:
                case SqlStructureConstants.ENAME_CASE_ELSE:
                case SqlStructureConstants.ENAME_IF_STATEMENT:
                case SqlStructureConstants.ENAME_ELSE_CLAUSE:
                case SqlStructureConstants.ENAME_WHILE_LOOP:
                case SqlStructureConstants.ENAME_DDL_AS_BLOCK:
                case SqlStructureConstants.ENAME_BETWEEN_CONDITION:
                case SqlStructureConstants.ENAME_BETWEEN_LOWERBOUND:
                case SqlStructureConstants.ENAME_BETWEEN_UPPERBOUND:
                case SqlStructureConstants.ENAME_CTE_WITH_CLAUSE:
                case SqlStructureConstants.ENAME_CTE_ALIAS:
                case SqlStructureConstants.ENAME_CTE_AS_BLOCK:
                case SqlStructureConstants.ENAME_CURSOR_FOR_BLOCK:
                case SqlStructureConstants.ENAME_CURSOR_FOR_OPTIONS:
                case SqlStructureConstants.ENAME_TRIGGER_CONDITION:
                case SqlStructureConstants.ENAME_COMPOUNDKEYWORD:
                case SqlStructureConstants.ENAME_BEGIN_TRANSACTION:
                case SqlStructureConstants.ENAME_ROLLBACK_TRANSACTION:
                case SqlStructureConstants.ENAME_SAVE_TRANSACTION:
                case SqlStructureConstants.ENAME_COMMIT_TRANSACTION:
                case SqlStructureConstants.ENAME_BATCH_SEPARATOR:
                case SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE:
                case SqlStructureConstants.ENAME_CONTAINER_OPEN:
                case SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT:
                case SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT:
                case SqlStructureConstants.ENAME_CONTAINER_CLOSE:
                case SqlStructureConstants.ENAME_SELECTIONTARGET:
                case SqlStructureConstants.ENAME_PERMISSIONS_BLOCK:
                case SqlStructureConstants.ENAME_PERMISSIONS_DETAIL:
                case SqlStructureConstants.ENAME_PERMISSIONS_TARGET:
                case SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT:
                case SqlStructureConstants.ENAME_DDL_WITH_CLAUSE:
                case SqlStructureConstants.ENAME_MERGE_CLAUSE:
                case SqlStructureConstants.ENAME_MERGE_TARGET:
                case SqlStructureConstants.ENAME_MERGE_USING:
                case SqlStructureConstants.ENAME_MERGE_CONDITION:
                case SqlStructureConstants.ENAME_MERGE_WHEN:
                case SqlStructureConstants.ENAME_MERGE_THEN:
                case SqlStructureConstants.ENAME_MERGE_ACTION:
                case SqlStructureConstants.ENAME_JOIN_ON_SECTION:
                case SqlStructureConstants.ENAME_DDL_RETURNS:
                    foreach (Node childNode in contentElement.Children)
                        ProcessSqlNode(state, childNode);
                    break;

                case SqlStructureConstants.ENAME_COMMENT_MULTILINE:
                    state.AddOutputContent("/*" + contentElement.TextValue + "*/", SqlHtmlConstants.CLASS_COMMENT);
                    break;
                case SqlStructureConstants.ENAME_COMMENT_SINGLELINE:
                    state.AddOutputContent("--" + contentElement.TextValue, SqlHtmlConstants.CLASS_COMMENT);
                    break;
                case SqlStructureConstants.ENAME_COMMENT_SINGLELINE_CSTYLE:
                    state.AddOutputContent("//" + contentElement.TextValue, SqlHtmlConstants.CLASS_COMMENT);
                    break;
                case SqlStructureConstants.ENAME_STRING:
                    state.AddOutputContent("'" + contentElement.TextValue.Replace("'", "''") + "'", SqlHtmlConstants.CLASS_STRING);
                    break;
                case SqlStructureConstants.ENAME_NSTRING:
                    state.AddOutputContent("N'" + contentElement.TextValue.Replace("'", "''") + "'", SqlHtmlConstants.CLASS_STRING);
                    break;
                case SqlStructureConstants.ENAME_QUOTED_STRING:
                    state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
                    break;
                case SqlStructureConstants.ENAME_BRACKET_QUOTED_NAME:
                    state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
                    break;

                case SqlStructureConstants.ENAME_COMMA:
                case SqlStructureConstants.ENAME_PERIOD:
                case SqlStructureConstants.ENAME_SEMICOLON:
                case SqlStructureConstants.ENAME_ASTERISK:
                case SqlStructureConstants.ENAME_EQUALSSIGN:
                case SqlStructureConstants.ENAME_SCOPERESOLUTIONOPERATOR:
                case SqlStructureConstants.ENAME_ALPHAOPERATOR:
                case SqlStructureConstants.ENAME_OTHEROPERATOR:
                    state.AddOutputContent(contentElement.TextValue, SqlHtmlConstants.CLASS_OPERATOR);
                    break;

                case SqlStructureConstants.ENAME_AND_OPERATOR:
                case SqlStructureConstants.ENAME_OR_OPERATOR:
                    state.AddOutputContent(contentElement.ChildByName(SqlStructureConstants.ENAME_OTHERKEYWORD).TextValue, SqlHtmlConstants.CLASS_OPERATOR);
                    break;

                case SqlStructureConstants.ENAME_FUNCTION_KEYWORD:
                    state.AddOutputContent(contentElement.TextValue, SqlHtmlConstants.CLASS_FUNCTION);
                    break;

                case SqlStructureConstants.ENAME_OTHERKEYWORD:
                case SqlStructureConstants.ENAME_DATATYPE_KEYWORD:
                case SqlStructureConstants.ENAME_PSEUDONAME:
                    state.AddOutputContent(contentElement.TextValue, SqlHtmlConstants.CLASS_KEYWORD);
                    break;

                case SqlStructureConstants.ENAME_OTHERNODE:
                case SqlStructureConstants.ENAME_WHITESPACE:
                case SqlStructureConstants.ENAME_NUMBER_VALUE:
                case SqlStructureConstants.ENAME_MONETARY_VALUE:
                case SqlStructureConstants.ENAME_BINARY_VALUE:
                case SqlStructureConstants.ENAME_LABEL:
                    state.AddOutputContent(contentElement.TextValue);
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }

            if (contentElement.GetAttributeValue(SqlStructureConstants.ANAME_HASERROR) == "1")
                state.CloseClass();
        }


        public string FormatSQLTokens(ITokenList sqlTokenList)
        {
            StringBuilder outString = new StringBuilder();

            if (sqlTokenList.HasUnfinishedToken)
                outString.Append(ErrorOutputPrefix);

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
                    case SqlTokenType.SingleLineCommentCStyle:
                        outString.Append("//");
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
                    case SqlTokenType.CloseParens:
                    case SqlTokenType.Comma:
                    case SqlTokenType.Period:
                    case SqlTokenType.Semicolon:
                    case SqlTokenType.Colon:
                    case SqlTokenType.Asterisk:
                    case SqlTokenType.EqualsSign:
                    case SqlTokenType.OtherNode:
                    case SqlTokenType.WhiteSpace:
                    case SqlTokenType.OtherOperator:
                    case SqlTokenType.Number:
                    case SqlTokenType.BinaryValue:
                    case SqlTokenType.MonetaryValue:
                    case SqlTokenType.PseudoName:
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
