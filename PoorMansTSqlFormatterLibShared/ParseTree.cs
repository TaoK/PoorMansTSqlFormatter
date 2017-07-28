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
using System.Linq;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib
{
    internal class ParseTree : NodeImpl, Node
    {
        public ParseTree(string rootName) : base()
        {
            Name = rootName;
            CurrentContainer = this;
        }

        private Node _currentContainer;
        internal Node CurrentContainer
        {
            get
            {
                return _currentContainer;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("CurrentContainer");

                if (!value.RootContainer().Equals(this))
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
                    this.SetAttribute(SqlStructureConstants.ANAME_ERRORFOUND, "1");
                }
                else 
                {
                    this.RemoveAttribute(SqlStructureConstants.ANAME_ERRORFOUND);
                }
            }
        }

        internal void SetError()
        {
            CurrentContainer.SetAttribute(SqlStructureConstants.ANAME_HASERROR, "1");
            ErrorFound = true;
        }

        internal Node SaveNewElement(string newElementName, string newElementValue)
        {
            return SaveNewElement(newElementName, newElementValue, CurrentContainer);
        }
        internal Node SaveNewElement(string newElementName, string newElementValue, Node targetNode)
        {
            Node newElement = NodeFactory.CreateNode(newElementName, newElementValue);
            targetNode.AddChild(newElement);
            return newElement;
        }

        internal Node SaveNewElementWithError(string newElementName, string newElementValue)
        {
            Node newElement = SaveNewElement(newElementName, newElementValue);
            SetError();
            return newElement;
        }

        internal Node SaveNewElementAsPriorSibling(string newElementName, string newElementValue, Node nodeToSaveBefore)
        {
            Node newElement = NodeFactory.CreateNode(newElementName, newElementValue);
            nodeToSaveBefore.Parent.InsertChildBefore(newElement, nodeToSaveBefore);
            return newElement;
        }

        internal void StartNewContainer(string newElementName, string containerOpenValue, string containerType)
        {
            CurrentContainer = SaveNewElement(newElementName, "");
            Node containerOpen = SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, "");
            SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, containerOpenValue, containerOpen);
            CurrentContainer = SaveNewElement(containerType, "");
        }

        internal void StartNewStatement()
        {
            StartNewStatement(CurrentContainer);
        }
        internal void StartNewStatement(Node targetNode)
        {
            NewStatementDue = false;
            Node newStatement = SaveNewElement(SqlStructureConstants.ENAME_SQL_STATEMENT, "", targetNode);
            CurrentContainer = SaveNewElement(SqlStructureConstants.ENAME_SQL_CLAUSE, "", newStatement);
        }

        internal void EscapeAnyBetweenConditions()
        {
            if (PathNameMatches(0, SqlStructureConstants.ENAME_BETWEEN_UPPERBOUND)
                && PathNameMatches(1, SqlStructureConstants.ENAME_BETWEEN_CONDITION)
                )
            {
                //we just ended the upper bound of a "BETWEEN" condition, need to pop back to the enclosing context
                MoveToAncestorContainer(2);
            }
        }

        internal void EscapeMergeAction()
        {
            if (PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                    && PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                    && PathNameMatches(2, SqlStructureConstants.ENAME_MERGE_ACTION)
                    && HasNonWhiteSpaceNonCommentContent(CurrentContainer)
                )
                MoveToAncestorContainer(4);
        }

        internal void EscapePartialStatementContainers()
        {
            if (PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                || PathNameMatches(0, SqlStructureConstants.ENAME_DDL_OTHER_BLOCK)
                || PathNameMatches(0, SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK)
                )
                MoveToAncestorContainer(1);
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlStructureConstants.ENAME_CURSOR_FOR_OPTIONS)
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT)
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                    && PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                    && (PathNameMatches(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK)
                        || PathNameMatches(2, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                        || PathNameMatches(2, SqlStructureConstants.ENAME_DDL_OTHER_BLOCK)
                        || PathNameMatches(2, SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK)
                        )
                )
                MoveToAncestorContainer(3);
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_MERGE_WHEN))
                MoveToAncestorContainer(2);
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                && (PathNameMatches(1, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE)
                    || PathNameMatches(1, SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK)
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
                    if (PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                        && PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                        && PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                        )
                    {
                        Node currentSingleContainer = CurrentContainer.Parent.Parent;
                        if (PathNameMatches(currentSingleContainer, 1, SqlStructureConstants.ENAME_ELSE_CLAUSE))
                        {
                            //we just ended the one and only statement in an else clause, and need to pop out to the same level as its parent if
                            // singleContainer.else.if.CANDIDATE
                            CurrentContainer = currentSingleContainer.Parent.Parent.Parent;
                        }
                        else
                        {
                            //we just ended the one statement of an if or while, and need to pop out the same level as that if or while
                            // singleContainer.(if or while).CANDIDATE
                            CurrentContainer = currentSingleContainer.Parent.Parent;
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
            if (PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                && PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                && PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(3, SqlStructureConstants.ENAME_CURSOR_FOR_BLOCK)
                && HasNonWhiteSpaceNonCommentContent(CurrentContainer)
                )
                //we just ended the one select statement in a cursor declaration, and need to pop out to the same level as the cursor
                MoveToAncestorContainer(5);
        }

        private Node EscapeAndLocateNextStatementContainer(bool escapeEmptyContainer)
        {
            EscapeAnySingleOrPartialStatementContainers();

            if (PathNameMatches(0, SqlStructureConstants.ENAME_BOOLEAN_EXPRESSION)
                && (PathNameMatches(1, SqlStructureConstants.ENAME_IF_STATEMENT)
                    || PathNameMatches(1, SqlStructureConstants.ENAME_WHILE_LOOP)
                    )
                )
            {
                //we just ended the boolean clause of an if or while, and need to pop to the single-statement container.
                return SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT, "", CurrentContainer.Parent);
            }
            else if (PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                && PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                && (escapeEmptyContainer || HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer))
                )
                return CurrentContainer.Parent.Parent;
            else
                return null;
        }

        private void MigrateApplicableCommentsFromContainer(Node previousContainerElement)
        {
            Node migrationContext = previousContainerElement;
            Node migrationCandidate = previousContainerElement.Children.Last();

            //keep track of where we're going to be prepending - this will change as we go moving stuff.
            Node insertBeforeNode = CurrentContainer;

            while (migrationCandidate != null)
            {
                if (migrationCandidate.Name.Equals(SqlStructureConstants.ENAME_WHITESPACE))
                {
                    migrationCandidate = migrationCandidate.PreviousSibling();
                    continue;
                }
                else if (migrationCandidate.PreviousSibling() != null
                    && SqlStructureConstants.ENAMELIST_COMMENT.Contains(migrationCandidate.Name)
                    && SqlStructureConstants.ENAMELIST_NONCONTENT.Contains(migrationCandidate.PreviousSibling().Name)
                    )
                {
                    if (migrationCandidate.PreviousSibling().Name.Equals(SqlStructureConstants.ENAME_WHITESPACE)
                        && Regex.IsMatch(migrationCandidate.PreviousSibling().TextValue, @"(\r|\n)+")
                        )
                    {
                        //we have a match, so migrate everything considered so far (backwards from the end). need to keep track of where we're inserting.
                        while (!migrationContext.Children.Last().Equals(migrationCandidate))
                        {
                            Node movingNode = migrationContext.Children.Last();
                            movingNode.Parent.RemoveChild(movingNode);
                            CurrentContainer.Parent.InsertChildBefore(movingNode, insertBeforeNode);
                            insertBeforeNode = movingNode;
                        }
                        migrationCandidate.Parent.RemoveChild(migrationCandidate);
                        CurrentContainer.Parent.InsertChildBefore(migrationCandidate, insertBeforeNode);
                        insertBeforeNode = migrationCandidate;

                        //move on to the next candidate element for consideration.
                        migrationCandidate = migrationContext.Children.Last();
                    }
                    else
                    {
                        //this one wasn't properly separated from the previous node/entry, keep going in case there's a linebreak further up.
                        migrationCandidate = migrationCandidate.PreviousSibling();
                    }
                }
                else if (!string.IsNullOrEmpty(migrationCandidate.TextValue))
                {
                    //we found a non-whitespace non-comment node with text content. Stop trying to migrate comments.
                    migrationCandidate = null;
                }
                else
                {
                    //walk up the last found node, in case the comment got trapped in some substructure.
                    migrationContext = migrationCandidate;
                    migrationCandidate = migrationCandidate.Children.LastOrDefault();
                }
            }
        }

        internal void ConsiderStartingNewStatement()
        {
            EscapeAnyBetweenConditions();
            EscapeAnySelectionTarget();
            EscapeJoinCondition();

            //before single-statement-escaping
            Node previousContainerElement = CurrentContainer;

            //context might change AND suitable ancestor selected
            Node nextStatementContainer = EscapeAndLocateNextStatementContainer(false);

            //if suitable ancestor found, start statement and migrate in-between comments to the new statement
            if (nextStatementContainer != null)
            {
                Node inBetweenContainerElement = CurrentContainer;
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

            if (CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SQL_CLAUSE)
                && HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer)
                )
            {
                //complete current clause, start a new one in the same container
                Node previousContainerElement = CurrentContainer;
                CurrentContainer = SaveNewElement(SqlStructureConstants.ENAME_SQL_CLAUSE, "", CurrentContainer.Parent);
                MigrateApplicableCommentsFromContainer(previousContainerElement);
            }
            else if (CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_EXPRESSION_PARENS)
				|| CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_IN_PARENS)
				|| CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS)
				|| CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SQL_STATEMENT)
				)
            {
                //create new clause and set context to it.
                CurrentContainer = SaveNewElement(SqlStructureConstants.ENAME_SQL_CLAUSE, "");
            }
        }

        internal void EscapeAnySelectionTarget()
        {
            if (PathNameMatches(0, SqlStructureConstants.ENAME_SELECTIONTARGET))
                CurrentContainer = CurrentContainer.Parent;
        }

        internal void EscapeJoinCondition()
        {
            if (PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                && PathNameMatches(1, SqlStructureConstants.ENAME_JOIN_ON_SECTION)
                )
                MoveToAncestorContainer(2);
        }

        internal bool FindValidBatchEnd()
        {
            Node nextStatementContainer = EscapeAndLocateNextStatementContainer(true);
            return nextStatementContainer != null
                && (nextStatementContainer.Name.Equals(SqlStructureConstants.ENAME_SQL_ROOT)
                    || (nextStatementContainer.Name.Equals(SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                        && nextStatementContainer.Parent.Name.Equals(SqlStructureConstants.ENAME_DDL_AS_BLOCK)
                        )
                    );
        }

        internal bool PathNameMatches(int levelsUp, string nameToMatch)
        {
            return PathNameMatches(CurrentContainer, levelsUp, nameToMatch);
        }

        internal bool PathNameMatches(Node targetNode, int levelsUp, string nameToMatch)
        {
            Node currentNode = targetNode;
            while (levelsUp > 0)
            {
                currentNode = currentNode.Parent;
                levelsUp--;
            }
            return currentNode != null && currentNode.Name.Equals(nameToMatch);
        }

        private static bool HasNonWhiteSpaceNonSingleCommentContent(Node containerNode)
        {
            foreach (Node testElement in containerNode.Children)
                if (!testElement.Name.Equals(SqlStructureConstants.ENAME_WHITESPACE)
                    && !testElement.Name.Equals(SqlStructureConstants.ENAME_COMMENT_SINGLELINE)
                    && !testElement.Name.Equals(SqlStructureConstants.ENAME_COMMENT_SINGLELINE_CSTYLE)
                    && (!testElement.Name.Equals(SqlStructureConstants.ENAME_COMMENT_MULTILINE)
                        || Regex.IsMatch(testElement.TextValue, @"(\r|\n)+")
                        )
                    )
                    return true;

            return false;
        }

        internal bool HasNonWhiteSpaceNonCommentContent(Node containerNode)
        {
            return containerNode.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).Any();
        }

		internal Node GetFirstNonWhitespaceNonCommentChildElement(Node targetElement)
		{
            return targetElement.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).FirstOrDefault();
		}

		internal Node GetLastNonWhitespaceNonCommentChildElement(Node targetElement)
		{
            return targetElement.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
		}

        internal void MoveToAncestorContainer(int levelsUp)
        {
            MoveToAncestorContainer(levelsUp, null);
        }
        internal void MoveToAncestorContainer(int levelsUp, string targetContainerName)
        {
            Node candidateContainer = CurrentContainer;
            while (levelsUp > 0)
            {
                candidateContainer = candidateContainer.Parent;
                levelsUp--;
            }
            if (string.IsNullOrEmpty(targetContainerName) || candidateContainer.Name.Equals(targetContainerName))
                CurrentContainer = candidateContainer;
            else
                throw new Exception("Ancestor node does not match expected name!");
        }
    }
}
