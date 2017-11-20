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
using System.Collections.Generic;
using System.Linq;

namespace PoorMansTSqlFormatterLib.ParseStructure
{
    public static class NodeExtensions
    {
        public static Node FollowingChild(this Node value, Node fromChild)
        {
            if (value == null)
                return null;

            if (fromChild == null)
                throw new ArgumentNullException("fromChild");

            bool targetFound = false;
            Node sibling = null;

            foreach (var child in value.Children)
            {
                if (targetFound)
                {
                    sibling = child;
                    break;
                }

                if (child == fromChild)
                    targetFound = true;
            }

            return sibling;
        }

        public static Node PreviousChild(this Node value, Node fromChild)
        {
            if (value == null)
                return null;

            if (fromChild == null)
                throw new ArgumentNullException("fromChild");

            Node previousSibling = null;

            foreach (var child in value.Children)
            {
                if (child == fromChild)
                    return previousSibling;

                previousSibling = child;
            }

            return null;
        }

        public static Node NextSibling(this Node value)
        {
            if (value == null || value.Parent == null)
                return null;

            return value.Parent.FollowingChild(value);
        }

        public static Node PreviousSibling(this Node value)
        {
            if (value == null || value.Parent == null)
                return null;

            return value.Parent.PreviousChild(value);
        }

        public static Node RootContainer(this Node value)
        {
            if (value == null)
                return null;

            Node currentParent = value;
            while (currentParent.Parent != null)
                currentParent = currentParent.Parent;
            return currentParent;
        }

        public static IEnumerable<Node> ChildrenByName(this Node value, string name)
        {
            if (value == null)
                return Enumerable.Empty<Node>();

            return value.Children.Where(p => p.Name == name);
        }

        public static IEnumerable<Node> ChildrenByNames(this Node value, IEnumerable<string> names)
        {
            if (value == null)
                return Enumerable.Empty<Node>();

            return value.Children.Where(p => names.Contains(p.Name));
        }

        public static IEnumerable<Node> ChildrenExcludingNames(this Node value, IEnumerable<string> names)
        {
            if (value == null)
                return Enumerable.Empty<Node>();

            return value.Children.Where(p => !names.Contains(p.Name));
        }

        public static Node ChildByName(this Node value, string name)
        {
            return value.ChildrenByName(name).SingleOrDefault();
        }

        public static Node ChildByNames(this Node value, IEnumerable<string> names)
        {
            return value.ChildrenByNames(names).SingleOrDefault();
        }

        public static Node ChildExcludingNames(this Node value, IEnumerable<string> names)
        {
            return value.ChildrenExcludingNames(names).SingleOrDefault();
        }

        public static Node ExtractStructureBetween(Node startingElement, Node endingElement)
        {
            Node currentNode = startingElement;
            Node previousNode = null;
            Node remainder = null;
            Node remainderPosition = null;

            while (currentNode != null)
            {
                if (currentNode.Equals(endingElement))
                    break;

                if (previousNode != null)
                {
                    Node copyOfThisNode = NodeFactory.CreateNode(currentNode.Name, currentNode.TextValue);

                    foreach (var attribute in currentNode.Attributes)
                        copyOfThisNode.SetAttribute(attribute.Key, attribute.Value);

                    if (remainderPosition == null)
                    {
                        remainderPosition = copyOfThisNode;
                        remainder = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.Parent) && remainderPosition.Parent != null)
                    {
                        remainderPosition = remainderPosition.Parent;
                    }
                    else if (currentNode.Equals(previousNode.Parent) && remainderPosition.Parent == null)
                    {
                        copyOfThisNode.AddChild(remainderPosition);
                        remainderPosition = copyOfThisNode;
                        remainder = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.NextSibling()) && remainderPosition.Parent != null)
                    {
                        remainderPosition.Parent.AddChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                    else if (currentNode.Equals(previousNode.NextSibling()) && remainderPosition.Parent == null)
                    {
                        Node copyOfThisNodesParent = NodeFactory.CreateNode(currentNode.Parent.Name, currentNode.Parent.TextValue);
                        remainder = copyOfThisNodesParent;
                        remainder.AddChild(remainderPosition);
                        remainder.AddChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                    else
                    {
                        //we must be a child
                        remainderPosition.AddChild(copyOfThisNode);
                        remainderPosition = copyOfThisNode;
                    }
                }

                Node nextNode = null;
                if (previousNode != null
                    && currentNode.Children.Any()
                    && !(currentNode.Equals(previousNode.Parent)))
                {
                    nextNode = currentNode.Children.FirstOrDefault();
                }
                else if (currentNode.NextSibling() != null)
                {
                    nextNode = currentNode.NextSibling();
                }
                else
                {
                    nextNode = currentNode.Parent;
                }

                previousNode = currentNode;
                currentNode = nextNode;
            }

            return remainder;
        }

    }
}
