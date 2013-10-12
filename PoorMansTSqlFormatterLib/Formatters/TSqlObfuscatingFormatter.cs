/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2013 Tao Klerks

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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlObfuscatingFormatter : Interfaces.ISqlTreeFormatter
    {

        public TSqlObfuscatingFormatter() : this(false, false, false, false, false) { }

        public TSqlObfuscatingFormatter(bool randomizeCase, bool randomizeColor, bool randomizeLineLength, bool preserveComments, bool subtituteKeywords)
        {
            RandomizeCase = randomizeCase;
            RandomizeColor = randomizeColor;
            RandomizeLineLength = randomizeLineLength;
            PreserveComments = preserveComments;
            if (subtituteKeywords)
                KeywordMapping = ObfuscatingKeywordMapping.Instance;

            ErrorOutputPrefix = Interfaces.MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;

            if (RandomizeCase)
            {
                _currentCaseLimit = _randomizer.Next(MIN_CASE_WORD_LENGTH, MAX_CASE_WORD_LENGTH);
                _currentlyUppercase = _randomizer.Next(0, 2) == 0;
            }
        }

        public bool RandomizeCase { get; set; }
        public bool RandomizeColor { get; set; }
        public bool RandomizeLineLength { get; set; }
        public bool PreserveComments { get; set; }

        public bool HTMLFormatted { get { return RandomizeColor; } }

        public IDictionary<string, string> KeywordMapping = new Dictionary<string, string>();

        public string ErrorOutputPrefix { get; set; }

        private const int MIN_CASE_WORD_LENGTH = 2;
        private const int MAX_CASE_WORD_LENGTH = 8;
        private Random _randomizer = new Random();
        private int _currentCaseLength = 0;
        private int _currentCaseLimit = MAX_CASE_WORD_LENGTH;
        private bool _currentlyUppercase = false;

        public string FormatSQLTree(XmlDocument sqlTreeDoc)
        {
            //thread-safe - each call to FormatSQLTree() gets its own independent state object
            TSqlObfuscatingFormattingState state = new TSqlObfuscatingFormattingState(RandomizeColor, RandomizeLineLength);

            if (sqlTreeDoc.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", SqlXmlConstants.ENAME_SQL_ROOT, SqlXmlConstants.ANAME_ERRORFOUND)) != null)
                state.AddOutputContent(ErrorOutputPrefix);

            XmlNodeList rootList = sqlTreeDoc.SelectNodes(string.Format("/{0}/*", SqlXmlConstants.ENAME_SQL_ROOT));
            return FormatSQLNodes(rootList, state);
        }

        public string FormatSQLTree(XmlNode fragmentNode)
        {
            TSqlObfuscatingFormattingState state = new TSqlObfuscatingFormattingState(false, false);
            return FormatSQLNodes(fragmentNode.SelectNodes("."), state);
        }

        private string FormatSQLNodes(XmlNodeList nodes, TSqlObfuscatingFormattingState state)
        {
            ProcessSqlNodeList(nodes, state);
            state.BreakIfExpected();
            return state.DumpOutput();
        }

        private void ProcessSqlNodeList(XmlNodeList rootList, TSqlObfuscatingFormattingState state)
        {
            foreach (XmlElement contentElement in rootList)
                ProcessSqlNode(contentElement, state);
        }

        private void ProcessSqlNode(XmlElement contentElement, TSqlObfuscatingFormattingState state)
        {
            switch (contentElement.Name)
            {
                case SqlXmlConstants.ENAME_SQL_ROOT:
                case SqlXmlConstants.ENAME_SQL_STATEMENT:
                case SqlXmlConstants.ENAME_SQL_CLAUSE:
                case SqlXmlConstants.ENAME_SET_OPERATOR_CLAUSE:
                case SqlXmlConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case SqlXmlConstants.ENAME_DDL_OTHER_BLOCK:
                case SqlXmlConstants.ENAME_DDL_DECLARE_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_DECLARATION:
                case SqlXmlConstants.ENAME_BEGIN_TRANSACTION:
                case SqlXmlConstants.ENAME_SAVE_TRANSACTION:
                case SqlXmlConstants.ENAME_COMMIT_TRANSACTION:
                case SqlXmlConstants.ENAME_ROLLBACK_TRANSACTION:
                case SqlXmlConstants.ENAME_CONTAINER_OPEN:
                case SqlXmlConstants.ENAME_CONTAINER_CLOSE:
                case SqlXmlConstants.ENAME_WHILE_LOOP:
                case SqlXmlConstants.ENAME_IF_STATEMENT:
                case SqlXmlConstants.ENAME_SELECTIONTARGET:
                case SqlXmlConstants.ENAME_CONTAINER_GENERALCONTENT:
                case SqlXmlConstants.ENAME_CTE_WITH_CLAUSE:
                case SqlXmlConstants.ENAME_PERMISSIONS_BLOCK:
                case SqlXmlConstants.ENAME_PERMISSIONS_DETAIL:
                case SqlXmlConstants.ENAME_MERGE_CLAUSE:
                case SqlXmlConstants.ENAME_MERGE_TARGET:
                case SqlXmlConstants.ENAME_CASE_INPUT:
                case SqlXmlConstants.ENAME_BOOLEAN_EXPRESSION:
                case SqlXmlConstants.ENAME_BETWEEN_LOWERBOUND:
                case SqlXmlConstants.ENAME_BETWEEN_UPPERBOUND:
                case SqlXmlConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case SqlXmlConstants.ENAME_CONTAINER_MULTISTATEMENT:
                case SqlXmlConstants.ENAME_MERGE_ACTION:
                case SqlXmlConstants.ENAME_PERMISSIONS_TARGET:
                case SqlXmlConstants.ENAME_PERMISSIONS_RECIPIENT:
                case SqlXmlConstants.ENAME_DDL_WITH_CLAUSE:
                case SqlXmlConstants.ENAME_MERGE_CONDITION:
                case SqlXmlConstants.ENAME_MERGE_THEN:
                case SqlXmlConstants.ENAME_JOIN_ON_SECTION:
                case SqlXmlConstants.ENAME_CTE_ALIAS:
                case SqlXmlConstants.ENAME_ELSE_CLAUSE:
                case SqlXmlConstants.ENAME_DDL_AS_BLOCK:
                case SqlXmlConstants.ENAME_CURSOR_FOR_BLOCK:
                case SqlXmlConstants.ENAME_TRIGGER_CONDITION:
                case SqlXmlConstants.ENAME_CURSOR_FOR_OPTIONS:
                case SqlXmlConstants.ENAME_CTE_AS_BLOCK:
                case SqlXmlConstants.ENAME_DDL_RETURNS:
                case SqlXmlConstants.ENAME_MERGE_USING:
                case SqlXmlConstants.ENAME_MERGE_WHEN:
                case SqlXmlConstants.ENAME_BETWEEN_CONDITION:
                case SqlXmlConstants.ENAME_BEGIN_END_BLOCK:
                case SqlXmlConstants.ENAME_TRY_BLOCK:
                case SqlXmlConstants.ENAME_CATCH_BLOCK:
                case SqlXmlConstants.ENAME_CASE_STATEMENT:
                case SqlXmlConstants.ENAME_CASE_WHEN:
                case SqlXmlConstants.ENAME_CASE_THEN:
                case SqlXmlConstants.ENAME_CASE_ELSE:
                case SqlXmlConstants.ENAME_AND_OPERATOR:
                case SqlXmlConstants.ENAME_OR_OPERATOR:
                    //these are all containers, and therefore have no impact on obfuscated output.
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    break;

                case SqlXmlConstants.ENAME_DDLDETAIL_PARENS:
				case SqlXmlConstants.ENAME_FUNCTION_PARENS:
				case SqlXmlConstants.ENAME_IN_PARENS:
				case SqlXmlConstants.ENAME_DDL_PARENS:
                case SqlXmlConstants.ENAME_EXPRESSION_PARENS:
                case SqlXmlConstants.ENAME_SELECTIONTARGET_PARENS:
                    state.SpaceExpected = false;
                    state.AddOutputContent("(");
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.SpaceExpected = false;
                    state.SpaceExpectedForAnsiString = false;
                    state.AddOutputContent(")");
                    break;

                case SqlXmlConstants.ENAME_WHITESPACE:
                    //do nothing
                    break;

                case SqlXmlConstants.ENAME_COMMENT_MULTILINE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("/*" + contentElement.InnerText + "*/");
                    }
                    break;
                case SqlXmlConstants.ENAME_COMMENT_SINGLELINE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("--" + contentElement.InnerText.Replace("\r", "").Replace("\n", ""));
                        state.BreakExpected = true;
                    }
                    break;

                case SqlXmlConstants.ENAME_COMMENT_SINGLELINE_CSTYLE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("//" + contentElement.InnerText.Replace("\r", "").Replace("\n", ""));
                        state.BreakExpected = true;
                    }
                    break;

                case SqlXmlConstants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.SelectNodes("*"), state);
                    state.BreakExpected = true;
                    break;

                case SqlXmlConstants.ENAME_STRING:
                    state.SpaceIfExpectedForAnsiString();
                    state.SpaceExpected = false;
                    state.AddOutputContent("'" + contentElement.InnerText.Replace("'", "''") + "'");
                    state.SpaceExpectedForAnsiString = true;
                    break;

                case SqlXmlConstants.ENAME_NSTRING:
                    state.AddOutputContent("N'" + contentElement.InnerText.Replace("'", "''") + "'");
                    state.SpaceExpectedForAnsiString = true;
                    break;

                case SqlXmlConstants.ENAME_BRACKET_QUOTED_NAME:
                    state.SpaceExpected = false;
                    state.AddOutputContent("[" + contentElement.InnerText.Replace("]", "]]") + "]");
                    break;

                case SqlXmlConstants.ENAME_QUOTED_STRING:
                    state.SpaceExpected = false;
                    state.AddOutputContent("\"" + contentElement.InnerText.Replace("\"", "\"\"") + "\"");
                    break;

                case SqlXmlConstants.ENAME_COMMA:
                case SqlXmlConstants.ENAME_PERIOD:
                case SqlXmlConstants.ENAME_SEMICOLON:
                case SqlXmlConstants.ENAME_SCOPERESOLUTIONOPERATOR:
                case SqlXmlConstants.ENAME_ASTERISK:
                case SqlXmlConstants.ENAME_EQUALSSIGN:
                case SqlXmlConstants.ENAME_OTHEROPERATOR:
                    state.SpaceExpected = false;
                    state.AddOutputContent(contentElement.InnerText);
                    break;

                case SqlXmlConstants.ENAME_COMPOUNDKEYWORD:
                    state.AddOutputContent(FormatKeyword(contentElement.Attributes[SqlXmlConstants.ANAME_SIMPLETEXT].Value));
                    state.SpaceExpected = true;
                    break;


                case SqlXmlConstants.ENAME_LABEL:
                    state.AddOutputContent(contentElement.InnerText);
                    state.BreakExpected = true;
                    break;

                case SqlXmlConstants.ENAME_OTHERKEYWORD:
                case SqlXmlConstants.ENAME_ALPHAOPERATOR:
                case SqlXmlConstants.ENAME_DATATYPE_KEYWORD:
                case SqlXmlConstants.ENAME_PSEUDONAME:
                case SqlXmlConstants.ENAME_BINARY_VALUE:
                    state.AddOutputContent(FormatKeyword(contentElement.InnerText));
                    state.SpaceExpected = true;
                    break;

                case SqlXmlConstants.ENAME_NUMBER_VALUE:
                    state.AddOutputContent(FormatKeyword(contentElement.InnerText));
                    if (!contentElement.InnerText.ToLowerInvariant().Contains("e"))
                    {
                        state.SpaceExpectedForE = true;
                        if (contentElement.InnerText.Equals("0"))
                            state.SpaceExpectedForX = true;
                    }
                    break;

                case SqlXmlConstants.ENAME_MONETARY_VALUE:
                    if (!contentElement.InnerText.Substring(0, 1).Equals("$"))
                        state.SpaceExpected = false;

                    state.AddOutputContent(contentElement.InnerText);

                    if (contentElement.InnerText.Length == 1)
                        state.SpaceExpectedForPlusMinus = true;
                    break;

                case SqlXmlConstants.ENAME_OTHERNODE:
                case SqlXmlConstants.ENAME_FUNCTION_KEYWORD:
                    state.AddOutputContent(contentElement.InnerText);
                    state.SpaceExpected = true;
                    break;

                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
        }

        private string FormatKeyword(string keyword)
        {
            string outputKeyword;
            if (!KeywordMapping.TryGetValue(keyword, out outputKeyword))
                outputKeyword = keyword;

            if (RandomizeCase)
                return GetCaseRandomized(outputKeyword);
            else
                return outputKeyword;
        }

        private string GetCaseRandomized(string outputKeyword)
        {
            char[] keywordCharArray = outputKeyword.ToCharArray();
            for (int i = 0; i < keywordCharArray.Length; i++)
            {
                if (_currentCaseLength == _currentCaseLimit)
                {
                    _currentCaseLimit = _randomizer.Next(MIN_CASE_WORD_LENGTH, MAX_CASE_WORD_LENGTH);
                    _currentlyUppercase = _randomizer.Next(0, 2) == 0;
                    _currentCaseLength = 0;
                }

                keywordCharArray[i] = _currentlyUppercase ? char.ToUpperInvariant(keywordCharArray[i]) : char.ToLowerInvariant(keywordCharArray[i]);
                _currentCaseLength++;
            }
            return new string(keywordCharArray);
        }

        class TSqlObfuscatingFormattingState : BaseFormatterState
        {
            public TSqlObfuscatingFormattingState(bool randomizeColor, bool randomizeLineLength)
                : base(randomizeColor)
            {
                RandomizeColor = randomizeColor;
                RandomizeLineLength = randomizeLineLength;

                if (RandomizeColor)
                {
                    _currentColorLimit = _randomizer.Next(MIN_COLOR_WORD_LENGTH, MAX_COLOR_WORD_LENGTH);
                    _currentColor = string.Format("#{0:x2}{1:x2}{2:x2}", _randomizer.Next(0, 127), _randomizer.Next(0, 127), _randomizer.Next(0, 127));
                }
                if (RandomizeLineLength)
                {
                    _thisLineLimit = _randomizer.Next(MIN_LINE_LENGTH, MAX_LINE_LENGTH);
                }
            }

            private const int MIN_COLOR_WORD_LENGTH = 3;
            private const int MAX_COLOR_WORD_LENGTH = 15;
            private const int MIN_LINE_LENGTH = 10;
            private const int MAX_LINE_LENGTH = 80;

            private bool RandomizeColor { get; set; }
            private bool RandomizeLineLength { get; set; }

            internal bool BreakExpected { get; set; }
            internal bool SpaceExpectedForAnsiString { get; set; }
            internal bool SpaceExpectedForE { get; set; }
            internal bool SpaceExpectedForX { get; set; }
            internal bool SpaceExpectedForPlusMinus { get; set; }
            internal bool SpaceExpected { get; set; }

            private Random _randomizer = new Random();
            private int _currentLineLength = 0;
            private int _thisLineLimit = MAX_LINE_LENGTH;

            private int _currentColorLength = 0;
            private int _currentColorLimit = MAX_COLOR_WORD_LENGTH;
            private string _currentColor = null;

            public void BreakIfExpected()
            {
                if (BreakExpected)
                {
                    BreakExpected = false;
                    base.AddOutputLineBreak();
                    SetSpaceNoLongerExpected();
                    _currentLineLength = 0;

                    if (RandomizeLineLength)
                        _thisLineLimit = _randomizer.Next(10, 80);
                }
            }

            public void SpaceIfExpectedForAnsiString()
            {
                if (SpaceExpectedForAnsiString)
                {
                    base.AddOutputContent(" ", null);
                    SetSpaceNoLongerExpected();
                }
            }

            public void SpaceIfExpected()
            {
                if (SpaceExpected)
                {
                    base.AddOutputContent(" ", null);
                    SetSpaceNoLongerExpected();
                }
            }

            public override void AddOutputContent(string content, string htmlClassName)
            {
                if (htmlClassName != null)
                    throw new NotSupportedException("Obfuscating formatter does not use html class names...");

                BreakIfExpected();
                SpaceIfExpected();
                if (_currentLineLength > 0 && _currentLineLength + content.Length > _thisLineLimit)
                {
                    BreakExpected = true;
                    BreakIfExpected();
                }
                else if ((SpaceExpectedForE && content.Substring(0, 1).ToLower().Equals("e"))
                    || (SpaceExpectedForX && content.Substring(0, 1).ToLower().Equals("x"))
                    || (SpaceExpectedForPlusMinus && content.Substring(0, 1).Equals("+"))
                    || (SpaceExpectedForPlusMinus && content.Substring(0, 1).Equals("-"))
                    )
                {
                    SpaceExpected = true;
                    SpaceIfExpected();
                }

                _currentLineLength += content.Length;
                if (RandomizeColor)
                {
                    int lengthWritten = 0;
                    while (lengthWritten < content.Length)
                    {
                        if (_currentColorLength == _currentColorLimit)
                        {
                            _currentColorLimit = _randomizer.Next(MIN_COLOR_WORD_LENGTH, MAX_COLOR_WORD_LENGTH);
                            _currentColor = string.Format("#{0:x2}{1:x2}{2:x2}", _randomizer.Next(0, 127), _randomizer.Next(0, 127), _randomizer.Next(0, 127));
                            _currentColorLength = 0;
                        }
                        
                        int writing;
                        if (content.Length - lengthWritten < _currentColorLimit - _currentColorLength)
                            writing = content.Length - lengthWritten;
                        else
                            writing = _currentColorLimit - _currentColorLength;

                        base.AddOutputContentRaw("<span style=\"color: ");
                        base.AddOutputContentRaw(_currentColor);
                        base.AddOutputContentRaw(";\">");
                        base.AddOutputContentRaw(Utils.HtmlEncode(content.Substring(lengthWritten, writing)));
                        base.AddOutputContentRaw("</span>");
                        lengthWritten += writing;
                        _currentColorLength += writing;
                    }
                }
                else
                {
                    base.AddOutputContent(content, null);
                }
                SetSpaceNoLongerExpected();
            }

            private void SetSpaceNoLongerExpected()
            {
                SpaceExpected = false;
                SpaceExpectedForAnsiString = false;
                SpaceExpectedForE = false;
                SpaceExpectedForX = false;
                SpaceExpectedForPlusMinus = false;
            }

            public override void AddOutputLineBreak()
            {
                //don't want the outer code to write line breaks at all
                throw new NotSupportedException();
            }
        }
    }
}
