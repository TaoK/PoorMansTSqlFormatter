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

using PoorMansTSqlFormatterLib.ParseStructure;
using System.Xml;

namespace PoorMansTSqlFormatterLib
{
    public static class NetUtils
    {
        public static XmlDocument ToXmlDoc(this Node value)
        {
            var outDoc = new XmlDocument();
            outDoc.AppendChild(ConvertThingToXmlNode(outDoc, value));
            return outDoc;
        }

        private static XmlNode ConvertThingToXmlNode(XmlDocument outDoc, Node currentNode)
        {
            XmlNode copyOfThisNode = outDoc.CreateNode(XmlNodeType.Element, currentNode.Name, null);

            foreach (var attribute in currentNode.Attributes)
            {
                XmlAttribute newAttribute = outDoc.CreateAttribute(null, attribute.Key, null);
                newAttribute.Value = attribute.Value;
                copyOfThisNode.Attributes.Append(newAttribute);
            }

            copyOfThisNode.InnerText = currentNode.TextValue ?? "";

            foreach (Node child in currentNode.Children)
                copyOfThisNode.AppendChild(ConvertThingToXmlNode(outDoc, child));

            return copyOfThisNode;
        }

        public static char ToLowerInvariant(this char value) => char.ToLowerInvariant(value);
        public static char ToUpperInvariant(this char value) => char.ToLowerInvariant(value);
    }
}
