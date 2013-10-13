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

namespace PoorMansTSqlFormatterPluginShared
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
			this.btn_Save = new System.Windows.Forms.Button();
			this.btn_Cancel = new System.Windows.Forms.Button();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.chk_ExpandCommaLists = new System.Windows.Forms.CheckBox();
			this.chk_TrailingCommas = new System.Windows.Forms.CheckBox();
			this.chk_ExpandBooleanExpressions = new System.Windows.Forms.CheckBox();
			this.chk_ExpandCaseStatements = new System.Windows.Forms.CheckBox();
			this.chk_ExpandBetweenConditions = new System.Windows.Forms.CheckBox();
			this.chk_ExpandInLists = new System.Windows.Forms.CheckBox();
			this.chk_UppercaseKeywords = new System.Windows.Forms.CheckBox();
			this.chk_SpaceAfterExpandedComma = new System.Windows.Forms.CheckBox();
			this.chk_BreakJoinOnSections = new System.Windows.Forms.CheckBox();
			this.chk_StandardizeKeywords = new System.Windows.Forms.CheckBox();
			this.txt_IndentString = new System.Windows.Forms.TextBox();
			this.lbl_IndentString = new System.Windows.Forms.Label();
			this.lbl_IndentHint = new System.Windows.Forms.Label();
			this.txt_Hotkey = new System.Windows.Forms.TextBox();
			this.lbl_Hotkey = new System.Windows.Forms.Label();
			this.lbl_HotkeyHint = new System.Windows.Forms.LinkLabel();
			this.btn_About = new System.Windows.Forms.Button();
			this.btn_Reset = new System.Windows.Forms.Button();
			this.txt_SpacesPerTab = new System.Windows.Forms.TextBox();
			this.txt_MaxLineWidth = new System.Windows.Forms.TextBox();
			this.lbl_SpacesPerTab = new System.Windows.Forms.Label();
			this.lbl_SpacesPerTab_Extra = new System.Windows.Forms.Label();
			this.lbl_MaxLineWidth = new System.Windows.Forms.Label();
			this.txt_StatementBreaks = new System.Windows.Forms.TextBox();
			this.lbl_StatementBreaks = new System.Windows.Forms.Label();
			this.txt_ClauseBreaks = new System.Windows.Forms.TextBox();
			this.lbl_ClauseBreaks = new System.Windows.Forms.Label();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btn_Save
			// 
			resources.ApplyResources(this.btn_Save, "btn_Save");
			this.btn_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btn_Save.Name = "btn_Save";
			this.btn_Save.UseVisualStyleBackColor = true;
			this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
			// 
			// btn_Cancel
			// 
			resources.ApplyResources(this.btn_Cancel, "btn_Cancel");
			this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_Cancel.Name = "btn_Cancel";
			this.btn_Cancel.UseVisualStyleBackColor = true;
			// 
			// flowLayoutPanel1
			// 
			resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
			this.flowLayoutPanel1.Controls.Add(this.chk_ExpandCommaLists);
			this.flowLayoutPanel1.Controls.Add(this.chk_TrailingCommas);
			this.flowLayoutPanel1.Controls.Add(this.chk_ExpandBooleanExpressions);
			this.flowLayoutPanel1.Controls.Add(this.chk_ExpandCaseStatements);
			this.flowLayoutPanel1.Controls.Add(this.chk_ExpandBetweenConditions);
			this.flowLayoutPanel1.Controls.Add(this.chk_ExpandInLists);
			this.flowLayoutPanel1.Controls.Add(this.chk_UppercaseKeywords);
			this.flowLayoutPanel1.Controls.Add(this.chk_SpaceAfterExpandedComma);
			this.flowLayoutPanel1.Controls.Add(this.chk_BreakJoinOnSections);
			this.flowLayoutPanel1.Controls.Add(this.chk_StandardizeKeywords);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// chk_ExpandCommaLists
			// 
			resources.ApplyResources(this.chk_ExpandCommaLists, "chk_ExpandCommaLists");
			this.chk_ExpandCommaLists.Name = "chk_ExpandCommaLists";
			this.chk_ExpandCommaLists.UseVisualStyleBackColor = true;
			// 
			// chk_TrailingCommas
			// 
			resources.ApplyResources(this.chk_TrailingCommas, "chk_TrailingCommas");
			this.chk_TrailingCommas.Name = "chk_TrailingCommas";
			this.chk_TrailingCommas.UseVisualStyleBackColor = true;
			// 
			// chk_ExpandBooleanExpressions
			// 
			resources.ApplyResources(this.chk_ExpandBooleanExpressions, "chk_ExpandBooleanExpressions");
			this.chk_ExpandBooleanExpressions.Name = "chk_ExpandBooleanExpressions";
			this.chk_ExpandBooleanExpressions.UseVisualStyleBackColor = true;
			// 
			// chk_ExpandCaseStatements
			// 
			resources.ApplyResources(this.chk_ExpandCaseStatements, "chk_ExpandCaseStatements");
			this.chk_ExpandCaseStatements.Name = "chk_ExpandCaseStatements";
			this.chk_ExpandCaseStatements.UseVisualStyleBackColor = true;
			// 
			// chk_ExpandBetweenConditions
			// 
			resources.ApplyResources(this.chk_ExpandBetweenConditions, "chk_ExpandBetweenConditions");
			this.chk_ExpandBetweenConditions.Name = "chk_ExpandBetweenConditions";
			this.chk_ExpandBetweenConditions.UseVisualStyleBackColor = true;
			// 
			// chk_ExpandInLists
			// 
			resources.ApplyResources(this.chk_ExpandInLists, "chk_ExpandInLists");
			this.chk_ExpandInLists.Name = "chk_ExpandInLists";
			this.chk_ExpandInLists.UseVisualStyleBackColor = true;
			// 
			// chk_UppercaseKeywords
			// 
			resources.ApplyResources(this.chk_UppercaseKeywords, "chk_UppercaseKeywords");
			this.chk_UppercaseKeywords.Name = "chk_UppercaseKeywords";
			this.chk_UppercaseKeywords.UseVisualStyleBackColor = true;
			// 
			// chk_SpaceAfterExpandedComma
			// 
			resources.ApplyResources(this.chk_SpaceAfterExpandedComma, "chk_SpaceAfterExpandedComma");
			this.chk_SpaceAfterExpandedComma.Name = "chk_SpaceAfterExpandedComma";
			this.chk_SpaceAfterExpandedComma.UseVisualStyleBackColor = true;
			// 
			// chk_BreakJoinOnSections
			// 
			resources.ApplyResources(this.chk_BreakJoinOnSections, "chk_BreakJoinOnSections");
			this.chk_BreakJoinOnSections.Name = "chk_BreakJoinOnSections";
			this.chk_BreakJoinOnSections.UseVisualStyleBackColor = true;
			// 
			// chk_StandardizeKeywords
			// 
			resources.ApplyResources(this.chk_StandardizeKeywords, "chk_StandardizeKeywords");
			this.chk_StandardizeKeywords.Name = "chk_StandardizeKeywords";
			this.chk_StandardizeKeywords.UseVisualStyleBackColor = true;
			// 
			// txt_IndentString
			// 
			resources.ApplyResources(this.txt_IndentString, "txt_IndentString");
			this.txt_IndentString.Name = "txt_IndentString";
			// 
			// lbl_IndentString
			// 
			resources.ApplyResources(this.lbl_IndentString, "lbl_IndentString");
			this.lbl_IndentString.Name = "lbl_IndentString";
			// 
			// lbl_IndentHint
			// 
			resources.ApplyResources(this.lbl_IndentHint, "lbl_IndentHint");
			this.lbl_IndentHint.Name = "lbl_IndentHint";
			// 
			// txt_Hotkey
			// 
			resources.ApplyResources(this.txt_Hotkey, "txt_Hotkey");
			this.txt_Hotkey.Name = "txt_Hotkey";
			// 
			// lbl_Hotkey
			// 
			resources.ApplyResources(this.lbl_Hotkey, "lbl_Hotkey");
			this.lbl_Hotkey.Name = "lbl_Hotkey";
			// 
			// lbl_HotkeyHint
			// 
			resources.ApplyResources(this.lbl_HotkeyHint, "lbl_HotkeyHint");
			this.lbl_HotkeyHint.Name = "lbl_HotkeyHint";
			this.lbl_HotkeyHint.TabStop = true;
			this.lbl_HotkeyHint.UseCompatibleTextRendering = true;
			this.lbl_HotkeyHint.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbl_HotkeyHint_LinkClicked);
			// 
			// btn_About
			// 
			resources.ApplyResources(this.btn_About, "btn_About");
			this.btn_About.Name = "btn_About";
			this.btn_About.UseVisualStyleBackColor = true;
			this.btn_About.Click += new System.EventHandler(this.btn_About_Click);
			// 
			// btn_Reset
			// 
			resources.ApplyResources(this.btn_Reset, "btn_Reset");
			this.btn_Reset.Name = "btn_Reset";
			this.btn_Reset.UseVisualStyleBackColor = true;
			this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
			// 
			// txt_SpacesPerTab
			// 
			resources.ApplyResources(this.txt_SpacesPerTab, "txt_SpacesPerTab");
			this.txt_SpacesPerTab.Name = "txt_SpacesPerTab";
			// 
			// txt_MaxLineWidth
			// 
			resources.ApplyResources(this.txt_MaxLineWidth, "txt_MaxLineWidth");
			this.txt_MaxLineWidth.Name = "txt_MaxLineWidth";
			// 
			// lbl_SpacesPerTab
			// 
			resources.ApplyResources(this.lbl_SpacesPerTab, "lbl_SpacesPerTab");
			this.lbl_SpacesPerTab.Name = "lbl_SpacesPerTab";
			// 
			// lbl_SpacesPerTab_Extra
			// 
			resources.ApplyResources(this.lbl_SpacesPerTab_Extra, "lbl_SpacesPerTab_Extra");
			this.lbl_SpacesPerTab_Extra.Name = "lbl_SpacesPerTab_Extra";
			// 
			// lbl_MaxLineWidth
			// 
			resources.ApplyResources(this.lbl_MaxLineWidth, "lbl_MaxLineWidth");
			this.lbl_MaxLineWidth.Name = "lbl_MaxLineWidth";
			// 
			// txt_StatementBreaks
			// 
			resources.ApplyResources(this.txt_StatementBreaks, "txt_StatementBreaks");
			this.txt_StatementBreaks.Name = "txt_StatementBreaks";
			// 
			// lbl_StatementBreaks
			// 
			resources.ApplyResources(this.lbl_StatementBreaks, "lbl_StatementBreaks");
			this.lbl_StatementBreaks.Name = "lbl_StatementBreaks";
			// 
			// txt_ClauseBreaks
			// 
			resources.ApplyResources(this.txt_ClauseBreaks, "txt_ClauseBreaks");
			this.txt_ClauseBreaks.Name = "txt_ClauseBreaks";
			// 
			// lbl_ClauseBreaks
			// 
			resources.ApplyResources(this.lbl_ClauseBreaks, "lbl_ClauseBreaks");
			this.lbl_ClauseBreaks.Name = "lbl_ClauseBreaks";
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btn_Save;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btn_Cancel;
			this.Controls.Add(this.lbl_MaxLineWidth);
			this.Controls.Add(this.lbl_SpacesPerTab_Extra);
			this.Controls.Add(this.lbl_ClauseBreaks);
			this.Controls.Add(this.lbl_StatementBreaks);
			this.Controls.Add(this.lbl_SpacesPerTab);
			this.Controls.Add(this.txt_MaxLineWidth);
			this.Controls.Add(this.txt_ClauseBreaks);
			this.Controls.Add(this.txt_StatementBreaks);
			this.Controls.Add(this.txt_SpacesPerTab);
			this.Controls.Add(this.btn_Reset);
			this.Controls.Add(this.btn_About);
			this.Controls.Add(this.lbl_HotkeyHint);
			this.Controls.Add(this.lbl_Hotkey);
			this.Controls.Add(this.lbl_IndentHint);
			this.Controls.Add(this.txt_IndentString);
			this.Controls.Add(this.lbl_IndentString);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.btn_Cancel);
			this.Controls.Add(this.btn_Save);
			this.Controls.Add(this.txt_Hotkey);
			this.Name = "SettingsForm";
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
        private System.Windows.Forms.LinkLabel lbl_HotkeyHint;
        private System.Windows.Forms.Button btn_About;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.TextBox txt_SpacesPerTab;
        private System.Windows.Forms.TextBox txt_MaxLineWidth;
        private System.Windows.Forms.Label lbl_SpacesPerTab;
        private System.Windows.Forms.Label lbl_SpacesPerTab_Extra;
        private System.Windows.Forms.Label lbl_MaxLineWidth;
        private System.Windows.Forms.CheckBox chk_SpaceAfterExpandedComma;
        private System.Windows.Forms.CheckBox chk_StandardizeKeywords;
        private System.Windows.Forms.CheckBox chk_BreakJoinOnSections;
		private System.Windows.Forms.CheckBox chk_ExpandInLists;
		private System.Windows.Forms.TextBox txt_StatementBreaks;
		private System.Windows.Forms.Label lbl_StatementBreaks;
		private System.Windows.Forms.TextBox txt_ClauseBreaks;
		private System.Windows.Forms.Label lbl_ClauseBreaks;
    }
}