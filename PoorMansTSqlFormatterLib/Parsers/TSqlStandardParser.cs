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

namespace PoorMansTSqlFormatterLib.Parsers
{
    public class TSqlStandardParser : Interfaces.ISqlTokenParser
    {
        /*
         * TODO:
         *  - Implement Clauses within statements
         *    - Handle compound terms better (BEGIN TRY, LEFT JOIN, etc)
         *  - Handle SELECT as non-starter (inside INSERT section) 
         *  - support clauses in parens? (for derived tables)
         *    - UNION clauses get special formatting?
         *  
         *  - Tests
         *    - Samples illustrating all the tokens and container combinations implemented
         *    - Samples illustrating all forms of container violations
         *    - Sample requests and their XML equivalent - once the xml format is more-or-less formalized
         *    - Sample requests and their formatted versions (a few for each) - once the "standard" format is more-or-less formalized
         */

        public XmlDocument ParseSQL(XmlDocument tokenListDoc)
        {
            XmlDocument sqlTree = new XmlDocument();
            XmlElement firstStatement;
            XmlElement currentContainerNode;
            SqlParsingState currentState;
            StringBuilder inProgressTextValue = new StringBuilder();
            bool errorFound = false;

            if (tokenListDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.Constants.ENAME_SQLTOKENS_ROOT, Interfaces.Constants.ANAME_ERRORFOUND)) != null)
                errorFound = true;

            sqlTree.AppendChild(sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_ROOT));
            firstStatement = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
            currentContainerNode = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
            firstStatement.AppendChild(currentContainerNode);
            sqlTree.DocumentElement.AppendChild(firstStatement);
            currentState = SqlParsingState.Default;
            inProgressTextValue.Length = 0;

            XmlNodeList tokenList = tokenListDoc.SelectNodes(string.Format("/{0}/*", Interfaces.Constants.ENAME_SQLTOKENS_ROOT));
            int tokenCount = tokenList.Count;
            int tokenID = 0;
            while (tokenID < tokenCount)
            {
                XmlElement token = (XmlElement)tokenList[tokenID];

                XmlElement newElement = null;
                XmlElement newElement2 = null;
                XmlElement newElement3 = null;
                XmlElement newElement4 = null;
                string uppercaseContent = token.InnerText.ToUpper();
                switch (currentState)
                {
                    case SqlParsingState.Default:
                        ProcessThisTokenNormally(sqlTree, ref currentContainerNode, ref currentState, inProgressTextValue, token, ref errorFound);
                        break;

                    case SqlParsingState.FoundBegin:
                        if (token.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE))
                        {
                            inProgressTextValue.Append(token.InnerText);
                        }
                        else if (token.Name.Equals(Interfaces.Constants.ENAME_OTHERNODE) && uppercaseContent.Equals("TRY"))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            inProgressTextValue.Append(token.InnerText);
                            newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_TRY_BLOCK);
                            newElement.AppendChild(sqlTree.CreateTextNode(inProgressTextValue.ToString()));
                            currentContainerNode.AppendChild(newElement);
                            newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                            newElement.AppendChild(newElement2);
                            newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                            newElement2.AppendChild(newElement3);
                            currentContainerNode = newElement3;
                            currentState = SqlParsingState.Default;
                        }
                        else if (token.Name.Equals(Interfaces.Constants.ENAME_OTHERNODE) && uppercaseContent.Equals("TRANSACTION"))
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            inProgressTextValue.Append(token.InnerText);
                            newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_BEGIN_TRANSACTION);
                            newElement.AppendChild(sqlTree.CreateTextNode(inProgressTextValue.ToString()));
                            currentContainerNode.AppendChild(newElement);
                            currentState = SqlParsingState.Default;
                        }
                        else
                        {
                            ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                            newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_BEGIN_END_BLOCK);
                            string beginAndWhiteSpace = inProgressTextValue.ToString();
                            newElement.AppendChild(sqlTree.CreateTextNode(beginAndWhiteSpace.Substring(0, 5)));
                            currentContainerNode.AppendChild(newElement);
                            newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                            newElement.AppendChild(newElement2);
                            newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                            newElement2.AppendChild(newElement3);
                            currentContainerNode = newElement3;
                            newElement4 = sqlTree.CreateElement(Interfaces.Constants.ENAME_WHITESPACE);
                            newElement4.InnerText = beginAndWhiteSpace.Substring(5);
                            currentContainerNode.AppendChild(newElement4);
                            currentState = SqlParsingState.Default;
                            ProcessThisTokenNormally(sqlTree, ref currentContainerNode, ref currentState, inProgressTextValue, token, ref errorFound);
                        }
                        break;

                    case SqlParsingState.FoundEnd:
                        if (token.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE))
                        {
                            inProgressTextValue.Append(token.InnerText);
                        }
                        else if (token.Name.Equals(Interfaces.Constants.ENAME_OTHERNODE) && uppercaseContent.Equals("TRY"))
                        {
                            EscapeAnySingleStatementContainers(ref currentContainerNode);

                            //check whether we expected to end the block...
                            inProgressTextValue.Append(token.InnerText);
                            if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_TRY_BLOCK))
                            {
                                currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(inProgressTextValue.ToString()));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                            }
                            else
                            {
                                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_OTHERNODE);
                                newElement.InnerText = inProgressTextValue.ToString();
                                currentContainerNode.AppendChild(newElement);
#if DEBUG
                                System.Diagnostics.Debugger.Break();
#endif
                                errorFound = true;
                            }
                            currentState = SqlParsingState.Default;
                        }
                        else
                        {
                            EscapeAnySingleStatementContainers(ref currentContainerNode);

                            //check whether we expected to end the block...
                            if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_BEGIN_END_BLOCK))
                            {
                                string beginAndWhiteSpace = inProgressTextValue.ToString();
                                currentContainerNode.ParentNode.ParentNode.AppendChild(sqlTree.CreateTextNode(beginAndWhiteSpace.Substring(0, 3)));
                                currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                                newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_WHITESPACE);
                                newElement3.InnerText = beginAndWhiteSpace.Substring(3);
                                currentContainerNode.AppendChild(newElement3);
                            }
                            else
                            {
                                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_OTHERNODE);
                                newElement.InnerText = inProgressTextValue.ToString();
                                currentContainerNode.AppendChild(newElement);
#if DEBUG
                                System.Diagnostics.Debugger.Break();
#endif
                                errorFound = true;
                            }
                            currentState = SqlParsingState.Default;
                            ProcessThisTokenNormally(sqlTree, ref currentContainerNode, ref currentState, inProgressTextValue, token, ref errorFound);
                        }
                        break;

                    default:
                        throw new Exception("Invalid Parsing State encountered!");

                }

                tokenID++;
            }

            EscapeAnySingleStatementContainers(ref currentContainerNode);
            if (!currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                || !currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                || !currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_ROOT)
                )
                errorFound = true;

            if (errorFound)
            {
                sqlTree.DocumentElement.SetAttribute(Interfaces.Constants.ANAME_ERRORFOUND, "1");
            }

            return sqlTree;
        }

        private static void ProcessThisTokenNormally(XmlDocument sqlTree, ref XmlElement currentContainerNode, ref SqlParsingState currentState, StringBuilder currentValue, XmlElement token, ref bool errorFound)
        {
            XmlElement newElement = null;
            XmlElement newElement2 = null;
            XmlElement newElement3 = null;
            switch (token.Name)
            {
                case Interfaces.Constants.ENAME_PARENS_OPEN:
                    newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_PARENS);
                    currentContainerNode.AppendChild(newElement);
                    currentContainerNode = newElement;
                    break;

                case Interfaces.Constants.ENAME_PARENS_CLOSE:
                    //check whether we expected to end the parens...
                    if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_PARENS))
                    {
                        currentContainerNode = (XmlElement)currentContainerNode.ParentNode;
                    }
                    else
                    {
                        newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_OTHERNODE);
                        newElement.InnerText = ")";
                        currentContainerNode.AppendChild(newElement);
#if DEBUG
                        System.Diagnostics.Debugger.Break();
#endif
                        errorFound = true;
                    }
                    break;

                case Interfaces.Constants.ENAME_OTHERNODE:
                    if (token.InnerText.ToUpper().Equals("BEGIN"))
                    {
                        currentState = SqlParsingState.FoundBegin;
                        currentValue.Length = 0;
                        currentValue.Append(token.InnerText);
                    }
                    else if (token.InnerText.ToUpper().Equals("CASE"))
                    {
                        newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_CASE_STATEMENT);
                        newElement.InnerText = token.InnerText;
                        currentContainerNode.AppendChild(newElement);
                        currentContainerNode = newElement;
                    }
                    else if (token.InnerText.ToUpper().Equals("END"))
                    {
                        if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_CASE_STATEMENT))
                        {
                            currentContainerNode.AppendChild(sqlTree.CreateTextNode(token.InnerText));
                            currentContainerNode = (XmlElement)currentContainerNode.ParentNode;
                        }
                        else
                        {
                            currentState = SqlParsingState.FoundEnd;
                            currentValue.Length = 0;
                            currentValue.Append(token.InnerText);
                        }
                    }
                    else if (token.InnerText.ToUpper().Equals("GO"))
                    {
                        EscapeAnySingleStatementContainers(ref currentContainerNode);

                        //this looks a little simplistic... might need to review.
                        if ((token.PreviousSibling == null || IsLineBreakingWhiteSpace((XmlElement)token.PreviousSibling))
                            && (token.NextSibling == null || IsLineBreakingWhiteSpace((XmlElement)token.NextSibling))
                            )
                        {
                            // we found a batch separator - were we supposed to?
                            if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_ROOT)
                                )
                            {
                                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_BATCH_SEPARATOR);
                                newElement.InnerText = token.InnerText;
                                currentContainerNode.ParentNode.ParentNode.AppendChild(newElement);

                                newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                                currentContainerNode.ParentNode.ParentNode.AppendChild(newElement2);

                                newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                                newElement2.AppendChild(newElement3);

                                currentContainerNode = newElement3;
                            }
                            else
                            {
                                newElement = (XmlElement)sqlTree.ImportNode(token, true);
                                currentContainerNode.AppendChild(newElement);
#if DEBUG
                                System.Diagnostics.Debugger.Break();
#endif
                                errorFound = true;
                            }
                        }
                        else
                        {
                            newElement = (XmlElement)sqlTree.ImportNode(token, true);
                            currentContainerNode.AppendChild(newElement);
                        }
                    }
                    else if (token.InnerText.ToUpper().Equals("WHILE"))
                    {
                        ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                        newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_WHILE_LOOP);
                        newElement.InnerText = token.InnerText;
                        currentContainerNode.AppendChild(newElement);
                        newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION);
                        newElement.AppendChild(newElement2);
                        currentContainerNode = newElement2;
                    }
                    /*
                    else if (token.InnerText.ToUpper().Equals("JOIN")
                        || token.InnerText.ToUpper().Equals("FULL")
                        || token.InnerText.ToUpper().Equals("LEFT")
                        || token.InnerText.ToUpper().Equals("RIGHT")
                        || token.InnerText.ToUpper().Equals("INNER")
                        || token.InnerText.ToUpper().Equals("CROSS")
                        )
                    {
                        ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                        newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_WHILE_LOOP);
                        newElement.InnerText = token.InnerText;
                        currentContainerNode.AppendChild(newElement);
                        newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION);
                        newElement.AppendChild(newElement2);
                        currentContainerNode = newElement2;
                    }
                    */
                    else if (token.InnerText.ToUpper().Equals("IF"))
                    {
                        ConsiderStartingNewStatement(sqlTree, ref currentContainerNode);
                        newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_IF_STATEMENT);
                        newElement.InnerText = token.InnerText;
                        currentContainerNode.AppendChild(newElement);
                        newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION);
                        newElement.AppendChild(newElement2);
                        currentContainerNode = newElement2;
                    }
                    else if (token.InnerText.ToUpper().Equals("ELSE"))
                    {
                        if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_CASE_STATEMENT))
                        {
                            //we don't really do anything with case statement structure yet
                            newElement = (XmlElement)sqlTree.ImportNode(token, true);
                            currentContainerNode.AppendChild(newElement);
                        }
                        else if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT))
                        {
                            //topmost if - just pop back one.
                            newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_ELSE_CLAUSE);
                            newElement.InnerText = token.InnerText;
                            currentContainerNode.ParentNode.ParentNode.AppendChild(newElement);

                            newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                            newElement.AppendChild(newElement2);

                            newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                            newElement2.AppendChild(newElement3);

                            currentContainerNode = newElement3;
                        }
                        else if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_ELSE_CLAUSE)
                                && currentContainerNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
                            )
                        {
                            //not topmost if; we need to pop up the single-statement containers stack to the next "if" that doesn't have an "else".
                            XmlElement currentNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                            while (currentNode != null 
                                && (currentNode.Name.Equals(Interfaces.Constants.ENAME_WHILE_LOOP)
                                    || currentNode.SelectSingleNode(Interfaces.Constants.ENAME_ELSE_CLAUSE) != null
                                    )
                                )
                            {
                                if (currentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                    && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
                                    )
                                    currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode;
                                else if (currentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                    && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_WHILE_LOOP)
                                    )
                                    currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode;
                                else if (currentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                                    && currentNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_ELSE_CLAUSE)
                                    && currentNode.ParentNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
                                    )
                                    currentNode = (XmlElement)currentNode.ParentNode.ParentNode.ParentNode.ParentNode;
                                else
                                    currentNode = null;
                            }

                            if (currentNode != null)
                            {
                                //found the next available if!
                                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_ELSE_CLAUSE);
                                newElement.InnerText = token.InnerText;
                                currentNode.AppendChild(newElement);

                                newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                                newElement.AppendChild(newElement2);

                                newElement3 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                                newElement2.AppendChild(newElement3);

                                currentContainerNode = newElement3;
                            }
                            else
                            {
                                newElement = (XmlElement)sqlTree.ImportNode(token, true);
                                currentContainerNode.AppendChild(newElement);
#if DEBUG
                                System.Diagnostics.Debugger.Break();
#endif
                                errorFound = true;
                            }
                        }
                        else
                        {
                            newElement = (XmlElement)sqlTree.ImportNode(token, true);
                            currentContainerNode.AppendChild(newElement);
#if DEBUG
                            System.Diagnostics.Debugger.Break();
#endif
                            errorFound = true;
                        }
                    }
                    else
                    {
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
                        newElement = (XmlElement)sqlTree.ImportNode(token, true);
                        currentContainerNode.AppendChild(newElement);
                    }
                    break;

                case Interfaces.Constants.ENAME_ASTERISK:
                case Interfaces.Constants.ENAME_COMMA:
                case Interfaces.Constants.ENAME_COMMENT_MULTILINE:
                case Interfaces.Constants.ENAME_COMMENT_SINGLELINE:
                case Interfaces.Constants.ENAME_NSTRING:
                case Interfaces.Constants.ENAME_OTHEROPERATOR:
                case Interfaces.Constants.ENAME_QUOTED_IDENTIFIER:
                case Interfaces.Constants.ENAME_STRING:
                case Interfaces.Constants.ENAME_WHITESPACE:
                    newElement = (XmlElement)sqlTree.ImportNode(token, true);
                    currentContainerNode.AppendChild(newElement);
                    break;
                default:
                    throw new Exception("Unrecognized element encountered!");
            }
        }

        private static void ConsiderStartingNewStatement(XmlDocument sqlTree, ref XmlElement currentContainerNode)
        {
            XmlElement newElement = null;
            XmlElement newElement2 = null;
            XmlElement previousContainerElement = currentContainerNode;

            if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_BOOLEAN_EXPRESSION)
                && (currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
                    || currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_WHILE_LOOP)
                    )
                )
            {
                //we just ended the boolean clause of an if or while, and need to pop to the first (and only) statement.
                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                currentContainerNode.ParentNode.AppendChild(newElement);
                newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                newElement.AppendChild(newElement2);
                currentContainerNode = newElement2;
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
            else if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                && HasNonWhiteSpaceNonSingleCommentContent(currentContainerNode)
                )
            {
                EscapeAnySingleStatementContainers(ref currentContainerNode);
                XmlElement inBetweenContainerElement = currentContainerNode;
                newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_STATEMENT);
                currentContainerNode.ParentNode.ParentNode.AppendChild(newElement);

                newElement2 = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                newElement.AppendChild(newElement2);
                currentContainerNode = newElement2;

                if (!inBetweenContainerElement.Equals(previousContainerElement))
                    MigrateApplicableComments(inBetweenContainerElement, currentContainerNode);
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
        }

        private static void ConsiderStartingNewClause(XmlDocument sqlTree, ref XmlElement currentContainerNode)
        {
            if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                && HasNonWhiteSpaceNonSingleCommentContent(currentContainerNode)
                )
            {
                XmlElement previousContainerElement = currentContainerNode;
                XmlElement newElement = sqlTree.CreateElement(Interfaces.Constants.ENAME_SQL_CLAUSE);
                currentContainerNode.ParentNode.AppendChild(newElement);
                currentContainerNode = newElement;
                MigrateApplicableComments(previousContainerElement, currentContainerNode);
            }
        }

        private static void MigrateApplicableComments(XmlElement previousContainerElement, XmlElement currentContainerNode)
        {
            XmlNode migrationCandidate = previousContainerElement.LastChild;

            while (migrationCandidate != null)
            {
                if (migrationCandidate.NodeType == XmlNodeType.Whitespace
                    || migrationCandidate.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE))
                {
                    migrationCandidate = migrationCandidate.PreviousSibling;
                    continue;
                }
                else if (migrationCandidate.PreviousSibling != null
                    && (migrationCandidate.Name.Equals(Interfaces.Constants.ENAME_COMMENT_SINGLELINE)
                        || (migrationCandidate.Name.Equals(Interfaces.Constants.ENAME_COMMENT_MULTILINE)
                            && !Regex.IsMatch(migrationCandidate.InnerText, @"(\r|\n)+")
                            )
                        )
                    && (migrationCandidate.PreviousSibling.NodeType == XmlNodeType.Whitespace
                        || migrationCandidate.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE)
                        || migrationCandidate.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_COMMENT_SINGLELINE)
                        || migrationCandidate.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_COMMENT_MULTILINE)
                        )
                    )
                {
                    if ((migrationCandidate.PreviousSibling.NodeType == XmlNodeType.Whitespace
                            || migrationCandidate.PreviousSibling.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE)
                            )
                        && Regex.IsMatch(migrationCandidate.PreviousSibling.InnerText, @"(\r|\n)+")
                        )
                    {
                        while (!previousContainerElement.LastChild.Equals(migrationCandidate))
                        {
                            currentContainerNode.PrependChild(previousContainerElement.LastChild);
                        }
                        currentContainerNode.PrependChild(migrationCandidate);
                        migrationCandidate = previousContainerElement.LastChild;
                    }
                    else
                    {
                        migrationCandidate = migrationCandidate.PreviousSibling;
                    }
                }
                else
                {
                    migrationCandidate = null;
                }
            }
        }

        private static void EscapeAnySingleStatementContainers(ref XmlElement currentContainerNode)
        {
            while (true)
            {
                if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                    && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                    && (currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
                        || currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_WHILE_LOOP)
                        )
                    )
                {

                    //we just ended the one statement of an if or while, and need to pop out to a new statement at the same level as the IF or WHILE
                    currentContainerNode = (XmlElement)currentContainerNode.ParentNode.ParentNode.ParentNode;
                }
                else if (currentContainerNode.Name.Equals(Interfaces.Constants.ENAME_SQL_CLAUSE)
                    && currentContainerNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_SQL_STATEMENT)
                    && currentContainerNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_ELSE_CLAUSE)
                    && currentContainerNode.ParentNode.ParentNode.ParentNode.Name.Equals(Interfaces.Constants.ENAME_IF_STATEMENT)
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

        private static bool IsStatementStarter(XmlElement token)
        {
            string uppercaseValue = token.InnerText.ToUpper();
            return (token.Name.Equals(Interfaces.Constants.ENAME_OTHERNODE)
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

        private static bool IsClauseStarter(XmlElement token)
        {
            string uppercaseValue = token.InnerText.ToUpper();
            return (token.Name.Equals(Interfaces.Constants.ENAME_OTHERNODE)
                && (uppercaseValue.Equals("INNER")
                    || uppercaseValue.Equals("LEFT")
                    || uppercaseValue.Equals("JOIN")
                    || uppercaseValue.Equals("WHERE")
                    || uppercaseValue.Equals("FROM")
                    || uppercaseValue.Equals("ORDER")
                    || uppercaseValue.Equals("GROUP")
                    || uppercaseValue.Equals("INTO")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("UNION")
                    )
                );
        }

        private static bool IsLineBreakingWhiteSpace(XmlElement token)
        {
            return (token.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE) 
                && Regex.IsMatch(token.InnerText, @"(\r|\n)+"));
        }

        private static bool HasNonWhiteSpaceNonSingleCommentContent(XmlElement containerNode)
        {
            foreach (XmlElement testElement in containerNode.SelectNodes("*"))
                if (!testElement.Name.Equals(Interfaces.Constants.ENAME_WHITESPACE)
                    && !testElement.Name.Equals(Interfaces.Constants.ENAME_COMMENT_SINGLELINE)
                    && (!testElement.Name.Equals(Interfaces.Constants.ENAME_COMMENT_MULTILINE)
                        || Regex.IsMatch(testElement.InnerText, @"(\r|\n)+")
                        )
                    )
                    return true;

            return false;
        }

        public enum SqlParsingState
        {
            Default,
            FoundBegin,
            FoundEnd,
            FoundLeft,
            FoundRight,
            FoundInner,
        }
    }
}
