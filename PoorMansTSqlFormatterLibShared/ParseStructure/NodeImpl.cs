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
    internal class NodeImpl : Node
    {
        public NodeImpl()
        {
            Attributes = new Dictionary<string, string>();
            Children = new List<Node>();
        }

        public string Name { get; set; }
        public string TextValue { get; set; }
        public Node Parent { get; set; }

        public IDictionary<string, string> Attributes { get; private set; }
        public IEnumerable<Node> Children { get; private set; }

        public void AddChild(Node child)
        {
            SetParentOnChild(child);
            ((IList<Node>)Children).Add(child);
        }

        public void InsertChildBefore(Node newChild, Node existingChild)
        {
            SetParentOnChild(newChild);
            var childList = Children as IList<Node>;
            childList.Insert(childList.IndexOf(existingChild), newChild);
        }

        private void SetParentOnChild(Node child)
        {
            //TODO: NOT THREAD-SAFE AT ALL!
            if (child.Parent != null)
                throw new ArgumentException("Child cannot already have a parent!");
            ((NodeImpl)child).Parent = this;
        }

        public void RemoveChild(Node child)
        {
            //TODO: NOT THREAD-SAFE AT ALL!
            ((IList<Node>)Children).Remove(child);
            ((NodeImpl)child).Parent = null;
        }


        public string GetAttributeValue(string aName)
        {
            string outVal = null;
            Attributes.TryGetValue(aName, out outVal);
            return outVal;
        }

        public void SetAttribute(string name, string value)
        {
            Attributes[name] = value;
        }

        public void RemoveAttribute(string name)
        {
            Attributes.Remove(name);
        }

    }
}
