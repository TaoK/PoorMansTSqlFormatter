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
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib
{
    internal class BaseFormatterState
    {
        public BaseFormatterState(bool htmlOutput)
        {
            HtmlOutput = htmlOutput;
        }

        protected bool HtmlOutput { get; set; }
        protected StringBuilder _outBuilder = new StringBuilder();

        public virtual void AddOutputContent(string content)
        {
            AddOutputContent(content, null);
        }

        public virtual void AddOutputContent(string content, string htmlClassName)
        {
            if (HtmlOutput)
            {
                if (!string.IsNullOrEmpty(htmlClassName))
                    _outBuilder.Append(@"<span class=""" + htmlClassName + @""">");
                _outBuilder.Append(Utils.HtmlEncode(content));
                if (!string.IsNullOrEmpty(htmlClassName))
                    _outBuilder.Append("</span>");
            }
            else
                _outBuilder.Append(content);
        }

        public virtual void OpenClass(string htmlClassName)
        {
            if (htmlClassName == null)
                throw new ArgumentNullException("htmlClassName");

            if (HtmlOutput)
                _outBuilder.Append(@"<span class=""" + htmlClassName + @""">");
        }

        public virtual void CloseClass()
        {
            if (HtmlOutput)
                _outBuilder.Append(@"</span>");
        }

        public virtual void AddOutputContentRaw(string content)
        {
            _outBuilder.Append(content);
        }

        public virtual void AddOutputLineBreak()
        {
            _outBuilder.Append(Environment.NewLine);
        }

        public string DumpOutput()
        {
            return _outBuilder.ToString();
        }

    }
}
