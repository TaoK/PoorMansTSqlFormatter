/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

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
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.Interfaces;
using System.Linq;

namespace PoorMansTSqlFormatterLib.Parsers
{
    public class TSqlStandardParser : Interfaces.ISqlTokenParser
    {
        /*
         * TODO:
         *  - handle Ranking Functions with multiple partition or order by columns/clauses
         *  - detect table hints, to avoid them looking like function parens
         *  - Handle DDL triggers
         *  - Detect ALTER keywords that are clauses of other statements, vs those that are statements
         *  
         *  - Tests
         *    - Samples illustrating all the tokens and container combinations implemented
         *    - Samples illustrating all forms of container violations
         *    - Sample requests and their XML equivalent - once the xml format is more-or-less formalized
         *    - Sample requests and their formatted versions (a few for each) - once the "standard" format is more-or-less formalized
         */

        //yay for static constructors!
        public static Dictionary<string, KeywordType> KeywordList { get; set; }
        static TSqlStandardParser()
        {
            InitializeKeywordList();
            //temporary, to convince VisualStudio to copy the LinqBridge DLL, otherwise ILMerge fails because of the missing file.
            // - maybe instead I should remove LinqBridge, as I'm not using it at the moment...
            KeywordList.Take(3);
        }

        static Regex _JoinDetector = new Regex("^((RIGHT|INNER|LEFT|CROSS|FULL) )?(OUTER )?((HASH|LOOP|MERGE|REMOTE) )?(JOIN|APPLY) ");
        static Regex _CursorDetector = new Regex(@"^DECLARE [\p{L}0-9_\$\@\#]+ ((INSENSITIVE|SCROLL) ){0,2}CURSOR "); //note the use of "unicode letter" in identifier rule
        static Regex _TriggerConditionDetector = new Regex(@"^(FOR|AFTER|INSTEAD OF)( (INSERT|UPDATE|DELETE) (, (INSERT|UPDATE|DELETE) )?(, (INSERT|UPDATE|DELETE) )?)"); //note the use of "unicode letter" in identifier rule

        public XmlDocument ParseSQL(ITokenList tokenList)
        {
            ParseTree sqlTree = new ParseTree(SqlXmlConstants.ENAME_SQL_ROOT);
            sqlTree.StartNewStatement();

            int tokenCount = tokenList.Count;
            int tokenID = 0;
            while (tokenID < tokenCount)
            {
                IToken token = tokenList[tokenID];

                switch (token.Type)
                {
                    case SqlTokenType.OpenParens:
						XmlElement firstNonCommentParensSibling = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
						XmlElement lastNonCommentParensSibling = sqlTree.GetLastNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
						bool isInsertOrValuesClause = (
                            firstNonCommentParensSibling != null
                            && (
                                (firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                   && firstNonCommentParensSibling.InnerText.ToUpperInvariant().StartsWith("INSERT")
                                   )
                                || 
                                (firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_COMPOUNDKEYWORD)
                                   && firstNonCommentParensSibling.GetAttribute(SqlXmlConstants.ANAME_SIMPLETEXT).ToUpperInvariant().StartsWith("INSERT ")
                                   )
                                ||
                                (firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                   && firstNonCommentParensSibling.InnerText.ToUpperInvariant().StartsWith("VALUES")
                                   )
                               )
                            );

                        if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_CTE_ALIAS)
                            && sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CTE_WITH_CLAUSE)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_PARENS, "");
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CTE_AS_BLOCK)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        else if (firstNonCommentParensSibling == null
                            && sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SELECTIONTARGET)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        else if (firstNonCommentParensSibling != null
                            && firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE)
                            )
                        {
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        }
                        else if (IsLatestTokenADDLDetailValue(sqlTree))
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDLDETAIL_PARENS, "");
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            || sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                            || sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK)
                            || (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE) 
                                && (firstNonCommentParensSibling != null
                                    && firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                    && firstNonCommentParensSibling.InnerText.ToUpperInvariant().StartsWith("OPTION")
                                    )
                                )
                            || isInsertOrValuesClause
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_PARENS, "");
						else if ((lastNonCommentParensSibling != null
									&& lastNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_ALPHAOPERATOR)
									&& lastNonCommentParensSibling.InnerText.ToUpperInvariant().Equals("IN")
									)
							)
							sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_IN_PARENS, "");
						else if (IsLatestTokenAMiscName(sqlTree.CurrentContainer))
							sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_FUNCTION_PARENS, "");
						else
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_EXPRESSION_PARENS, "");
                        break;

                    case SqlTokenType.CloseParens:
                        //we're not likely to actually have a "SingleStatement" in parens, but 
                        // we definitely want the side-effects (all the lower-level escapes)
                        sqlTree.EscapeAnySingleOrPartialStatementContainers();

                        //check whether we expected to end the parens...
                        if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                            || sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_FUNCTION_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                            || sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS)
                            )
                        {
                            sqlTree.MoveToAncestorContainer(1); //unspecified parent node...
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS)
                                && sqlTree.CurrentContainer.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.CurrentContainer.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CTE_AS_BLOCK)
                                )
                        {
                            sqlTree.MoveToAncestorContainer(4, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT, "");
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && (
                                    sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
									|| sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
									|| sqlTree.CurrentContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS)
                                )
                            )
                        {
                            sqlTree.MoveToAncestorContainer(2); //unspecified grandfather node.
                        }
                        else
                        {
                            sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERNODE, ")");
                        }
                        break;

                    case SqlTokenType.OtherNode:

                        //prepare multi-keyword detection by "peeking" up to 7 keywords ahead
                        List<int> significantTokenPositions = GetSignificantTokenPositions(tokenList, tokenID, 7);
                        string significantTokensString = ExtractTokensString(tokenList, significantTokenPositions);

                        if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_PERMISSIONS_DETAIL))
                        {
                            //if we're in a permissions detail clause, we can expect all sorts of statements 
                            // starters and should ignore them all; the only possible keywords to escape are
                            // "ON" and "TO".
                            if (significantTokensString.StartsWith("ON "))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_PERMISSIONS_TARGET, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (significantTokensString.StartsWith("TO ")
                                || significantTokensString.StartsWith("FROM ")
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else 
                            {
                                //default to "some classification of permission"
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("CREATE PROC")
                            || significantTokensString.StartsWith("CREATE FUNC")
                            || significantTokensString.StartsWith("CREATE TRIGGER ")
                            || significantTokensString.StartsWith("CREATE VIEW ")
                            || significantTokensString.StartsWith("ALTER PROC")
                            || significantTokensString.StartsWith("ALTER FUNC")
                            || significantTokensString.StartsWith("ALTER TRIGGER ")
                            || significantTokensString.StartsWith("ALTER VIEW ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (_CursorDetector.IsMatch(significantTokensString))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CURSOR_DECLARATION, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            && _TriggerConditionDetector.IsMatch(significantTokensString)
                            )
                        {
                            //horrible complicated forward-search, to avoid having to keep a different "Trigger Condition" state for Update, Insert and Delete statement-starting keywords 
                            Match triggerConditions = _TriggerConditionDetector.Match(significantTokensString);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_TRIGGER_CONDITION, "");
                            XmlElement triggerConditionType = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_COMPOUNDKEYWORD, "");

                            //first set the "trigger condition type": FOR, INSTEAD OF, AFTER
                            string triggerConditionTypeSimpleText = triggerConditions.Groups[1].Value;
                            triggerConditionType.SetAttribute(SqlXmlConstants.ANAME_SIMPLETEXT, triggerConditionTypeSimpleText);
                            int triggerConditionTypeNodeCount = triggerConditionTypeSimpleText.Split(new char[] { ' ' }).Length; //there's probably a better way of counting words...
                            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[triggerConditionTypeNodeCount - 1]), SqlXmlConstants.ENAME_OTHERKEYWORD, triggerConditionType);

                            //then get the count of conditions (INSERT, UPDATE, DELETE) and add those too...
                            int triggerConditionNodeCount = triggerConditions.Groups[2].Value.Split(new char[] { ' ' }).Length - 2; //there's probably a better way of counting words...
                            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[triggerConditionTypeNodeCount - 1] + 1, significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1]), SqlXmlConstants.ENAME_OTHERKEYWORD, sqlTree.CurrentContainer);
                            tokenID = significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1];
                            sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK);
                        }
                        else if (significantTokensString.StartsWith("FOR "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.EscapeAnySelectionTarget();
                            sqlTree.EscapeJoinCondition();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CURSOR_DECLARATION))
                            {
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                                sqlTree.StartNewStatement();
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(3, SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(4, SqlXmlConstants.ENAME_CURSOR_DECLARATION);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                //Assume FOR clause if we're at clause level
                                // (otherwise, eg in OPTIMIZE FOR UNKNOWN, this will just not do anything)
                                sqlTree.ConsiderStartingNewClause();

                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("DECLARE "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("CREATE ")
                            || significantTokensString.StartsWith("ALTER ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_OTHER_BLOCK, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("GRANT ")
                            || significantTokensString.StartsWith("DENY ")
                            || significantTokensString.StartsWith("REVOKE ")
                            )
                        {
                            if (significantTokensString.StartsWith("GRANT ")
                                && sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                                && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK)
                                && sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer) == null
                                )
                            {
                                //this MUST be a "WITH GRANT OPTION" option...
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_PERMISSIONS_BLOCK, token.Value, SqlXmlConstants.ENAME_PERMISSIONS_DETAIL);
                            }
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            && significantTokensString.StartsWith("RETURNS ")
                            )
                        {
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_RETURNS, ""));
                        }
                        else if (significantTokensString.StartsWith("AS "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK))
                            {
                                KeywordType nextKeywordType;
                                bool isDataTypeDefinition = false;
                                if (significantTokenPositions.Count > 1
                                    && KeywordList.TryGetValue(tokenList[significantTokenPositions[1]].Value, out nextKeywordType)
                                    )
                                    if (nextKeywordType == KeywordType.DataTypeKeyword)
                                        isDataTypeDefinition = true;

                                if (isDataTypeDefinition)
                                {
                                    //this is actually a data type declaration (redundant "AS"...), save as regular token.
                                    sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                                else
                                {
                                    //this is the start of the object content definition
                                    sqlTree.StartNewContainer(SqlXmlConstants.ENAME_DDL_AS_BLOCK, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                                    sqlTree.StartNewStatement();
                                }
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                                && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_DDL_AS_BLOCK, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                                sqlTree.StartNewStatement();
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CTE_ALIAS)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CTE_AS_BLOCK, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("BEGIN DISTRIBUTED TRANSACTION ")
                            || significantTokensString.StartsWith("BEGIN DISTRIBUTED TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BEGIN_TRANSACTION, ""), ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("BEGIN TRANSACTION ")
                            || significantTokensString.StartsWith("BEGIN TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BEGIN_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("SAVE TRANSACTION ")
                            || significantTokensString.StartsWith("SAVE TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SAVE_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("COMMIT TRANSACTION ")
                            || significantTokensString.StartsWith("COMMIT TRAN ")
                            || significantTokensString.StartsWith("COMMIT WORK ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_COMMIT_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("COMMIT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_COMMIT_TRANSACTION, token.Value));
                        }
                        else if (significantTokensString.StartsWith("ROLLBACK TRANSACTION ")
                            || significantTokensString.StartsWith("ROLLBACK TRAN ")
                            || significantTokensString.StartsWith("ROLLBACK WORK ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("ROLLBACK "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION, token.Value));
                        }
                        else if (significantTokensString.StartsWith("BEGIN TRY "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            XmlElement newTryBlock = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_TRY_BLOCK, "");
                            XmlElement tryContainerOpen = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, "", newTryBlock);
                            ProcessCompoundKeyword(tokenList, sqlTree, tryContainerOpen, ref tokenID, significantTokenPositions, 2);
                            XmlElement tryMultiContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT, "", newTryBlock);
                            sqlTree.StartNewStatement(tryMultiContainer);
                        }
                        else if (significantTokensString.StartsWith("BEGIN CATCH "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            XmlElement newCatchBlock = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CATCH_BLOCK, "");
                            XmlElement catchContainerOpen = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, "", newCatchBlock);
                            ProcessCompoundKeyword(tokenList, sqlTree, catchContainerOpen, ref tokenID, significantTokenPositions, 2);
                            XmlElement catchMultiContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT, "", newCatchBlock);
                            sqlTree.StartNewStatement(catchMultiContainer);
                        }
                        else if (significantTokensString.StartsWith("BEGIN "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.StartNewContainer(SqlXmlConstants.ENAME_BEGIN_END_BLOCK, token.Value, SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT);
                            sqlTree.StartNewStatement();
                        }
                        else if (significantTokensString.StartsWith("MERGE "))
                        {
                            //According to BOL, MERGE is a fully reserved keyword from compat 100 onwards, for the MERGE statement only.
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.StartNewContainer(SqlXmlConstants.ENAME_MERGE_CLAUSE, token.Value, SqlXmlConstants.ENAME_MERGE_TARGET);
                        }
                        else if (significantTokensString.StartsWith("USING "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_MERGE_TARGET))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_MERGE_CLAUSE);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_MERGE_USING, token.Value, SqlXmlConstants.ENAME_SELECTIONTARGET);
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("ON "))
                        {
                            sqlTree.EscapeAnySelectionTarget();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_MERGE_USING))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_MERGE_CLAUSE);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_MERGE_CONDITION, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (!sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                && !sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                                && !sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                                && !sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                                && !ContentStartsWithKeyword(sqlTree.CurrentContainer, "SET")
                                )
                            {
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_JOIN_ON_SECTION, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("CASE "))
                        {
                            sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CASE_STATEMENT, token.Value, SqlXmlConstants.ENAME_CASE_INPUT);
                        }
                        else if (significantTokensString.StartsWith("WHEN "))
                        {
                            sqlTree.EscapeMergeAction();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CASE_INPUT)
                                || (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CASE_THEN)
                                    )
                                )
                            {
                                if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CASE_INPUT))
                                    sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_CASE_STATEMENT);
                                else
                                    sqlTree.MoveToAncestorContainer(3, SqlXmlConstants.ENAME_CASE_STATEMENT);

                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CASE_WHEN, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if ((sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_MERGE_CONDITION)
                                    )
                                || sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_MERGE_WHEN)
                                )
                            {
                                if (sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_MERGE_CONDITION))
                                    sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_MERGE_CLAUSE);
                                else
                                    sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_MERGE_CLAUSE);

                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_MERGE_WHEN, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("THEN "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CASE_WHEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_CASE_WHEN);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CASE_THEN, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_MERGE_WHEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_MERGE_WHEN);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_MERGE_THEN, token.Value, SqlXmlConstants.ENAME_MERGE_ACTION);
                                sqlTree.StartNewStatement();
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("OUTPUT "))
                        {
                            bool isSprocArgument = false;

                            //We're looking for sproc calls - they can't be nested inside anything else (as far as I know)
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXEC")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXECUTE")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, null)
                                    )
                                )
                            {
                                isSprocArgument = true;
                            }

                            //Also proc definitions - argument lists without parens
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK))
                                isSprocArgument = true;

                            if (!isSprocArgument)
                            {
                                sqlTree.EscapeMergeAction();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("OPTION "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                                )
                            {
                                //"OPTION" keyword here is NOT indicative of a new clause.
                            }
                            else
                            {
                                sqlTree.EscapeMergeAction();
                                sqlTree.ConsiderStartingNewClause();
                            }
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("END TRY "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                && sqlTree.PathNameMatches(3, SqlXmlConstants.ENAME_TRY_BLOCK)
                                )
                            {
                                //clause.statement.multicontainer.try
                                XmlElement tryBlock = (XmlElement)sqlTree.CurrentContainer.ParentNode.ParentNode.ParentNode;
                                XmlElement tryContainerClose = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, "", tryBlock);
                                ProcessCompoundKeyword(tokenList, sqlTree, tryContainerClose, ref tokenID, significantTokenPositions, 2);
                                sqlTree.CurrentContainer = (XmlElement)tryBlock.ParentNode;
                            }
                            else
                            {
                                ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                            }
                        }
                        else if (significantTokensString.StartsWith("END CATCH "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                && sqlTree.PathNameMatches(3, SqlXmlConstants.ENAME_CATCH_BLOCK)
                                )
                            {
                                //clause.statement.multicontainer.catch
                                XmlElement catchBlock = (XmlElement)sqlTree.CurrentContainer.ParentNode.ParentNode.ParentNode;
                                XmlElement catchContainerClose = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, "", catchBlock);
                                ProcessCompoundKeyword(tokenList, sqlTree, catchContainerClose, ref tokenID, significantTokenPositions, 2);
                                sqlTree.CurrentContainer = (XmlElement)catchBlock.ParentNode;
                            }
                            else
                            {
                                ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                            }
                        }
                        else if (significantTokensString.StartsWith("END "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CASE_THEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(3, SqlXmlConstants.ENAME_CASE_STATEMENT);
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.MoveToAncestorContainer(1); //unnamed container
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CASE_ELSE)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_CASE_STATEMENT);
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.MoveToAncestorContainer(1); //unnamed container
                            }
                            else
                            {
                                //Begin/End block handling
                                sqlTree.EscapeAnySingleOrPartialStatementContainers();

                                if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                    && sqlTree.PathNameMatches(3, SqlXmlConstants.ENAME_BEGIN_END_BLOCK)
                                    )
                                {
                                    XmlElement beginBlock = (XmlElement)sqlTree.CurrentContainer.ParentNode.ParentNode.ParentNode;
                                    XmlElement beginContainerClose = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, "", beginBlock);
                                    sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, beginContainerClose);
                                    sqlTree.CurrentContainer = (XmlElement)beginBlock.ParentNode;
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                        }
                        else if (significantTokensString.StartsWith("GO "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if ((tokenID == 0 || IsLineBreakingWhiteSpaceOrComment(tokenList[tokenID - 1]))
                                && IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(tokenList, tokenID)
                                )
                            {
                                // we found a batch separator - were we supposed to?
                                if (sqlTree.FindValidBatchEnd())
                                {
                                    XmlElement sqlRoot = sqlTree.DocumentElement;
                                    XmlElement batchSeparator = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BATCH_SEPARATOR, "", sqlRoot);
                                    sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, batchSeparator);
                                    sqlTree.StartNewStatement(sqlRoot);
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("EXECUTE AS "))
                        {
                            bool executeAsInWithOptions = false;
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                                && (IsLatestTokenAComma(sqlTree)
                                    || !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer)
                                    )
                                )
                                executeAsInWithOptions = true;

                            if (!executeAsInWithOptions)
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("EXEC ")
                            || significantTokensString.StartsWith("EXECUTE ")
                            )
                        {
                            bool execShouldntTryToStartNewStatement = false;

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT INTO")
                                    )
                                )
                            {
                                int existingClauseCount = sqlTree.CurrentContainer.SelectNodes(string.Format("../{0}", SqlXmlConstants.ENAME_SQL_CLAUSE)).Count;
                                if (existingClauseCount == 1)
                                    execShouldntTryToStartNewStatement = true;
                            }

                            if (!execShouldntTryToStartNewStatement)
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();

                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (_JoinDetector.IsMatch(significantTokensString))
                        {
                            sqlTree.ConsiderStartingNewClause();
                            string joinText = _JoinDetector.Match(significantTokensString).Value;
                            int targetKeywordCount = joinText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, targetKeywordCount);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SELECTIONTARGET, "");
                        }
                        else if (significantTokensString.StartsWith("UNION ALL "))
                        {
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("UNION ")
                            || significantTokensString.StartsWith("INTERSECT ")
                            || significantTokensString.StartsWith("EXCEPT ")
                            )
                        {
                            sqlTree.ConsiderStartingNewClause();
                            XmlElement unionClause = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, unionClause);
                        }
                        else if (significantTokensString.StartsWith("WHILE "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            XmlElement newWhileLoop = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_WHILE_LOOP, "");
                            XmlElement whileContainerOpen = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, "", newWhileLoop);
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, whileContainerOpen);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION, "", newWhileLoop);
                        }
                        else if (significantTokensString.StartsWith("IF "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.StartNewContainer(SqlXmlConstants.ENAME_IF_STATEMENT, token.Value, SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION);
                        }
                        else if (significantTokensString.StartsWith("ELSE "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.EscapeAnySelectionTarget();
                            sqlTree.EscapeJoinCondition();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CASE_THEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(3, SqlXmlConstants.ENAME_CASE_STATEMENT);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_CASE_ELSE, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.EscapePartialStatementContainers();

                                if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && sqlTree.PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                                    )
                                {
                                    //we need to pop up the single-statement containers stack to the next "if" that doesn't have an "else" (if any; else error).
                                    // LOCAL SEARCH - we're not actually changing the "CurrentContainer" until we decide to start a statement.
                                    XmlElement currentNode = (XmlElement)sqlTree.CurrentContainer.ParentNode.ParentNode;
                                    bool stopSearching = false;
                                    while (!stopSearching)
                                    {
                                        if (sqlTree.PathNameMatches(currentNode, 1, SqlXmlConstants.ENAME_IF_STATEMENT))
                                        {
                                            //if this is in an "If", then the "Else" must still be available - yay!
                                            sqlTree.CurrentContainer = (XmlElement)currentNode.ParentNode;
                                            sqlTree.StartNewContainer(SqlXmlConstants.ENAME_ELSE_CLAUSE, token.Value, SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT);
                                            sqlTree.StartNewStatement();
                                            stopSearching = true;
                                        }
                                        else if (sqlTree.PathNameMatches(currentNode, 1, SqlXmlConstants.ENAME_ELSE_CLAUSE))
                                        {
                                            //If this is in an "Else", we should skip its parent "IF" altogether, and go to the next singlestatementcontainer candidate.
                                            //singlestatementcontainer.else.if.clause.statement.NEWCANDIDATE
                                            currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode;
                                        }
                                        else if (sqlTree.PathNameMatches(currentNode, 1, SqlXmlConstants.ENAME_WHILE_LOOP))
                                        {
                                            //If this is in a "While", we should skip to the next singlestatementcontainer candidate.
                                            //singlestatementcontainer.while.clause.statement.NEWCANDIDATE
                                            currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode.ParentNode;
                                        }
                                        else
                                        {
                                            //if this isn't a known single-statement container, then we're lost.
                                            sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                            stopSearching = true;
                                        }
                                    }
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                        }
                        else if (significantTokensString.StartsWith("INSERT INTO "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHARACTER VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHAR VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("BINARY VARYING "))
                        {
                            //TODO: Figure out how to handle "Compound Keyword Datatypes" so they are still correctly highlighted
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("CHAR VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("CHARACTER VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("DOUBLE PRECISION "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHARACTER "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHAR "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL TEXT "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("INSERT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("BULK INSERT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("SELECT "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            bool selectShouldntTryToStartNewStatement = false;

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE))
                            {
                                XmlElement firstStatementClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer.ParentNode);

                                bool isPrecededByInsertStatement = false;
                                foreach (XmlElement clause in sqlTree.CurrentContainer.ParentNode.SelectNodes(SqlXmlConstants.ENAME_SQL_CLAUSE))
                                    if (ContentStartsWithKeyword(clause, "INSERT"))
                                        isPrecededByInsertStatement = true;

                                if (isPrecededByInsertStatement)
                                {
                                    bool existingSelectClauseFound = false;
                                    foreach (XmlElement clause in sqlTree.CurrentContainer.ParentNode.SelectNodes(SqlXmlConstants.ENAME_SQL_CLAUSE))
                                        if (ContentStartsWithKeyword(clause, "SELECT"))
                                            existingSelectClauseFound = true;

                                    bool existingValuesClauseFound = false;
                                    foreach (XmlElement clause in sqlTree.CurrentContainer.ParentNode.SelectNodes(SqlXmlConstants.ENAME_SQL_CLAUSE))
                                        if (ContentStartsWithKeyword(clause, "VALUES"))
                                            existingValuesClauseFound = true;

                                    bool existingExecClauseFound = false;
                                    foreach (XmlElement clause in sqlTree.CurrentContainer.ParentNode.SelectNodes(SqlXmlConstants.ENAME_SQL_CLAUSE))
                                        if (ContentStartsWithKeyword(clause, "EXEC")
                                            || ContentStartsWithKeyword(clause, "EXECUTE"))
                                            existingExecClauseFound = true;

                                    if (!existingSelectClauseFound
                                        && !existingValuesClauseFound
                                        && !existingExecClauseFound
                                        )
                                        selectShouldntTryToStartNewStatement = true;
                                }

                                XmlElement firstEntryOfThisClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
                                if (firstEntryOfThisClause != null && firstEntryOfThisClause.Name.Equals(SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE))
                                    selectShouldntTryToStartNewStatement = true;
                            }

                            if (!selectShouldntTryToStartNewStatement)
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();

                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("UPDATE "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            if (!(sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS)
                                    )
                                )
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("TO "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_PERMISSIONS_TARGET)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                //I don't currently know whether there is any other place where "TO" can be used in T-SQL...
                                // TODO: look into that.
                                // -> for now, we'll just save as a random keyword without raising an error.
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("FROM "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_PERMISSIONS_TARGET)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewClause();
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SELECTIONTARGET, "");
                            }
                        }
                        else if (significantTokensString.StartsWith("CASCADE ")
                            && sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT)
                            )
                        {
                            sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT, "", sqlTree.SaveNewElement(SqlXmlConstants.ENAME_DDL_WITH_CLAUSE, ""));
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("SET "))
                        {
                            XmlElement firstNonCommentSibling2 = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
                            if (!(
                                    firstNonCommentSibling2 != null
                                    && firstNonCommentSibling2.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                    && firstNonCommentSibling2.InnerText.ToUpperInvariant().StartsWith("UPDATE")
                                    )
                                )
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("BETWEEN "))
                        {
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BETWEEN_CONDITION, "");
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, ""));
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND, "");
                        }
                        else if (significantTokensString.StartsWith("AND "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_BETWEEN_CONDITION);
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND, "");
                            }
                            else
                            {
                                sqlTree.EscapeAnyBetweenConditions();
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_AND_OPERATOR, ""));
                            }
                        }
                        else if (significantTokensString.StartsWith("OR "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OR_OPERATOR, ""));
                        }
                        else if (significantTokensString.StartsWith("WITH "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer)
                                )
                            {
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CTE_WITH_CLAUSE, "");
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, ""));
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CTE_ALIAS, "");
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_DDL_WITH_CLAUSE, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                || sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                                )
                            {
                                sqlTree.StartNewContainer(SqlXmlConstants.ENAME_DDL_WITH_CLAUSE, token.Value, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SELECTIONTARGET))
                            {
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewClause();
                                sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.Colon
                            && !(tokenList.Count > tokenID + 2
                                && tokenList[tokenID + 2].Type == SqlTokenType.Colon
                                )
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_LABEL, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else
                        {
                            //miscellaneous single-word tokens, which may or may not be statement starters and/or clause starters

                            //check for statements starting...
                            if (IsStatementStarter(token) || sqlTree.NewStatementDue)
                            {
                                sqlTree.ConsiderStartingNewStatement();
                            }

                            //check for statements starting...
                            if (IsClauseStarter(token))
                            {
                                sqlTree.ConsiderStartingNewClause();
                            }

                            string newNodeName = SqlXmlConstants.ENAME_OTHERNODE;
                            KeywordType matchedKeywordType;
                            if (KeywordList.TryGetValue(token.Value, out matchedKeywordType))
                            {
                                switch (matchedKeywordType)
                                {
                                    case KeywordType.OperatorKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_ALPHAOPERATOR;
                                        break;
                                    case KeywordType.FunctionKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_FUNCTION_KEYWORD;
                                        break;
                                    case KeywordType.DataTypeKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_DATATYPE_KEYWORD;
                                        break;
                                    case KeywordType.OtherKeyword:
                                        sqlTree.EscapeAnySelectionTarget();
                                        newNodeName = SqlXmlConstants.ENAME_OTHERKEYWORD;
                                        break;
                                    default:
                                        throw new Exception("Unrecognized Keyword Type!");
                                }
                            }

                            sqlTree.SaveNewElement(newNodeName, token.Value);
                        }
                        break;

                    case SqlTokenType.Semicolon:
                        sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SEMICOLON, token.Value);
                        sqlTree.NewStatementDue = true;
                        break;

                    case SqlTokenType.Colon:
                        if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.Colon
                            )
                        {
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_SCOPERESOLUTIONOPERATOR, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.OtherNode
                            )
                        {
                            //This SHOULD never happen in valid T-SQL, but can happen in DB2 or NexusDB or PostgreSQL 
                            // code (host variables) - so be nice and handle it anyway.
                            sqlTree.SaveNewElement(SqlXmlConstants.ENAME_OTHERNODE, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else
                        {
                            sqlTree.SaveNewElementWithError(SqlXmlConstants.ENAME_OTHEROPERATOR, token.Value);
                        }
                        break;

                    case SqlTokenType.Comma:
                        bool isCTESplitter = (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE)
                            );

                        sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);

                        if (isCTESplitter)
                        {
                            sqlTree.MoveToAncestorContainer(1, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CTE_ALIAS, "");
                        }
                        break;

                    case SqlTokenType.EqualsSign:
                        sqlTree.SaveNewElement(SqlXmlConstants.ENAME_EQUALSSIGN, token.Value);
                        if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK))
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT, "");
                        break;

                    case SqlTokenType.MultiLineComment:
                    case SqlTokenType.SingleLineComment:
                    case SqlTokenType.SingleLineCommentCStyle:
                    case SqlTokenType.WhiteSpace:
                        //create in statement rather than clause if there are no siblings yet
                        if (sqlTree.PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                            && sqlTree.PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                            && sqlTree.CurrentContainer.SelectSingleNode("*") == null
                            )
                            sqlTree.SaveNewElementAsPriorSibling(GetEquivalentSqlNodeName(token.Type), token.Value, sqlTree.CurrentContainer);
                        else
                            sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
                        break;

                    case SqlTokenType.BracketQuotedName:
                    case SqlTokenType.Asterisk:
                    case SqlTokenType.Period:
                    case SqlTokenType.OtherOperator:
                    case SqlTokenType.NationalString:
                    case SqlTokenType.String:
                    case SqlTokenType.QuotedString:
                    case SqlTokenType.Number:
                    case SqlTokenType.BinaryValue:
                    case SqlTokenType.MonetaryValue:
                    case SqlTokenType.PseudoName:
                        sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
                        break;
                    default:
                        throw new Exception("Unrecognized element encountered!");
                }

                tokenID++;
            }

            if (tokenList.HasUnfinishedToken)
                sqlTree.SetError();

            if (!sqlTree.FindValidBatchEnd())
                sqlTree.SetError();

            return sqlTree;
        }

        //TODO: move into parse tree
        private static bool ContentStartsWithKeyword(XmlElement providedContainer, string contentToMatch)
        {
            ParseTree parentDoc = (ParseTree)providedContainer.OwnerDocument;
            XmlElement firstEntryOfProvidedContainer = parentDoc.GetFirstNonWhitespaceNonCommentChildElement(providedContainer);
            bool targetFound = false;
            string keywordUpperValue = null;

            if (firstEntryOfProvidedContainer != null
                && firstEntryOfProvidedContainer.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                && firstEntryOfProvidedContainer.InnerText != null
                )
                keywordUpperValue = firstEntryOfProvidedContainer.InnerText.ToUpperInvariant();

            if (firstEntryOfProvidedContainer != null
                && firstEntryOfProvidedContainer.Name.Equals(SqlXmlConstants.ENAME_COMPOUNDKEYWORD)
                )
                keywordUpperValue = firstEntryOfProvidedContainer.GetAttribute(SqlXmlConstants.ANAME_SIMPLETEXT);

            if (keywordUpperValue != null)
            {
                targetFound = keywordUpperValue.Equals(contentToMatch) || keywordUpperValue.StartsWith(contentToMatch + " ");
            }
            else
            {
                //if contentToMatch was passed in as null, means we were looking for a NON-keyword.
                targetFound = contentToMatch == null;
            }

            return targetFound;
        }

        private void ProcessCompoundKeywordWithError(ITokenList tokenList, ParseTree sqlTree, XmlElement currentContainerElement, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
        {
            ProcessCompoundKeyword(tokenList, sqlTree, currentContainerElement, ref tokenID, significantTokenPositions, keywordCount);
            sqlTree.SetError();
        }

        private void ProcessCompoundKeyword(ITokenList tokenList, ParseTree sqlTree, XmlElement targetContainer, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
        {
            XmlElement compoundKeyword = sqlTree.SaveNewElement(SqlXmlConstants.ENAME_COMPOUNDKEYWORD, "", targetContainer);
            string targetText = ExtractTokensString(tokenList, significantTokenPositions.GetRange(0, keywordCount)).TrimEnd();
            compoundKeyword.SetAttribute(SqlXmlConstants.ANAME_SIMPLETEXT, targetText);
            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[keywordCount - 1]), SqlXmlConstants.ENAME_OTHERKEYWORD, compoundKeyword);
            tokenID = significantTokenPositions[keywordCount - 1];
        }

        private void AppendNodesWithMapping(ParseTree sqlTree, IEnumerable<IToken> tokens, string otherTokenMappingName, XmlElement targetContainer)
        {
            foreach (var token in tokens)
            {
                string elementName;
                if (token.Type == SqlTokenType.OtherNode)
                    elementName = otherTokenMappingName;
                else
                    elementName = GetEquivalentSqlNodeName(token.Type);

                sqlTree.SaveNewElement(elementName, token.Value, targetContainer);
            }
        }

        private string ExtractTokensString(ITokenList tokenList, IList<int> significantTokenPositions)
        {
            StringBuilder keywordSB = new StringBuilder();
            foreach (int tokenPos in significantTokenPositions)
            {
                //grr, this could be more elegant.
                if (tokenList[tokenPos].Type == SqlTokenType.Comma)
                    keywordSB.Append(",");
                else
                    keywordSB.Append(tokenList[tokenPos].Value.ToUpperInvariant());
                keywordSB.Append(" ");
            }
            return keywordSB.ToString();
        }

        private string GetEquivalentSqlNodeName(SqlTokenType tokenType)
        {
            switch (tokenType)
            {
                case SqlTokenType.WhiteSpace:
                    return SqlXmlConstants.ENAME_WHITESPACE;
                case SqlTokenType.SingleLineComment:
                    return SqlXmlConstants.ENAME_COMMENT_SINGLELINE;
                case SqlTokenType.SingleLineCommentCStyle:
                    return SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE;
                case SqlTokenType.MultiLineComment:
                    return SqlXmlConstants.ENAME_COMMENT_MULTILINE;
                case SqlTokenType.BracketQuotedName:
                    return SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME;
                case SqlTokenType.Asterisk:
                    return SqlXmlConstants.ENAME_ASTERISK;
                case SqlTokenType.EqualsSign:
                    return SqlXmlConstants.ENAME_EQUALSSIGN;
                case SqlTokenType.Comma:
                    return SqlXmlConstants.ENAME_COMMA;
                case SqlTokenType.Period:
                    return SqlXmlConstants.ENAME_PERIOD;
                case SqlTokenType.NationalString:
                    return SqlXmlConstants.ENAME_NSTRING;
                case SqlTokenType.String:
                    return SqlXmlConstants.ENAME_STRING;
                case SqlTokenType.QuotedString:
                    return SqlXmlConstants.ENAME_QUOTED_STRING;
                case SqlTokenType.OtherOperator:
                    return SqlXmlConstants.ENAME_OTHEROPERATOR;
                case SqlTokenType.Number:
                    return SqlXmlConstants.ENAME_NUMBER_VALUE;
                case SqlTokenType.MonetaryValue:
                    return SqlXmlConstants.ENAME_MONETARY_VALUE;
                case SqlTokenType.BinaryValue:
                    return SqlXmlConstants.ENAME_BINARY_VALUE;
                case SqlTokenType.PseudoName:
                    return SqlXmlConstants.ENAME_PSEUDONAME;
                default:
                    throw new Exception("Mapping not found for provided Token Type");
            }
        }

        private string GetKeywordMatchPhrase(ITokenList tokenList, int tokenID, ref List<string> rawKeywordParts, ref List<int> tokenCounts, ref List<List<IToken>> overflowNodes)
        {
            string phrase = "";
            int phraseComponentsFound = 0;
            rawKeywordParts = new List<string>();
            overflowNodes = new List<List<IToken>>();
            tokenCounts = new List<int>();
            string precedingWhitespace = "";
            int originalTokenID = tokenID;

            while (tokenID < tokenList.Count && phraseComponentsFound < 7)
            {
                if (tokenList[tokenID].Type == SqlTokenType.OtherNode
                    || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName
                    || tokenList[tokenID].Type == SqlTokenType.Comma
                    )
                {
                    phrase += tokenList[tokenID].Value.ToUpperInvariant() + " ";
                    phraseComponentsFound++;
                    rawKeywordParts.Add(precedingWhitespace + tokenList[tokenID].Value);

                    tokenID++;
                    tokenCounts.Add(tokenID - originalTokenID);

                    //found a possible phrase component - skip past any upcoming whitespace or comments, keeping track.
                    overflowNodes.Add(new List<IToken>());
                    precedingWhitespace = "";
                    while (tokenID < tokenList.Count
                        && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace
                            || tokenList[tokenID].Type == SqlTokenType.SingleLineComment
                            || tokenList[tokenID].Type == SqlTokenType.MultiLineComment
                            )
                        )
                    {
                        if (tokenList[tokenID].Type == SqlTokenType.WhiteSpace)
                            precedingWhitespace += tokenList[tokenID].Value;
                        else
                            overflowNodes[phraseComponentsFound-1].Add(tokenList[tokenID]);

                        tokenID++;
                    }
                }
                else
                    //we're not interested in any other node types
                    break;
            }

            return phrase;
        }

        private List<int> GetSignificantTokenPositions(ITokenList tokenList, int tokenID, int searchDistance)
        {
            List<int> significantTokenPositions = new List<int>();
            int originalTokenID = tokenID;

            while (tokenID < tokenList.Count && significantTokenPositions.Count < searchDistance)
            {
                if (tokenList[tokenID].Type == SqlTokenType.OtherNode
                    || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName
                    || tokenList[tokenID].Type == SqlTokenType.Comma
                    )
                {
                    significantTokenPositions.Add(tokenID);
                    tokenID++;

                    //found a possible phrase component - skip past any upcoming whitespace or comments, keeping track.
                    while (tokenID < tokenList.Count
                        && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace
                            || tokenList[tokenID].Type == SqlTokenType.SingleLineComment
                            || tokenList[tokenID].Type == SqlTokenType.MultiLineComment
                            )
                        )
                    {
                        tokenID++;
                    }
                }
                else
                    //we're not interested in any other node types
                    break;
            }

            return significantTokenPositions;
        }

        private XmlElement ProcessCompoundKeyword(ParseTree sqlTree, string newElementName, ref int tokenID, XmlElement currentContainerElement, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
        {
            XmlElement newElement = sqlTree.CreateElement(newElementName);
            newElement.InnerText = GetCompoundKeyword(ref tokenID, compoundKeywordCount, compoundKeywordTokenCounts, compoundKeywordRawStrings);
            sqlTree.CurrentContainer.AppendChild(newElement);
            return newElement;
        }

        private string GetCompoundKeyword(ref int tokenID, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
        {
            tokenID += compoundKeywordTokenCounts[compoundKeywordCount - 1] - 1;
            string outString = "";
            for (int i = 0; i < compoundKeywordCount; i++)
                outString += compoundKeywordRawStrings[i];
            return outString;
        }

        private static bool IsStatementStarter(IToken token)
        {
            //List created from experience, and augmented with individual sections of MSDN:
            // http://msdn.microsoft.com/en-us/library/ff848799.aspx
            // http://msdn.microsoft.com/en-us/library/ff848727.aspx
            // http://msdn.microsoft.com/en-us/library/ms174290.aspx
            // etc...
            string uppercaseValue = token.Value.ToUpperInvariant();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("ALTER")
                    || uppercaseValue.Equals("BACKUP")
                    || uppercaseValue.Equals("BREAK")
                    || uppercaseValue.Equals("CLOSE")
                    || uppercaseValue.Equals("CHECKPOINT")
                    || uppercaseValue.Equals("COMMIT")
                    || uppercaseValue.Equals("CONTINUE")
                    || uppercaseValue.Equals("CREATE")
                    || uppercaseValue.Equals("DBCC")
                    || uppercaseValue.Equals("DEALLOCATE")
                    || uppercaseValue.Equals("DELETE")
                    || uppercaseValue.Equals("DECLARE")
                    || uppercaseValue.Equals("DENY")
                    || uppercaseValue.Equals("DROP")
                    || uppercaseValue.Equals("EXEC")
                    || uppercaseValue.Equals("EXECUTE")
                    || uppercaseValue.Equals("FETCH")
                    || uppercaseValue.Equals("GOTO")
                    || uppercaseValue.Equals("GRANT")
                    || uppercaseValue.Equals("IF")
                    || uppercaseValue.Equals("INSERT")
                    || uppercaseValue.Equals("KILL")
                    || uppercaseValue.Equals("MERGE")
                    || uppercaseValue.Equals("OPEN")
                    || uppercaseValue.Equals("PRINT")
                    || uppercaseValue.Equals("RAISERROR")
                    || uppercaseValue.Equals("RECONFIGURE")
                    || uppercaseValue.Equals("RESTORE")
                    || uppercaseValue.Equals("RETURN")
                    || uppercaseValue.Equals("REVERT")
                    || uppercaseValue.Equals("REVOKE")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("SET")
                    || uppercaseValue.Equals("SETUSER")
                    || uppercaseValue.Equals("SHUTDOWN")
                    || uppercaseValue.Equals("TRUNCATE")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("USE")
                    || uppercaseValue.Equals("WAITFOR")
                    || uppercaseValue.Equals("WHILE")
                    )
                );
        }

        private static bool IsClauseStarter(IToken token)
        {
            //Note: some clause starters are handled separately: Joins, RETURNS clauses, etc.
            string uppercaseValue = token.Value.ToUpperInvariant();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("DELETE")
                    || uppercaseValue.Equals("EXCEPT")
                    || uppercaseValue.Equals("FOR")
                    || uppercaseValue.Equals("FROM")
                    || uppercaseValue.Equals("GROUP")
                    || uppercaseValue.Equals("HAVING")
                    || uppercaseValue.Equals("INNER")
                    || uppercaseValue.Equals("INTERSECT")
                    || uppercaseValue.Equals("INTO")
                    || uppercaseValue.Equals("INSERT")
                    || uppercaseValue.Equals("MERGE")
                    || uppercaseValue.Equals("ORDER")
                    || uppercaseValue.Equals("OUTPUT") //this is complicated... in sprocs output means something else!
                    || uppercaseValue.Equals("PIVOT")
                    || uppercaseValue.Equals("RETURNS")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("UNION")
                    || uppercaseValue.Equals("UNPIVOT")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("USING")
                    || uppercaseValue.Equals("VALUES")
                    || uppercaseValue.Equals("WHERE")
                    || uppercaseValue.Equals("WITH")
                    )
                );
        }

        private bool IsLatestTokenADDLDetailValue(ParseTree sqlTree)
        {
            XmlNode currentNode = sqlTree.CurrentContainer.LastChild;
            while (currentNode != null)
            {
                if (currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                        || currentNode.Name.Equals(SqlXmlConstants.ENAME_DATATYPE_KEYWORD)
                        || currentNode.Name.Equals(SqlXmlConstants.ENAME_COMPOUNDKEYWORD)
                        )
                {
                    string uppercaseText = null;
                    if (currentNode.Name.Equals(SqlXmlConstants.ENAME_COMPOUNDKEYWORD))
                        uppercaseText = currentNode.Attributes[SqlXmlConstants.ANAME_SIMPLETEXT].Value;
                    else
                        uppercaseText = currentNode.InnerText.ToUpperInvariant();

                    return (
                        uppercaseText.Equals("NVARCHAR")
                        || uppercaseText.Equals("VARCHAR")
                        || uppercaseText.Equals("DECIMAL")
                        || uppercaseText.Equals("DEC")
                        || uppercaseText.Equals("NUMERIC")
                        || uppercaseText.Equals("VARBINARY")
                        || uppercaseText.Equals("DEFAULT")
                        || uppercaseText.Equals("IDENTITY")
                        || uppercaseText.Equals("XML")
                        || uppercaseText.EndsWith("VARYING")
                        || uppercaseText.EndsWith("CHAR")
                        || uppercaseText.EndsWith("CHARACTER")
                        || uppercaseText.Equals("FLOAT")
                        || uppercaseText.Equals("DATETIMEOFFSET")
                        || uppercaseText.Equals("DATETIME2")
                        || uppercaseText.Equals("TIME")
                        );
                }
                else if (ParseTree.IsCommentOrWhiteSpace(currentNode.Name))
                {
                    currentNode = currentNode.PreviousSibling;
                }
                else
                    currentNode = null;
            }
            return false;
        }

        private bool IsLatestTokenAComma(ParseTree sqlTree)
        {
            XmlNode currentNode = sqlTree.CurrentContainer.LastChild;
            while (currentNode != null)
            {
                if (currentNode.Name.Equals(SqlXmlConstants.ENAME_COMMA))
                    return true;
                else if (ParseTree.IsCommentOrWhiteSpace(currentNode.Name))
                    currentNode = currentNode.PreviousSibling;
                else
                    currentNode = null;
            }
            return false;
        }

        private bool IsLatestTokenAMiscName(XmlElement currentContainerElement)
        {
            XmlNode currentNode = currentContainerElement.LastChild;
            while (currentNode != null)
            {
                string testValue = currentNode.InnerText.ToUpperInvariant();
                if (currentNode.Name.Equals(SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME)
                    || ((currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
                        || currentNode.Name.Equals(SqlXmlConstants.ENAME_FUNCTION_KEYWORD)
                        )
                        && !(testValue.Equals("AND")
                            || testValue.Equals("OR")
                            || testValue.Equals("NOT")
                            || testValue.Equals("BETWEEN")
                            || testValue.Equals("LIKE")
                            || testValue.Equals("CONTAINS")
                            || testValue.Equals("EXISTS")
                            || testValue.Equals("FREETEXT")
                            || testValue.Equals("IN")
                            || testValue.Equals("ALL")
                            || testValue.Equals("SOME")
                            || testValue.Equals("ANY")
                            || testValue.Equals("FROM")
                            || testValue.Equals("JOIN")
                            || testValue.EndsWith(" JOIN")
                            || testValue.Equals("UNION")
                            || testValue.Equals("UNION ALL")
                            || testValue.Equals("USING")
                            || testValue.Equals("AS")
                            || testValue.EndsWith(" APPLY")
                            )
                        )
                    )
                {
                    return true;
                }
                else if (ParseTree.IsCommentOrWhiteSpace(currentNode.Name))
                {
                    currentNode = currentNode.PreviousSibling;
                }
                else
                    currentNode = null;
            }
            return false;
        }

        private static bool IsLineBreakingWhiteSpaceOrComment(IToken token)
        {
            return (token.Type == SqlTokenType.WhiteSpace
                    && Regex.IsMatch(token.Value, @"(\r|\n)+"))
                || token.Type == SqlTokenType.SingleLineComment;
        }

        private bool IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(ITokenList tokenList, int tokenID)
        {
            int currTokenID = tokenID + 1;
            while (tokenList.Count >= currTokenID + 1)
            {
                if (tokenList[currTokenID].Type == SqlTokenType.SingleLineComment)
                    return true;
                else if (tokenList[currTokenID].Type == SqlTokenType.WhiteSpace)
                {
                    if (Regex.IsMatch(tokenList[currTokenID].Value, @"(\r|\n)+"))
                        return true;
                    else
                        currTokenID++;
                }
                else
                    return false;
            }
            return true;
        }

        private static void InitializeKeywordList()
        {
            //List originally copied from Side by Side SQL Comparer project from CodeProject:
            // http://www.codeproject.com/KB/database/SideBySideSQLComparer.aspx
            // Added some entries that are not strictly speaking keywords, such as 
            // cursor options "READ_ONLY", "FAST_FORWARD", etc.
            // also added numerous missing entries, such as "Xml", etc
            // Could/Should check against MSDN Ref: http://msdn.microsoft.com/en-us/library/ms189822.aspx
            KeywordList = new Dictionary<string, KeywordType>(StringComparer.OrdinalIgnoreCase);
            KeywordList.Add("@@CONNECTIONS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@CPU_BUSY", KeywordType.FunctionKeyword);
            KeywordList.Add("@@CURSOR_ROWS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@DATEFIRST", KeywordType.FunctionKeyword);
            KeywordList.Add("@@DBTS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@ERROR", KeywordType.FunctionKeyword);
            KeywordList.Add("@@FETCH_STATUS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@IDENTITY", KeywordType.FunctionKeyword);
            KeywordList.Add("@@IDLE", KeywordType.FunctionKeyword);
            KeywordList.Add("@@IO_BUSY", KeywordType.FunctionKeyword);
            KeywordList.Add("@@LANGID", KeywordType.FunctionKeyword);
            KeywordList.Add("@@LANGUAGE", KeywordType.FunctionKeyword);
            KeywordList.Add("@@LOCK_TIMEOUT", KeywordType.FunctionKeyword);
            KeywordList.Add("@@MAX_CONNECTIONS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@MAX_PRECISION", KeywordType.FunctionKeyword);
            KeywordList.Add("@@NESTLEVEL", KeywordType.FunctionKeyword);
            KeywordList.Add("@@OPTIONS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@PACKET_ERRORS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@PACK_RECEIVED", KeywordType.FunctionKeyword);
            KeywordList.Add("@@PACK_SENT", KeywordType.FunctionKeyword);
            KeywordList.Add("@@PROCID", KeywordType.FunctionKeyword);
            KeywordList.Add("@@REMSERVER", KeywordType.FunctionKeyword);
            KeywordList.Add("@@ROWCOUNT", KeywordType.FunctionKeyword);
            KeywordList.Add("@@SERVERNAME", KeywordType.FunctionKeyword);
            KeywordList.Add("@@SERVICENAME", KeywordType.FunctionKeyword);
            KeywordList.Add("@@SPID", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TEXTSIZE", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TIMETICKS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TOTAL_ERRORS", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TOTAL_READ", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TOTAL_WRITE", KeywordType.FunctionKeyword);
            KeywordList.Add("@@TRANCOUNT", KeywordType.FunctionKeyword);
            KeywordList.Add("@@VERSION", KeywordType.FunctionKeyword);
            KeywordList.Add("ABS", KeywordType.FunctionKeyword);
            KeywordList.Add("ACOS", KeywordType.FunctionKeyword);
            KeywordList.Add("ACTIVATION", KeywordType.OtherKeyword);
            KeywordList.Add("ADD", KeywordType.OtherKeyword);
            KeywordList.Add("ALL", KeywordType.OperatorKeyword);
            KeywordList.Add("ALTER", KeywordType.OtherKeyword);
            KeywordList.Add("AND", KeywordType.OperatorKeyword);
            KeywordList.Add("ANSI_DEFAULTS", KeywordType.OtherKeyword);
            KeywordList.Add("ANSI_NULLS", KeywordType.OtherKeyword);
            KeywordList.Add("ANSI_NULL_DFLT_OFF", KeywordType.OtherKeyword);
            KeywordList.Add("ANSI_NULL_DFLT_ON", KeywordType.OtherKeyword);
            KeywordList.Add("ANSI_PADDING", KeywordType.OtherKeyword);
            KeywordList.Add("ANSI_WARNINGS", KeywordType.OtherKeyword);
            KeywordList.Add("ANY", KeywordType.OperatorKeyword);
            KeywordList.Add("APP_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("ARITHABORT", KeywordType.OtherKeyword);
            KeywordList.Add("ARITHIGNORE", KeywordType.OtherKeyword);
            KeywordList.Add("AS", KeywordType.OtherKeyword);
            KeywordList.Add("ASC", KeywordType.OtherKeyword);
            KeywordList.Add("ASCII", KeywordType.FunctionKeyword);
            KeywordList.Add("ASIN", KeywordType.FunctionKeyword);
            KeywordList.Add("ATAN", KeywordType.FunctionKeyword);
            KeywordList.Add("ATN2", KeywordType.FunctionKeyword);
            KeywordList.Add("AUTHORIZATION", KeywordType.OtherKeyword);
            KeywordList.Add("AVG", KeywordType.FunctionKeyword);
            KeywordList.Add("BACKUP", KeywordType.OtherKeyword);
            KeywordList.Add("BEGIN", KeywordType.OtherKeyword);
            KeywordList.Add("BETWEEN", KeywordType.OperatorKeyword);
            KeywordList.Add("BIGINT", KeywordType.DataTypeKeyword);
            KeywordList.Add("BINARY", KeywordType.DataTypeKeyword);
            KeywordList.Add("BIT", KeywordType.DataTypeKeyword);
            KeywordList.Add("BREAK", KeywordType.OtherKeyword);
            KeywordList.Add("BROWSE", KeywordType.OtherKeyword);
            KeywordList.Add("BULK", KeywordType.OtherKeyword);
            KeywordList.Add("BY", KeywordType.OtherKeyword);
            KeywordList.Add("CALLER", KeywordType.OtherKeyword);
            KeywordList.Add("CASCADE", KeywordType.OtherKeyword);
            KeywordList.Add("CASE", KeywordType.FunctionKeyword);
            KeywordList.Add("CAST", KeywordType.FunctionKeyword);
            KeywordList.Add("CATALOG", KeywordType.OtherKeyword);
            KeywordList.Add("CEILING", KeywordType.FunctionKeyword);
            KeywordList.Add("CHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("CHARACTER", KeywordType.DataTypeKeyword);
            KeywordList.Add("CHARINDEX", KeywordType.FunctionKeyword);
            KeywordList.Add("CHECK", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKALLOC", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKCATALOG", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKCONSTRAINTS", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKDB", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKFILEGROUP", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKIDENT", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKPOINT", KeywordType.OtherKeyword);
            KeywordList.Add("CHECKSUM", KeywordType.FunctionKeyword);
            KeywordList.Add("CHECKSUM_AGG", KeywordType.FunctionKeyword);
            KeywordList.Add("CHECKTABLE", KeywordType.OtherKeyword);
            KeywordList.Add("CLEANTABLE", KeywordType.OtherKeyword);
            KeywordList.Add("CLOSE", KeywordType.OtherKeyword);
            KeywordList.Add("CLUSTERED", KeywordType.OtherKeyword);
            KeywordList.Add("COALESCE", KeywordType.FunctionKeyword);
            KeywordList.Add("COLLATIONPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("COLLECTION", KeywordType.OtherKeyword);
            KeywordList.Add("COLUMN", KeywordType.OtherKeyword);
            KeywordList.Add("COLUMNPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("COL_LENGTH", KeywordType.FunctionKeyword);
            KeywordList.Add("COL_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("COMMIT", KeywordType.OtherKeyword);
            KeywordList.Add("COMMITTED", KeywordType.OtherKeyword);
            KeywordList.Add("COMPUTE", KeywordType.OtherKeyword);
            KeywordList.Add("CONCAT", KeywordType.OtherKeyword);
            KeywordList.Add("CONCAT_NULL_YIELDS_NULL", KeywordType.OtherKeyword);
            KeywordList.Add("CONCURRENCYVIOLATION", KeywordType.OtherKeyword);
            KeywordList.Add("CONFIRM", KeywordType.OtherKeyword);
            KeywordList.Add("CONSTRAINT", KeywordType.OtherKeyword);
            KeywordList.Add("CONTAINS", KeywordType.OtherKeyword);
            KeywordList.Add("CONTAINSTABLE", KeywordType.FunctionKeyword);
            KeywordList.Add("CONTINUE", KeywordType.OtherKeyword);
            KeywordList.Add("CONTROL", KeywordType.OtherKeyword);
            KeywordList.Add("CONTROLROW", KeywordType.OtherKeyword);
            KeywordList.Add("CONVERT", KeywordType.FunctionKeyword);
            KeywordList.Add("COS", KeywordType.FunctionKeyword);
            KeywordList.Add("COT", KeywordType.FunctionKeyword);
            KeywordList.Add("COUNT", KeywordType.FunctionKeyword);
            KeywordList.Add("COUNT_BIG", KeywordType.FunctionKeyword);
            KeywordList.Add("CREATE", KeywordType.OtherKeyword);
            KeywordList.Add("CROSS", KeywordType.OtherKeyword);
            KeywordList.Add("CURRENT", KeywordType.OtherKeyword);
            KeywordList.Add("CURRENT_DATE", KeywordType.OtherKeyword);
            KeywordList.Add("CURRENT_TIME", KeywordType.OtherKeyword);
            KeywordList.Add("CURRENT_TIMESTAMP", KeywordType.FunctionKeyword);
            KeywordList.Add("CURRENT_USER", KeywordType.FunctionKeyword);
            KeywordList.Add("CURSOR", KeywordType.OtherKeyword);
            KeywordList.Add("CURSOR_CLOSE_ON_COMMIT", KeywordType.OtherKeyword);
            KeywordList.Add("CURSOR_STATUS", KeywordType.FunctionKeyword);
            KeywordList.Add("DATABASE", KeywordType.OtherKeyword);
            KeywordList.Add("DATABASEPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("DATABASEPROPERTYEX", KeywordType.FunctionKeyword);
            KeywordList.Add("DATALENGTH", KeywordType.FunctionKeyword);
            KeywordList.Add("DATEADD", KeywordType.FunctionKeyword);
            KeywordList.Add("DATEDIFF", KeywordType.FunctionKeyword);
            KeywordList.Add("DATEFIRST", KeywordType.OtherKeyword);
            KeywordList.Add("DATEFORMAT", KeywordType.OtherKeyword);
            KeywordList.Add("DATENAME", KeywordType.FunctionKeyword);
            KeywordList.Add("DATE", KeywordType.DataTypeKeyword);
            KeywordList.Add("DATEPART", KeywordType.FunctionKeyword);
            KeywordList.Add("DATETIME", KeywordType.DataTypeKeyword);
            KeywordList.Add("DATETIME2", KeywordType.DataTypeKeyword);
            KeywordList.Add("DATETIMEOFFSET", KeywordType.DataTypeKeyword);
            KeywordList.Add("DAY", KeywordType.FunctionKeyword);
            KeywordList.Add("DBCC", KeywordType.OtherKeyword);
            KeywordList.Add("DBREINDEX", KeywordType.OtherKeyword);
            KeywordList.Add("DBREPAIR", KeywordType.OtherKeyword);
            KeywordList.Add("DB_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("DB_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("DEADLOCK_PRIORITY", KeywordType.OtherKeyword);
            KeywordList.Add("DEALLOCATE", KeywordType.OtherKeyword);
            KeywordList.Add("DEC", KeywordType.DataTypeKeyword);
            KeywordList.Add("DECIMAL", KeywordType.DataTypeKeyword);
            KeywordList.Add("DECLARE", KeywordType.OtherKeyword);
            KeywordList.Add("DEFAULT", KeywordType.OtherKeyword);
            KeywordList.Add("DEFINITION", KeywordType.OtherKeyword);
            KeywordList.Add("DEGREES", KeywordType.FunctionKeyword);
            KeywordList.Add("DELAY", KeywordType.OtherKeyword);
            KeywordList.Add("DELETE", KeywordType.OtherKeyword);
            KeywordList.Add("DENY", KeywordType.OtherKeyword);
            KeywordList.Add("DESC", KeywordType.OtherKeyword);
            KeywordList.Add("DIFFERENCE", KeywordType.FunctionKeyword);
            KeywordList.Add("DISABLE_DEF_CNST_CHK", KeywordType.OtherKeyword);
            KeywordList.Add("DISK", KeywordType.OtherKeyword);
            KeywordList.Add("DISTINCT", KeywordType.OtherKeyword);
            KeywordList.Add("DISTRIBUTED", KeywordType.OtherKeyword);
            KeywordList.Add("DOUBLE", KeywordType.DataTypeKeyword);
            KeywordList.Add("DROP", KeywordType.OtherKeyword);
            KeywordList.Add("DROPCLEANBUFFERS", KeywordType.OtherKeyword);
            KeywordList.Add("DUMMY", KeywordType.OtherKeyword);
            KeywordList.Add("DUMP", KeywordType.OtherKeyword);
            KeywordList.Add("DYNAMIC", KeywordType.OtherKeyword);
            KeywordList.Add("ELSE", KeywordType.OtherKeyword);
            KeywordList.Add("ENCRYPTION", KeywordType.OtherKeyword);
            KeywordList.Add("ERRLVL", KeywordType.OtherKeyword);
            KeywordList.Add("ERROREXIT", KeywordType.OtherKeyword);
            KeywordList.Add("ESCAPE", KeywordType.OtherKeyword);
            KeywordList.Add("EXCEPT", KeywordType.OtherKeyword);
            KeywordList.Add("EXEC", KeywordType.OtherKeyword);
            KeywordList.Add("EXECUTE", KeywordType.OtherKeyword);
            KeywordList.Add("EXISTS", KeywordType.OperatorKeyword);
            KeywordList.Add("EXIT", KeywordType.OtherKeyword);
            KeywordList.Add("EXP", KeywordType.FunctionKeyword);
            KeywordList.Add("EXPAND", KeywordType.OtherKeyword);
            KeywordList.Add("EXTERNAL", KeywordType.OtherKeyword);
            KeywordList.Add("FAST", KeywordType.OtherKeyword);
            KeywordList.Add("FAST_FORWARD", KeywordType.OtherKeyword);
            KeywordList.Add("FASTFIRSTROW", KeywordType.OtherKeyword);
            KeywordList.Add("FETCH", KeywordType.OtherKeyword);
            KeywordList.Add("FILE", KeywordType.OtherKeyword);
            KeywordList.Add("FILEGROUPPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FILEGROUP_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("FILEGROUP_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("FILEPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FILE_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("FILE_IDEX", KeywordType.FunctionKeyword);
            KeywordList.Add("FILE_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("FILLFACTOR", KeywordType.OtherKeyword);
            KeywordList.Add("FIPS_FLAGGER", KeywordType.OtherKeyword);
            KeywordList.Add("FLOAT", KeywordType.DataTypeKeyword);
            KeywordList.Add("FLOOR", KeywordType.FunctionKeyword);
            KeywordList.Add("FLOPPY", KeywordType.OtherKeyword);
            KeywordList.Add("FMTONLY", KeywordType.OtherKeyword);
            KeywordList.Add("FOR", KeywordType.OtherKeyword);
            KeywordList.Add("FORCE", KeywordType.OtherKeyword);
            KeywordList.Add("FORCED", KeywordType.OtherKeyword);
            KeywordList.Add("FORCEPLAN", KeywordType.OtherKeyword);
            KeywordList.Add("FOREIGN", KeywordType.OtherKeyword);
            KeywordList.Add("FORMATMESSAGE", KeywordType.FunctionKeyword);
            KeywordList.Add("FORWARD_ONLY", KeywordType.OtherKeyword);
            KeywordList.Add("FREEPROCCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREESESSIONCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREESYSTEMCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREETEXT", KeywordType.OtherKeyword);
            KeywordList.Add("FREETEXTTABLE", KeywordType.FunctionKeyword);
            KeywordList.Add("FROM", KeywordType.OtherKeyword);
            KeywordList.Add("FULL", KeywordType.OtherKeyword);
            KeywordList.Add("FULLTEXT", KeywordType.OtherKeyword);
            KeywordList.Add("FULLTEXTCATALOGPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FULLTEXTSERVICEPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FUNCTION", KeywordType.OtherKeyword);
            KeywordList.Add("GEOGRAPHY", KeywordType.DataTypeKeyword);
            KeywordList.Add("GETANCESTOR", KeywordType.FunctionKeyword);
            KeywordList.Add("GETANSINULL", KeywordType.FunctionKeyword);
            KeywordList.Add("GETDATE", KeywordType.FunctionKeyword);
            KeywordList.Add("GETDESCENDANT", KeywordType.FunctionKeyword);
            KeywordList.Add("GETLEVEL", KeywordType.FunctionKeyword);
            KeywordList.Add("GETREPARENTEDVALUE", KeywordType.FunctionKeyword);
            KeywordList.Add("GETROOT", KeywordType.FunctionKeyword);
            KeywordList.Add("GLOBAL", KeywordType.OtherKeyword);
            KeywordList.Add("GO", KeywordType.OtherKeyword);
            KeywordList.Add("GOTO", KeywordType.OtherKeyword);
            KeywordList.Add("GRANT", KeywordType.OtherKeyword);
            KeywordList.Add("GROUP", KeywordType.OtherKeyword);
            KeywordList.Add("GROUPING", KeywordType.FunctionKeyword);
            KeywordList.Add("HASH", KeywordType.OtherKeyword);
            KeywordList.Add("HAVING", KeywordType.OtherKeyword);
            KeywordList.Add("HELP", KeywordType.OtherKeyword);
            KeywordList.Add("HIERARCHYID", KeywordType.DataTypeKeyword);
            KeywordList.Add("HOLDLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("HOST_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("HOST_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("IDENTITY", KeywordType.FunctionKeyword);
            KeywordList.Add("IDENTITYCOL", KeywordType.OtherKeyword);
            KeywordList.Add("IDENTITY_INSERT", KeywordType.OtherKeyword);
            KeywordList.Add("IDENT_CURRENT", KeywordType.FunctionKeyword);
            KeywordList.Add("IDENT_INCR", KeywordType.FunctionKeyword);
            KeywordList.Add("IDENT_SEED", KeywordType.FunctionKeyword);
            KeywordList.Add("IF", KeywordType.OtherKeyword);
            KeywordList.Add("IGNORE_CONSTRAINTS", KeywordType.OtherKeyword);
            KeywordList.Add("IGNORE_TRIGGERS", KeywordType.OtherKeyword);
            KeywordList.Add("IMAGE", KeywordType.DataTypeKeyword);
            KeywordList.Add("IMPLICIT_TRANSACTIONS", KeywordType.OtherKeyword);
            KeywordList.Add("IN", KeywordType.OperatorKeyword);
            KeywordList.Add("INDEX", KeywordType.OtherKeyword);
            KeywordList.Add("INDEXDEFRAG", KeywordType.OtherKeyword);
            KeywordList.Add("INDEXKEY_PROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("INDEXPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("INDEX_COL", KeywordType.FunctionKeyword);
            KeywordList.Add("INNER", KeywordType.OtherKeyword);
            KeywordList.Add("INPUTBUFFER", KeywordType.OtherKeyword);
            KeywordList.Add("INSENSITIVE", KeywordType.DataTypeKeyword);
            KeywordList.Add("INSERT", KeywordType.OtherKeyword);
            KeywordList.Add("INT", KeywordType.DataTypeKeyword);
            KeywordList.Add("INTEGER", KeywordType.DataTypeKeyword);
            KeywordList.Add("INTERSECT", KeywordType.OtherKeyword);
            KeywordList.Add("INTO", KeywordType.OtherKeyword);
            KeywordList.Add("IO", KeywordType.OtherKeyword);
            KeywordList.Add("IS", KeywordType.OtherKeyword);
            KeywordList.Add("ISDATE", KeywordType.FunctionKeyword);
            KeywordList.Add("ISDESCENDANTOF", KeywordType.FunctionKeyword);
            KeywordList.Add("ISNULL", KeywordType.FunctionKeyword);
            KeywordList.Add("ISNUMERIC", KeywordType.FunctionKeyword);
            KeywordList.Add("ISOLATION", KeywordType.OtherKeyword);
            KeywordList.Add("IS_MEMBER", KeywordType.FunctionKeyword);
            KeywordList.Add("IS_SRVROLEMEMBER", KeywordType.FunctionKeyword);
            KeywordList.Add("JOIN", KeywordType.OtherKeyword);
            KeywordList.Add("KEEP", KeywordType.OtherKeyword);
            KeywordList.Add("KEEPDEFAULTS", KeywordType.OtherKeyword);
            KeywordList.Add("KEEPFIXED", KeywordType.OtherKeyword);
            KeywordList.Add("KEEPIDENTITY", KeywordType.OtherKeyword);
            KeywordList.Add("KEY", KeywordType.OtherKeyword);
            KeywordList.Add("KEYSET", KeywordType.OtherKeyword);
            KeywordList.Add("KILL", KeywordType.OtherKeyword);
            KeywordList.Add("LANGUAGE", KeywordType.OtherKeyword);
            KeywordList.Add("LEFT", KeywordType.FunctionKeyword);
            KeywordList.Add("LEN", KeywordType.FunctionKeyword);
            KeywordList.Add("LEVEL", KeywordType.OtherKeyword);
            KeywordList.Add("LIKE", KeywordType.OperatorKeyword);
            KeywordList.Add("LINENO", KeywordType.OtherKeyword);
            KeywordList.Add("LOAD", KeywordType.OtherKeyword);
            KeywordList.Add("LOCAL", KeywordType.OtherKeyword);
            KeywordList.Add("LOCK_TIMEOUT", KeywordType.OtherKeyword);
            KeywordList.Add("LOG", KeywordType.FunctionKeyword);
            KeywordList.Add("LOG10", KeywordType.FunctionKeyword);
            KeywordList.Add("LOGIN", KeywordType.OtherKeyword);
            KeywordList.Add("LOOP", KeywordType.OtherKeyword);
            KeywordList.Add("LOWER", KeywordType.FunctionKeyword);
            KeywordList.Add("LTRIM", KeywordType.FunctionKeyword);
            KeywordList.Add("MATCHED", KeywordType.OtherKeyword);
            KeywordList.Add("MAX", KeywordType.FunctionKeyword);
            KeywordList.Add("MAX_QUEUE_READERS", KeywordType.OtherKeyword);
            KeywordList.Add("MAXDOP", KeywordType.OtherKeyword);
            KeywordList.Add("MAXRECURSION", KeywordType.OtherKeyword);
            KeywordList.Add("MERGE", KeywordType.OtherKeyword);
            KeywordList.Add("MIN", KeywordType.FunctionKeyword);
            KeywordList.Add("MIRROREXIT", KeywordType.OtherKeyword);
            KeywordList.Add("MODIFY", KeywordType.FunctionKeyword);
            KeywordList.Add("MONEY", KeywordType.DataTypeKeyword);
            KeywordList.Add("MONTH", KeywordType.FunctionKeyword);
            KeywordList.Add("MOVE", KeywordType.OtherKeyword);
            KeywordList.Add("NAME", KeywordType.OtherKeyword);
            KeywordList.Add("NATIONAL", KeywordType.DataTypeKeyword);
            KeywordList.Add("NCHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("NEWID", KeywordType.FunctionKeyword);
            KeywordList.Add("NEXT", KeywordType.OtherKeyword);
            KeywordList.Add("NOCHECK", KeywordType.OtherKeyword);
            KeywordList.Add("NOCOUNT", KeywordType.OtherKeyword);
            KeywordList.Add("NODES", KeywordType.FunctionKeyword);
            KeywordList.Add("NOEXEC", KeywordType.OtherKeyword);
            KeywordList.Add("NOEXPAND", KeywordType.OtherKeyword);
            KeywordList.Add("NOLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("NONCLUSTERED", KeywordType.OtherKeyword);
            KeywordList.Add("NOT", KeywordType.OperatorKeyword);
            KeywordList.Add("NOWAIT", KeywordType.OtherKeyword);
            KeywordList.Add("NTEXT", KeywordType.DataTypeKeyword);
            KeywordList.Add("NTILE", KeywordType.FunctionKeyword);
            KeywordList.Add("NULL", KeywordType.OtherKeyword);
            KeywordList.Add("NULLIF", KeywordType.FunctionKeyword);
            KeywordList.Add("NUMERIC", KeywordType.DataTypeKeyword);
            KeywordList.Add("NUMERIC_ROUNDABORT", KeywordType.OtherKeyword);
            KeywordList.Add("NVARCHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("OBJECTPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("OBJECTPROPERTYEX", KeywordType.FunctionKeyword);
            KeywordList.Add("OBJECT", KeywordType.OtherKeyword);
            KeywordList.Add("OBJECT_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("OBJECT_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("OF", KeywordType.OtherKeyword);
            KeywordList.Add("OFF", KeywordType.OtherKeyword);
            KeywordList.Add("OFFSETS", KeywordType.OtherKeyword);
            KeywordList.Add("ON", KeywordType.OtherKeyword);
            KeywordList.Add("ONCE", KeywordType.OtherKeyword);
            KeywordList.Add("ONLY", KeywordType.OtherKeyword);
            KeywordList.Add("OPEN", KeywordType.OtherKeyword);
            KeywordList.Add("OPENDATASOURCE", KeywordType.OtherKeyword);
            KeywordList.Add("OPENQUERY", KeywordType.FunctionKeyword);
            KeywordList.Add("OPENROWSET", KeywordType.FunctionKeyword);
            KeywordList.Add("OPENTRAN", KeywordType.OtherKeyword);
            KeywordList.Add("OPTIMIZE", KeywordType.OtherKeyword);
            KeywordList.Add("OPTIMISTIC", KeywordType.OtherKeyword);
            KeywordList.Add("OPTION", KeywordType.OtherKeyword);
            KeywordList.Add("OR", KeywordType.OperatorKeyword);
            KeywordList.Add("ORDER", KeywordType.OtherKeyword);
            KeywordList.Add("OUTER", KeywordType.OtherKeyword);
			KeywordList.Add("OUT", KeywordType.OtherKeyword);
			KeywordList.Add("OUTPUT", KeywordType.OtherKeyword);
			KeywordList.Add("OUTPUTBUFFER", KeywordType.OtherKeyword);
            KeywordList.Add("OVER", KeywordType.OtherKeyword);
            KeywordList.Add("OWNER", KeywordType.OtherKeyword);
            KeywordList.Add("PAGLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("PARAMETERIZATION", KeywordType.OtherKeyword);
            KeywordList.Add("PARSE", KeywordType.FunctionKeyword);
            KeywordList.Add("PARSENAME", KeywordType.FunctionKeyword);
            KeywordList.Add("PARSEONLY", KeywordType.OtherKeyword);
            KeywordList.Add("PARTITION", KeywordType.OtherKeyword);
            KeywordList.Add("PATINDEX", KeywordType.FunctionKeyword);
            KeywordList.Add("PERCENT", KeywordType.OtherKeyword);
            KeywordList.Add("PERM", KeywordType.OtherKeyword);
            KeywordList.Add("PERMANENT", KeywordType.OtherKeyword);
            KeywordList.Add("PERMISSIONS", KeywordType.FunctionKeyword);
            KeywordList.Add("PI", KeywordType.FunctionKeyword);
            KeywordList.Add("PINTABLE", KeywordType.OtherKeyword);
            KeywordList.Add("PIPE", KeywordType.OtherKeyword);
            KeywordList.Add("PLAN", KeywordType.OtherKeyword);
            KeywordList.Add("POWER", KeywordType.FunctionKeyword);
            KeywordList.Add("PREPARE", KeywordType.OtherKeyword);
            KeywordList.Add("PRIMARY", KeywordType.OtherKeyword);
            KeywordList.Add("PRINT", KeywordType.OtherKeyword);
            KeywordList.Add("PRIVILEGES", KeywordType.OtherKeyword);
            KeywordList.Add("PROC", KeywordType.OtherKeyword);
            KeywordList.Add("PROCCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("PROCEDURE", KeywordType.OtherKeyword);
            KeywordList.Add("PROCEDURE_NAME", KeywordType.OtherKeyword);
            KeywordList.Add("PROCESSEXIT", KeywordType.OtherKeyword);
            KeywordList.Add("PROCID", KeywordType.OtherKeyword);
            KeywordList.Add("PROFILE", KeywordType.OtherKeyword);
            KeywordList.Add("PUBLIC", KeywordType.OtherKeyword);
            KeywordList.Add("QUERY", KeywordType.FunctionKeyword);
            KeywordList.Add("QUERY_GOVERNOR_COST_LIMIT", KeywordType.OtherKeyword);
            KeywordList.Add("QUEUE", KeywordType.OtherKeyword);
            KeywordList.Add("QUOTED_IDENTIFIER", KeywordType.OtherKeyword);
            KeywordList.Add("QUOTENAME", KeywordType.FunctionKeyword);
            KeywordList.Add("RADIANS", KeywordType.FunctionKeyword);
            KeywordList.Add("RAISERROR", KeywordType.OtherKeyword);
            KeywordList.Add("RAND", KeywordType.FunctionKeyword);
            KeywordList.Add("READ", KeywordType.OtherKeyword);
            KeywordList.Add("READCOMMITTED", KeywordType.OtherKeyword);
            KeywordList.Add("READCOMMITTEDLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("READPAST", KeywordType.OtherKeyword);
            KeywordList.Add("READTEXT", KeywordType.OtherKeyword);
            KeywordList.Add("READUNCOMMITTED", KeywordType.OtherKeyword);
            KeywordList.Add("READ_ONLY", KeywordType.OtherKeyword);
            KeywordList.Add("REAL", KeywordType.DataTypeKeyword);
            KeywordList.Add("RECOMPILE", KeywordType.OtherKeyword);
            KeywordList.Add("RECONFIGURE", KeywordType.OtherKeyword);
            KeywordList.Add("REFERENCES", KeywordType.OtherKeyword);
            KeywordList.Add("REMOTE_PROC_TRANSACTIONS", KeywordType.OtherKeyword);
            KeywordList.Add("REPEATABLE", KeywordType.OtherKeyword);
            KeywordList.Add("REPEATABLEREAD", KeywordType.OtherKeyword);
            KeywordList.Add("REPLACE", KeywordType.FunctionKeyword);
            KeywordList.Add("REPLICATE", KeywordType.FunctionKeyword);
            KeywordList.Add("REPLICATION", KeywordType.OtherKeyword);
            KeywordList.Add("RESTORE", KeywordType.OtherKeyword);
            KeywordList.Add("RESTRICT", KeywordType.OtherKeyword);
            KeywordList.Add("RETURN", KeywordType.OtherKeyword);
            KeywordList.Add("RETURNS", KeywordType.OtherKeyword);
            KeywordList.Add("REVERSE", KeywordType.FunctionKeyword);
            KeywordList.Add("REVERT", KeywordType.OtherKeyword);
            KeywordList.Add("REVOKE", KeywordType.OtherKeyword);
            KeywordList.Add("RIGHT", KeywordType.FunctionKeyword);
            KeywordList.Add("ROBUST", KeywordType.OtherKeyword);
            KeywordList.Add("ROLE", KeywordType.OtherKeyword);
            KeywordList.Add("ROLLBACK", KeywordType.OtherKeyword);
            KeywordList.Add("ROUND", KeywordType.FunctionKeyword);
            KeywordList.Add("ROWCOUNT", KeywordType.OtherKeyword);
            KeywordList.Add("ROWGUIDCOL", KeywordType.OtherKeyword);
            KeywordList.Add("ROWLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("ROWVERSION", KeywordType.DataTypeKeyword);
            KeywordList.Add("RTRIM", KeywordType.FunctionKeyword);
            KeywordList.Add("RULE", KeywordType.OtherKeyword);
            KeywordList.Add("SAVE", KeywordType.OtherKeyword);
            KeywordList.Add("SCHEMA", KeywordType.OtherKeyword);
            KeywordList.Add("SCHEMA_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("SCHEMA_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("SCOPE_IDENTITY", KeywordType.FunctionKeyword);
            KeywordList.Add("SCROLL", KeywordType.OtherKeyword);
            KeywordList.Add("SCROLL_LOCKS", KeywordType.OtherKeyword);
            KeywordList.Add("SELECT", KeywordType.OtherKeyword);
            KeywordList.Add("SELF", KeywordType.OtherKeyword);
            KeywordList.Add("SERIALIZABLE", KeywordType.OtherKeyword);
            KeywordList.Add("SERVER", KeywordType.OtherKeyword);
            KeywordList.Add("SERVERPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("SESSIONPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("SESSION_USER", KeywordType.FunctionKeyword);
            KeywordList.Add("SET", KeywordType.OtherKeyword);
            KeywordList.Add("SETUSER", KeywordType.OtherKeyword);
            KeywordList.Add("SHOWCONTIG", KeywordType.OtherKeyword);
            KeywordList.Add("SHOWPLAN_ALL", KeywordType.OtherKeyword);
            KeywordList.Add("SHOWPLAN_TEXT", KeywordType.OtherKeyword);
            KeywordList.Add("SHOW_STATISTICS", KeywordType.OtherKeyword);
            KeywordList.Add("SHRINKDATABASE", KeywordType.OtherKeyword);
            KeywordList.Add("SHRINKFILE", KeywordType.OtherKeyword);
            KeywordList.Add("SHUTDOWN", KeywordType.OtherKeyword);
            KeywordList.Add("SIGN", KeywordType.FunctionKeyword);
            KeywordList.Add("SIMPLE", KeywordType.OtherKeyword);
            KeywordList.Add("SIN", KeywordType.FunctionKeyword);
            KeywordList.Add("SMALLDATETIME", KeywordType.DataTypeKeyword);
            KeywordList.Add("SMALLINT", KeywordType.DataTypeKeyword);
            KeywordList.Add("SMALLMONEY", KeywordType.DataTypeKeyword);
            KeywordList.Add("SOME", KeywordType.OperatorKeyword);
            KeywordList.Add("SOUNDEX", KeywordType.FunctionKeyword);
            KeywordList.Add("SPACE", KeywordType.FunctionKeyword);
            KeywordList.Add("SQLPERF", KeywordType.OtherKeyword);
            KeywordList.Add("SQL_VARIANT", KeywordType.DataTypeKeyword);
            KeywordList.Add("SQL_VARIANT_PROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("SQRT", KeywordType.FunctionKeyword);
            KeywordList.Add("SQUARE", KeywordType.FunctionKeyword);
            KeywordList.Add("STATE", KeywordType.OtherKeyword);
            KeywordList.Add("STATISTICS", KeywordType.OtherKeyword);
            KeywordList.Add("STATIC", KeywordType.OtherKeyword);
            KeywordList.Add("STATS_DATE", KeywordType.FunctionKeyword);
            KeywordList.Add("STATUS", KeywordType.OtherKeyword);
            KeywordList.Add("STDEV", KeywordType.FunctionKeyword);
            KeywordList.Add("STDEVP", KeywordType.FunctionKeyword);
            KeywordList.Add("STOPLIST", KeywordType.OtherKeyword);
            KeywordList.Add("STR", KeywordType.FunctionKeyword);
            KeywordList.Add("STUFF", KeywordType.FunctionKeyword);
            KeywordList.Add("SUBSTRING", KeywordType.FunctionKeyword);
            KeywordList.Add("SUM", KeywordType.FunctionKeyword);
            KeywordList.Add("SUSER_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("SUSER_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("SUSER_SID", KeywordType.FunctionKeyword);
            KeywordList.Add("SUSER_SNAME", KeywordType.FunctionKeyword);
            KeywordList.Add("SYNONYM", KeywordType.OtherKeyword);
            KeywordList.Add("SYSNAME", KeywordType.DataTypeKeyword);
            KeywordList.Add("SYSTEM_USER", KeywordType.FunctionKeyword);
            KeywordList.Add("TABLE", KeywordType.OtherKeyword);
            KeywordList.Add("TABLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("TABLOCKX", KeywordType.OtherKeyword);
            KeywordList.Add("TAN", KeywordType.FunctionKeyword);
            KeywordList.Add("TAPE", KeywordType.OtherKeyword);
            KeywordList.Add("TEMP", KeywordType.OtherKeyword);
            KeywordList.Add("TEMPORARY", KeywordType.OtherKeyword);
            KeywordList.Add("TEXT", KeywordType.DataTypeKeyword);
            KeywordList.Add("TEXTPTR", KeywordType.FunctionKeyword);
            KeywordList.Add("TEXTSIZE", KeywordType.OtherKeyword);
            KeywordList.Add("TEXTVALID", KeywordType.FunctionKeyword);
            KeywordList.Add("THEN", KeywordType.OtherKeyword);
            KeywordList.Add("TIME", KeywordType.DataTypeKeyword); //not strictly-speaking true, can also be keyword in WAITFOR TIME
            KeywordList.Add("TIMESTAMP", KeywordType.DataTypeKeyword);
            KeywordList.Add("TINYINT", KeywordType.DataTypeKeyword);
            KeywordList.Add("TO", KeywordType.OtherKeyword);
            KeywordList.Add("TOP", KeywordType.OtherKeyword);
            KeywordList.Add("TOSTRING", KeywordType.FunctionKeyword);
            KeywordList.Add("TRACEOFF", KeywordType.OtherKeyword);
            KeywordList.Add("TRACEON", KeywordType.OtherKeyword);
            KeywordList.Add("TRACESTATUS", KeywordType.OtherKeyword);
            KeywordList.Add("TRAN", KeywordType.OtherKeyword);
            KeywordList.Add("TRANSACTION", KeywordType.OtherKeyword);
            KeywordList.Add("TRIGGER", KeywordType.OtherKeyword);
            KeywordList.Add("TRUNCATE", KeywordType.OtherKeyword);
            KeywordList.Add("TSEQUAL", KeywordType.OtherKeyword);
            KeywordList.Add("TYPEPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("TYPE_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("TYPE_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("TYPE_WARNING", KeywordType.OtherKeyword);
            KeywordList.Add("UNCOMMITTED", KeywordType.OtherKeyword);
            KeywordList.Add("UNICODE", KeywordType.FunctionKeyword);
            KeywordList.Add("UNION", KeywordType.OtherKeyword);
            KeywordList.Add("UNIQUE", KeywordType.OtherKeyword);
            KeywordList.Add("UNIQUEIDENTIFIER", KeywordType.DataTypeKeyword);
            KeywordList.Add("UNKNOWN", KeywordType.OtherKeyword);
            KeywordList.Add("UNPINTABLE", KeywordType.OtherKeyword);
            KeywordList.Add("UPDATE", KeywordType.OtherKeyword);
            KeywordList.Add("UPDATETEXT", KeywordType.OtherKeyword);
            KeywordList.Add("UPDATEUSAGE", KeywordType.OtherKeyword);
            KeywordList.Add("UPDLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("UPPER", KeywordType.FunctionKeyword);
            KeywordList.Add("USE", KeywordType.OtherKeyword);
            KeywordList.Add("USER", KeywordType.FunctionKeyword);
            KeywordList.Add("USEROPTIONS", KeywordType.OtherKeyword);
            KeywordList.Add("USER_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("USER_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("USING", KeywordType.OtherKeyword);
            KeywordList.Add("VALUE", KeywordType.FunctionKeyword);
            KeywordList.Add("VALUES", KeywordType.OtherKeyword);
            KeywordList.Add("VAR", KeywordType.FunctionKeyword);
            KeywordList.Add("VARBINARY", KeywordType.DataTypeKeyword);
            KeywordList.Add("VARCHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("VARP", KeywordType.FunctionKeyword);
            KeywordList.Add("VARYING", KeywordType.OtherKeyword);
            KeywordList.Add("VIEW", KeywordType.OtherKeyword);
            KeywordList.Add("VIEWS", KeywordType.OtherKeyword);
            KeywordList.Add("WAITFOR", KeywordType.OtherKeyword);
            KeywordList.Add("WHEN", KeywordType.OtherKeyword);
            KeywordList.Add("WHERE", KeywordType.OtherKeyword);
            KeywordList.Add("WHILE", KeywordType.OtherKeyword);
            KeywordList.Add("WITH", KeywordType.OtherKeyword);
            KeywordList.Add("WORK", KeywordType.OtherKeyword);
            KeywordList.Add("WRITE", KeywordType.FunctionKeyword);
            KeywordList.Add("WRITETEXT", KeywordType.OtherKeyword);
            KeywordList.Add("XACT_ABORT", KeywordType.OtherKeyword);
            KeywordList.Add("XLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("XML", KeywordType.DataTypeKeyword);
            KeywordList.Add("YEAR", KeywordType.FunctionKeyword);
        }

        public enum KeywordType
        {
            OperatorKeyword,
            FunctionKeyword,
            DataTypeKeyword,
            OtherKeyword
        }
    }
}
