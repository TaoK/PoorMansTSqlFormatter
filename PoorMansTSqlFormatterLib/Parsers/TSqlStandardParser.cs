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
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Parsers
{
    public class TSqlStandardParser : Interfaces.ISqlTokenParser
    {
        /*
         * TODO:
         *  - update the demo UI to reference GPL, and publish the program
         *  - Manually review the output from all test cases for "strange" effects
         *  - handle Ranking Functions with multiple partition or order by columns/clauses
         *  - parse ON sections, for those who prefer to start ON on the next line and indent from there
         *  - detect table hints, to avoid them looking like function parens
         *  - Fix join phrases from "Compound Keywords" to a new container, for better whitespace handling.
         *  
         *  - Tests
         *    - Samples illustrating all the tokens and container combinations implemented
         *    - Samples illustrating all forms of container violations
         *    - Sample requests and their XML equivalent - once the xml format is more-or-less formalized
         *    - Sample requests and their formatted versions (a few for each) - once the "standard" format is more-or-less formalized
         */

        //for performance, it may make sense to make a singleton of this at some point...
        public Dictionary<string, KeywordType> KeywordList { get; set; }
        Regex _JoinDetector = new Regex("^(RIGHT|INNER|LEFT|CROSS|FULL) (OUTER )?((HASH|LOOP|MERGE|REMOTE) )?(JOIN|APPLY) ");

        public TSqlStandardParser()
        {
            InitializeKeywordList();
        }

        public XmlDocument ParseSQL(ITokenList tokenList)
        {
            XmlDocument sqlTree = new XmlDocument();
            XmlElement firstStatement;
            XmlElement currentContainerNode;
            bool errorFound = false;
            bool dataShufflingForced = false;

            errorFound = tokenList.HasErrors;

            sqlTree.AppendChild(sqlTree.CreateElement(SqlXmlConstants.ENAME_SQL_ROOT));
            firstStatement = sqlTree.CreateElement(SqlXmlConstants.ENAME_SQL_STATEMENT);
            currentContainerNode = sqlTree.CreateElement(SqlXmlConstants.ENAME_SQL_CLAUSE);
            firstStatement.AppendChild(currentContainerNode);
            sqlTree.DocumentElement.AppendChild(firstStatement);

            int tokenCount = tokenList.Count;
            int tokenID = 0;
            while (tokenID < tokenCount)
            {
                IToken token = tokenList[tokenID];

                switch (token.Type)
                {
                    case SqlTokenType.OpenParens:
                        XmlElement firstNonCommentParensSibling = GetFirstNonWhitespaceNonCommentChildElement(currentContainerNode);
                        bool isInsertClause = (
                            firstNonCommentParensSibling != null
                            && firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                            && firstNonCommentParensSibling.InnerText.ToUpper().StartsWith("INSERT")
                            );

                        if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CTE_WITH_CLAUSE))
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_PARENS, "", currentContainerNode);
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CTE_AS_BLOCK))
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_EXPRESSION_PARENS, "", currentContainerNode);
                        else if (IsLatestTokenADDLDetailValue(currentContainerNode))
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDLDETAIL_PARENS, "", currentContainerNode);
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                            || isInsertClause
                            )
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_PARENS, "", currentContainerNode);
                        else if (IsLatestTokenAMiscName(currentContainerNode))
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_FUNCTION_PARENS, "", currentContainerNode);
                        else
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_EXPRESSION_PARENS, "", currentContainerNode);
                        break;

                    case SqlTokenType.CloseParens:
                        EscapeAnyBetweenConditions(ref currentContainerNode);
                        //check whether we expected to end the parens...
                        if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDLDETAIL_PARENS)
                            || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_PARENS)
                            || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_FUNCTION_PARENS)
                            || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                            )
                        {
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode;
                        }
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CTE_AS_BLOCK)
                                )
                        {
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode.ParentNode;
                        }
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                                )
                        {
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode;
                        }
                        else
                        {
                            SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, ")", currentContainerNode, ref errorFound);
                        }
                        break;

                    case SqlTokenType.OtherNode:

                        //prepare multi-keyword detection by "peeking" up to 4 keywords ahead
                        List<List<IToken>> compoundKeywordOverflowNodes = null;
                        List<int> compoundKeywordTokenCounts = null;
                        List<string> compoundKeywordRawStrings = null;
                        string keywordMatchPhrase = GetKeywordMatchPhrase(tokenList, tokenID, ref compoundKeywordRawStrings, ref compoundKeywordTokenCounts, ref compoundKeywordOverflowNodes);
                        int keywordMatchStringsUsed = 0;

                        if (keywordMatchPhrase.StartsWith("CREATE PROC")
                            || keywordMatchPhrase.StartsWith("CREATE FUNC")
                            || keywordMatchPhrase.StartsWith("ALTER PROC")
                            || keywordMatchPhrase.StartsWith("ALTER FUNC")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK, "", currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("CREATE ")
                            || keywordMatchPhrase.StartsWith("ALTER ")
                            || keywordMatchPhrase.StartsWith("DECLARE ")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_OTHER_BLOCK, "", currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            && keywordMatchPhrase.StartsWith("RETURNS ")
                            )
                        {
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_RETURNS, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("AS "))
                        {
                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK))
                            {
                                XmlElement newASBlock = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_AS_BLOCK, token.Value, currentContainerNode);
                                currentContainerNode = StartNewStatement(sqlTree, newASBlock);
                            }
                            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CTE_WITH_CLAUSE))
                            {
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CTE_AS_BLOCK, token.Value, currentContainerNode);
                            }
                            else
                            {
                                SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("BEGIN TRANSACTION ")
                            || keywordMatchPhrase.StartsWith("BEGIN TRAN ")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_BEGIN_TRANSACTION, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                        }
                        else if (keywordMatchPhrase.StartsWith("COMMIT TRANSACTION ")
                            || keywordMatchPhrase.StartsWith("COMMIT TRAN ")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_COMMIT_TRANSACTION, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                        }
                        else if (keywordMatchPhrase.StartsWith("ROLLBACK TRANSACTION ")
                            || keywordMatchPhrase.StartsWith("ROLLBACK TRAN ")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                        }
                        else if (keywordMatchPhrase.StartsWith("BEGIN TRY "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            XmlElement newTryBlock = ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_TRY_BLOCK, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            currentContainerNode = StartNewStatement(sqlTree, newTryBlock);
                        }
                        else if (keywordMatchPhrase.StartsWith("BEGIN CATCH "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            XmlElement newCatchBlock = ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_CATCH_BLOCK, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            currentContainerNode = StartNewStatement(sqlTree, newCatchBlock);
                        }
                        else if (keywordMatchPhrase.StartsWith("BEGIN "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            XmlElement newBeginBlock = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BEGIN_END_BLOCK, token.Value, currentContainerNode);
                            currentContainerNode = StartNewStatement(sqlTree, newBeginBlock);
                        }
                        else if (keywordMatchPhrase.StartsWith("CASE "))
                        {
                            XmlElement newCaseStatement = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_STATEMENT, token.Value, currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_INPUT, "", newCaseStatement);
                        }
                        else if (keywordMatchPhrase.StartsWith("WHEN "))
                        {
                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_INPUT))
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_WHEN, token.Value, (XmlElement)currentContainerNode.ParentNode);
                            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_THEN))
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_WHEN, token.Value, (XmlElement)currentContainerNode.ParentNode.ParentNode);
                            else
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                        }
                        else if (keywordMatchPhrase.StartsWith("THEN "))
                        {
                            EscapeAnyBetweenConditions(ref currentContainerNode);

                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_WHEN))
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_THEN, token.Value, currentContainerNode);
                            else
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                        }
                        else if (keywordMatchPhrase.StartsWith("END TRY "))
                        {
                            EscapeAnySingleStatementContainers(ref currentContainerNode);

                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);

                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_TRY_BLOCK))
                            {
                                currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(keywordString));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                            }
                            else
                            {
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, keywordString, currentContainerNode, ref errorFound);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("END CATCH "))
                        {
                            EscapeAnySingleStatementContainers(ref currentContainerNode);

                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);

                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_CATCH_BLOCK))
                            {
                                currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(keywordString));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                            }
                            else
                            {
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, keywordString, currentContainerNode, ref errorFound);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("END "))
                        {
                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_THEN))
                            {
                                currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(token.Value));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                            }
                            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_ELSE))
                            {
                                currentContainerNode.ParentNode.AppendChild(sqlTree.CreateTextNode(token.Value));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode;
                            }
                            else
                            {
                                //Begin/End block handling
                                EscapeAnySingleStatementContainers(ref currentContainerNode);

                                if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_BEGIN_END_BLOCK))
                                {
                                    currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(token.Value));
                                    currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                                }
                                else
                                {
                                    SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                                }
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("GO "))
                        {
                            EscapeAnySingleStatementContainers(ref currentContainerNode);

                            //this looks a little simplistic... might need to review.
                            if ((tokenID == 0 || IsLineBreakingWhiteSpace(tokenList[tokenID - 1]))
                                && (tokenID == tokenCount - 1
                                    || (tokenID == tokenCount - 2 && tokenList[tokenID + 1].Type == SqlTokenType.WhiteSpace)
                                    || IsLineBreakingWhiteSpace(tokenList[tokenID + 1]))
                                )
                            {
                                // we found a batch separator - were we supposed to?
                                if (ValidBatchEnd(ref currentContainerNode))
                                {
                                    XmlElement sqlRoot = sqlTree.DocumentElement;
                                    SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BATCH_SEPARATOR, token.Value, sqlRoot);
                                    currentContainerNode = StartNewStatement(sqlTree, sqlRoot);
                                }
                                else
                                {
                                    SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                                }
                            }
                            else
                            {
                                SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("JOIN "))
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (_JoinDetector.IsMatch(keywordMatchPhrase))
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            string joinText = _JoinDetector.Match(keywordMatchPhrase).Value;
                            keywordMatchStringsUsed = joinText.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries).Length;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, keywordString, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("UNION ALL "))
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_UNION_CLAUSE, keywordString, currentContainerNode);
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode;
                        }
                        else if (keywordMatchPhrase.StartsWith("UNION "))
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_UNION_CLAUSE, token.Value, currentContainerNode);
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode;
                        }
                        else if (keywordMatchPhrase.StartsWith("WHILE "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            XmlElement newWhileLoop = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_WHILE_LOOP, token.Value, currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION, "", newWhileLoop);
                        }
                        else if (keywordMatchPhrase.StartsWith("IF "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            XmlElement newIfStatement = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_IF_STATEMENT, token.Value, currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION, "", newIfStatement);
                        }
                        else if (keywordMatchPhrase.StartsWith("ELSE "))
                        {
                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_CASE_THEN))
                            {
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CASE_ELSE, token.Value, (XmlElement)currentContainerNode.ParentNode.ParentNode);
                            }
                            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT))
                            {
                                //topmost if - just pop back one.
                                XmlElement containerIf = (XmlElement)currentContainerNode.ParentNode.ParentNode;
                                XmlElement newElseClause = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_ELSE_CLAUSE, token.Value, containerIf);
                                currentContainerNode = StartNewStatement(sqlTree, newElseClause);
                            }
                            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_ELSE_CLAUSE)
                                    && currentContainerNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                                )
                            {
                                //not topmost if; we need to pop up the single-statement containers stack to the next "if" that doesn't have an "else".
                                XmlElement currentNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                                while (currentNode != null
                                    && (currentNode.Name.Equals(SqlXmlConstants.ENAME_WHILE_LOOP)
                                        || currentNode.SelectSingleNode(SqlXmlConstants.ENAME_ELSE_CLAUSE) != null
                                        )
                                    )
                                {
                                    if (currentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                        && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                                        )
                                        currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode;
                                    else if (currentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                        && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_WHILE_LOOP)
                                        )
                                        currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode;
                                    else if (currentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                        && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_ELSE_CLAUSE)
                                        && currentNode.ParentNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                                        )
                                        currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode.ParentNode;
                                    else
                                        currentNode = null;
                                }

                                if (currentNode != null)
                                {
                                    XmlElement newElseClause2 = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_ELSE_CLAUSE, token.Value, currentNode);
                                    currentContainerNode = StartNewStatement(sqlTree, newElseClause2);
                                }
                                else
                                {
                                    SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode, ref errorFound);
                                }
                            }
                            else
                            {
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode, ref errorFound);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("INSERT INTO "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, keywordString, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("INSERT "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("SELECT "))
                        {
                            XmlElement firstNonCommentSibling = GetFirstNonWhitespaceNonCommentChildElement(currentContainerNode);
                            if (!(
                                    firstNonCommentSibling != null
                                    && firstNonCommentSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                    && firstNonCommentSibling.InnerText.ToUpper().StartsWith("INSERT")
                                    )
                                )
                                ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);

                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("SET "))
                        {
                            XmlElement firstNonCommentSibling2 = GetFirstNonWhitespaceNonCommentChildElement(currentContainerNode);
                            if (!(
                                    firstNonCommentSibling2 != null
                                    && firstNonCommentSibling2.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                                    && firstNonCommentSibling2.InnerText.ToUpper().StartsWith("UPDATE")
                                    )
                                )
                                ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);

                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("BETWEEN "))
                        {
                            XmlElement newBetweenCondition = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BETWEEN_CONDITION, token.Value, currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND, "", newBetweenCondition);
                        }
                        else if (keywordMatchPhrase.StartsWith("AND "))
                        {
                            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND))
                            {
                                currentContainerNode.ParentNode.AppendChild(sqlTree.CreateTextNode(token.Value));
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND, "", (XmlElement)currentContainerNode.ParentNode);
                            }
                            else
                            {
                                EscapeAnyBetweenConditions(ref currentContainerNode);
                                SaveNewElement(sqlTree, SqlXmlConstants.ENAME_AND_OPERATOR, token.Value, currentContainerNode);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("OR "))
                        {
                            EscapeAnyBetweenConditions(ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OR_OPERATOR, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("WITH "))
                        {
                            if (currentContainerNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.SqlXmlConstants.ENAME_SQL_STATEMENT)
                                && !HasNonWhiteSpaceNonCommentContent(currentContainerNode)
                                )
                            {
                                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE, token.Value, currentContainerNode);
                            }
                            else
                            {
                                SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERKEYWORD, token.Value, currentContainerNode);
                            }
                        }
                        else
                        {
                            //miscellaneous single-word tokens, which may or may not be statement starters and/or clause starters

                            //check for statements starting...
                            if (IsStatementStarter(token))
                            {
                                ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            }

                            //check for statements starting...
                            if (IsClauseStarter(token))
                            {
                                ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            }

                            string newNodeName = SqlXmlConstants.ENAME_OTHERNODE;
                            KeywordType matchedKeywordType;
                            if (KeywordList.TryGetValue(token.Value.ToUpper(), out matchedKeywordType))
                            {
                                switch(matchedKeywordType)
                                {
                                    case KeywordType.OperatorKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_OTHEROPERATOR;
                                        break;
                                    case KeywordType.FunctionKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_FUNCTION_KEYWORD;
                                        break;
                                    case KeywordType.DataTypeKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_DATATYPE_KEYWORD;
                                        break;
                                    case KeywordType.OtherKeyword:
                                        newNodeName = SqlXmlConstants.ENAME_OTHERKEYWORD;
                                        break;
                                    default:
                                        throw new Exception("Unrecognized Keyword Type!");
                                }
                            }

                            SaveNewElement(sqlTree, newNodeName, token.Value, currentContainerNode);
                        }

                        //handle any Overflow Nodes
                        if (keywordMatchStringsUsed > 1)
                        {
                            for (int i = 0; i < keywordMatchStringsUsed - 1; i++)
                            {
                                foreach (XmlElement overflowEntry in compoundKeywordOverflowNodes[i])
                                {
                                    currentContainerNode.AppendChild(overflowEntry);
                                    dataShufflingForced = true;
                                }
                            }
                        }

                        break;

                    case SqlTokenType.Semicolon:
                        SaveNewElement(sqlTree, SqlXmlConstants.ENAME_SEMICOLON, token.Value, currentContainerNode);
                        ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                        break;

                    case SqlTokenType.MultiLineComment:
                    case SqlTokenType.SingleLineComment:
                    case SqlTokenType.WhiteSpace:
                        //create in statement rather than clause if there are no siblings yet
                        if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                            && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                            && currentContainerNode.SelectSingleNode("*") == null
                            )
                            SaveNewElementAsPriorSibling(sqlTree, GetEquivalentSqlNodeName(token.Type), token.Value, currentContainerNode);
                        else
                            SaveNewElement(sqlTree, GetEquivalentSqlNodeName(token.Type), token.Value, currentContainerNode);
                        break;

                    case SqlTokenType.QuotedIdentifier:
                    case SqlTokenType.Asterisk:
                    case SqlTokenType.Comma:
                    case SqlTokenType.Period:
                    case SqlTokenType.NationalString:
                    case SqlTokenType.String:
                    case SqlTokenType.OtherOperator:
                        SaveNewElement(sqlTree, GetEquivalentSqlNodeName(token.Type), token.Value, currentContainerNode);
                        break;
                    default:
                        throw new Exception("Unrecognized element encountered!");
                }

                tokenID++;
            }

            if (!ValidBatchEnd(ref currentContainerNode))
            {
                errorFound = true;
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
            }

            if (errorFound)
            {
                sqlTree.DocumentElement.SetAttribute(SqlXmlConstants.ANAME_ERRORFOUND, "1");
            }

            if (dataShufflingForced)
            {
                sqlTree.DocumentElement.SetAttribute(SqlXmlConstants.ANAME_DATALOSS, "1");
            }

            return sqlTree;
        }

        private bool ValidBatchEnd(ref XmlElement currentContainerNode)
        {
            XmlElement nextStatementContainer = LocateNextStatementContainer(ref currentContainerNode, true);
            return nextStatementContainer != null 
                && (nextStatementContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_ROOT)
                    || nextStatementContainer.Name.Equals(SqlXmlConstants.ENAME_DDL_AS_BLOCK)
                    );
        }

        private string GetEquivalentSqlNodeName(SqlTokenType tokenType)
        {
            switch (tokenType)
            {
                case SqlTokenType.WhiteSpace:
                    return SqlXmlConstants.ENAME_WHITESPACE;
                case SqlTokenType.SingleLineComment:
                    return SqlXmlConstants.ENAME_COMMENT_SINGLELINE;
                case SqlTokenType.MultiLineComment:
                    return SqlXmlConstants.ENAME_COMMENT_MULTILINE;
                case SqlTokenType.QuotedIdentifier:
                    return SqlXmlConstants.ENAME_QUOTED_IDENTIFIER;
                case SqlTokenType.Asterisk:
                    return SqlXmlConstants.ENAME_ASTERISK;
                case SqlTokenType.Comma:
                    return SqlXmlConstants.ENAME_COMMA;
                case SqlTokenType.Period:
                    return SqlXmlConstants.ENAME_PERIOD;
                case SqlTokenType.NationalString:
                    return SqlXmlConstants.ENAME_NSTRING;
                case SqlTokenType.String:
                    return SqlXmlConstants.ENAME_STRING;
                case SqlTokenType.OtherOperator:
                    return SqlXmlConstants.ENAME_OTHEROPERATOR;
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

            while (tokenID < tokenList.Count && phraseComponentsFound < 4)
            {
                if (tokenList[tokenID].Type == SqlTokenType.OtherNode
                    || tokenList[tokenID].Type == SqlTokenType.QuotedIdentifier
                    )
                {
                    phrase += tokenList[tokenID].Value.ToUpper() + " ";
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

        private XmlElement SaveNewElement(XmlDocument sqlTree, string newElementName, string newElementValue, XmlElement currentContainerNode)
        {
            XmlElement newElement = sqlTree.CreateElement(newElementName);
            newElement.InnerText = newElementValue;
            currentContainerNode.AppendChild(newElement);
            return newElement;
        }

        private XmlElement SaveNewElementAsPriorSibling(XmlDocument sqlTree, string newElementName, string newElementValue, XmlElement nodeToSaveBefore)
        {
            XmlElement newElement = sqlTree.CreateElement(newElementName);
            newElement.InnerText = newElementValue;
            nodeToSaveBefore.ParentNode.InsertBefore(newElement, nodeToSaveBefore);
            return newElement;
        }

        private void SaveNewElementWithError(XmlDocument sqlTree, string newElementName, string newElementValue, XmlElement currentContainerNode, ref bool errorFound)
        {
            SaveNewElement(sqlTree, newElementName, newElementValue, currentContainerNode);
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            errorFound = true;
        }

        private XmlElement ProcessCompoundKeyword(XmlDocument sqlTree, string newElementName, ref int tokenID, XmlElement currentContainerNode, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
        {
            XmlElement newElement = sqlTree.CreateElement(newElementName);
            newElement.InnerText = GetCompoundKeyword(ref tokenID, compoundKeywordCount, compoundKeywordTokenCounts, compoundKeywordRawStrings);
            currentContainerNode.AppendChild(newElement);
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

        private void ConsiderStartingNewStatement(XmlDocument sqlTree, ref XmlElement currentContainerNode)
        {
            EscapeAnyBetweenConditions(ref currentContainerNode);

            XmlElement previousContainerElement = currentContainerNode;

            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION)
                && (currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                    || currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_WHILE_LOOP)
                    )
                )
            {
                //we just ended the boolean clause of an if or while, and need to pop to the first (and only) statement.
                currentContainerNode = StartNewStatement(sqlTree, (XmlElement)currentContainerNode.ParentNode);
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
            else
            {
                XmlElement nextStatementContainer = LocateNextStatementContainer(ref currentContainerNode, false);
                if (nextStatementContainer != null)
                {
                    XmlElement inBetweenContainerElement = currentContainerNode;
                    currentContainerNode = StartNewStatement(sqlTree, nextStatementContainer);
                    if (!inBetweenContainerElement.Equals(previousContainerElement))
                        MigrateApplicableComments(inBetweenContainerElement, currentContainerNode);
                    MigrateApplicableComments(previousContainerElement, currentContainerNode);
                }
            }
        }

        private XmlElement LocateNextStatementContainer(ref XmlElement currentContainerNode, bool escapeEmptyContainer)
        {
            EscapeAnySingleStatementContainers(ref currentContainerNode);
            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                && (escapeEmptyContainer || HasNonWhiteSpaceNonSingleCommentContent(currentContainerNode))
                )
                return (XmlElement)currentContainerNode.ParentNode.ParentNode;
            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                )
                return (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
            else
                return null;
        }

        private XmlElement StartNewStatement(XmlDocument sqlTree, XmlElement containerElement)
        {
            XmlElement newStatement = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_SQL_STATEMENT, "", containerElement);
            return SaveNewElement(sqlTree, SqlXmlConstants.ENAME_SQL_CLAUSE, "", newStatement);
        }

        private void ConsiderStartingNewClause(XmlDocument sqlTree, ref XmlElement currentContainerNode)
        {
            EscapeAnyBetweenConditions(ref currentContainerNode);

            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                && HasNonWhiteSpaceNonSingleCommentContent(currentContainerNode)
                )
            {
                //complete current clause, start a new one in the same container
                XmlElement previousContainerElement = currentContainerNode;
                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_SQL_CLAUSE, "", (XmlElement)currentContainerNode.ParentNode);
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
                || currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT))
            {
                //create new clause and set context to it.
                currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_SQL_CLAUSE, "", currentContainerNode);
            }
        }

        private static void MigrateApplicableComments(XmlElement previousContainerElement, XmlElement currentContainerNode)
        {
            XmlNode migrationCandidate = previousContainerElement.LastChild;

            while (migrationCandidate != null)
            {
                if (migrationCandidate.NodeType == XmlNodeType.Whitespace
                    || migrationCandidate.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE))
                {
                    migrationCandidate = migrationCandidate.PreviousSibling;
                    continue;
                }
                else if (migrationCandidate.PreviousSibling != null
                    && (migrationCandidate.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                        || migrationCandidate.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                        )
                    && (migrationCandidate.PreviousSibling.NodeType == XmlNodeType.Whitespace
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                        )
                    )
                {
                    if ((migrationCandidate.PreviousSibling.NodeType == XmlNodeType.Whitespace
                            || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                            )
                        && Regex.IsMatch(migrationCandidate.PreviousSibling.InnerText, @"(\r|\n)+")
                        )
                    {
                        //migrate everything considered so far, and move on to the next one for consideration.
                        while (!previousContainerElement.LastChild.Equals(migrationCandidate))
                        {
                            currentContainerNode.ParentNode.PrependChild(previousContainerElement.LastChild);
                        }
                        currentContainerNode.ParentNode.PrependChild(migrationCandidate);
                        migrationCandidate = previousContainerElement.LastChild;
                    }
                    else
                    {
                        //this one wasn't properly separated from the previous node/entry, keep going in case there's a linebreak further up.
                        migrationCandidate = migrationCandidate.PreviousSibling;
                    }
                }
                else
                {
                    //we found a non-whitespace non-comment node. Stop trying to migrate comments.
                    migrationCandidate = null;
                }
            }
        }

        private static void EscapeAnySingleStatementContainers(ref XmlElement currentContainerNode)
        {
            EscapeAnyBetweenConditions(ref currentContainerNode);

            if (HasNonWhiteSpaceNonCommentContent(currentContainerNode))
            {
                while (true)
                {
                    if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && (currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                            || currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_WHILE_LOOP)
                            )
                        )
                    {

                        //we just ended the one statement of an if or while, and need to pop out to a new statement at the same level as the IF or WHILE
                        currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                    }
                    else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_ELSE_CLAUSE)
                        && currentContainerNode.ParentNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_IF_STATEMENT)
                        )
                    {
                        //we just ended the one and only statement in an else clause, and need to pop out to a new statement at the same level as the IF
                        currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode.ParentNode;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void EscapeAnyBetweenConditions(ref XmlElement currentContainerNode)
        {
            if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND)
                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_BETWEEN_CONDITION))
            {
                //we just ended the upper bound of a "BETWEEN" condition, need to pop back to the enclosing context
                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode;
            }
        }

        private XmlElement GetFirstNonWhitespaceNonCommentChildElement(XmlElement currentContainerNode)
        {
            XmlNode currentNode = currentContainerNode.FirstChild;
            while (currentNode != null)
            {
                if (IsCommentOrWhiteSpace(currentNode) || currentNode.NodeType != XmlNodeType.Element)
                    currentNode = currentNode.NextSibling;
                else
                    return (XmlElement)currentNode;
            }
            return null;
        }

        private static bool IsStatementStarter(IToken token)
        {
            string uppercaseValue = token.Value.ToUpper();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("ALTER")
                    || uppercaseValue.Equals("BREAK")
                    || uppercaseValue.Equals("CLOSE")
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
                    || uppercaseValue.Equals("OPEN")
                    || uppercaseValue.Equals("PRINT")
                    || uppercaseValue.Equals("RAISERROR")
                    || uppercaseValue.Equals("RETURN")
                    || uppercaseValue.Equals("SET")
                    || uppercaseValue.Equals("SETUSER")
                    || uppercaseValue.Equals("TRUNCATE")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("WHILE")
                    || uppercaseValue.Equals("USE")
                    || uppercaseValue.Equals("WAITFOR")
                    )
                );
        }

        private static bool IsClauseStarter(IToken token)
        {
            string uppercaseValue = token.Value.ToUpper();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("INNER")
                    || uppercaseValue.Equals("LEFT")
                    || uppercaseValue.Equals("JOIN")
                    || uppercaseValue.Equals("WHERE")
                    || uppercaseValue.Equals("FROM")
                    || uppercaseValue.Equals("ORDER")
                    || uppercaseValue.Equals("GROUP")
                    || uppercaseValue.Equals("HAVING")
                    || uppercaseValue.Equals("INTO")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("UNION")
                    || uppercaseValue.Equals("VALUES")
                    || uppercaseValue.Equals("RETURNS")
                    || uppercaseValue.Equals("FOR")
                    || uppercaseValue.Equals("PIVOT")
                    || uppercaseValue.Equals("UNPIVOT")
                    )
                );
        }

        private bool IsLatestTokenADDLDetailValue(XmlElement currentContainerNode)
        {
            XmlNode currentNode = currentContainerNode.LastChild;
            while (currentNode != null)
            {
                if ((currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
                        || currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERKEYWORD)
                        || currentNode.Name.Equals(SqlXmlConstants.ENAME_DATATYPE_KEYWORD)
                        )
                    && (
                        currentNode.InnerText.ToUpper().Equals("NVARCHAR")
                        || currentNode.InnerText.ToUpper().Equals("VARCHAR")
                        || currentNode.InnerText.ToUpper().Equals("DECIMAL")
                        || currentNode.InnerText.ToUpper().Equals("NUMERIC")
                        || currentNode.InnerText.ToUpper().Equals("VARBINARY")
                        || currentNode.InnerText.ToUpper().Equals("DEFAULT")
                        || currentNode.InnerText.ToUpper().Equals("IDENTITY")
                        )
                    )
                {
                    return true;
                }
                else if (IsCommentOrWhiteSpace(currentNode))
                {
                    currentNode = currentNode.PreviousSibling;
                }
                else 
                    currentNode = null;
            }
            return false;
        }

        private bool IsLatestTokenAMiscName(XmlElement currentContainerNode)
        {
            XmlNode currentNode = currentContainerNode.LastChild;
            while (currentNode != null)
            {
                string testValue = currentNode.InnerText.ToUpper();
                if (currentNode.Name.Equals(SqlXmlConstants.ENAME_QUOTED_IDENTIFIER)
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
                            || testValue.Equals("AS")
                            || testValue.EndsWith(" APPLY")
                            )
                        )
                    )
                {
                    return true;
                }
                else if (IsCommentOrWhiteSpace(currentNode))
                {
                    currentNode = currentNode.PreviousSibling;
                }
                else
                    currentNode = null;
            }
            return false;
        }

        private static bool IsLineBreakingWhiteSpace(IToken token)
        {
            return (token.Type == SqlTokenType.WhiteSpace
                && Regex.IsMatch(token.Value, @"(\r|\n)+"));
        }

        private bool IsCommentOrWhiteSpace(XmlNode currentNode)
        {
            return (currentNode.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                || currentNode.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                || currentNode.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                );
        }

        private static bool HasNonWhiteSpaceNonSingleCommentContent(XmlElement containerNode)
        {
            foreach (XmlElement testElement in containerNode.SelectNodes("*"))
                if (!testElement.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                    && !testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                    && (!testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                        || Regex.IsMatch(testElement.InnerText, @"(\r|\n)+")
                        )
                    )
                    return true;

            return false;
        }

        private static bool HasNonWhiteSpaceNonCommentContent(XmlElement containerNode)
        {
            foreach (XmlElement testElement in containerNode.SelectNodes("*"))
                if (!testElement.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                    && !testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                    && !testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                    )
                    return true;

            return false;
        }

        private void InitializeKeywordList()
        {
            //List looks pretty comprehensive, it's basically copied from Side by Side SQL Comparer project from CodeProject:
            // http://www.codeproject.com/KB/database/SideBySideSQLComparer.aspx
            KeywordList = new Dictionary<string, KeywordType>();
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
            KeywordList.Add("CASCADE", KeywordType.OtherKeyword);
            KeywordList.Add("CASE", KeywordType.FunctionKeyword);
            KeywordList.Add("CAST", KeywordType.FunctionKeyword);
            KeywordList.Add("CEILING", KeywordType.FunctionKeyword);
            KeywordList.Add("CHAR", KeywordType.DataTypeKeyword);
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
            KeywordList.Add("DATEPART", KeywordType.FunctionKeyword);
            KeywordList.Add("DATETIME", KeywordType.DataTypeKeyword);
            KeywordList.Add("DAY", KeywordType.FunctionKeyword);
            KeywordList.Add("DBCC", KeywordType.OtherKeyword);
            KeywordList.Add("DBREINDEX", KeywordType.OtherKeyword);
            KeywordList.Add("DBREPAIR", KeywordType.OtherKeyword);
            KeywordList.Add("DB_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("DB_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("DEADLOCK_PRIORITY", KeywordType.OtherKeyword);
            KeywordList.Add("DEALLOCATE", KeywordType.OtherKeyword);
            KeywordList.Add("DECIMAL", KeywordType.DataTypeKeyword);
            KeywordList.Add("DECLARE", KeywordType.OtherKeyword);
            KeywordList.Add("DEFAULT", KeywordType.OtherKeyword);
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
            KeywordList.Add("DROP", KeywordType.OtherKeyword);
            KeywordList.Add("DROPCLEANBUFFERS", KeywordType.OtherKeyword);
            KeywordList.Add("DUMMY", KeywordType.OtherKeyword);
            KeywordList.Add("DUMP", KeywordType.OtherKeyword);
            KeywordList.Add("ELSE", KeywordType.OtherKeyword);
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
            KeywordList.Add("FAST", KeywordType.OtherKeyword);
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
            KeywordList.Add("FREEPROCCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREESESSIONCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREESYSTEMCACHE", KeywordType.OtherKeyword);
            KeywordList.Add("FREETEXT", KeywordType.OtherKeyword);
            KeywordList.Add("FREETEXTTABLE", KeywordType.FunctionKeyword);
            KeywordList.Add("FROM", KeywordType.OtherKeyword);
            KeywordList.Add("FULL", KeywordType.OtherKeyword);
            KeywordList.Add("FULLTEXTCATALOGPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FULLTEXTSERVICEPROPERTY", KeywordType.FunctionKeyword);
            KeywordList.Add("FUNCTION", KeywordType.OtherKeyword);
            KeywordList.Add("GETANSINULL", KeywordType.FunctionKeyword);
            KeywordList.Add("GETDATE", KeywordType.FunctionKeyword);
            KeywordList.Add("GO", KeywordType.OtherKeyword);
            KeywordList.Add("GOTO", KeywordType.OtherKeyword);
            KeywordList.Add("GRANT", KeywordType.OtherKeyword);
            KeywordList.Add("GROUP", KeywordType.OtherKeyword);
            KeywordList.Add("GROUPING", KeywordType.FunctionKeyword);
            KeywordList.Add("HASH", KeywordType.OtherKeyword);
            KeywordList.Add("HAVING", KeywordType.OtherKeyword);
            KeywordList.Add("HELP", KeywordType.OtherKeyword);
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
            KeywordList.Add("INSERT", KeywordType.OtherKeyword);
            KeywordList.Add("INT", KeywordType.DataTypeKeyword);
            KeywordList.Add("INTERSECT", KeywordType.OtherKeyword);
            KeywordList.Add("INTO", KeywordType.OtherKeyword);
            KeywordList.Add("IO", KeywordType.OtherKeyword);
            KeywordList.Add("IS", KeywordType.OtherKeyword);
            KeywordList.Add("ISDATE", KeywordType.FunctionKeyword);
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
            KeywordList.Add("KILL", KeywordType.OtherKeyword);
            KeywordList.Add("LANGUAGE", KeywordType.OtherKeyword);
            KeywordList.Add("LEFT", KeywordType.FunctionKeyword);
            KeywordList.Add("LEN", KeywordType.FunctionKeyword);
            KeywordList.Add("LEVEL", KeywordType.OtherKeyword);
            KeywordList.Add("LIKE", KeywordType.OperatorKeyword);
            KeywordList.Add("LINENO", KeywordType.OtherKeyword);
            KeywordList.Add("LOAD", KeywordType.OtherKeyword);
            KeywordList.Add("LOCK_TIMEOUT", KeywordType.OtherKeyword);
            KeywordList.Add("LOG", KeywordType.FunctionKeyword);
            KeywordList.Add("LOG10", KeywordType.FunctionKeyword);
            KeywordList.Add("LOOP", KeywordType.OtherKeyword);
            KeywordList.Add("LOWER", KeywordType.FunctionKeyword);
            KeywordList.Add("LTRIM", KeywordType.FunctionKeyword);
            KeywordList.Add("MAX", KeywordType.FunctionKeyword);
            KeywordList.Add("MAXDOP", KeywordType.OtherKeyword);
            KeywordList.Add("MAXRECURSION", KeywordType.OtherKeyword);
            KeywordList.Add("MERGE", KeywordType.OtherKeyword);
            KeywordList.Add("MIN", KeywordType.FunctionKeyword);
            KeywordList.Add("MIRROREXIT", KeywordType.OtherKeyword);
            KeywordList.Add("MONEY", KeywordType.DataTypeKeyword);
            KeywordList.Add("MONTH", KeywordType.FunctionKeyword);
            KeywordList.Add("NCHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("NEWID", KeywordType.FunctionKeyword);
            KeywordList.Add("NEXT", KeywordType.OtherKeyword);
            KeywordList.Add("NOCHECK", KeywordType.OtherKeyword);
            KeywordList.Add("NOCOUNT", KeywordType.OtherKeyword);
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
            KeywordList.Add("OPTION", KeywordType.OtherKeyword);
            KeywordList.Add("OR", KeywordType.OperatorKeyword);
            KeywordList.Add("ORDER", KeywordType.OtherKeyword);
            KeywordList.Add("OUTER", KeywordType.OtherKeyword);
            KeywordList.Add("OUTPUTBUFFER", KeywordType.OtherKeyword);
            KeywordList.Add("OVER", KeywordType.OtherKeyword);
            KeywordList.Add("PAGLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("PARAMETERIZATION", KeywordType.OtherKeyword);
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
            KeywordList.Add("PROCESSEXIT", KeywordType.OtherKeyword);
            KeywordList.Add("PROCID", KeywordType.OtherKeyword);
            KeywordList.Add("PROFILE", KeywordType.OtherKeyword);
            KeywordList.Add("PUBLIC", KeywordType.OtherKeyword);
            KeywordList.Add("QUERY_GOVERNOR_COST_LIMIT", KeywordType.OtherKeyword);
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
            KeywordList.Add("REVOKE", KeywordType.OtherKeyword);
            KeywordList.Add("RIGHT", KeywordType.FunctionKeyword);
            KeywordList.Add("ROBUST", KeywordType.OtherKeyword);
            KeywordList.Add("ROLLBACK", KeywordType.OtherKeyword);
            KeywordList.Add("ROUND", KeywordType.FunctionKeyword);
            KeywordList.Add("ROWCOUNT", KeywordType.OtherKeyword);
            KeywordList.Add("ROWGUIDCOL", KeywordType.OtherKeyword);
            KeywordList.Add("ROWLOCK", KeywordType.OtherKeyword);
            KeywordList.Add("RTRIM", KeywordType.FunctionKeyword);
            KeywordList.Add("RULE", KeywordType.OtherKeyword);
            KeywordList.Add("SAVE", KeywordType.OtherKeyword);
            KeywordList.Add("SCHEMA", KeywordType.OtherKeyword);
            KeywordList.Add("SCHEMA_ID", KeywordType.FunctionKeyword);
            KeywordList.Add("SCHEMA_NAME", KeywordType.FunctionKeyword);
            KeywordList.Add("SCOPE_IDENTITY", KeywordType.FunctionKeyword);
            KeywordList.Add("SELECT", KeywordType.OtherKeyword);
            KeywordList.Add("SERIALIZABLE", KeywordType.OtherKeyword);
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
            KeywordList.Add("STATISTICS", KeywordType.OtherKeyword);
            KeywordList.Add("STATS_DATE", KeywordType.FunctionKeyword);
            KeywordList.Add("STDEV", KeywordType.FunctionKeyword);
            KeywordList.Add("STDEVP", KeywordType.FunctionKeyword);
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
            KeywordList.Add("TIME", KeywordType.OtherKeyword);
            KeywordList.Add("TIMESTAMP", KeywordType.DataTypeKeyword);
            KeywordList.Add("TINYINT", KeywordType.DataTypeKeyword);
            KeywordList.Add("TO", KeywordType.OtherKeyword);
            KeywordList.Add("TOP", KeywordType.OtherKeyword);
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
            KeywordList.Add("UNCOMMITTED", KeywordType.OtherKeyword);
            KeywordList.Add("UNICODE", KeywordType.FunctionKeyword);
            KeywordList.Add("UNION", KeywordType.OtherKeyword);
            KeywordList.Add("UNIQUE", KeywordType.OtherKeyword);
            KeywordList.Add("UNIQUEIDENTIFIER", KeywordType.DataTypeKeyword);
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
            KeywordList.Add("VALUES", KeywordType.OtherKeyword);
            KeywordList.Add("VAR", KeywordType.FunctionKeyword);
            KeywordList.Add("VARBINARY", KeywordType.DataTypeKeyword);
            KeywordList.Add("VARCHAR", KeywordType.DataTypeKeyword);
            KeywordList.Add("VARP", KeywordType.FunctionKeyword);
            KeywordList.Add("VIEW", KeywordType.OtherKeyword);
            KeywordList.Add("VIEWS", KeywordType.OtherKeyword);
            KeywordList.Add("WAITFOR", KeywordType.OtherKeyword);
            KeywordList.Add("WHEN", KeywordType.OtherKeyword);
            KeywordList.Add("WHERE", KeywordType.OtherKeyword);
            KeywordList.Add("WHILE", KeywordType.OtherKeyword);
            KeywordList.Add("WITH", KeywordType.OtherKeyword);
            KeywordList.Add("WORK", KeywordType.OtherKeyword);
            KeywordList.Add("WRITETEXT", KeywordType.OtherKeyword);
            KeywordList.Add("XACT_ABORT", KeywordType.OtherKeyword);
            KeywordList.Add("XLOCK", KeywordType.OtherKeyword);
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
