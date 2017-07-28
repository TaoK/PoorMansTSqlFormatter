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

using System.Collections.Generic;

namespace PoorMansTSqlFormatterLib.ParseStructure
{
    public interface Node
    {
        string Name { get; }
        string TextValue { get; }
        IDictionary<string, string> Attributes { get; }
        void SetAttribute(string name, string value);
        string GetAttributeValue(string name);
        void RemoveAttribute(string name);
        Node Parent { get; }
        IEnumerable<Node> Children { get; }
        void AddChild(Node child);
        void InsertChildBefore(Node newChild, Node existingChild);
        void RemoveChild(Node childThing);
    }
}
