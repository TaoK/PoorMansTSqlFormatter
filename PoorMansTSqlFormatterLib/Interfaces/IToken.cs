using System;
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib.Interfaces
{
    public interface IToken
    {
        TokenType Type { get; set; }
        string Value { get; set; }
    }
}
