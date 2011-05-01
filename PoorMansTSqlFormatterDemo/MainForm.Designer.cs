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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txt_Input = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.txt_TokenizedXml = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.txt_ParsedXml = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.txt_OutputSql = new PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox();
            this.radio_Formatting_Standard = new System.Windows.Forms.RadioButton();
            this.radio_Formatting_Identity = new System.Windows.Forms.RadioButton();
            this.grp_Options = new System.Windows.Forms.GroupBox();
            this.chk_ExpandCaseStatements = new System.Windows.Forms.CheckBox();
            this.chk_ExpandBooleanExpressions = new System.Windows.Forms.CheckBox();
            this.chk_TrailingCommas = new System.Windows.Forms.CheckBox();
            this.chk_ExpandCommaLists = new System.Windows.Forms.CheckBox();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chk_UppercaseKeywords = new System.Windows.Forms.CheckBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grp_Options.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(987, 513);
            this.splitContainer1.SplitterDistance = 214;
            this.splitContainer1.TabIndex = 0;
            // 
            // txt_Input
            // 
            this.txt_Input.AcceptsReturn = true;
            this.txt_Input.AcceptsTab = true;
            this.txt_Input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Input.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txt_Input.Location = new System.Drawing.Point(0, 0);
            this.txt_Input.Multiline = true;
            this.txt_Input.Name = "txt_Input";
            this.txt_Input.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Input.Size = new System.Drawing.Size(531, 214);
            this.txt_Input.TabIndex = 0;
            this.txt_Input.WordWrap = false;
            this.txt_Input.Leave += new System.EventHandler(this.txt_Input_Leave);
            // 
            // txt_TokenizedXml
            // 
            this.txt_TokenizedXml.AcceptsReturn = true;
            this.txt_TokenizedXml.AcceptsTab = true;
            this.txt_TokenizedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_TokenizedXml.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txt_TokenizedXml.Location = new System.Drawing.Point(0, 0);
            this.txt_TokenizedXml.Multiline = true;
            this.txt_TokenizedXml.Name = "txt_TokenizedXml";
            this.txt_TokenizedXml.ReadOnly = true;
            this.txt_TokenizedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_TokenizedXml.Size = new System.Drawing.Size(452, 104);
            this.txt_TokenizedXml.TabIndex = 0;
            this.txt_TokenizedXml.WordWrap = false;
            // 
            // txt_ParsedXml
            // 
            this.txt_ParsedXml.AcceptsReturn = true;
            this.txt_ParsedXml.AcceptsTab = true;
            this.txt_ParsedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_ParsedXml.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txt_ParsedXml.Location = new System.Drawing.Point(0, 0);
            this.txt_ParsedXml.Multiline = true;
            this.txt_ParsedXml.Name = "txt_ParsedXml";
            this.txt_ParsedXml.ReadOnly = true;
            this.txt_ParsedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_ParsedXml.Size = new System.Drawing.Size(452, 106);
            this.txt_ParsedXml.TabIndex = 0;
            this.txt_ParsedXml.WordWrap = false;
            // 
            // txt_OutputSql
            // 
            this.txt_OutputSql.AcceptsReturn = true;
            this.txt_OutputSql.AcceptsTab = true;
            this.txt_OutputSql.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_OutputSql.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txt_OutputSql.Location = new System.Drawing.Point(0, 0);
            this.txt_OutputSql.Multiline = true;
            this.txt_OutputSql.Name = "txt_OutputSql";
            this.txt_OutputSql.ReadOnly = true;
            this.txt_OutputSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_OutputSql.Size = new System.Drawing.Size(746, 295);
            this.txt_OutputSql.TabIndex = 0;
            this.txt_OutputSql.WordWrap = false;
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
            this.radio_Formatting_Standard.CheckedChanged += new System.EventHandler(this.radio_Formatting_Standard_CheckedChanged);
            // 
            // radio_Formatting_Identity
            // 
            this.radio_Formatting_Identity.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.radio_Formatting_Identity, 2);
            this.radio_Formatting_Identity.Location = new System.Drawing.Point(3, 188);
            this.radio_Formatting_Identity.Name = "radio_Formatting_Identity";
            this.radio_Formatting_Identity.Size = new System.Drawing.Size(159, 17);
            this.radio_Formatting_Identity.TabIndex = 2;
            this.radio_Formatting_Identity.Text = "Identity (mirroring) Formatting";
            this.radio_Formatting_Identity.UseVisualStyleBackColor = true;
            this.radio_Formatting_Identity.CheckedChanged += new System.EventHandler(this.radio_Formatting_Identity_CheckedChanged);
            // 
            // grp_Options
            // 
            this.grp_Options.Controls.Add(this.chk_UppercaseKeywords);
            this.grp_Options.Controls.Add(this.chk_ExpandCaseStatements);
            this.grp_Options.Controls.Add(this.chk_ExpandBooleanExpressions);
            this.grp_Options.Controls.Add(this.chk_TrailingCommas);
            this.grp_Options.Controls.Add(this.chk_ExpandCommaLists);
            this.grp_Options.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grp_Options.Location = new System.Drawing.Point(53, 28);
            this.grp_Options.Name = "grp_Options";
            this.grp_Options.Size = new System.Drawing.Size(181, 154);
            this.grp_Options.TabIndex = 4;
            this.grp_Options.TabStop = false;
            this.grp_Options.Text = "Options";
            // 
            // chk_ExpandCaseStatements
            // 
            this.chk_ExpandCaseStatements.AutoSize = true;
            this.chk_ExpandCaseStatements.Checked = true;
            this.chk_ExpandCaseStatements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandCaseStatements.Location = new System.Drawing.Point(6, 88);
            this.chk_ExpandCaseStatements.Name = "chk_ExpandCaseStatements";
            this.chk_ExpandCaseStatements.Size = new System.Drawing.Size(149, 17);
            this.chk_ExpandCaseStatements.TabIndex = 6;
            this.chk_ExpandCaseStatements.Text = "Expand CASE Statements";
            this.chk_ExpandCaseStatements.UseVisualStyleBackColor = true;
            this.chk_ExpandCaseStatements.CheckedChanged += new System.EventHandler(this.chk_ExpandCaseStatements_CheckedChanged);
            // 
            // chk_ExpandBooleanExpressions
            // 
            this.chk_ExpandBooleanExpressions.AutoSize = true;
            this.chk_ExpandBooleanExpressions.Checked = true;
            this.chk_ExpandBooleanExpressions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandBooleanExpressions.Location = new System.Drawing.Point(6, 65);
            this.chk_ExpandBooleanExpressions.Name = "chk_ExpandBooleanExpressions";
            this.chk_ExpandBooleanExpressions.Size = new System.Drawing.Size(163, 17);
            this.chk_ExpandBooleanExpressions.TabIndex = 5;
            this.chk_ExpandBooleanExpressions.Text = "Expand Boolean Expressions";
            this.chk_ExpandBooleanExpressions.UseVisualStyleBackColor = true;
            this.chk_ExpandBooleanExpressions.CheckedChanged += new System.EventHandler(this.chk_ExpandBooleanExpressions_CheckedChanged);
            // 
            // chk_TrailingCommas
            // 
            this.chk_TrailingCommas.AutoSize = true;
            this.chk_TrailingCommas.Location = new System.Drawing.Point(39, 42);
            this.chk_TrailingCommas.Name = "chk_TrailingCommas";
            this.chk_TrailingCommas.Size = new System.Drawing.Size(103, 17);
            this.chk_TrailingCommas.TabIndex = 4;
            this.chk_TrailingCommas.Text = "Trailing Commas";
            this.chk_TrailingCommas.UseVisualStyleBackColor = true;
            this.chk_TrailingCommas.CheckedChanged += new System.EventHandler(this.chk_TrailingCommas_CheckedChanged);
            // 
            // chk_ExpandCommaLists
            // 
            this.chk_ExpandCommaLists.AutoSize = true;
            this.chk_ExpandCommaLists.Checked = true;
            this.chk_ExpandCommaLists.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExpandCommaLists.Location = new System.Drawing.Point(6, 19);
            this.chk_ExpandCommaLists.Name = "chk_ExpandCommaLists";
            this.chk_ExpandCommaLists.Size = new System.Drawing.Size(124, 17);
            this.chk_ExpandCommaLists.TabIndex = 3;
            this.chk_ExpandCommaLists.Text = "Expand Comma Lists";
            this.chk_ExpandCommaLists.UseVisualStyleBackColor = true;
            this.chk_ExpandCommaLists.CheckedChanged += new System.EventHandler(this.chk_ExpandParens_CheckedChanged);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.txt_Input);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer4.Size = new System.Drawing.Size(987, 214);
            this.splitContainer4.SplitterDistance = 531;
            this.splitContainer4.TabIndex = 1;
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
            this.splitContainer5.Panel1.Controls.Add(this.txt_TokenizedXml);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.txt_ParsedXml);
            this.splitContainer5.Size = new System.Drawing.Size(452, 214);
            this.splitContainer5.SplitterDistance = 104;
            this.splitContainer5.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.txt_OutputSql);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer3.Size = new System.Drawing.Size(987, 295);
            this.splitContainer3.SplitterDistance = 746;
            this.splitContainer3.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Identity, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.radio_Formatting_Standard, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.grp_Options, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(237, 295);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // chk_UppercaseKeywords
            // 
            this.chk_UppercaseKeywords.AutoSize = true;
            this.chk_UppercaseKeywords.Checked = true;
            this.chk_UppercaseKeywords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_UppercaseKeywords.Location = new System.Drawing.Point(6, 111);
            this.chk_UppercaseKeywords.Name = "chk_UppercaseKeywords";
            this.chk_UppercaseKeywords.Size = new System.Drawing.Size(127, 17);
            this.chk_UppercaseKeywords.TabIndex = 7;
            this.chk_UppercaseKeywords.Text = "Uppercase Keywords";
            this.chk_UppercaseKeywords.UseVisualStyleBackColor = true;
            this.chk_UppercaseKeywords.CheckedChanged += new System.EventHandler(this.chk_UppercaseKeywords_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 513);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "SQL Formatter";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.grp_Options.ResumeLayout(false);
            this.grp_Options.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_Input;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_TokenizedXml;
        private PoorMansTSqlFormatterDemo.FrameworkClassReplacements.SelectableTextBox txt_OutputSql;
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
    }
}

