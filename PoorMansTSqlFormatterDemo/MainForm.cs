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

        public MainForm()
        {
            InitializeComponent();
            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();
            SetFormatter();
        }

        private void txt_Input_Leave(object sender, EventArgs e)
        {
            DoFormatting();
        }

        private void radio_Formatting_Standard_CheckedChanged(object sender, EventArgs e)
        {
            SetFormatter();
            DoFormatting();
        }

        private void radio_Formatting_Identity_CheckedChanged(object sender, EventArgs e)
        {
            SetFormatter();
            DoFormatting();
        }

        private void SetFormatter()
        {
            if (radio_Formatting_Standard.Checked)
                _formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter();
            else
                _formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlIdentityFormatter();
        }

        private void DoFormatting()
        {
            var tokenizedSql = _tokenizer.TokenizeSQL(txt_Input.Text);
            txt_TokenizedXml.Text = tokenizedSql.OuterXml;
            var parsedSql = _parser.ParseSQL(tokenizedSql);
            txt_ParsedXml.Text = parsedSql.OuterXml;
            txt_OutputSql.Text = _formatter.FormatSQLTree(parsedSql);
        }

    }
}
