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

namespace PoorMansTSqlFormatterLib
{
    static class ObfuscatingKeywordMapping
    {
        public static Dictionary<string, string> Instance { get; private set; }
        static ObfuscatingKeywordMapping()
        {
            Instance = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Instance.Add("PROCEDURE", "PROC");
            Instance.Add("LEFT OUTER JOIN", "LEFT JOIN");
            Instance.Add("RIGHT OUTER JOIN", "RIGHT JOIN");
            Instance.Add("FULL OUTER JOIN", "FULL JOIN");
            Instance.Add("INNER JOIN", "JOIN");
            Instance.Add("TRANSACTION", "TRAN");
            Instance.Add("BEGIN TRANSACTION", "BEGIN TRAN");
            Instance.Add("COMMIT TRANSACTION", "COMMIT TRAN");
            Instance.Add("ROLLBACK TRANSACTION", "ROLLBACK TRAN");
            Instance.Add("VARBINARY", "BINARY VARYING");
            Instance.Add("VARCHAR", "CHARACTER VARYING");
            Instance.Add("CHARACTER", "CHAR");
            Instance.Add("CHAR VARYING", "VARCHAR");
            Instance.Add("DECIMAL", "DEC");
            Instance.Add("FLOAT", "DOUBLE PRECISION");
            Instance.Add("INTEGER", "INT");
            Instance.Add("NCHAR", "NATIONAL CHARACTER");
            Instance.Add("NATIONAL CHAR", "NCHAR");
            Instance.Add("NVARCHAR", "NATIONAL CHARACTER VARYING");
            Instance.Add("NATIONAL CHAR VARYING", "NVARCHAR");
            Instance.Add("NTEXT", "NATIONAL TEXT");
        }
    }
}
