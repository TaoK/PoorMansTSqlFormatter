namespace PoorMansTSqlFormatterSSMSLib.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings {
        
        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
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

        private const string LOAD_LEGACY = "~load options from backward compatible settings";

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(LOAD_LEGACY)]
        public string OptionsSerialized
        {
            get
            {
                string serializedOptions = (string)this["OptionsSerialized"];
                if (serializedOptions == LOAD_LEGACY)
                    serializedOptions = LoadFromLegacySettings();
                return serializedOptions;
            }
            set
            {
                this["OptionsSerialized"] = value;
            }
        }

        private string LoadFromLegacySettings()
        {

            // In previous versions the Options were stored in individual setting properties.
            // So that this and future versions are backward compatible, 
            // If the settings file doesn't contain an Options element assume that the file has the old individual settings.

            var options = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions()
            {
                ExpandCommaLists = this.ExpandCommaLists,
                TrailingCommas = this.TrailingCommas,
                ExpandBooleanExpressions = this.ExpandBooleanExpressions,
                ExpandCaseStatements = this.ExpandCaseStatements,
                ExpandBetweenConditions = this.ExpandBetweenConditions,
                UppercaseKeywords = this.UppercaseKeywords,
                IndentString = this.IndentString,
                SpaceAfterExpandedComma = this.SpaceAfterExpandedComma,
                SpacesPerTab = this.SpacesPerTab,
                MaxLineWidth = this.MaxLineWidth,
                KeywordStandardization = this.KeywordStandardization,
                BreakJoinOnSections = this.BreakJoinOnSections
            };

            return options.ToSerializedString();

        }

    }
}
