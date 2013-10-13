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
    static class StandardKeywordRemapping
    {
        public static Dictionary<string, string> Instance { get; private set;  }
        static StandardKeywordRemapping()
        {
            Instance = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Instance.Add("PROC", "PROCEDURE");
            Instance.Add("LEFT OUTER JOIN", "LEFT JOIN");
            Instance.Add("RIGHT OUTER JOIN", "RIGHT JOIN");
            Instance.Add("FULL OUTER JOIN", "FULL JOIN");
            Instance.Add("JOIN", "INNER JOIN");
            //TODO: This is now wrong in MERGE statements... we now need a scope-limitation strategy :(
            //Instance.Add("INSERT", "INSERT INTO");
            Instance.Add("TRAN", "TRANSACTION");
            Instance.Add("BEGIN TRAN", "BEGIN TRANSACTION");
            Instance.Add("COMMIT TRAN", "COMMIT TRANSACTION");
            Instance.Add("ROLLBACK TRAN", "ROLLBACK TRANSACTION");
            Instance.Add("BINARY VARYING", "VARBINARY");
            Instance.Add("CHAR VARYING", "VARCHAR");
            Instance.Add("CHARACTER", "CHAR");
            Instance.Add("CHARACTER VARYING", "VARCHAR");
            Instance.Add("DEC", "DECIMAL");
            Instance.Add("DOUBLE PRECISION", "FLOAT");
            Instance.Add("INTEGER", "INT");
            Instance.Add("NATIONAL CHARACTER", "NCHAR");
            Instance.Add("NATIONAL CHAR", "NCHAR");
            Instance.Add("NATIONAL CHARACTER VARYING", "NVARCHAR");
            Instance.Add("NATIONAL CHAR VARYING", "NVARCHAR");
			Instance.Add("NATIONAL TEXT", "NTEXT");
			Instance.Add("OUT", "OUTPUT");
			//TODO: This is wrong when a TIMESTAMP column is unnamed; ROWVERSION does not auto-name. Due to context-sensitivity, this mapping is disabled for now.
            // REF: http://msdn.microsoft.com/en-us/library/ms182776.aspx
            //Instance.Add("TIMESTAMP", "ROWVERSION");
        }
    }
}
