﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PoorMansTSqlFormatterDemo.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DisplayTokenList {
            get {
                return ((bool)(this["DisplayTokenList"]));
            }
            set {
                this["DisplayTokenList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DisplayParsedSqlXml {
            get {
                return ((bool)(this["DisplayParsedSqlXml"]));
            }
            set {
                this["DisplayParsedSqlXml"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DisplayFormattingOptions {
            get {
                return ((bool)(this["DisplayFormattingOptions"]));
            }
            set {
                this["DisplayFormattingOptions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UpgradeCompleted {
            get {
                return ((bool)(this["UpgradeCompleted"]));
            }
            set {
                this["UpgradeCompleted"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Standard")]
        public string Formatter {
            get {
                return ((string)(this["Formatter"]));
            }
            set {
                this["Formatter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IdentityColoring {
            get {
                return ((bool)(this["IdentityColoring"]));
            }
            set {
                this["IdentityColoring"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\t")]
        public string Indent {
            get {
                return ((string)(this["Indent"]));
            }
            set {
                this["Indent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int IndentWidth {
            get {
                return ((int)(this["IndentWidth"]));
            }
            set {
                this["IndentWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("999")]
        public int MaxWidth {
            get {
                return ((int)(this["MaxWidth"]));
            }
            set {
                this["MaxWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool StandardColoring {
            get {
                return ((bool)(this["StandardColoring"]));
            }
            set {
                this["StandardColoring"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExpandCommaLists {
            get {
                return ((bool)(this["ExpandCommaLists"]));
            }
            set {
                this["ExpandCommaLists"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TrailingCommas {
            get {
                return ((bool)(this["TrailingCommas"]));
            }
            set {
                this["TrailingCommas"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SpaceAfterComma {
            get {
                return ((bool)(this["SpaceAfterComma"]));
            }
            set {
                this["SpaceAfterComma"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExpandBooleanExpressions {
            get {
                return ((bool)(this["ExpandBooleanExpressions"]));
            }
            set {
                this["ExpandBooleanExpressions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExpandCaseStatements {
            get {
                return ((bool)(this["ExpandCaseStatements"]));
            }
            set {
                this["ExpandCaseStatements"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ExpandSelectStatements
        {
            get
            {
                return ((bool)(this["ExpandSelectStatements"]));
            }
            set
            {
                this["ExpandSelectStatements"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExpandBetweenConditions {
            get {
                return ((bool)(this["ExpandBetweenConditions"]));
            }
            set {
                this["ExpandBetweenConditions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool BreakJoinOnSections {
            get {
                return ((bool)(this["BreakJoinOnSections"]));
            }
            set {
                this["BreakJoinOnSections"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UppercaseKeywords {
            get {
                return ((bool)(this["UppercaseKeywords"]));
            }
            set {
                this["UppercaseKeywords"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableKeywordStandardization {
            get {
                return ((bool)(this["EnableKeywordStandardization"]));
            }
            set {
                this["EnableKeywordStandardization"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("EN")]
        public string UILanguage {
            get {
                return ((string)(this["UILanguage"]));
            }
            set {
                this["UILanguage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomizeColor {
            get {
                return ((bool)(this["RandomizeColor"]));
            }
            set {
                this["RandomizeColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomizeKeywordCase {
            get {
                return ((bool)(this["RandomizeKeywordCase"]));
            }
            set {
                this["RandomizeKeywordCase"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomizeLineLength {
            get {
                return ((bool)(this["RandomizeLineLength"]));
            }
            set {
                this["RandomizeLineLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PreserveComments {
            get {
                return ((bool)(this["PreserveComments"]));
            }
            set {
                this["PreserveComments"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool KeywordSubstitution {
            get {
                return ((bool)(this["KeywordSubstitution"]));
            }
            set {
                this["KeywordSubstitution"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExpandInLists {
            get {
                return ((bool)(this["ExpandInLists"]));
            }
            set {
                this["ExpandInLists"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int NewStatementLineBreaks {
            get {
                return ((int)(this["NewStatementLineBreaks"]));
            }
            set {
                this["NewStatementLineBreaks"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int NewClauseLineBreaks {
            get {
                return ((int)(this["NewClauseLineBreaks"]));
            }
            set {
                this["NewClauseLineBreaks"] = value;
            }
        }
    }
}
