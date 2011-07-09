using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PoorMansTSqlFormatterLib.Interfaces
{
    interface IParseTree
    {
        XmlDocument ToXmlDoc();
    }
}
