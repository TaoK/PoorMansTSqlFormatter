/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2012 Tao Klerks

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
using System.Text;

namespace PoorMansTSqlFormatterLib
{
    public static class Utils
    {
        public static string HtmlEncode(string raw)
        {
            /*
             * This is a "Roll Your Own" implementation of HtmlEncode, which was necessary in the end because people want
             * to use the library with .Net 3.5 Client Profile and other restricted environments; the dependency on 
             * System.Web just for HtmlEncode was always a little disturbing anyway.
             * I've attempted to optimize the implementation towards strings that don't actually contain any special 
             * characters, and I've also skipped some of the more interesting stuff that I see in the MS implementation
             * (pointers, and some special handling in the WinAnsi special range of characters?), keeping it to the basic 
             * 4 "known bad" characters.
             */

            if (raw == null)
                return null;

            StringBuilder outBuilder = null;
            int latestCheckPos = 0;
            int latestReplacementPos = 0;

            foreach (char c in raw)
            {
                string replacementString = null;

                switch (c)
                {
                    case '>':
                        replacementString = "&gt;";
                        break;
                    case '<':
                        replacementString = "&lt;";
                        break;
                    case '&':
                        replacementString = "&amp;";
                        break;
                    case '"':
                        replacementString = "&quot;";
                        break;
                }

                if (replacementString != null)
                {
                    if (outBuilder == null)
                        outBuilder = new StringBuilder(raw.Length);

                    if (latestReplacementPos < latestCheckPos)
                        outBuilder.Append(raw.Substring(latestReplacementPos, latestCheckPos - latestReplacementPos));

                    outBuilder.Append(replacementString);

                    latestReplacementPos = latestCheckPos + 1;
                }

                latestCheckPos++;
            }

            if (outBuilder != null)
            {
                if (latestReplacementPos < latestCheckPos)
                    outBuilder.Append(raw.Substring(latestReplacementPos));

                return outBuilder.ToString();
            }
            else
                return raw;
        }
    }
}
