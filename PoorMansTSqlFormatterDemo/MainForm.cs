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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace PoorMansTSqlFormatterDemo
{
    public partial class MainForm : Form
    {
        const string FORMATTER_STANDARD = "Standard";
        const string FORMATTER_IDENTITY = "Identity";
        const string FORMATTER_OBFUSCATE = "Obfuscate";

        const string UILANGUAGE_EN = "EN";
        const string UILANGUAGE_FR = "FR";
        const string UILANGUAGE_ES = "ES";

        PoorMansTSqlFormatterLib.Interfaces.ISqlTokenizer _tokenizer;
        PoorMansTSqlFormatterLib.Interfaces.ISqlTokenParser _parser;
        PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter _formatter;

        bool _queuedRefresh = false;
        object _refreshLock = new object();
        bool _settingsLoaded = false;

        private FrameworkClassReplacements.SingleAssemblyResourceManager _generalResourceManager;

        public MainForm()
        {
            if (!Properties.Settings.Default.UpgradeCompleted)
            {
                Properties.Settings.Default.Upgrade();
                if (!Properties.Settings.Default.UpgradeCompleted)
                {
                    //this is an initial install - detect language if possible
                    if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.Equals(UILANGUAGE_FR, StringComparison.InvariantCultureIgnoreCase))
                        Properties.Settings.Default.UILanguage = UILANGUAGE_FR;
                    else if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.Equals(UILANGUAGE_ES, StringComparison.InvariantCultureIgnoreCase))
                        Properties.Settings.Default.UILanguage = UILANGUAGE_ES;
                    else
                        Properties.Settings.Default.UILanguage = UILANGUAGE_EN;
                }
                Properties.Settings.Default.UpgradeCompleted = true;
                Properties.Settings.Default.Save();
            }

            //set the UI language BEFORE initializeComponent...
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Properties.Settings.Default.UILanguage);
            System.Globalization.CultureInfo test1 = Thread.CurrentThread.CurrentUICulture;
            _generalResourceManager = new FrameworkClassReplacements.SingleAssemblyResourceManager("GeneralLanguageContent", System.Reflection.Assembly.GetExecutingAssembly(), typeof(Program));

            InitializeComponent();

            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();

            //Now that controls exist, update UI from settings.
            if (Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_EN)) englishToolStripMenuItem.Checked = true;
            if (Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_FR)) frenchToolStripMenuItem.Checked = true;
            if (Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_ES)) spanishToolStripMenuItem.Checked = true;

            displayTokenListToolStripMenuItem.Checked = Properties.Settings.Default.DisplayTokenList;
            displayParsedSqlToolStripMenuItem.Checked = Properties.Settings.Default.DisplayParsedSqlXml;
            displayFormattingOptionsAreaToolStripMenuItem.Checked = Properties.Settings.Default.DisplayFormattingOptions;

            radio_Formatting_Standard.Checked = Properties.Settings.Default.Formatter.Equals(FORMATTER_STANDARD, StringComparison.InvariantCultureIgnoreCase);
            txt_Indent.Text = Properties.Settings.Default.Indent;
            txt_IndentWidth.Text = Properties.Settings.Default.IndentWidth.ToString();
			txt_MaxWidth.Text = Properties.Settings.Default.MaxWidth.ToString();
			txt_StatementBreaks.Text = Properties.Settings.Default.NewStatementLineBreaks.ToString();
			txt_ClauseBreaks.Text = Properties.Settings.Default.NewClauseLineBreaks.ToString();
			chk_ExpandCommaLists.Checked = Properties.Settings.Default.ExpandCommaLists;
            chk_TrailingCommas.Checked = Properties.Settings.Default.TrailingCommas;
            chk_SpaceAfterComma.Checked = Properties.Settings.Default.SpaceAfterComma;
            chk_ExpandBooleanExpressions.Checked = Properties.Settings.Default.ExpandBooleanExpressions;
            chk_ExpandCaseStatements.Checked = Properties.Settings.Default.ExpandCaseStatements;
			chk_ExpandBetweenConditions.Checked = Properties.Settings.Default.ExpandBetweenConditions;
			chk_ExpandInLists.Checked = Properties.Settings.Default.ExpandInLists;
			chk_BreakJoinOnSections.Checked = Properties.Settings.Default.BreakJoinOnSections;
            chk_UppercaseKeywords.Checked = Properties.Settings.Default.UppercaseKeywords;
            chk_Coloring.Checked = Properties.Settings.Default.StandardColoring;
            chk_EnableKeywordStandardization.Checked = Properties.Settings.Default.EnableKeywordStandardization;

            radio_Formatting_Identity.Checked = Properties.Settings.Default.Formatter.Equals(FORMATTER_IDENTITY, StringComparison.InvariantCultureIgnoreCase);
            chk_IdentityColoring.Checked = Properties.Settings.Default.IdentityColoring;

            radio_Formatting_Obfuscate.Checked = Properties.Settings.Default.Formatter.Equals(FORMATTER_OBFUSCATE, StringComparison.InvariantCultureIgnoreCase);
            chk_RandomizeKeywordCase.Checked = Properties.Settings.Default.RandomizeKeywordCase;
            chk_RandomizeColor.Checked = Properties.Settings.Default.RandomizeColor;
            chk_RandomizeLineLength.Checked = Properties.Settings.Default.RandomizeLineLength;
            chk_PreserveComments.Checked = Properties.Settings.Default.PreserveComments;
            chk_KeywordSubstitution.Checked = Properties.Settings.Default.KeywordSubstitution;

            _settingsLoaded = true;

            SetFormatter();
            UpdateDisplayLayout();
        }

        private void FormatSettingsControlChanged(object sender, EventArgs e)
        {
            if (_settingsLoaded)
            {
                SaveFormatSettings();
                SetFormatter();
                TryToDoFormatting();
            }
        }

        private void SaveFormatSettings()
        {
            if (radio_Formatting_Standard.Checked) Properties.Settings.Default.Formatter = FORMATTER_STANDARD;
            Properties.Settings.Default.Indent = txt_Indent.Text.Replace("\t", "\\t").Replace(" ", "\\s");
            Properties.Settings.Default.IndentWidth = int.Parse(txt_IndentWidth.Text);
            Properties.Settings.Default.MaxWidth = int.Parse(txt_MaxWidth.Text);
			Properties.Settings.Default.NewStatementLineBreaks = int.Parse(txt_StatementBreaks.Text);
			Properties.Settings.Default.NewClauseLineBreaks = int.Parse(txt_ClauseBreaks.Text);
			Properties.Settings.Default.ExpandCommaLists = chk_ExpandCommaLists.Checked;
            Properties.Settings.Default.TrailingCommas = chk_TrailingCommas.Checked;
            Properties.Settings.Default.SpaceAfterComma = chk_SpaceAfterComma.Checked;
            Properties.Settings.Default.ExpandBooleanExpressions = chk_ExpandBooleanExpressions.Checked;
            Properties.Settings.Default.ExpandCaseStatements = chk_ExpandCaseStatements.Checked;
			Properties.Settings.Default.ExpandBetweenConditions = chk_ExpandBetweenConditions.Checked;
			Properties.Settings.Default.ExpandInLists = chk_ExpandInLists.Checked;
			Properties.Settings.Default.BreakJoinOnSections = chk_BreakJoinOnSections.Checked;
            Properties.Settings.Default.UppercaseKeywords = chk_UppercaseKeywords.Checked;
            Properties.Settings.Default.StandardColoring = chk_Coloring.Checked;
            Properties.Settings.Default.EnableKeywordStandardization = chk_EnableKeywordStandardization.Checked;

            if (radio_Formatting_Identity.Checked) Properties.Settings.Default.Formatter = FORMATTER_IDENTITY; 
            Properties.Settings.Default.IdentityColoring = chk_IdentityColoring.Checked;

            if (radio_Formatting_Obfuscate.Checked) Properties.Settings.Default.Formatter = FORMATTER_OBFUSCATE;
            Properties.Settings.Default.RandomizeKeywordCase = chk_RandomizeKeywordCase.Checked;
            Properties.Settings.Default.RandomizeColor = chk_RandomizeColor.Checked;
            Properties.Settings.Default.RandomizeLineLength = chk_RandomizeLineLength.Checked;
            Properties.Settings.Default.PreserveComments = chk_PreserveComments.Checked;
            Properties.Settings.Default.KeywordSubstitution = chk_KeywordSubstitution.Checked;

            Properties.Settings.Default.Save();
        }

        private void SetFormatter()
        {
            PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter innerFormatter;
            if (radio_Formatting_Standard.Checked)
            {
                innerFormatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions
                    {
                        IndentString = txt_Indent.Text,
                        SpacesPerTab = int.Parse(txt_IndentWidth.Text),
                        MaxLineWidth = int.Parse(txt_MaxWidth.Text),
                        ExpandCommaLists = chk_ExpandCommaLists.Checked,
                        TrailingCommas = chk_TrailingCommas.Checked,
                        SpaceAfterExpandedComma = chk_SpaceAfterComma.Checked,
                        ExpandBooleanExpressions = chk_ExpandBooleanExpressions.Checked,
                        ExpandCaseStatements = chk_ExpandCaseStatements.Checked,
						ExpandBetweenConditions = chk_ExpandBetweenConditions.Checked,
						ExpandInLists = chk_ExpandInLists.Checked,
						BreakJoinOnSections = chk_BreakJoinOnSections.Checked,
                        UppercaseKeywords = chk_UppercaseKeywords.Checked,
                        HTMLColoring = chk_Coloring.Checked,
						KeywordStandardization = chk_EnableKeywordStandardization.Checked,
						NewStatementLineBreaks = int.Parse(txt_StatementBreaks.Text),
						NewClauseLineBreaks = int.Parse(txt_ClauseBreaks.Text)
					});
            }
            else if (radio_Formatting_Identity.Checked)
                innerFormatter = new PoorMansTSqlFormatterLib.Formatters.TSqlIdentityFormatter(chk_IdentityColoring.Checked);
            else
                innerFormatter = new PoorMansTSqlFormatterLib.Formatters.TSqlObfuscatingFormatter(
                    chk_RandomizeKeywordCase.Checked,
                    chk_RandomizeColor.Checked,
                    chk_RandomizeLineLength.Checked,
                    chk_PreserveComments.Checked,
                    chk_KeywordSubstitution.Checked
                    );

            innerFormatter.ErrorOutputPrefix = _generalResourceManager.GetString("ParseErrorWarningPrefix") + Environment.NewLine;
            _formatter = new PoorMansTSqlFormatterLib.Formatters.HtmlPageWrapper(innerFormatter);
        }

        private void DoFormatting()
        {
            var tokenizedSql = _tokenizer.TokenizeSQL(txt_Input.Text);

            if (!splitContainer4.Panel2Collapsed && !splitContainer5.Panel1Collapsed)
                txt_TokenizedSql.Text = tokenizedSql.PrettyPrint();

            var parsedSql = _parser.ParseSQL(tokenizedSql);
            
            if (!splitContainer4.Panel2Collapsed && !splitContainer5.Panel2Collapsed)
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

        private void UpdateDisplayLayout()
        {
            //Update main upper splitter right panel for collapse state
            if (splitContainer4.Panel2Collapsed && (Properties.Settings.Default.DisplayTokenList || Properties.Settings.Default.DisplayParsedSqlXml))
                splitContainer4.Panel2Collapsed = false;
            else if (!splitContainer4.Panel2Collapsed && (!Properties.Settings.Default.DisplayTokenList && !Properties.Settings.Default.DisplayParsedSqlXml))
                splitContainer4.Panel2Collapsed = true;

            //Update Upper right splitter for upper or lower collapse
            if (splitContainer5.Panel1Collapsed && Properties.Settings.Default.DisplayTokenList)
                splitContainer5.Panel1Collapsed = false;
            else if (!splitContainer5.Panel1Collapsed && !Properties.Settings.Default.DisplayTokenList)
                splitContainer5.Panel1Collapsed = true;

            if (splitContainer5.Panel2Collapsed && Properties.Settings.Default.DisplayParsedSqlXml)
                splitContainer5.Panel2Collapsed = false;
            else if (!splitContainer5.Panel2Collapsed && !Properties.Settings.Default.DisplayParsedSqlXml)
                splitContainer5.Panel2Collapsed = true;

            //Update Lower splitter for right panel collapse
            if (splitContainer3.Panel2Collapsed && Properties.Settings.Default.DisplayFormattingOptions)
                splitContainer3.Panel2Collapsed = false;
            else if (!splitContainer3.Panel2Collapsed && !Properties.Settings.Default.DisplayFormattingOptions)
                splitContainer3.Panel2Collapsed = true;
        }

        private void displaySettingsHandler(object sender, EventArgs e)
        {
            if (_settingsLoaded)
            {
                Properties.Settings.Default.DisplayFormattingOptions = displayFormattingOptionsAreaToolStripMenuItem.Checked;
                Properties.Settings.Default.DisplayParsedSqlXml = displayParsedSqlToolStripMenuItem.Checked;
                Properties.Settings.Default.DisplayTokenList = displayTokenListToolStripMenuItem.Checked;
                Properties.Settings.Default.Save();
                UpdateDisplayLayout();
            }
        }

        private void languageSettingsHandler(object sender, EventArgs e)
        {
            if (_settingsLoaded)
            {
                bool changeHappened = false;

                if (englishToolStripMenuItem.Checked && !Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_EN))
                {
                    Properties.Settings.Default.UILanguage = UILANGUAGE_EN;
                    changeHappened = true;
                }
                else if (frenchToolStripMenuItem.Checked && !Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_FR))
                {
                    Properties.Settings.Default.UILanguage = UILANGUAGE_FR;
                    changeHappened = true;
                }
                else if (spanishToolStripMenuItem.Checked && !Properties.Settings.Default.UILanguage.Equals(UILANGUAGE_ES))
                {
                    Properties.Settings.Default.UILanguage = UILANGUAGE_ES;
                    changeHappened = true;
                }

                if (changeHappened)
                {
                    Properties.Settings.Default.Save();
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Properties.Settings.Default.UILanguage);
                    MessageBox.Show(_generalResourceManager.GetString("LanguageChangeWarningMessage"), _generalResourceManager.GetString("LanguageChangeWarningTitle"));
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox about = new AboutBox())
                about.ShowDialog();
        }

    }
}
