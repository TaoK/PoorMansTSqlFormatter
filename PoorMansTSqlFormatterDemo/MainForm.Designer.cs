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

namespace PoorMansTSqlFormatterDemo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			FrameworkClassReplacements.SingleAssemblyComponentResourceManager resources = new FrameworkClassReplacements.SingleAssemblyComponentResourceManager(typeof(MainForm));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer4 = new System.Windows.Forms.SplitContainer();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txt_Input = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
			this.splitContainer5 = new System.Windows.Forms.SplitContainer();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txt_TokenizedSql = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.txt_ParsedXml = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.webBrowser_OutputSql = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.CustomContentWebBrowser();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.radio_Formatting_Identity = new System.Windows.Forms.RadioButton();
			this.radio_Formatting_Standard = new System.Windows.Forms.RadioButton();
			this.grp_Options = new System.Windows.Forms.GroupBox();
			this.chk_BreakJoinOnSections = new System.Windows.Forms.CheckBox();
			this.chk_ExpandInLists = new System.Windows.Forms.CheckBox();
			this.chk_EnableKeywordStandardization = new System.Windows.Forms.CheckBox();
			this.txt_MaxWidth = new System.Windows.Forms.TextBox();
			this.lbl_MaxWidth = new System.Windows.Forms.Label();
			this.txt_IndentWidth = new System.Windows.Forms.TextBox();
			this.lbl_IndentWidth = new System.Windows.Forms.Label();
			this.txt_Indent = new System.Windows.Forms.TextBox();
			this.lbl_Indent = new System.Windows.Forms.Label();
			this.chk_ExpandBetweenConditions = new System.Windows.Forms.CheckBox();
			this.chk_SpaceAfterComma = new System.Windows.Forms.CheckBox();
			this.chk_Coloring = new System.Windows.Forms.CheckBox();
			this.chk_UppercaseKeywords = new System.Windows.Forms.CheckBox();
			this.chk_ExpandCaseStatements = new System.Windows.Forms.CheckBox();
			this.chk_ExpandBooleanExpressions = new System.Windows.Forms.CheckBox();
			this.chk_TrailingCommas = new System.Windows.Forms.CheckBox();
			this.chk_ExpandCommaLists = new System.Windows.Forms.CheckBox();
			this.grp_IdentityFormattingOptions = new System.Windows.Forms.GroupBox();
			this.chk_IdentityColoring = new System.Windows.Forms.CheckBox();
			this.radio_Formatting_Obfuscate = new System.Windows.Forms.RadioButton();
			this.grp_ObfuscationOptions = new System.Windows.Forms.GroupBox();
			this.chk_KeywordSubstitution = new System.Windows.Forms.CheckBox();
			this.chk_PreserveComments = new System.Windows.Forms.CheckBox();
			this.chk_RandomizeKeywordCase = new System.Windows.Forms.CheckBox();
			this.chk_RandomizeLineLength = new System.Windows.Forms.CheckBox();
			this.chk_RandomizeColor = new System.Windows.Forms.CheckBox();
			this.timer_TextChangeDelay = new System.Windows.Forms.Timer(this.components);
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayTokenListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayParsedSqlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayFormattingOptionsAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.englishToolStripMenuItem = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.RadioToolStripMenuItem();
			this.frenchToolStripMenuItem = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.RadioToolStripMenuItem();
			this.spanishToolStripMenuItem = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.RadioToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lbl_StatementBreaks = new System.Windows.Forms.Label();
			this.txt_StatementBreaks = new System.Windows.Forms.TextBox();
			this.lbl_ClauseBreaks = new System.Windows.Forms.Label();
			this.txt_ClauseBreaks = new System.Windows.Forms.TextBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer4.Panel1.SuspendLayout();
			this.splitContainer4.Panel2.SuspendLayout();
			this.splitContainer4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.splitContainer5.Panel1.SuspendLayout();
			this.splitContainer5.Panel2.SuspendLayout();
			this.splitContainer5.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.grp_Options.SuspendLayout();
			this.grp_IdentityFormattingOptions.SuspendLayout();
			this.grp_ObfuscationOptions.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer4);
			// 
			// splitContainer1.Panel2
			// 
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
			this.splitContainer1.Panel2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			// 
			// splitContainer4
			// 
			resources.ApplyResources(this.splitContainer4, "splitContainer4");
			this.splitContainer4.Name = "splitContainer4";
			// 
			// splitContainer4.Panel1
			// 
			this.splitContainer4.Panel1.Controls.Add(this.groupBox1);
			// 
			// splitContainer4.Panel2
			// 
			this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txt_Input);
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// txt_Input
			// 
			this.txt_Input.AcceptsReturn = true;
			this.txt_Input.AcceptsTab = true;
			resources.ApplyResources(this.txt_Input, "txt_Input");
			this.txt_Input.Name = "txt_Input";
			this.txt_Input.TextChanged += new System.EventHandler(this.txt_Input_TextChanged);
			// 
			// splitContainer5
			// 
			resources.ApplyResources(this.splitContainer5, "splitContainer5");
			this.splitContainer5.Name = "splitContainer5";
			// 
			// splitContainer5.Panel1
			// 
			this.splitContainer5.Panel1.Controls.Add(this.groupBox2);
			// 
			// splitContainer5.Panel2
			// 
			this.splitContainer5.Panel2.Controls.Add(this.groupBox3);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txt_TokenizedSql);
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// txt_TokenizedSql
			// 
			this.txt_TokenizedSql.AcceptsReturn = true;
			this.txt_TokenizedSql.AcceptsTab = true;
			resources.ApplyResources(this.txt_TokenizedSql, "txt_TokenizedSql");
			this.txt_TokenizedSql.Name = "txt_TokenizedSql";
			this.txt_TokenizedSql.ReadOnly = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.txt_ParsedXml);
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// txt_ParsedXml
			// 
			this.txt_ParsedXml.AcceptsReturn = true;
			this.txt_ParsedXml.AcceptsTab = true;
			resources.ApplyResources(this.txt_ParsedXml, "txt_ParsedXml");
			this.txt_ParsedXml.Name = "txt_ParsedXml";
			this.txt_ParsedXml.ReadOnly = true;
			// 
			// splitContainer3
			// 
			resources.ApplyResources(this.splitContainer3, "splitContainer3");
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.groupBox4);
			// 
			// splitContainer3.Panel2
			// 
			resources.ApplyResources(this.splitContainer3.Panel2, "splitContainer3.Panel2");
			this.splitContainer3.Panel2.Controls.Add(this.groupBox5);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.panel1);
			resources.ApplyResources(this.groupBox4, "groupBox4");
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.webBrowser_OutputSql);
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Name = "panel1";
			// 
			// webBrowser_OutputSql
			// 
			resources.ApplyResources(this.webBrowser_OutputSql, "webBrowser_OutputSql");
			this.webBrowser_OutputSql.MinimumSize = new System.Drawing.Size(27, 25);
			this.webBrowser_OutputSql.Name = "webBrowser_OutputSql";
			this.webBrowser_OutputSql.ScriptErrorsSuppressed = true;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.tableLayoutPanel2);
			resources.ApplyResources(this.groupBox5, "groupBox5");
			this.groupBox5.MinimumSize = new System.Drawing.Size(317, 714);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
			this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Identity, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Standard, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.grp_Options, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.grp_IdentityFormattingOptions, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Obfuscate, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.grp_ObfuscationOptions, 1, 5);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// radio_Formatting_Identity
			// 
			resources.ApplyResources(this.radio_Formatting_Identity, "radio_Formatting_Identity");
			this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Identity, 2);
			this.radio_Formatting_Identity.Name = "radio_Formatting_Identity";
			this.radio_Formatting_Identity.UseVisualStyleBackColor = true;
			this.radio_Formatting_Identity.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// radio_Formatting_Standard
			// 
			resources.ApplyResources(this.radio_Formatting_Standard, "radio_Formatting_Standard");
			this.radio_Formatting_Standard.Checked = true;
			this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Standard, 2);
			this.radio_Formatting_Standard.Name = "radio_Formatting_Standard";
			this.radio_Formatting_Standard.TabStop = true;
			this.radio_Formatting_Standard.UseVisualStyleBackColor = true;
			this.radio_Formatting_Standard.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// grp_Options
			// 
			this.grp_Options.Controls.Add(this.chk_BreakJoinOnSections);
			this.grp_Options.Controls.Add(this.chk_ExpandInLists);
			this.grp_Options.Controls.Add(this.chk_EnableKeywordStandardization);
			this.grp_Options.Controls.Add(this.txt_ClauseBreaks);
			this.grp_Options.Controls.Add(this.lbl_ClauseBreaks);
			this.grp_Options.Controls.Add(this.txt_StatementBreaks);
			this.grp_Options.Controls.Add(this.lbl_StatementBreaks);
			this.grp_Options.Controls.Add(this.txt_MaxWidth);
			this.grp_Options.Controls.Add(this.lbl_MaxWidth);
			this.grp_Options.Controls.Add(this.txt_IndentWidth);
			this.grp_Options.Controls.Add(this.lbl_IndentWidth);
			this.grp_Options.Controls.Add(this.txt_Indent);
			this.grp_Options.Controls.Add(this.lbl_Indent);
			this.grp_Options.Controls.Add(this.chk_ExpandBetweenConditions);
			this.grp_Options.Controls.Add(this.chk_SpaceAfterComma);
			this.grp_Options.Controls.Add(this.chk_Coloring);
			this.grp_Options.Controls.Add(this.chk_UppercaseKeywords);
			this.grp_Options.Controls.Add(this.chk_ExpandCaseStatements);
			this.grp_Options.Controls.Add(this.chk_ExpandBooleanExpressions);
			this.grp_Options.Controls.Add(this.chk_TrailingCommas);
			this.grp_Options.Controls.Add(this.chk_ExpandCommaLists);
			resources.ApplyResources(this.grp_Options, "grp_Options");
			this.grp_Options.Name = "grp_Options";
			this.grp_Options.TabStop = false;
			// 
			// chk_BreakJoinOnSections
			// 
			resources.ApplyResources(this.chk_BreakJoinOnSections, "chk_BreakJoinOnSections");
			this.chk_BreakJoinOnSections.Name = "chk_BreakJoinOnSections";
			this.chk_BreakJoinOnSections.UseVisualStyleBackColor = true;
			this.chk_BreakJoinOnSections.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_ExpandInLists
			// 
			resources.ApplyResources(this.chk_ExpandInLists, "chk_ExpandInLists");
			this.chk_ExpandInLists.Checked = true;
			this.chk_ExpandInLists.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_ExpandInLists.Name = "chk_ExpandInLists";
			this.chk_ExpandInLists.UseVisualStyleBackColor = true;
			this.chk_ExpandInLists.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_EnableKeywordStandardization
			// 
			resources.ApplyResources(this.chk_EnableKeywordStandardization, "chk_EnableKeywordStandardization");
			this.chk_EnableKeywordStandardization.Name = "chk_EnableKeywordStandardization";
			this.chk_EnableKeywordStandardization.UseVisualStyleBackColor = true;
			this.chk_EnableKeywordStandardization.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// txt_MaxWidth
			// 
			resources.ApplyResources(this.txt_MaxWidth, "txt_MaxWidth");
			this.txt_MaxWidth.Name = "txt_MaxWidth";
			this.txt_MaxWidth.TextChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// lbl_MaxWidth
			// 
			resources.ApplyResources(this.lbl_MaxWidth, "lbl_MaxWidth");
			this.lbl_MaxWidth.Name = "lbl_MaxWidth";
			// 
			// txt_IndentWidth
			// 
			resources.ApplyResources(this.txt_IndentWidth, "txt_IndentWidth");
			this.txt_IndentWidth.Name = "txt_IndentWidth";
			this.txt_IndentWidth.TextChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// lbl_IndentWidth
			// 
			resources.ApplyResources(this.lbl_IndentWidth, "lbl_IndentWidth");
			this.lbl_IndentWidth.Name = "lbl_IndentWidth";
			// 
			// txt_Indent
			// 
			resources.ApplyResources(this.txt_Indent, "txt_Indent");
			this.txt_Indent.Name = "txt_Indent";
			this.txt_Indent.TextChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// lbl_Indent
			// 
			resources.ApplyResources(this.lbl_Indent, "lbl_Indent");
			this.lbl_Indent.Name = "lbl_Indent";
			// 
			// chk_ExpandBetweenConditions
			// 
			resources.ApplyResources(this.chk_ExpandBetweenConditions, "chk_ExpandBetweenConditions");
			this.chk_ExpandBetweenConditions.Checked = true;
			this.chk_ExpandBetweenConditions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_ExpandBetweenConditions.Name = "chk_ExpandBetweenConditions";
			this.chk_ExpandBetweenConditions.UseVisualStyleBackColor = true;
			this.chk_ExpandBetweenConditions.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_SpaceAfterComma
			// 
			resources.ApplyResources(this.chk_SpaceAfterComma, "chk_SpaceAfterComma");
			this.chk_SpaceAfterComma.Name = "chk_SpaceAfterComma";
			this.chk_SpaceAfterComma.UseVisualStyleBackColor = true;
			this.chk_SpaceAfterComma.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_Coloring
			// 
			resources.ApplyResources(this.chk_Coloring, "chk_Coloring");
			this.chk_Coloring.Checked = true;
			this.chk_Coloring.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_Coloring.Name = "chk_Coloring";
			this.chk_Coloring.UseVisualStyleBackColor = true;
			this.chk_Coloring.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_UppercaseKeywords
			// 
			resources.ApplyResources(this.chk_UppercaseKeywords, "chk_UppercaseKeywords");
			this.chk_UppercaseKeywords.Checked = true;
			this.chk_UppercaseKeywords.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_UppercaseKeywords.Name = "chk_UppercaseKeywords";
			this.chk_UppercaseKeywords.UseVisualStyleBackColor = true;
			this.chk_UppercaseKeywords.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_ExpandCaseStatements
			// 
			resources.ApplyResources(this.chk_ExpandCaseStatements, "chk_ExpandCaseStatements");
			this.chk_ExpandCaseStatements.Checked = true;
			this.chk_ExpandCaseStatements.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_ExpandCaseStatements.Name = "chk_ExpandCaseStatements";
			this.chk_ExpandCaseStatements.UseVisualStyleBackColor = true;
			this.chk_ExpandCaseStatements.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_ExpandBooleanExpressions
			// 
			resources.ApplyResources(this.chk_ExpandBooleanExpressions, "chk_ExpandBooleanExpressions");
			this.chk_ExpandBooleanExpressions.Checked = true;
			this.chk_ExpandBooleanExpressions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_ExpandBooleanExpressions.Name = "chk_ExpandBooleanExpressions";
			this.chk_ExpandBooleanExpressions.UseVisualStyleBackColor = true;
			this.chk_ExpandBooleanExpressions.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_TrailingCommas
			// 
			resources.ApplyResources(this.chk_TrailingCommas, "chk_TrailingCommas");
			this.chk_TrailingCommas.Name = "chk_TrailingCommas";
			this.chk_TrailingCommas.UseVisualStyleBackColor = true;
			this.chk_TrailingCommas.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// chk_ExpandCommaLists
			// 
			resources.ApplyResources(this.chk_ExpandCommaLists, "chk_ExpandCommaLists");
			this.chk_ExpandCommaLists.Checked = true;
			this.chk_ExpandCommaLists.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chk_ExpandCommaLists.Name = "chk_ExpandCommaLists";
			this.chk_ExpandCommaLists.UseVisualStyleBackColor = true;
			this.chk_ExpandCommaLists.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// grp_IdentityFormattingOptions
			// 
			this.grp_IdentityFormattingOptions.Controls.Add(this.chk_IdentityColoring);
			resources.ApplyResources(this.grp_IdentityFormattingOptions, "grp_IdentityFormattingOptions");
			this.grp_IdentityFormattingOptions.Name = "grp_IdentityFormattingOptions";
			this.grp_IdentityFormattingOptions.TabStop = false;
			// 
			// chk_IdentityColoring
			// 
			resources.ApplyResources(this.chk_IdentityColoring, "chk_IdentityColoring");
			this.chk_IdentityColoring.Name = "chk_IdentityColoring";
			this.chk_IdentityColoring.UseVisualStyleBackColor = true;
			this.chk_IdentityColoring.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// radio_Formatting_Obfuscate
			// 
			resources.ApplyResources(this.radio_Formatting_Obfuscate, "radio_Formatting_Obfuscate");
			this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Obfuscate, 2);
			this.radio_Formatting_Obfuscate.Name = "radio_Formatting_Obfuscate";
			this.radio_Formatting_Obfuscate.TabStop = true;
			this.radio_Formatting_Obfuscate.UseVisualStyleBackColor = true;
			this.radio_Formatting_Obfuscate.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// grp_ObfuscationOptions
			// 
			this.grp_ObfuscationOptions.Controls.Add(this.chk_KeywordSubstitution);
			this.grp_ObfuscationOptions.Controls.Add(this.chk_PreserveComments);
			this.grp_ObfuscationOptions.Controls.Add(this.chk_RandomizeKeywordCase);
			this.grp_ObfuscationOptions.Controls.Add(this.chk_RandomizeLineLength);
			this.grp_ObfuscationOptions.Controls.Add(this.chk_RandomizeColor);
			resources.ApplyResources(this.grp_ObfuscationOptions, "grp_ObfuscationOptions");
			this.grp_ObfuscationOptions.Name = "grp_ObfuscationOptions";
			this.grp_ObfuscationOptions.TabStop = false;
			// 
			// chk_KeywordSubstitution
			// 
			resources.ApplyResources(this.chk_KeywordSubstitution, "chk_KeywordSubstitution");
			this.chk_KeywordSubstitution.Name = "chk_KeywordSubstitution";
			this.chk_KeywordSubstitution.UseVisualStyleBackColor = true;
			// 
			// chk_PreserveComments
			// 
			resources.ApplyResources(this.chk_PreserveComments, "chk_PreserveComments");
			this.chk_PreserveComments.Name = "chk_PreserveComments";
			this.chk_PreserveComments.UseVisualStyleBackColor = true;
			// 
			// chk_RandomizeKeywordCase
			// 
			resources.ApplyResources(this.chk_RandomizeKeywordCase, "chk_RandomizeKeywordCase");
			this.chk_RandomizeKeywordCase.Name = "chk_RandomizeKeywordCase";
			this.chk_RandomizeKeywordCase.UseVisualStyleBackColor = true;
			// 
			// chk_RandomizeLineLength
			// 
			resources.ApplyResources(this.chk_RandomizeLineLength, "chk_RandomizeLineLength");
			this.chk_RandomizeLineLength.Name = "chk_RandomizeLineLength";
			this.chk_RandomizeLineLength.UseVisualStyleBackColor = true;
			// 
			// chk_RandomizeColor
			// 
			resources.ApplyResources(this.chk_RandomizeColor, "chk_RandomizeColor");
			this.chk_RandomizeColor.Name = "chk_RandomizeColor";
			this.chk_RandomizeColor.UseVisualStyleBackColor = true;
			this.chk_RandomizeColor.CheckedChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// timer_TextChangeDelay
			// 
			this.timer_TextChangeDelay.Interval = 500;
			this.timer_TextChangeDelay.Tick += new System.EventHandler(this.timer_TextChangeDelay_Tick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
			resources.ApplyResources(this.menuStrip1, "menuStrip1");
			this.menuStrip1.Name = "menuStrip1";
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayTokenListToolStripMenuItem,
            this.displayParsedSqlToolStripMenuItem,
            this.displayFormattingOptionsAreaToolStripMenuItem,
            this.languageToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
			// 
			// displayTokenListToolStripMenuItem
			// 
			this.displayTokenListToolStripMenuItem.Checked = true;
			this.displayTokenListToolStripMenuItem.CheckOnClick = true;
			this.displayTokenListToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.displayTokenListToolStripMenuItem.Name = "displayTokenListToolStripMenuItem";
			resources.ApplyResources(this.displayTokenListToolStripMenuItem, "displayTokenListToolStripMenuItem");
			this.displayTokenListToolStripMenuItem.CheckedChanged += new System.EventHandler(this.displaySettingsHandler);
			// 
			// displayParsedSqlToolStripMenuItem
			// 
			this.displayParsedSqlToolStripMenuItem.Checked = true;
			this.displayParsedSqlToolStripMenuItem.CheckOnClick = true;
			this.displayParsedSqlToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.displayParsedSqlToolStripMenuItem.Name = "displayParsedSqlToolStripMenuItem";
			resources.ApplyResources(this.displayParsedSqlToolStripMenuItem, "displayParsedSqlToolStripMenuItem");
			this.displayParsedSqlToolStripMenuItem.CheckedChanged += new System.EventHandler(this.displaySettingsHandler);
			// 
			// displayFormattingOptionsAreaToolStripMenuItem
			// 
			this.displayFormattingOptionsAreaToolStripMenuItem.Checked = true;
			this.displayFormattingOptionsAreaToolStripMenuItem.CheckOnClick = true;
			this.displayFormattingOptionsAreaToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.displayFormattingOptionsAreaToolStripMenuItem.Name = "displayFormattingOptionsAreaToolStripMenuItem";
			resources.ApplyResources(this.displayFormattingOptionsAreaToolStripMenuItem, "displayFormattingOptionsAreaToolStripMenuItem");
			this.displayFormattingOptionsAreaToolStripMenuItem.CheckedChanged += new System.EventHandler(this.displaySettingsHandler);
			// 
			// languageToolStripMenuItem
			// 
			this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.frenchToolStripMenuItem,
            this.spanishToolStripMenuItem});
			this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
			resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
			// 
			// englishToolStripMenuItem
			// 
			this.englishToolStripMenuItem.CheckOnClick = true;
			this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
			resources.ApplyResources(this.englishToolStripMenuItem, "englishToolStripMenuItem");
			this.englishToolStripMenuItem.CheckedChanged += new System.EventHandler(this.languageSettingsHandler);
			// 
			// frenchToolStripMenuItem
			// 
			this.frenchToolStripMenuItem.CheckOnClick = true;
			this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
			resources.ApplyResources(this.frenchToolStripMenuItem, "frenchToolStripMenuItem");
			this.frenchToolStripMenuItem.CheckedChanged += new System.EventHandler(this.languageSettingsHandler);
			// 
			// spanishToolStripMenuItem
			// 
			this.spanishToolStripMenuItem.CheckOnClick = true;
			this.spanishToolStripMenuItem.Name = "spanishToolStripMenuItem";
			resources.ApplyResources(this.spanishToolStripMenuItem, "spanishToolStripMenuItem");
			this.spanishToolStripMenuItem.CheckedChanged += new System.EventHandler(this.languageSettingsHandler);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// lbl_StatementBreaks
			// 
			resources.ApplyResources(this.lbl_StatementBreaks, "lbl_StatementBreaks");
			this.lbl_StatementBreaks.Name = "lbl_StatementBreaks";
			// 
			// txt_StatementBreaks
			// 
			resources.ApplyResources(this.txt_StatementBreaks, "txt_StatementBreaks");
			this.txt_StatementBreaks.Name = "txt_StatementBreaks";
			this.txt_StatementBreaks.TextChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// lbl_ClauseBreaks
			// 
			resources.ApplyResources(this.lbl_ClauseBreaks, "lbl_ClauseBreaks");
			this.lbl_ClauseBreaks.Name = "lbl_ClauseBreaks";
			// 
			// txt_ClauseBreaks
			// 
			resources.ApplyResources(this.txt_ClauseBreaks, "txt_ClauseBreaks");
			this.txt_ClauseBreaks.Name = "txt_ClauseBreaks";
			this.txt_ClauseBreaks.TextChanged += new System.EventHandler(this.FormatSettingsControlChanged);
			// 
			// MainForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer4.Panel1.ResumeLayout(false);
			this.splitContainer4.Panel2.ResumeLayout(false);
			this.splitContainer4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.splitContainer5.Panel1.ResumeLayout(false);
			this.splitContainer5.Panel2.ResumeLayout(false);
			this.splitContainer5.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			this.splitContainer3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.grp_Options.ResumeLayout(false);
			this.grp_Options.PerformLayout();
			this.grp_IdentityFormattingOptions.ResumeLayout(false);
			this.grp_IdentityFormattingOptions.PerformLayout();
			this.grp_ObfuscationOptions.ResumeLayout(false);
			this.grp_ObfuscationOptions.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_Input;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_TokenizedSql;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_ParsedXml;
        private System.Windows.Forms.RadioButton radio_Formatting_Standard;
        private System.Windows.Forms.RadioButton radio_Formatting_Identity;
        private System.Windows.Forms.CheckBox chk_ExpandCommaLists;
        private System.Windows.Forms.GroupBox grp_Options;
        private System.Windows.Forms.CheckBox chk_TrailingCommas;
        private System.Windows.Forms.CheckBox chk_ExpandBooleanExpressions;
        private System.Windows.Forms.CheckBox chk_ExpandCaseStatements;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox chk_UppercaseKeywords;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.CustomContentWebBrowser webBrowser_OutputSql;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chk_Coloring;
        private System.Windows.Forms.Timer timer_TextChangeDelay;
        private System.Windows.Forms.CheckBox chk_SpaceAfterComma;
        private System.Windows.Forms.CheckBox chk_ExpandBetweenConditions;
        private System.Windows.Forms.TextBox txt_MaxWidth;
        private System.Windows.Forms.Label lbl_MaxWidth;
        private System.Windows.Forms.TextBox txt_IndentWidth;
        private System.Windows.Forms.Label lbl_IndentWidth;
        private System.Windows.Forms.TextBox txt_Indent;
        private System.Windows.Forms.Label lbl_Indent;
        private System.Windows.Forms.GroupBox grp_IdentityFormattingOptions;
        private System.Windows.Forms.CheckBox chk_IdentityColoring;
        private System.Windows.Forms.CheckBox chk_EnableKeywordStandardization;
        private System.Windows.Forms.CheckBox chk_BreakJoinOnSections;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayTokenListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayParsedSqlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayFormattingOptionsAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private FrameworkClassReplacements.RadioToolStripMenuItem englishToolStripMenuItem;
        private FrameworkClassReplacements.RadioToolStripMenuItem frenchToolStripMenuItem;
        private FrameworkClassReplacements.RadioToolStripMenuItem spanishToolStripMenuItem;
        private System.Windows.Forms.RadioButton radio_Formatting_Obfuscate;
        private System.Windows.Forms.GroupBox grp_ObfuscationOptions;
        private System.Windows.Forms.CheckBox chk_RandomizeColor;
        private System.Windows.Forms.CheckBox chk_PreserveComments;
        private System.Windows.Forms.CheckBox chk_RandomizeKeywordCase;
        private System.Windows.Forms.CheckBox chk_RandomizeLineLength;
        private System.Windows.Forms.CheckBox chk_KeywordSubstitution;
		private System.Windows.Forms.CheckBox chk_ExpandInLists;
		private System.Windows.Forms.TextBox txt_ClauseBreaks;
		private System.Windows.Forms.Label lbl_ClauseBreaks;
		private System.Windows.Forms.TextBox txt_StatementBreaks;
		private System.Windows.Forms.Label lbl_StatementBreaks;
    }
}

