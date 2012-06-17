/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011 Tao Klerks

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
            txt_IndentString.Text = _settings.IndentString;
            if (_supportsHotkey) txt_Hotkey.Text = (string)_settings["Hotkey"];
            txt_MaxLineWidth.Text = _settings.MaxLineWidth.ToString();
            txt_SpacesPerTab.Text = _settings.SpacesPerTab.ToString();
            chk_ExpandBetweenConditions.Checked = _settings.ExpandBetweenConditions;
            chk_ExpandBooleanExpressions.Checked = _settings.ExpandBooleanExpressions;
            chk_ExpandCaseStatements.Checked = _settings.ExpandCaseStatements;
            chk_ExpandCommaLists.Checked = _settings.ExpandCommaLists;
            chk_TrailingCommas.Checked = _settings.TrailingCommas;
            chk_BreakJoinOnSections.Checked = _settings.BreakJoinOnSections;
            chk_UppercaseKeywords.Checked = _settings.UppercaseKeywords;
            chk_SpaceAfterExpandedComma.Checked = _settings.SpaceAfterExpandedComma;
            chk_StandardizeKeywords.Checked = _settings.KeywordStandardization;
        }

        private void SetSettingsFromControlValues()
        {
            _settings.IndentString = txt_IndentString.Text.Replace("\t", "\\t").Replace(" ", "\\s");
            if (_supportsHotkey) _settings["Hotkey"] = txt_Hotkey.Text;
            _settings.MaxLineWidth = int.Parse(txt_MaxLineWidth.Text);
            _settings.SpacesPerTab = int.Parse(txt_SpacesPerTab.Text);
            _settings.SpaceAfterExpandedComma = chk_SpaceAfterExpandedComma.Checked;
            _settings.ExpandBetweenConditions = chk_ExpandBetweenConditions.Checked;
            _settings.ExpandBooleanExpressions = chk_ExpandBooleanExpressions.Checked;
            _settings.ExpandCaseStatements = chk_ExpandCaseStatements.Checked;
            _settings.ExpandCommaLists = chk_ExpandCommaLists.Checked;
            _settings.TrailingCommas = chk_TrailingCommas.Checked;
            _settings.BreakJoinOnSections = chk_BreakJoinOnSections.Checked;
            _settings.UppercaseKeywords = chk_UppercaseKeywords.Checked;
            _settings.KeywordStandardization = chk_StandardizeKeywords.Checked;
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
