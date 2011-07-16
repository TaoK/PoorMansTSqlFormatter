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

namespace PoorMansTSqlFormatterSSMSAddIn
{
    partial class SettingsForm
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
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chk_ExpandCommaLists = new System.Windows.Forms.CheckBox();
            this.chk_TrailingCommas = new System.Windows.Forms.CheckBox();
            this.chk_ExpandBooleanExpressions = new System.Windows.Forms.CheckBox();
            this.chk_ExpandCaseStatements = new System.Windows.Forms.CheckBox();
            this.chk_ExpandBetweenConditions = new System.Windows.Forms.CheckBox();
            this.chk_UppercaseKeywords = new System.Windows.Forms.CheckBox();
            this.txt_IndentString = new System.Windows.Forms.TextBox();
            this.lbl_IndentString = new System.Windows.Forms.Label();
            this.lbl_IndentHint = new System.Windows.Forms.Label();
            this.txt_Hotkey = new System.Windows.Forms.TextBox();
            this.lbl_Hotkey = new System.Windows.Forms.Label();
            this.llbl_HotkeyHint = new System.Windows.Forms.LinkLabel();
            this.btn_About = new System.Windows.Forms.Button();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.txt_SpacesPerTab = new System.Windows.Forms.TextBox();
            this.txt_MaxLineWidth = new System.Windows.Forms.TextBox();
            this.lbl_SpacesPerTab = new System.Windows.Forms.Label();
            this.lbl_SpacesPerTab_Extra = new System.Windows.Forms.Label();
            this.lbl_MaxLineWidth = new System.Windows.Forms.Label();
            this.chk_SpaceAfterExpandedComma = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Save
            // 
            this.btn_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Save.Location = new System.Drawing.Point(215, 240);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 23);
            this.btn_Save.TabIndex = 0;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(296, 240);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.chk_ExpandCommaLists);
            this.flowLayoutPanel1.Controls.Add(this.chk_TrailingCommas);
            this.flowLayoutPanel1.Controls.Add(this.chk_ExpandBooleanExpressions);
            this.flowLayoutPanel1.Controls.Add(this.chk_ExpandCaseStatements);
            this.flowLayoutPanel1.Controls.Add(this.chk_ExpandBetweenConditions);
            this.flowLayoutPanel1.Controls.Add(this.chk_UppercaseKeywords);
            this.flowLayoutPanel1.Controls.Add(this.chk_SpaceAfterExpandedComma);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 123);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(359, 111);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // chk_ExpandCommaLists
            // 
            this.chk_ExpandCommaLists.AutoSize = true;
            this.chk_ExpandCommaLists.Location = new System.Drawing.Point(3, 3);
            this.chk_ExpandCommaLists.Name = "chk_ExpandCommaLists";
            this.chk_ExpandCommaLists.Size = new System.Drawing.Size(124, 17);
            this.chk_ExpandCommaLists.TabIndex = 2;
            this.chk_ExpandCommaLists.Text = "Expand Comma Lists";
            this.chk_ExpandCommaLists.UseVisualStyleBackColor = true;
            // 
            // chk_TrailingCommas
            // 
            this.chk_TrailingCommas.AutoSize = true;
            this.chk_TrailingCommas.Location = new System.Drawing.Point(133, 3);
            this.chk_TrailingCommas.Name = "chk_TrailingCommas";
            this.chk_TrailingCommas.Size = new System.Drawing.Size(103, 17);
            this.chk_TrailingCommas.TabIndex = 4;
            this.chk_TrailingCommas.Text = "Trailing Commas";
            this.chk_TrailingCommas.UseVisualStyleBackColor = true;
            // 
            // chk_ExpandBooleanExpressions
            // 
            this.chk_ExpandBooleanExpressions.AutoSize = true;
            this.chk_ExpandBooleanExpressions.Location = new System.Drawing.Point(3, 26);
            this.chk_ExpandBooleanExpressions.Name = "chk_ExpandBooleanExpressions";
            this.chk_ExpandBooleanExpressions.Size = new System.Drawing.Size(163, 17);
            this.chk_ExpandBooleanExpressions.TabIndex = 5;
            this.chk_ExpandBooleanExpressions.Text = "Expand Boolean Expressions";
            this.chk_ExpandBooleanExpressions.UseVisualStyleBackColor = true;
            // 
            // chk_ExpandCaseStatements
            // 
            this.chk_ExpandCaseStatements.AutoSize = true;
            this.chk_ExpandCaseStatements.Location = new System.Drawing.Point(172, 26);
            this.chk_ExpandCaseStatements.Name = "chk_ExpandCaseStatements";
            this.chk_ExpandCaseStatements.Size = new System.Drawing.Size(145, 17);
            this.chk_ExpandCaseStatements.TabIndex = 6;
            this.chk_ExpandCaseStatements.Text = "Expand Case Statements";
            this.chk_ExpandCaseStatements.UseVisualStyleBackColor = true;
            // 
            // chk_ExpandBetweenConditions
            // 
            this.chk_ExpandBetweenConditions.AutoSize = true;
            this.chk_ExpandBetweenConditions.Location = new System.Drawing.Point(3, 49);
            this.chk_ExpandBetweenConditions.Name = "chk_ExpandBetweenConditions";
            this.chk_ExpandBetweenConditions.Size = new System.Drawing.Size(159, 17);
            this.chk_ExpandBetweenConditions.TabIndex = 7;
            this.chk_ExpandBetweenConditions.Text = "Expand Between Conditions";
            this.chk_ExpandBetweenConditions.UseVisualStyleBackColor = true;
            // 
            // chk_UppercaseKeywords
            // 
            this.chk_UppercaseKeywords.AutoSize = true;
            this.chk_UppercaseKeywords.Location = new System.Drawing.Point(168, 49);
            this.chk_UppercaseKeywords.Name = "chk_UppercaseKeywords";
            this.chk_UppercaseKeywords.Size = new System.Drawing.Size(127, 17);
            this.chk_UppercaseKeywords.TabIndex = 8;
            this.chk_UppercaseKeywords.Text = "Uppercase Keywords";
            this.chk_UppercaseKeywords.UseVisualStyleBackColor = true;
            // 
            // txt_IndentString
            // 
            this.txt_IndentString.Location = new System.Drawing.Point(99, 12);
            this.txt_IndentString.Name = "txt_IndentString";
            this.txt_IndentString.Size = new System.Drawing.Size(128, 20);
            this.txt_IndentString.TabIndex = 0;
            // 
            // lbl_IndentString
            // 
            this.lbl_IndentString.AutoSize = true;
            this.lbl_IndentString.Location = new System.Drawing.Point(12, 15);
            this.lbl_IndentString.Name = "lbl_IndentString";
            this.lbl_IndentString.Size = new System.Drawing.Size(70, 13);
            this.lbl_IndentString.TabIndex = 1;
            this.lbl_IndentString.Text = "Indent String:";
            // 
            // lbl_IndentHint
            // 
            this.lbl_IndentHint.AutoSize = true;
            this.lbl_IndentHint.Location = new System.Drawing.Point(233, 15);
            this.lbl_IndentHint.Name = "lbl_IndentHint";
            this.lbl_IndentHint.Size = new System.Drawing.Size(74, 13);
            this.lbl_IndentHint.TabIndex = 3;
            this.lbl_IndentHint.Text = "(use \\t for tab)";
            // 
            // txt_Hotkey
            // 
            this.txt_Hotkey.Location = new System.Drawing.Point(99, 38);
            this.txt_Hotkey.Name = "txt_Hotkey";
            this.txt_Hotkey.Size = new System.Drawing.Size(128, 20);
            this.txt_Hotkey.TabIndex = 9;
            // 
            // lbl_Hotkey
            // 
            this.lbl_Hotkey.AutoSize = true;
            this.lbl_Hotkey.Location = new System.Drawing.Point(12, 41);
            this.lbl_Hotkey.Name = "lbl_Hotkey";
            this.lbl_Hotkey.Size = new System.Drawing.Size(78, 13);
            this.lbl_Hotkey.TabIndex = 10;
            this.lbl_Hotkey.Text = "SSMS HotKey:";
            // 
            // llbl_HotkeyHint
            // 
            this.llbl_HotkeyHint.AutoSize = true;
            this.llbl_HotkeyHint.LinkArea = new System.Windows.Forms.LinkArea(5, 16);
            this.llbl_HotkeyHint.Location = new System.Drawing.Point(233, 41);
            this.llbl_HotkeyHint.Name = "llbl_HotkeyHint";
            this.llbl_HotkeyHint.Size = new System.Drawing.Size(125, 17);
            this.llbl_HotkeyHint.TabIndex = 11;
            this.llbl_HotkeyHint.TabStop = true;
            this.llbl_HotkeyHint.Text = "(see VS documentation)";
            this.llbl_HotkeyHint.UseCompatibleTextRendering = true;
            this.llbl_HotkeyHint.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbl_HotkeyHint_LinkClicked);
            // 
            // btn_About
            // 
            this.btn_About.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_About.Location = new System.Drawing.Point(12, 240);
            this.btn_About.Name = "btn_About";
            this.btn_About.Size = new System.Drawing.Size(75, 23);
            this.btn_About.TabIndex = 12;
            this.btn_About.Text = "About...";
            this.btn_About.UseVisualStyleBackColor = true;
            this.btn_About.Click += new System.EventHandler(this.btn_About_Click);
            // 
            // btn_Reset
            // 
            this.btn_Reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Reset.Location = new System.Drawing.Point(93, 240);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(75, 23);
            this.btn_Reset.TabIndex = 13;
            this.btn_Reset.Text = "Reset";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // txt_SpacesPerTab
            // 
            this.txt_SpacesPerTab.Location = new System.Drawing.Point(99, 90);
            this.txt_SpacesPerTab.Name = "txt_SpacesPerTab";
            this.txt_SpacesPerTab.Size = new System.Drawing.Size(128, 20);
            this.txt_SpacesPerTab.TabIndex = 14;
            // 
            // txt_MaxLineWidth
            // 
            this.txt_MaxLineWidth.Location = new System.Drawing.Point(99, 64);
            this.txt_MaxLineWidth.Name = "txt_MaxLineWidth";
            this.txt_MaxLineWidth.Size = new System.Drawing.Size(128, 20);
            this.txt_MaxLineWidth.TabIndex = 15;
            // 
            // lbl_SpacesPerTab
            // 
            this.lbl_SpacesPerTab.AutoSize = true;
            this.lbl_SpacesPerTab.Location = new System.Drawing.Point(12, 93);
            this.lbl_SpacesPerTab.Name = "lbl_SpacesPerTab";
            this.lbl_SpacesPerTab.Size = new System.Drawing.Size(87, 13);
            this.lbl_SpacesPerTab.TabIndex = 16;
            this.lbl_SpacesPerTab.Text = "Spaces Per Tab:";
            // 
            // lbl_SpacesPerTab_Extra
            // 
            this.lbl_SpacesPerTab_Extra.AutoSize = true;
            this.lbl_SpacesPerTab_Extra.Location = new System.Drawing.Point(233, 93);
            this.lbl_SpacesPerTab_Extra.Name = "lbl_SpacesPerTab_Extra";
            this.lbl_SpacesPerTab_Extra.Size = new System.Drawing.Size(115, 13);
            this.lbl_SpacesPerTab_Extra.TabIndex = 17;
            this.lbl_SpacesPerTab_Extra.Text = "(for Max Width feature)";
            // 
            // lbl_MaxLineWidth
            // 
            this.lbl_MaxLineWidth.AutoSize = true;
            this.lbl_MaxLineWidth.Location = new System.Drawing.Point(12, 67);
            this.lbl_MaxLineWidth.Name = "lbl_MaxLineWidth";
            this.lbl_MaxLineWidth.Size = new System.Drawing.Size(84, 13);
            this.lbl_MaxLineWidth.TabIndex = 18;
            this.lbl_MaxLineWidth.Text = "Max Line Width:";
            // 
            // chk_SpaceAfterExpandedComma
            // 
            this.chk_SpaceAfterExpandedComma.AutoSize = true;
            this.chk_SpaceAfterExpandedComma.Location = new System.Drawing.Point(3, 72);
            this.chk_SpaceAfterExpandedComma.Name = "chk_SpaceAfterExpandedComma";
            this.chk_SpaceAfterExpandedComma.Size = new System.Drawing.Size(170, 17);
            this.chk_SpaceAfterExpandedComma.TabIndex = 9;
            this.chk_SpaceAfterExpandedComma.Text = "Space after Expanded Comma";
            this.chk_SpaceAfterExpandedComma.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(383, 275);
            this.Controls.Add(this.lbl_MaxLineWidth);
            this.Controls.Add(this.lbl_SpacesPerTab_Extra);
            this.Controls.Add(this.lbl_SpacesPerTab);
            this.Controls.Add(this.txt_MaxLineWidth);
            this.Controls.Add(this.txt_SpacesPerTab);
            this.Controls.Add(this.btn_Reset);
            this.Controls.Add(this.btn_About);
            this.Controls.Add(this.llbl_HotkeyHint);
            this.Controls.Add(this.lbl_Hotkey);
            this.Controls.Add(this.lbl_IndentHint);
            this.Controls.Add(this.txt_IndentString);
            this.Controls.Add(this.lbl_IndentString);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.txt_Hotkey);
            this.MinimumSize = new System.Drawing.Size(360, 206);
            this.Name = "SettingsForm";
            this.Text = "Poor Man\'s T-Sql Formatter SSMS Addin - Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lbl_IndentString;
        private System.Windows.Forms.TextBox txt_IndentString;
        private System.Windows.Forms.CheckBox chk_ExpandCommaLists;
        private System.Windows.Forms.Label lbl_IndentHint;
        private System.Windows.Forms.CheckBox chk_TrailingCommas;
        private System.Windows.Forms.CheckBox chk_ExpandBooleanExpressions;
        private System.Windows.Forms.CheckBox chk_ExpandCaseStatements;
        private System.Windows.Forms.CheckBox chk_ExpandBetweenConditions;
        private System.Windows.Forms.CheckBox chk_UppercaseKeywords;
        private System.Windows.Forms.TextBox txt_Hotkey;
        private System.Windows.Forms.Label lbl_Hotkey;
        private System.Windows.Forms.LinkLabel llbl_HotkeyHint;
        private System.Windows.Forms.Button btn_About;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.TextBox txt_SpacesPerTab;
        private System.Windows.Forms.TextBox txt_MaxLineWidth;
        private System.Windows.Forms.Label lbl_SpacesPerTab;
        private System.Windows.Forms.Label lbl_SpacesPerTab_Extra;
        private System.Windows.Forms.Label lbl_MaxLineWidth;
        private System.Windows.Forms.CheckBox chk_SpaceAfterExpandedComma;
    }
}