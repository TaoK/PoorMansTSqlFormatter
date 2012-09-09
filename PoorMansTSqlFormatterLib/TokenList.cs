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
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib
{
    public class TokenList : List<IToken>, ITokenList
    {
        public bool HasUnfinishedToken { get; set; }
        
        public string PrettyPrint()
        {
            StringBuilder outString = new StringBuilder();
            foreach(IToken contentToken in this)
            {
                string tokenType = contentToken.Type.ToString();
                outString.Append(tokenType.PadRight(20));
                outString.Append(": ");
                outString.AppendLine(contentToken.Value);
            }
            return outString.ToString();
        }

        public new IList<IToken> GetRange(int index, int count)
        {
            return base.GetRange(index, count);
        }

        public IList<IToken> GetRangeByIndex(int fromIndex, int toIndex)
        {
            return this.GetRange(fromIndex, toIndex - fromIndex + 1);
        }
    }
}
