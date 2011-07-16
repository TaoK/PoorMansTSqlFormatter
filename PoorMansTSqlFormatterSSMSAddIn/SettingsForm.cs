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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PoorMansTSqlFormatterSSMSAddIn
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
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
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings. Detail: " + ex.Message);
            }
        }

        private void LoadControlValuesFromSettings()
        {
            txt_IndentString.Text = Properties.Settings.Default.IndentString;
            txt_Hotkey.Text = Properties.Settings.Default.Hotkey;
            txt_MaxLineWidth.Text = Properties.Settings.Default.MaxLineWidth.ToString();
            txt_SpacesPerTab.Text = Properties.Settings.Default.SpacesPerTab.ToString();
            chk_ExpandBetweenConditions.Checked = Properties.Settings.Default.ExpandBetweenConditions;
            chk_ExpandBooleanExpressions.Checked = Properties.Settings.Default.ExpandBooleanExpressions;
            chk_ExpandCaseStatements.Checked = Properties.Settings.Default.ExpandCaseStatements;
            chk_ExpandCommaLists.Checked = Properties.Settings.Default.ExpandCommaLists;
            chk_TrailingCommas.Checked = Properties.Settings.Default.TrailingCommas;
            chk_UppercaseKeywords.Checked = Properties.Settings.Default.UppercaseKeywords;
            chk_SpaceAfterExpandedComma.Checked = Properties.Settings.Default.SpaceAfterExpandedComma;
        }

        private void SetSettingsFromControlValues()
        {
            Properties.Settings.Default.IndentString = txt_IndentString.Text.Replace("\t", "\\t");
            Properties.Settings.Default.Hotkey = txt_Hotkey.Text;
            Properties.Settings.Default.MaxLineWidth = int.Parse(txt_MaxLineWidth.Text);
            Properties.Settings.Default.SpacesPerTab = int.Parse(txt_SpacesPerTab.Text);
            Properties.Settings.Default.SpaceAfterExpandedComma = chk_SpaceAfterExpandedComma.Checked;
            Properties.Settings.Default.ExpandBetweenConditions = chk_ExpandBetweenConditions.Checked;
            Properties.Settings.Default.ExpandBooleanExpressions = chk_ExpandBooleanExpressions.Checked;
            Properties.Settings.Default.ExpandCaseStatements = chk_ExpandCaseStatements.Checked;
            Properties.Settings.Default.ExpandCommaLists = chk_ExpandCommaLists.Checked;
            Properties.Settings.Default.TrailingCommas = chk_TrailingCommas.Checked;
            Properties.Settings.Default.UppercaseKeywords = chk_UppercaseKeywords.Checked;
        }

        private void llbl_HotkeyHint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://msdn.microsoft.com/en-us/library/envdte.command.bindings(v=vs.80).aspx");
        }

        private void btn_About_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
            about.Dispose();
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> previousValues = new Dictionary<string,object>();
            foreach (System.Configuration.SettingsProperty prop in Properties.Settings.Default.Properties)
                previousValues.Add(prop.Name, Properties.Settings.Default[prop.Name]);

            Properties.Settings.Default.Reset();
            LoadControlValuesFromSettings();

            foreach (string prop in previousValues.Keys)
                Properties.Settings.Default[prop] = previousValues[prop];
            Properties.Settings.Default.Save(); //because reset, irritatingly, saves.
        }
    }
}
