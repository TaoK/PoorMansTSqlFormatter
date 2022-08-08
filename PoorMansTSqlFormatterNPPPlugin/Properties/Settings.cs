using System.Configuration;

namespace PoorMansTSqlFormatterNPPPlugin.Properties
{
    internal sealed partial class Settings : PoorMansTSqlFormatterPluginShared.ISqlSettings
    {
        public Settings()
        {

            SettingsProvider provider = new SettingsProviderCustomPathUnversioned();

            // Try to re-use an existing provider, since we cannot have multiple providers
            // with same name.
            if (Providers[provider.Name] == null)
                Providers.Add(provider);
            else
                provider = Providers[provider.Name];

            // Change default provider.
            foreach (SettingsProperty property in Properties)
            {
                if (
                    property.PropertyType.GetCustomAttributes(
                        typeof(SettingsProviderAttribute),
                        false
                    ).Length == 0
                 )
                {
                    property.Provider = provider;
                }
            }
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
