/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

Additional Contributors:
 * Timothy Klenke, 2012

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace PoorMansTSqlFormatterPluginShared
{
    public partial class SettingsForm : Form
    {
        //key binding "Text Editor" scope name is necessary for "Reset" action, but is implementation-specific.
        public delegate string GetTextEditorKeyBindingScopeName();

        ISqlSettings _settings = null;
        Assembly _pluginAssembly = null;
        string _aboutDescription = null;
        bool _supportsHotkey = false;
        GetTextEditorKeyBindingScopeName _keyBindingScopeNameMethod;

        public SettingsForm(ISqlSettings settings, Assembly pluginAssembly, string aboutDescription) : this(settings, pluginAssembly, aboutDescription, null)
        {
        }

        public SettingsForm(ISqlSettings settings, Assembly pluginAssembly, string aboutDescription, GetTextEditorKeyBindingScopeName keyBindingScopeNameMethod)
        {
            _settings = settings;
            _pluginAssembly = pluginAssembly;
            _aboutDescription = aboutDescription;

            _keyBindingScopeNameMethod = keyBindingScopeNameMethod;

            foreach (System.Configuration.SettingsProperty prop in _settings.Properties)
                if (prop.Name.Equals("Hotkey"))
                    _supportsHotkey = true;

            InitializeComponent();

            if (!_supportsHotkey)
            {
                txt_Hotkey.Visible = false;
                lbl_Hotkey.Visible = false;
                lbl_HotkeyHint.Visible = false;
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadControlValuesFromSettings();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                SetSettingsFromControlValues();
                _settings.Save();
            }
            catch (Exception ex)
            {
                var _generalResourceManager = new System.Resources.ResourceManager("PoorMansTSqlFormatterPluginShared.GeneralLanguageContent", System.Reflection.Assembly.GetExecutingAssembly());
                MessageBox.Show(string.Format(_generalResourceManager.GetString("SettingsSavingErrorMessage"), Environment.NewLine, ex.Message));
            }
        }

        private void LoadControlValuesFromSettings()
        {
            PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions options = _settings.Options;

            txt_IndentString.Text = options.IndentString.Replace("\t","\\t").Replace(" ","\\s");
            txt_MaxLineWidth.Text = options.MaxLineWidth.ToString();
			txt_SpacesPerTab.Text = options.SpacesPerTab.ToString();
			txt_StatementBreaks.Text = options.NewStatementLineBreaks.ToString();
			txt_ClauseBreaks.Text = options.NewClauseLineBreaks.ToString();
			chk_ExpandBetweenConditions.Checked = options.ExpandBetweenConditions;
            chk_ExpandBooleanExpressions.Checked = options.ExpandBooleanExpressions;
            chk_ExpandCaseStatements.Checked = options.ExpandCaseStatements;
			chk_ExpandCommaLists.Checked = options.ExpandCommaLists;
			chk_ExpandInLists.Checked = options.ExpandInLists;
			chk_TrailingCommas.Checked = options.TrailingCommas;
            chk_BreakJoinOnSections.Checked = options.BreakJoinOnSections;
            chk_UppercaseKeywords.Checked = options.UppercaseKeywords;
            chk_SpaceAfterExpandedComma.Checked = options.SpaceAfterExpandedComma;
            chk_StandardizeKeywords.Checked = options.KeywordStandardization;

            if (_supportsHotkey) txt_Hotkey.Text = (string)_settings["Hotkey"];
        }

        private void SetSettingsFromControlValues()
        {
            _settings.Options = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions() {
                IndentString = txt_IndentString.Text,
                MaxLineWidth = int.Parse(txt_MaxLineWidth.Text),
				SpacesPerTab = int.Parse(txt_SpacesPerTab.Text),
				NewStatementLineBreaks = int.Parse(txt_StatementBreaks.Text),
				NewClauseLineBreaks = int.Parse(txt_ClauseBreaks.Text),
				SpaceAfterExpandedComma = chk_SpaceAfterExpandedComma.Checked,
                ExpandBetweenConditions = chk_ExpandBetweenConditions.Checked,
                ExpandBooleanExpressions = chk_ExpandBooleanExpressions.Checked,
                ExpandCaseStatements = chk_ExpandCaseStatements.Checked,
				ExpandCommaLists = chk_ExpandCommaLists.Checked,
				ExpandInLists = chk_ExpandInLists.Checked,
				TrailingCommas = chk_TrailingCommas.Checked,
                BreakJoinOnSections = chk_BreakJoinOnSections.Checked,
                UppercaseKeywords = chk_UppercaseKeywords.Checked,
                KeywordStandardization = chk_StandardizeKeywords.Checked
            };
            
            if (_supportsHotkey) _settings["Hotkey"] = txt_Hotkey.Text;
        }

        private void llbl_HotkeyHint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://msdn.microsoft.com/en-us/library/envdte.command.bindings(v=vs.80).aspx");
        }

        private void btn_About_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox(_pluginAssembly, _aboutDescription);
            about.ShowDialog();
            about.Dispose();
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> previousValues = new Dictionary<string,object>();
            foreach (System.Configuration.SettingsProperty prop in _settings.Properties)
                previousValues.Add(prop.Name, _settings[prop.Name]);

            _settings.Reset();

            //unfortuntely, the Hotkey "True" default is not very useful in VS environments, need to 
            // grab the localized value from the VS context.
            if (_supportsHotkey && _keyBindingScopeNameMethod != null)
            {
                string scopeName = _keyBindingScopeNameMethod();
                if (scopeName != null)
                {
                    _settings["Hotkey"] = _settings["Hotkey"].ToString().Replace("Text Editor", scopeName);
                }
            }

            LoadControlValuesFromSettings();

            foreach (string prop in previousValues.Keys)
                _settings[prop] = previousValues[prop];
            _settings.Save(); //because reset, irritatingly, saves.
        }
    }
}
