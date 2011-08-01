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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txt_Input = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txt_TokenizedXml = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txt_ParsedXml = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.webBrowser_OutputSql = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.CustomContentWebBrowser();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.radio_Formatting_Identity = new System.Windows.Forms.RadioButton();
            this.radio_Formatting_Standard = new System.Windows.Forms.RadioButton();
            this.grp_Options = new System.Windows.Forms.GroupBox();
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
            this.btn_About = new System.Windows.Forms.Button();
            this.timer_TextChangeDelay = new System.Windows.Forms.Timer(this.components);
            this.chk_EnableKeywordStandardization = new System.Windows.Forms.CheckBox();
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
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.grp_Options.SuspendLayout();
            this.grp_IdentityFormattingOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(991, 693);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer4.Size = new System.Drawing.Size(991, 236);
            this.splitContainer4.SplitterDistance = 533;
            this.splitContainer4.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txt_Input);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(533, 236);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input SQL";
            // 
            // txt_Input
            // 
            this.txt_Input.AcceptsReturn = true;
            this.txt_Input.AcceptsTab = true;
            this.txt_Input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Input.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Input.Location = new System.Drawing.Point(3, 16);
            this.txt_Input.MaxLength = 1000000;
            this.txt_Input.Multiline = true;
            this.txt_Input.Name = "txt_Input";
            this.txt_Input.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Input.Size = new System.Drawing.Size(527, 217);
            this.txt_Input.TabIndex = 0;
            this.txt_Input.WordWrap = false;
            this.txt_Input.TextChanged += new System.EventHandler(this.txt_Input_TextChanged);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer5.Size = new System.Drawing.Size(454, 236);
            this.splitContainer5.SplitterDistance = 114;
            this.splitContainer5.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txt_TokenizedXml);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(454, 114);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Token List";
            // 
            // txt_TokenizedXml
            // 
            this.txt_TokenizedXml.AcceptsReturn = true;
            this.txt_TokenizedXml.AcceptsTab = true;
            this.txt_TokenizedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_TokenizedXml.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txt_TokenizedXml.Location = new System.Drawing.Point(3, 16);
            this.txt_TokenizedXml.Multiline = true;
            this.txt_TokenizedXml.Name = "txt_TokenizedXml";
            this.txt_TokenizedXml.ReadOnly = true;
            this.txt_TokenizedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_TokenizedXml.Size = new System.Drawing.Size(448, 95);
            this.txt_TokenizedXml.TabIndex = 0;
            this.txt_TokenizedXml.WordWrap = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txt_ParsedXml);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(454, 118);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Parse Tree";
            // 
            // txt_ParsedXml
            // 
            this.txt_ParsedXml.AcceptsReturn = true;
            this.txt_ParsedXml.AcceptsTab = true;
            this.txt_ParsedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_ParsedXml.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txt_ParsedXml.Location = new System.Drawing.Point(3, 16);
            this.txt_ParsedXml.Multiline = true;
            this.txt_ParsedXml.Name = "txt_ParsedXml";
            this.txt_ParsedXml.ReadOnly = true;
            this.txt_ParsedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_ParsedXml.Size = new System.Drawing.Size(448, 99);
            this.txt_ParsedXml.TabIndex = 0;
            this.txt_ParsedXml.WordWrap = false;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer3.Size = new System.Drawing.Size(991, 453);
            this.splitContainer3.SplitterDistance = 730;
            this.splitContainer3.TabIndex = 2;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.panel1);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(730, 453);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output SQL";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.webBrowser_OutputSql);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(724, 434);
            this.panel1.TabIndex = 2;
            // 
            // webBrowser_OutputSql
            // 
            this.webBrowser_OutputSql.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_OutputSql.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_OutputSql.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_OutputSql.Name = "webBrowser_OutputSql";
            this.webBrowser_OutputSql.ScriptErrorsSuppressed = true;
            this.webBrowser_OutputSql.Size = new System.Drawing.Size(720, 430);
            this.webBrowser_OutputSql.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn_About, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(257, 453);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tableLayoutPanel2);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(3, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(251, 417);
            this.groupBox5.TabIndex = 1;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Options";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Identity, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Standard, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.grp_Options, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.grp_IdentityFormattingOptions, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 285F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(245, 398);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // radio_Formatting_Identity
            // 
            this.radio_Formatting_Identity.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Identity, 2);
            this.radio_Formatting_Identity.Location = new System.Drawing.Point(3, 313);
            this.radio_Formatting_Identity.Name = "radio_Formatting_Identity";
            this.radio_Formatting_Identity.Size = new System.Drawing.Size(159, 17);
            this.radio_Formatting_Identity.TabIndex = 2;
            this.radio_Formatting_Identity.Text = "Identity (mirroring) Formatting";
            this.radio_Formatting_Identity.UseVisualStyleBackColor = true;
            this.radio_Formatting_Identity.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // radio_Formatting_Standard
            // 
            this.radio_Formatting_Standard.AutoSize = true;
            this.radio_Formatting_Standard.Checked = true;
            this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Standard, 2);
            this.radio_Formatting_Standard.Location = new System.Drawing.Point(3, 3);
            this.radio_Formatting_Standard.Name = "radio_Formatting_Standard";
            this.radio_Formatting_Standard.Size = new System.Drawing.Size(120, 17);
            this.radio_Formatting_Standard.TabIndex = 1;
            this.radio_Formatting_Standard.TabStop = true;
            this.radio_Formatting_Standard.Text = "Standard Formatting";
            this.radio_Formatting_Standard.UseVisualStyleBackColor = true;
            this.radio_Formatting_Standard.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // grp_Options
            // 
            this.grp_Options.Controls.Add(this.chk_EnableKeywordStandardization);
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
            this.grp_Options.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grp_Options.Location = new System.Drawing.Point(43, 28);
            this.grp_Options.Name = "grp_Options";
            this.grp_Options.Size = new System.Drawing.Size(199, 279);
            this.grp_Options.TabIndex = 4;
            this.grp_Options.TabStop = false;
            this.grp_Options.Text = "Standard Formatting Options";
            // 
            // txt_MaxWidth
            // 
            this.txt_MaxWidth.Location = new System.Drawing.Point(77, 46);
            this.txt_MaxWidth.Name = "txt_MaxWidth";
            this.txt_MaxWidth.Size = new System.Drawing.Size(77, 20);
            this.txt_MaxWidth.TabIndex = 16;
            this.txt_MaxWidth.Text = "999";
            this.txt_MaxWidth.TextChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // lbl_MaxWidth
            // 
            this.lbl_MaxWidth.AutoSize = true;
            this.lbl_MaxWidth.Location = new System.Drawing.Point(7, 49);
            this.lbl_MaxWidth.Name = "lbl_MaxWidth";
            this.lbl_MaxWidth.Size = new System.Drawing.Size(61, 13);
            this.lbl_MaxWidth.TabIndex = 15;
            this.lbl_MaxWidth.Text = "Max Width:";
            // 
            // txt_IndentWidth
            // 
            this.txt_IndentWidth.Location = new System.Drawing.Point(160, 20);
            this.txt_IndentWidth.Name = "txt_IndentWidth";
            this.txt_IndentWidth.Size = new System.Drawing.Size(33, 20);
            this.txt_IndentWidth.TabIndex = 14;
            this.txt_IndentWidth.Text = "4";
            this.txt_IndentWidth.TextChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // lbl_IndentWidth
            // 
            this.lbl_IndentWidth.AutoSize = true;
            this.lbl_IndentWidth.Location = new System.Drawing.Point(83, 23);
            this.lbl_IndentWidth.Name = "lbl_IndentWidth";
            this.lbl_IndentWidth.Size = new System.Drawing.Size(71, 13);
            this.lbl_IndentWidth.TabIndex = 13;
            this.lbl_IndentWidth.Text = "Indent Width:";
            // 
            // txt_Indent
            // 
            this.txt_Indent.Location = new System.Drawing.Point(47, 20);
            this.txt_Indent.Name = "txt_Indent";
            this.txt_Indent.Size = new System.Drawing.Size(33, 20);
            this.txt_Indent.TabIndex = 12;
            this.txt_Indent.Text = "\\t";
            this.txt_Indent.TextChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // lbl_Indent
            // 
            this.lbl_Indent.AutoSize = true;
            this.lbl_Indent.Location = new System.Drawing.Point(6, 23);
            this.lbl_Indent.Name = "lbl_Indent";
            this.lbl_Indent.Size = new System.Drawing.Size(40, 13);
            this.lbl_Indent.TabIndex = 11;
            this.lbl_Indent.Text = "Indent:";
            // 
            // chk_ExpandBetweenConditions
            // 
            this.chk_ExpandBetweenConditions.AutoSize = true;
            this.chk_ExpandBetweenConditions.Checked = true;
            this.chk_ExpandBetweenConditions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandBetweenConditions.Location = new System.Drawing.Point(6, 187);
            this.chk_ExpandBetweenConditions.Name = "chk_ExpandBetweenConditions";
            this.chk_ExpandBetweenConditions.Size = new System.Drawing.Size(171, 17);
            this.chk_ExpandBetweenConditions.TabIndex = 10;
            this.chk_ExpandBetweenConditions.Text = "Expand BETWEEN Conditions";
            this.chk_ExpandBetweenConditions.UseVisualStyleBackColor = true;
            this.chk_ExpandBetweenConditions.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_SpaceAfterComma
            // 
            this.chk_SpaceAfterComma.AutoSize = true;
            this.chk_SpaceAfterComma.Location = new System.Drawing.Point(39, 118);
            this.chk_SpaceAfterComma.Name = "chk_SpaceAfterComma";
            this.chk_SpaceAfterComma.Size = new System.Drawing.Size(120, 17);
            this.chk_SpaceAfterComma.TabIndex = 9;
            this.chk_SpaceAfterComma.Text = "Space After Comma";
            this.chk_SpaceAfterComma.UseVisualStyleBackColor = true;
            this.chk_SpaceAfterComma.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_Coloring
            // 
            this.chk_Coloring.AutoSize = true;
            this.chk_Coloring.Checked = true;
            this.chk_Coloring.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Coloring.Location = new System.Drawing.Point(6, 233);
            this.chk_Coloring.Name = "chk_Coloring";
            this.chk_Coloring.Size = new System.Drawing.Size(100, 17);
            this.chk_Coloring.TabIndex = 8;
            this.chk_Coloring.Text = "Enable Coloring";
            this.chk_Coloring.UseVisualStyleBackColor = true;
            this.chk_Coloring.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_UppercaseKeywords
            // 
            this.chk_UppercaseKeywords.AutoSize = true;
            this.chk_UppercaseKeywords.Checked = true;
            this.chk_UppercaseKeywords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_UppercaseKeywords.Location = new System.Drawing.Point(6, 210);
            this.chk_UppercaseKeywords.Name = "chk_UppercaseKeywords";
            this.chk_UppercaseKeywords.Size = new System.Drawing.Size(127, 17);
            this.chk_UppercaseKeywords.TabIndex = 7;
            this.chk_UppercaseKeywords.Text = "Uppercase Keywords";
            this.chk_UppercaseKeywords.UseVisualStyleBackColor = true;
            this.chk_UppercaseKeywords.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_ExpandCaseStatements
            // 
            this.chk_ExpandCaseStatements.AutoSize = true;
            this.chk_ExpandCaseStatements.Checked = true;
            this.chk_ExpandCaseStatements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandCaseStatements.Location = new System.Drawing.Point(6, 164);
            this.chk_ExpandCaseStatements.Name = "chk_ExpandCaseStatements";
            this.chk_ExpandCaseStatements.Size = new System.Drawing.Size(149, 17);
            this.chk_ExpandCaseStatements.TabIndex = 6;
            this.chk_ExpandCaseStatements.Text = "Expand CASE Statements";
            this.chk_ExpandCaseStatements.UseVisualStyleBackColor = true;
            this.chk_ExpandCaseStatements.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_ExpandBooleanExpressions
            // 
            this.chk_ExpandBooleanExpressions.AutoSize = true;
            this.chk_ExpandBooleanExpressions.Checked = true;
            this.chk_ExpandBooleanExpressions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandBooleanExpressions.Location = new System.Drawing.Point(6, 141);
            this.chk_ExpandBooleanExpressions.Name = "chk_ExpandBooleanExpressions";
            this.chk_ExpandBooleanExpressions.Size = new System.Drawing.Size(163, 17);
            this.chk_ExpandBooleanExpressions.TabIndex = 5;
            this.chk_ExpandBooleanExpressions.Text = "Expand Boolean Expressions";
            this.chk_ExpandBooleanExpressions.UseVisualStyleBackColor = true;
            this.chk_ExpandBooleanExpressions.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_TrailingCommas
            // 
            this.chk_TrailingCommas.AutoSize = true;
            this.chk_TrailingCommas.Location = new System.Drawing.Point(39, 95);
            this.chk_TrailingCommas.Name = "chk_TrailingCommas";
            this.chk_TrailingCommas.Size = new System.Drawing.Size(103, 17);
            this.chk_TrailingCommas.TabIndex = 4;
            this.chk_TrailingCommas.Text = "Trailing Commas";
            this.chk_TrailingCommas.UseVisualStyleBackColor = true;
            this.chk_TrailingCommas.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // chk_ExpandCommaLists
            // 
            this.chk_ExpandCommaLists.AutoSize = true;
            this.chk_ExpandCommaLists.Checked = true;
            this.chk_ExpandCommaLists.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandCommaLists.Location = new System.Drawing.Point(6, 72);
            this.chk_ExpandCommaLists.Name = "chk_ExpandCommaLists";
            this.chk_ExpandCommaLists.Size = new System.Drawing.Size(124, 17);
            this.chk_ExpandCommaLists.TabIndex = 3;
            this.chk_ExpandCommaLists.Text = "Expand Comma Lists";
            this.chk_ExpandCommaLists.UseVisualStyleBackColor = true;
            this.chk_ExpandCommaLists.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // grp_IdentityFormattingOptions
            // 
            this.grp_IdentityFormattingOptions.Controls.Add(this.chk_IdentityColoring);
            this.grp_IdentityFormattingOptions.Location = new System.Drawing.Point(43, 338);
            this.grp_IdentityFormattingOptions.Name = "grp_IdentityFormattingOptions";
            this.grp_IdentityFormattingOptions.Size = new System.Drawing.Size(199, 39);
            this.grp_IdentityFormattingOptions.TabIndex = 5;
            this.grp_IdentityFormattingOptions.TabStop = false;
            this.grp_IdentityFormattingOptions.Text = "Identity Formatting Options";
            // 
            // chk_IdentityColoring
            // 
            this.chk_IdentityColoring.AutoSize = true;
            this.chk_IdentityColoring.Location = new System.Drawing.Point(6, 19);
            this.chk_IdentityColoring.Name = "chk_IdentityColoring";
            this.chk_IdentityColoring.Size = new System.Drawing.Size(100, 17);
            this.chk_IdentityColoring.TabIndex = 5;
            this.chk_IdentityColoring.Text = "Enable Coloring";
            this.chk_IdentityColoring.UseVisualStyleBackColor = true;
            this.chk_IdentityColoring.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // btn_About
            // 
            this.btn_About.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_About.Location = new System.Drawing.Point(3, 426);
            this.btn_About.Name = "btn_About";
            this.btn_About.Size = new System.Drawing.Size(251, 24);
            this.btn_About.TabIndex = 2;
            this.btn_About.Text = "About / License...";
            this.btn_About.UseVisualStyleBackColor = true;
            this.btn_About.Click += new System.EventHandler(this.btn_About_Click);
            // 
            // timer_TextChangeDelay
            // 
            this.timer_TextChangeDelay.Interval = 500;
            this.timer_TextChangeDelay.Tick += new System.EventHandler(this.timer_TextChangeDelay_Tick);
            // 
            // chk_EnableKeywordStandardization
            // 
            this.chk_EnableKeywordStandardization.AutoSize = true;
            this.chk_EnableKeywordStandardization.Location = new System.Drawing.Point(6, 256);
            this.chk_EnableKeywordStandardization.Name = "chk_EnableKeywordStandardization";
            this.chk_EnableKeywordStandardization.Size = new System.Drawing.Size(179, 17);
            this.chk_EnableKeywordStandardization.TabIndex = 17;
            this.chk_EnableKeywordStandardization.Text = "Enable Keyword Standardization";
            this.chk_EnableKeywordStandardization.UseVisualStyleBackColor = true;
            this.chk_EnableKeywordStandardization.CheckedChanged += new System.EventHandler(this.SettingsControlChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(991, 693);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "SQL Formatter";
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
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.grp_Options.ResumeLayout(false);
            this.grp_Options.PerformLayout();
            this.grp_IdentityFormattingOptions.ResumeLayout(false);
            this.grp_IdentityFormattingOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_Input;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_TokenizedXml;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btn_About;
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
    }
}

