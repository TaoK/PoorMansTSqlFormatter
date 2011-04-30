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
         *  - Replace token list Xml Doc with a custom List
         *  - handle CTEs such that AS clause is on its own line
         *  - enhance DDL context to also have clauses (with a backtrack in the standard formatter), for RETURNS...? Or just detect it in formatting?
         *  - update the demo UI to reference GPL, and publish the program
         *  - Add support for join hints, such as "LOOP"
         *  - Manually review the output from all test cases for "strange" effects
         *  - parse ON sections, for those who prefer to start ON on the next line and indent from there
         *  
         *  - Tests
         *    - Samples illustrating all the tokens and container combinations implemented
         *    - Samples illustrating all forms of container violations
         *    - Sample requests and their XML equivalent - once the xml format is more-or-less formalized
         *    - Sample requests and their formatted versions (a few for each) - once the "standard" format is more-or-less formalized
         */

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
                            && firstNonCommentParensSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
                            && firstNonCommentParensSibling.InnerText.ToUpper().StartsWith("INSERT")
                            );

                        if (IsLatestTokenADDLDetailValue(currentContainerNode))
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDLDETAIL_PARENS, "", currentContainerNode);
                        else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_BLOCK)
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

                        if (keywordMatchPhrase.StartsWith("CREATE ")
                            || keywordMatchPhrase.StartsWith("ALTER ")
                            || keywordMatchPhrase.StartsWith("DECLARE ")
                            )
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            currentContainerNode = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_BLOCK, "", currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("AS ") 
                            && currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_BLOCK)
                            )
                        {
                            XmlElement newASBlock = SaveNewElement(sqlTree, SqlXmlConstants.ENAME_DDL_AS_BLOCK, token.Value, currentContainerNode);
                            currentContainerNode = StartNewStatement(sqlTree, newASBlock);
                        }
                        else if (keywordMatchPhrase.StartsWith("BEGIN TRANSACTION "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_BEGIN_TRANSACTION, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                        }
                        else if (keywordMatchPhrase.StartsWith("COMMIT TRANSACTION "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            ProcessCompoundKeyword(sqlTree, SqlXmlConstants.ENAME_COMMIT_TRANSACTION, ref tokenID, currentContainerNode, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                        }
                        else if (keywordMatchPhrase.StartsWith("ROLLBACK TRANSACTION "))
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
                                && (tokenID == tokenCount - 1 || IsLineBreakingWhiteSpace(tokenList[tokenID + 1]))
                                )
                            {
                                // we found a batch separator - were we supposed to?
                                if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                                    && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                                    && (
                                        currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_ROOT)
                                        || currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_DDL_AS_BLOCK)
                                        )
                                    )
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
                                SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("JOIN "))
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("LEFT JOIN ")
                            || keywordMatchPhrase.StartsWith("RIGHT JOIN ")
                            || keywordMatchPhrase.StartsWith("INNER JOIN ")
                            || keywordMatchPhrase.StartsWith("CROSS JOIN ")
                            || keywordMatchPhrase.StartsWith("CROSS APPLY ")
                            || keywordMatchPhrase.StartsWith("OUTER APPLY ")
                            )
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, keywordString, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("FULL OUTER JOIN ")
                            || keywordMatchPhrase.StartsWith("LEFT OUTER JOIN ")
                            || keywordMatchPhrase.StartsWith("RIGHT OUTER JOIN ")
                            )
                        {
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 3;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, keywordString, currentContainerNode);
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
                                    SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                                }
                            }
                            else
                            {
                                SaveNewElementWithError(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode, ref errorFound);
                            }
                        }
                        else if (keywordMatchPhrase.StartsWith("INSERT INTO "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            keywordMatchStringsUsed = 2;
                            string keywordString = GetCompoundKeyword(ref tokenID, keywordMatchStringsUsed, compoundKeywordTokenCounts, compoundKeywordRawStrings);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, keywordString, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("INSERT "))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
                        }
                        else if (keywordMatchPhrase.StartsWith("SELECT "))
                        {
                            XmlElement firstNonCommentSibling = GetFirstNonWhitespaceNonCommentChildElement(currentContainerNode);
                            if (!(
                                    firstNonCommentSibling != null
                                    && firstNonCommentSibling.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
                                    && firstNonCommentSibling.InnerText.ToUpper().StartsWith("INSERT")
                                    )
                                )
                                ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);

                            ConsiderStartingNewClause(sqlTree, ref currentContainerNode);
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
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
                        else if (keywordMatchPhrase.StartsWith("LIKE "))
                        {
                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHEROPERATOR, token.Value, currentContainerNode);
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

                            SaveNewElement(sqlTree, SqlXmlConstants.ENAME_OTHERNODE, token.Value, currentContainerNode);
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

            EscapeAnySingleStatementContainers(ref currentContainerNode);
            if (!currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                || !currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                || (
                    !currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_ROOT)
                    &&
                    !currentContainerNode.ParentNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_DDL_AS_BLOCK)
                    )
                )
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
            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                && currentContainerNode.ParentNode.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
                && HasNonWhiteSpaceNonSingleCommentContent(currentContainerNode)
                )
            {
                EscapeAnySingleStatementContainers(ref currentContainerNode);
                XmlElement inBetweenContainerElement = currentContainerNode;
                currentContainerNode = StartNewStatement(sqlTree, (XmlElement)currentContainerNode.ParentNode.ParentNode);
                if (!inBetweenContainerElement.Equals(previousContainerElement))
                    MigrateApplicableComments(inBetweenContainerElement, currentContainerNode);
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
            else if (currentContainerNode.Name.Equals(SqlXmlConstants.ENAME_DDL_BLOCK))
            {
                EscapeAnySingleStatementContainers(ref currentContainerNode);
                XmlElement inBetweenContainerElement = currentContainerNode;
                currentContainerNode = StartNewStatement(sqlTree, (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode);
                if (!inBetweenContainerElement.Equals(previousContainerElement))
                    MigrateApplicableComments(inBetweenContainerElement, currentContainerNode);
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
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
                    || uppercaseValue.Equals("DELETE")
                    || uppercaseValue.Equals("INSERT")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("IF")
                    || uppercaseValue.Equals("SET")
                    || uppercaseValue.Equals("CREATE")
                    || uppercaseValue.Equals("DROP")
                    || uppercaseValue.Equals("ALTER")
                    || uppercaseValue.Equals("TRUNCATE")
                    || uppercaseValue.Equals("DECLARE")
                    || uppercaseValue.Equals("EXEC")
                    || uppercaseValue.Equals("EXECUTE")
                    || uppercaseValue.Equals("WHILE")
                    || uppercaseValue.Equals("BREAK")
                    || uppercaseValue.Equals("CONTINUE")
                    || uppercaseValue.Equals("PRINT")
                    || uppercaseValue.Equals("USE")
                    || uppercaseValue.Equals("RETURN")
                    || uppercaseValue.Equals("WAITFOR")
                    || uppercaseValue.Equals("RAISERROR")
                    || uppercaseValue.Equals("COMMIT")
                    || uppercaseValue.Equals("OPEN")
                    || uppercaseValue.Equals("FETCH")
                    || uppercaseValue.Equals("CLOSE")
                    || uppercaseValue.Equals("DEALLOCATE")
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
                if (currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
                    && (
                        currentNode.InnerText.ToUpper().Equals("NVARCHAR")
                        || currentNode.InnerText.ToUpper().Equals("VARCHAR")
                        || currentNode.InnerText.ToUpper().Equals("DECIMAL")
                        || currentNode.InnerText.ToUpper().Equals("NUMERIC")
                        || currentNode.InnerText.ToUpper().Equals("VARBINARY")
                        || currentNode.InnerText.ToUpper().Equals("DEFAULT") //TODO: not really a data type, I'll have to rename the objects 
                        || currentNode.InnerText.ToUpper().Equals("IDENTITY") //TODO: not really a data type, I'll have to rename the objects 
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
                    || (currentNode.Name.Equals(SqlXmlConstants.ENAME_OTHERNODE)
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
    }
}
