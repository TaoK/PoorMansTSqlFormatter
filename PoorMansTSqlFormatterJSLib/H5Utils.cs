/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
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

namespace PoorMansTSqlFormatterLib
{
    public static class H5Utils
    {
        //Invariant conversions are not implemented in Bridge.Net and .Net Standard...
        public static string ToLowerInvariant(this string value) => value.ToLower();
        public static string ToUpperInvariant(this string value) => value.ToUpper();
        public static char ToLowerInvariant(this char value) => char.ToLower(value);
        public static char ToUpperInvariant(this char value) => char.ToUpper(value);
    }
}
