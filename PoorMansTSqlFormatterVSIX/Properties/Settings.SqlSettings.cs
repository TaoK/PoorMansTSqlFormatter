using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoorMansTSqlFormatterLib.Formatters;

namespace PoorMansTSqlFormatterVSIX.Properties
{
    internal sealed partial class Settings: PoorMansTSqlFormatterPluginShared.ISqlSettings
    {
        public PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions Options
        {
            get
            {
                return new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions(OptionsSerialized);
            }
            set
            {
                OptionsSerialized = value.ToSerializedString();
            }
        }
        

    }
}
