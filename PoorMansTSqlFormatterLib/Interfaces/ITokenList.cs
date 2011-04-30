using System;
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib.Interfaces
{
    public interface ITokenList : IList<IToken>
    {
        bool HasErrors { get; set; }
    }
}
