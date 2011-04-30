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

namespace PoorMansTSqlFormatterLib.Interfaces
{
    public static class SqlXmlConstants
    {
        public const string ENAME_WHITESPACE = "WhiteSpace";
        public const string ENAME_OTHERNODE = "Other";
        public const string ENAME_COMMENT_SINGLELINE = "SingleLineComment";
        public const string ENAME_COMMENT_MULTILINE = "MultiLineComment";
        public const string ENAME_STRING = "String";
        public const string ENAME_NSTRING = "NationalString";
        public const string ENAME_QUOTED_IDENTIFIER = "QuotedIdentifier";
        public const string ENAME_COMMA = "Comma";
        public const string ENAME_PERIOD = "Period";
        public const string ENAME_SEMICOLON = "Semicolon";
        public const string ENAME_ASTERISK = "Asterisk";
        public const string ENAME_OTHEROPERATOR = "OtherOperator";
        public const string ENAME_SQL_ROOT = "SqlRoot";
        public const string ENAME_SQL_STATEMENT = "SqlStatement";
        public const string ENAME_SQL_CLAUSE = "Clause";
        public const string ENAME_UNION_CLAUSE = "UnionClause";
        public const string ENAME_BEGIN_END_BLOCK = "BeginEndBlock";
        public const string ENAME_TRY_BLOCK = "TryBlock";
        public const string ENAME_BEGIN_TRANSACTION = "BeginTransaction";
        public const string ENAME_BATCH_SEPARATOR = "BatchSeparator";
        public const string ENAME_CASE_STATEMENT = "CaseStatement";
        public const string ENAME_CASE_INPUT = "Input";
        public const string ENAME_CASE_WHEN = "When";
        public const string ENAME_CASE_THEN = "Then";
        public const string ENAME_CASE_ELSE = "CaseElse";
        public const string ENAME_IF_STATEMENT = "IfStatement";
        public const string ENAME_ELSE_CLAUSE = "ElseClause";
        public const string ENAME_BOOLEAN_EXPRESSION = "BooleanExpression";
        public const string ENAME_WHILE_LOOP = "WhileLoop";
        public const string ENAME_CATCH_BLOCK = "CatchBlock";
        public const string ENAME_COMMIT_TRANSACTION = "CommitTransaction";
        public const string ENAME_ROLLBACK_TRANSACTION = "RollbackTransaction";
        public const string ENAME_DDL_BLOCK = "DDLBlock";
        public const string ENAME_DDL_AS_BLOCK = "DDLAsBlock";
        public const string ENAME_DDL_PARENS = "DDLParens";
        public const string ENAME_DDLDETAIL_PARENS = "DDLDetailParens";
        public const string ENAME_EXPRESSION_PARENS = "ExpressionParens";
        public const string ENAME_FUNCTION_PARENS = "FunctionParens";
        public const string ENAME_AND_OPERATOR = "And";
        public const string ENAME_OR_OPERATOR = "Or";
        public const string ENAME_BETWEEN_CONDITION = "Between";
        public const string ENAME_BETWEEN_LOWERBOUND = "LowerBound";
        public const string ENAME_BETWEEN_UPPERBOUND = "UpperBound";

        //attribute names
        public const string ANAME_ERRORFOUND = "errorFound";
        public const string ANAME_DATALOSS = "dataLossLimitation";
    }
}
