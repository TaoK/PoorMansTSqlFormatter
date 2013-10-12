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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib
{
    public class ParseTree : XmlDocument, Interfaces.IParseTree 
    {
        public ParseTree(string rootName)
        {
            XmlElement newRoot = CreateElement(rootName);
            this.AppendChild(newRoot);
            CurrentContainer = newRoot;
        }

        private XmlElement _currentContainer;
        internal XmlElement CurrentContainer
        {
            get
            {
                return _currentContainer;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("CurrentContainer");

                if (!value.OwnerDocument.Equals(this))
                    throw new Exception("Current Container node can only be set to an element in the current document.");

                _currentContainer = value;
            }
        }

        private bool _newStatementDue;
        internal bool NewStatementDue
        {
            get
            {
                return _newStatementDue;
            }
            set
            {
                _newStatementDue = value;
            }
        }

        internal bool ErrorFound
        {
            get
            {
                return _newStatementDue;
            }
            private set
            {
                if (value)
                {
                    DocumentElement.SetAttribute(SqlXmlConstants.ANAME_ERRORFOUND, "1");
                }
                else 
                {
                    DocumentElement.RemoveAttribute(SqlXmlConstants.ANAME_ERRORFOUND);
                }
            }
        }
        public XmlDocument ToXmlDoc()
        {
            return this;
        }

        internal void SetError()
        {
            CurrentContainer.SetAttribute(SqlXmlConstants.ANAME_HASERROR, "1");
            ErrorFound = true;
        }

        internal XmlElement SaveNewElement(string newElementName, string newElementValue)
        {
            return SaveNewElement(newElementName, newElementValue, CurrentContainer);
        }
        internal XmlElement SaveNewElement(string newElementName, string newElementValue, XmlElement targetNode)
        {
            XmlElement newElement = CreateElement(newElementName);
            newElement.InnerText = newElementValue;
            targetNode.AppendChild(newElement);
            return newElement;
        }

        internal XmlElement SaveNewElementWithError(string newElementName, string newElementValue)
        {
            XmlElement newElement = SaveNewElement(newElementName, newElementValue);
            SetError();
            return newElement;
        }

        internal XmlElement SaveNewElementAsPriorSibling(string newElementName, string newElementValue, XmlElement nodeToSaveBefore)
        {
            XmlElement newElement = CreateElement(newElementName);
            newElement.InnerText = newElementValue;
            nodeToSaveBefore.ParentNode.InsertBefore(newElement, nodeToSaveBefore);
            return newElement;
        }

        internal void StartNewContainer(string newElementName, string containerOpenValue, string containerType)
        {
            CurrentContainer = SaveNewElement(newElementName, "");
            XmlElement containerOpen = SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_OPEN, "");
            SaveNewElement(SqlXmlConstants.ENAME_OTHERKEYWORD, containerOpenValue, containerOpen);
            CurrentContainer = SaveNewElement(containerType, "");
        }

        internal void StartNewStatement()
        {
            StartNewStatement(CurrentContainer);
        }
        internal void StartNewStatement(XmlElement targetNode)
        {
            NewStatementDue = false;
            XmlElement newStatement = SaveNewElement(SqlXmlConstants.ENAME_SQL_STATEMENT, "", targetNode);
            CurrentContainer = SaveNewElement(SqlXmlConstants.ENAME_SQL_CLAUSE, "", newStatement);
        }

        internal void EscapeAnyBetweenConditions()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND)
                && PathNameMatches(1, SqlXmlConstants.ENAME_BETWEEN_CONDITION)
                )
            {
                //we just ended the upper bound of a "BETWEEN" condition, need to pop back to the enclosing context
                MoveToAncestorContainer(2);
            }
        }

        internal void EscapeMergeAction()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                    && PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                    && PathNameMatches(2, SqlXmlConstants.ENAME_MERGE_ACTION)
                    && HasNonWhiteSpaceNonCommentContent(CurrentContainer)
                )
                MoveToAncestorContainer(4);
        }

        internal void EscapePartialStatementContainers()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                || PathNameMatches(0, SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                || PathNameMatches(0, SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK)
                )
                MoveToAncestorContainer(1);
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS)
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT)
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                    && PathNameMatches(1, SqlXmlConstants.ENAME_DDL_WITH_CLAUSE)
                    && (PathNameMatches(2, SqlXmlConstants.ENAME_PERMISSIONS_BLOCK)
                        || PathNameMatches(2, SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                        || PathNameMatches(2, SqlXmlConstants.ENAME_DDL_OTHER_BLOCK)
                        || PathNameMatches(2, SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK)
                        )
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_MERGE_WHEN))
                MoveToAncestorContainer(2);
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                && (PathNameMatches(1, SqlXmlConstants.ENAME_CTE_WITH_CLAUSE)
                    || PathNameMatches(1, SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK)
                    )
                )
                MoveToAncestorContainer(2);
        }

        internal void EscapeAnySingleOrPartialStatementContainers()
        {
            EscapeAnyBetweenConditions();
            EscapeAnySelectionTarget();
            EscapeJoinCondition();

            if (HasNonWhiteSpaceNonCommentContent(CurrentContainer))
            {
                EscapeCursorForBlock();
                EscapeMergeAction();
                EscapePartialStatementContainers();

                while (true)
                {
                    if (PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                        && PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                        && PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                    {
                        XmlNode currentSingleContainer = CurrentContainer.ParentNode.ParentNode;
                        if (PathNameMatches(currentSingleContainer, 1, SqlXmlConstants.ENAME_ELSE_CLAUSE))
                        {
                            //we just ended the one and only statement in an else clause, and need to pop out to the same level as its parent if
                            // singleContainer.else.if.CANDIDATE
                            CurrentContainer = (XmlElement)currentSingleContainer.ParentNode.ParentNode.ParentNode;
                        }
                        else
                        {
                            //we just ended the one statement of an if or while, and need to pop out the same level as that if or while
                            // singleContainer.(if or while).CANDIDATE
                            CurrentContainer = (XmlElement)currentSingleContainer.ParentNode.ParentNode;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void EscapeCursorForBlock()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                && PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                && PathNameMatches(2, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(3, SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK)
                && HasNonWhiteSpaceNonCommentContent(CurrentContainer)
                )
                //we just ended the one select statement in a cursor declaration, and need to pop out to the same level as the cursor
                MoveToAncestorContainer(5);
        }

        private XmlElement EscapeAndLocateNextStatementContainer(bool escapeEmptyContainer)
        {
            EscapeAnySingleOrPartialStatementContainers();

            if (PathNameMatches(0, SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION)
                && (PathNameMatches(1, SqlXmlConstants.ENAME_IF_STATEMENT)
                    || PathNameMatches(1, SqlXmlConstants.ENAME_WHILE_LOOP)
                    )
                )
            {
                //we just ended the boolean clause of an if or while, and need to pop to the single-statement container.
                return SaveNewElement(SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT, "", (XmlElement)CurrentContainer.ParentNode);
            }
            else if (PathNameMatches(0, SqlXmlConstants.ENAME_SQL_CLAUSE)
                && PathNameMatches(1, SqlXmlConstants.ENAME_SQL_STATEMENT)
                && (escapeEmptyContainer || HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer))
                )
                return (XmlElement)CurrentContainer.ParentNode.ParentNode;
            else
                return null;
        }

        private void MigrateApplicableCommentsFromContainer(XmlElement previousContainerElement)
        {
            XmlNode migrationContext = previousContainerElement;
            XmlNode migrationCandidate = previousContainerElement.LastChild;

            //keep track of where we're going to be prepending - this will change as we go moving stuff.
            XmlElement insertBeforeNode = CurrentContainer;

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
                        || migrationCandidate.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE)
                        || migrationCandidate.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                        )
                    && (migrationCandidate.PreviousSibling.NodeType == XmlNodeType.Whitespace
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                        || migrationCandidate.PreviousSibling.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE)
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
                        //we have a match, so migrate everything considered so far (backwards from the end). need to keep track of where we're inserting.
                        while (!migrationContext.LastChild.Equals(migrationCandidate))
                        {
                            XmlElement movingNode = (XmlElement)migrationContext.LastChild;
                            CurrentContainer.ParentNode.InsertBefore(movingNode, insertBeforeNode);
                            insertBeforeNode = movingNode;
                        }
                        CurrentContainer.ParentNode.InsertBefore(migrationCandidate, insertBeforeNode);
                        insertBeforeNode = (XmlElement)migrationCandidate;

                        //move on to the next candidate element for consideration.
                        migrationCandidate = migrationContext.LastChild;
                    }
                    else
                    {
                        //this one wasn't properly separated from the previous node/entry, keep going in case there's a linebreak further up.
                        migrationCandidate = migrationCandidate.PreviousSibling;
                    }
                }
                else if (migrationCandidate.NodeType == XmlNodeType.Text && !string.IsNullOrEmpty(migrationCandidate.InnerText))
                {
                    //we found a non-whitespace non-comment node with text content. Stop trying to migrate comments.
                    migrationCandidate = null;
                }
                else
                {
                    //walk up the last found node, in case the comment got trapped in some substructure.
                    migrationContext = migrationCandidate;
                    migrationCandidate = migrationCandidate.LastChild;
                }
            }
        }

        internal void ConsiderStartingNewStatement()
        {
            EscapeAnyBetweenConditions();
            EscapeAnySelectionTarget();
            EscapeJoinCondition();

            //before single-statement-escaping
            XmlElement previousContainerElement = CurrentContainer;

            //context might change AND suitable ancestor selected
            XmlElement nextStatementContainer = EscapeAndLocateNextStatementContainer(false);

            //if suitable ancestor found, start statement and migrate in-between comments to the new statement
            if (nextStatementContainer != null)
            {
                XmlElement inBetweenContainerElement = CurrentContainer;
                StartNewStatement(nextStatementContainer);
                if (!inBetweenContainerElement.Equals(previousContainerElement))
                    MigrateApplicableCommentsFromContainer(inBetweenContainerElement);
                MigrateApplicableCommentsFromContainer(previousContainerElement);
            }
        }

        internal void ConsiderStartingNewClause()
        {
            EscapeAnySelectionTarget();
            EscapeAnyBetweenConditions();
            EscapePartialStatementContainers();
            EscapeJoinCondition();

            if (CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_CLAUSE)
                && HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer)
                )
            {
                //complete current clause, start a new one in the same container
                XmlElement previousContainerElement = CurrentContainer;
                CurrentContainer = SaveNewElement(SqlXmlConstants.ENAME_SQL_CLAUSE, "", (XmlElement)CurrentContainer.ParentNode);
                MigrateApplicableCommentsFromContainer(previousContainerElement);
            }
            else if (CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_EXPRESSION_PARENS)
				|| CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_IN_PARENS)
				|| CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS)
				|| CurrentContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_STATEMENT)
				)
            {
                //create new clause and set context to it.
                CurrentContainer = SaveNewElement(SqlXmlConstants.ENAME_SQL_CLAUSE, "");
            }
        }

        internal void EscapeAnySelectionTarget()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_SELECTIONTARGET))
                CurrentContainer = (XmlElement)CurrentContainer.ParentNode;
        }

        internal void EscapeJoinCondition()
        {
            if (PathNameMatches(0, SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlXmlConstants.ENAME_JOIN_ON_SECTION)
                )
                MoveToAncestorContainer(2);
        }

        internal bool FindValidBatchEnd()
        {
            XmlElement nextStatementContainer = EscapeAndLocateNextStatementContainer(true);
            return nextStatementContainer != null
                && (nextStatementContainer.Name.Equals(SqlXmlConstants.ENAME_SQL_ROOT)
                    || (nextStatementContainer.Name.Equals(SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT)
                        && nextStatementContainer.ParentNode.Name.Equals(SqlXmlConstants.ENAME_DDL_AS_BLOCK)
                        )
                    );
        }

        internal bool PathNameMatches(int levelsUp, string nameToMatch)
        {
            return PathNameMatches(CurrentContainer, levelsUp, nameToMatch);
        }

        internal bool PathNameMatches(XmlNode targetNode, int levelsUp, string nameToMatch)
        {
            XmlNode currentNode = targetNode;
            while (levelsUp > 0)
            {
                currentNode = currentNode.ParentNode;
                levelsUp--;
            }
            return currentNode.Name.Equals(nameToMatch);
        }

        private static bool HasNonWhiteSpaceNonSingleCommentContent(XmlElement containerNode)
        {
            foreach (XmlElement testElement in containerNode.SelectNodes("*"))
                if (!testElement.Name.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                    && !testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                    && !testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE)
                    && (!testElement.Name.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                        || Regex.IsMatch(testElement.InnerText, @"(\r|\n)+")
                        )
                    )
                    return true;

            return false;
        }

        internal bool HasNonWhiteSpaceNonCommentContent(XmlElement containerNode)
        {
            foreach (XmlElement testElement in containerNode.SelectNodes("*"))
                if (!IsCommentOrWhiteSpace(testElement.Name))
                    return true;

            return false;
        }

        internal static bool IsCommentOrWhiteSpace(string targetNodeName)
        {
            return (targetNodeName.Equals(SqlXmlConstants.ENAME_WHITESPACE)
                || targetNodeName.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE)
                || targetNodeName.Equals(SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE)
                || targetNodeName.Equals(SqlXmlConstants.ENAME_COMMENT_MULTILINE)
                );
        }

		internal XmlElement GetFirstNonWhitespaceNonCommentChildElement(XmlNode targetElement)
		{
			XmlNode currentNode = targetElement.FirstChild;
			while (currentNode != null)
			{
				if (currentNode.NodeType != XmlNodeType.Element || IsCommentOrWhiteSpace(currentNode.Name))
					currentNode = currentNode.NextSibling;
				else
					return (XmlElement)currentNode;
			}
			return null;
		}

		internal XmlElement GetLastNonWhitespaceNonCommentChildElement(XmlNode targetElement)
		{
			XmlNode currentNode = targetElement.LastChild;
			while (currentNode != null)
			{
				if (currentNode.NodeType != XmlNodeType.Element || IsCommentOrWhiteSpace(currentNode.Name))
					currentNode = currentNode.PreviousSibling;
				else
					return (XmlElement)currentNode;
			}
			return null;
		}

        internal void MoveToAncestorContainer(int levelsUp)
        {
            MoveToAncestorContainer(levelsUp, null);
        }
        internal void MoveToAncestorContainer(int levelsUp, string targetContainerName)
        {
            XmlElement candidateContainer = CurrentContainer;
            while (levelsUp > 0)
            {
                candidateContainer = (XmlElement)candidateContainer.ParentNode;
                levelsUp--;
            }
            if (string.IsNullOrEmpty(targetContainerName) || candidateContainer.Name.Equals(targetContainerName))
                CurrentContainer = candidateContainer;
            else
                throw new Exception("Ancestor node does not match expected name!");
        }
    }
}
