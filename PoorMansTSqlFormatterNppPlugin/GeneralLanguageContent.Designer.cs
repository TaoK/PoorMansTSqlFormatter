﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PoorMansTSqlFormatterNppPlugin {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class GeneralLanguageContent {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal GeneralLanguageContent() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PoorMansTSqlFormatterNppPlugin.GeneralLanguageContent", typeof(GeneralLanguageContent).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Format T-SQL Code.
        /// </summary>
        internal static string FormatButtonText {
            get {
                return ResourceManager.GetString("FormatButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to T-SQL Formating Options....
        /// </summary>
        internal static string OptionsButtonText {
            get {
                return ResourceManager.GetString("OptionsButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errors found during SQL parsing. Would you like to apply formatting anyway?.
        /// </summary>
        internal static string ParseErrorWarningMessage {
            get {
                return ResourceManager.GetString("ParseErrorWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errors found. Continue?.
        /// </summary>
        internal static string ParseErrorWarningMessageTitle {
            get {
                return ResourceManager.GetString("ParseErrorWarningMessageTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A simple free (AGPL) T-SQL Formatting Plugin for Notepad++..
        /// </summary>
        internal static string ProjectAboutDescription {
            get {
                return ResourceManager.GetString("ProjectAboutDescription", resourceCulture);
            }
        }
    }
}
