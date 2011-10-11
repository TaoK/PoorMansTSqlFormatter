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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PoorMansTSqlFormatterDemo
{
    public partial class MainForm : Form
    {

        PoorMansTSqlFormatterLib.Interfaces.ISqlTokenizer _tokenizer;
        PoorMansTSqlFormatterLib.Interfaces.ISqlTokenParser _parser;
        PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter _formatter;

        bool _queuedRefresh = false;
        object _refreshLock = new object();

        public MainForm()
        {
            InitializeComponent();
            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();
            SetFormatter();
        }

        private void SettingsControlChanged(object sender, EventArgs e)
        {
            SetFormatter();
            TryToDoFormatting();
        }

        private void SetFormatter()
        {
            PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter innerFormatter;
            if (radio_Formatting_Standard.Checked)
            {
                innerFormatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(
                    txt_Indent.Text, 
                    int.Parse(txt_IndentWidth.Text), 
                    int.Parse(txt_MaxWidth.Text), 
                    chk_ExpandCommaLists.Checked, 
                    chk_TrailingCommas.Checked, 
                    chk_SpaceAfterComma.Checked, 
                    chk_ExpandBooleanExpressions.Checked, 
                    chk_ExpandCaseStatements.Checked, 
                    chk_ExpandBetweenConditions.Checked,
                    chk_BreakJoinOnSections.Checked,
                    chk_UppercaseKeywords.Checked, 
                    chk_Coloring.Checked, 
                    chk_EnableKeywordStandardization.Checked
                    );
            }
            else
                innerFormatter = new PoorMansTSqlFormatterLib.Formatters.TSqlIdentityFormatter(chk_IdentityColoring.Checked);

            _formatter = new PoorMansTSqlFormatterLib.Formatters.HtmlPageWrapper(innerFormatter);
        }

        private void DoFormatting()
        {
            var tokenizedSql = _tokenizer.TokenizeSQL(txt_Input.Text);
            txt_TokenizedXml.Text = tokenizedSql.PrettyPrint();
            var parsedSql = _parser.ParseSQL(tokenizedSql);
            txt_ParsedXml.Text = parsedSql.OuterXml;
            webBrowser_OutputSql.SetHTML(_formatter.FormatSQLTree(parsedSql));
        }

        private void TryToDoFormatting()
        {
            lock (_refreshLock)
            {
                if (timer_TextChangeDelay.Enabled)
                    _queuedRefresh = true;
                else
                {
                    DoFormatting();
                    timer_TextChangeDelay.Start();
                }
            }
        }

        private void txt_Input_TextChanged(object sender, EventArgs e)
        {
            TryToDoFormatting();
        }

        private void timer_TextChangeDelay_Tick(object sender, EventArgs e)
        {
            timer_TextChangeDelay.Enabled = false;
            lock (_refreshLock)
            {
                if (_queuedRefresh)
                {
                    DoFormatting();
                    timer_TextChangeDelay.Start();
                    _queuedRefresh = false;
                }
            }

        }

        private void btn_About_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
            about.Dispose();
        }

    }
}
