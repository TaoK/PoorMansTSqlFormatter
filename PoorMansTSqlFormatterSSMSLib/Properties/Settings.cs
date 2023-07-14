namespace PoorMansTSqlFormatterSSMSLib.Properties {
    
    
    internal sealed partial class Settings {
        
        public Settings() {
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
        }

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

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string OptionsSerialized
        {
            get
            {
                string serializedOptions = (string)this["OptionsSerialized"];
                return serializedOptions;
            }
            set
            {
                this["OptionsSerialized"] = value;
            }
        }

    }
}
