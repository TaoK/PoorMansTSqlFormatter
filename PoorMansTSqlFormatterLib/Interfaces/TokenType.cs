using System;
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib.Interfaces
{
    public enum TokenType
    {
        OpenParens,
        CloseParens,
        WhiteSpace,
        OtherNode,
        SingleLineComment,
        MultiLineComment,
        String,
        NationalString,
        QuotedIdentifier,
        Comma,
        Period,
        Semicolon,
        Asterisk,
        OtherOperator
    }
}