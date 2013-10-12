/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

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

        public const string ENAME_SQL_ROOT = "SqlRoot";
        public const string ENAME_SQL_STATEMENT = "SqlStatement";
        public const string ENAME_SQL_CLAUSE = "Clause";
        public const string ENAME_SET_OPERATOR_CLAUSE = "SetOperatorClause";
        public const string ENAME_INSERT_CLAUSE = "InsertClause";
        public const string ENAME_BEGIN_END_BLOCK = "BeginEndBlock";
        public const string ENAME_TRY_BLOCK = "TryBlock";
        public const string ENAME_CATCH_BLOCK = "CatchBlock";
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
        public const string ENAME_CURSOR_DECLARATION = "CursorDeclaration";
        public const string ENAME_CURSOR_FOR_BLOCK = "CursorForBlock";
        public const string ENAME_CURSOR_FOR_OPTIONS = "CursorForOptions";
        public const string ENAME_CTE_WITH_CLAUSE = "CTEWithClause";
        public const string ENAME_CTE_ALIAS = "CTEAlias";
        public const string ENAME_CTE_AS_BLOCK = "CTEAsBlock";
        public const string ENAME_BEGIN_TRANSACTION = "BeginTransaction";
        public const string ENAME_COMMIT_TRANSACTION = "CommitTransaction";
        public const string ENAME_ROLLBACK_TRANSACTION = "RollbackTransaction";
        public const string ENAME_SAVE_TRANSACTION = "SaveTransaction";
        public const string ENAME_DDL_DECLARE_BLOCK = "DDLDeclareBlock";
        public const string ENAME_DDL_PROCEDURAL_BLOCK = "DDLProceduralBlock";
        public const string ENAME_DDL_OTHER_BLOCK = "DDLOtherBlock";
        public const string ENAME_DDL_AS_BLOCK = "DDLAsBlock";
        public const string ENAME_DDL_PARENS = "DDLParens";
        public const string ENAME_DDL_SUBCLAUSE = "DDLSubClause";
        public const string ENAME_DDL_RETURNS = "DDLReturns";
        public const string ENAME_DDLDETAIL_PARENS = "DDLDetailParens";
        public const string ENAME_DDL_WITH_CLAUSE = "DDLWith";
        public const string ENAME_PERMISSIONS_BLOCK = "PermissionsBlock";
        public const string ENAME_PERMISSIONS_DETAIL = "PermissionsDetail";
        public const string ENAME_PERMISSIONS_TARGET = "PermissionsTarget";
        public const string ENAME_PERMISSIONS_RECIPIENT = "PermissionsRecipient";
        public const string ENAME_TRIGGER_CONDITION = "TriggerCondition";
        public const string ENAME_SELECTIONTARGET_PARENS = "SelectionTargetParens";
        public const string ENAME_EXPRESSION_PARENS = "ExpressionParens";
        public const string ENAME_FUNCTION_PARENS = "FunctionParens";
		public const string ENAME_IN_PARENS = "InParens";
		public const string ENAME_FUNCTION_KEYWORD = "FunctionKeyword";
        public const string ENAME_DATATYPE_KEYWORD = "DataTypeKeyword";
        public const string ENAME_COMPOUNDKEYWORD = "CompoundKeyword";
        public const string ENAME_OTHERKEYWORD = "OtherKeyword";
        public const string ENAME_LABEL = "Label";
        public const string ENAME_CONTAINER_OPEN = "ContainerOpen";
        public const string ENAME_CONTAINER_MULTISTATEMENT = "ContainerMultiStatementBody";
        public const string ENAME_CONTAINER_SINGLESTATEMENT = "ContainerSingleStatementBody";
        public const string ENAME_CONTAINER_GENERALCONTENT = "ContainerContentBody";
        public const string ENAME_CONTAINER_CLOSE = "ContainerClose";
        public const string ENAME_SELECTIONTARGET = "SelectionTarget";
        public const string ENAME_MERGE_CLAUSE = "MergeClause";
        public const string ENAME_MERGE_TARGET = "MergeTarget";
        public const string ENAME_MERGE_USING = "MergeUsing";
        public const string ENAME_MERGE_CONDITION = "MergeCondition";
        public const string ENAME_MERGE_WHEN = "MergeWhen";
        public const string ENAME_MERGE_THEN = "MergeThen";
        public const string ENAME_MERGE_ACTION = "MergeAction";
        public const string ENAME_JOIN_ON_SECTION = "JoinOn";

        public const string ENAME_PSEUDONAME = "PseudoName";
        public const string ENAME_WHITESPACE = "WhiteSpace";
        public const string ENAME_OTHERNODE = "Other";
        public const string ENAME_COMMENT_SINGLELINE = "SingleLineComment";
        public const string ENAME_COMMENT_SINGLELINE_CSTYLE = "SingleLineCommentCStyle";
        public const string ENAME_COMMENT_MULTILINE = "MultiLineComment";
        public const string ENAME_STRING = "String";
        public const string ENAME_NSTRING = "NationalString";
        public const string ENAME_QUOTED_STRING = "QuotedString";
        public const string ENAME_BRACKET_QUOTED_NAME = "BracketQuotedName";
        public const string ENAME_COMMA = "Comma";
        public const string ENAME_PERIOD = "Period";
        public const string ENAME_SEMICOLON = "Semicolon";
        public const string ENAME_SCOPERESOLUTIONOPERATOR = "ScopeResolutionOperator";
        public const string ENAME_ASTERISK = "Asterisk";
        public const string ENAME_EQUALSSIGN = "EqualsSign";
        public const string ENAME_ALPHAOPERATOR = "AlphaOperator";
        public const string ENAME_OTHEROPERATOR = "OtherOperator";

        public const string ENAME_AND_OPERATOR = "And";
        public const string ENAME_OR_OPERATOR = "Or";
        public const string ENAME_BETWEEN_CONDITION = "Between";
        public const string ENAME_BETWEEN_LOWERBOUND = "LowerBound";
        public const string ENAME_BETWEEN_UPPERBOUND = "UpperBound";

        public const string ENAME_NUMBER_VALUE = "NumberValue";
        public const string ENAME_MONETARY_VALUE = "MonetaryValue";
        public const string ENAME_BINARY_VALUE = "BinaryValue";

        //attribute names
        public const string ANAME_ERRORFOUND = "errorFound";
        public const string ANAME_HASERROR = "hasError";
        public const string ANAME_DATALOSS = "dataLossLimitation";
        public const string ANAME_SIMPLETEXT = "simpleText";
    }
}
