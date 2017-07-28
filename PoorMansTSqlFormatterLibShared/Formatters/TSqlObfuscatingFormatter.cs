/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

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
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class TSqlObfuscatingFormatter : ISqlTreeFormatter
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

            ErrorOutputPrefix = MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;

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

        public string FormatSQLTree(Node sqlTreeDoc)
        {
            //thread-safe - each call to FormatSQLTree() gets its own independent state object
            TSqlObfuscatingFormattingState state = new TSqlObfuscatingFormattingState(RandomizeColor, RandomizeLineLength);

            if (sqlTreeDoc.Name == SqlStructureConstants.ENAME_SQL_ROOT && sqlTreeDoc.GetAttributeValue(SqlStructureConstants.ANAME_ERRORFOUND) == "1")
                state.AddOutputContent(ErrorOutputPrefix);

            ProcessSqlNodeList(sqlTreeDoc.Children, state);
            state.BreakIfExpected();
            return state.DumpOutput();
        }

        private void ProcessSqlNodeList(IEnumerable<Node> rootList, TSqlObfuscatingFormattingState state)
        {
            foreach (Node contentElement in rootList)
                ProcessSqlNode(contentElement, state);
        }

        private void ProcessSqlNode(Node contentElement, TSqlObfuscatingFormattingState state)
        {
            switch (contentElement.Name)
            {
                case SqlStructureConstants.ENAME_SQL_ROOT:
                case SqlStructureConstants.ENAME_SQL_STATEMENT:
                case SqlStructureConstants.ENAME_SQL_CLAUSE:
                case SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE:
                case SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK:
                case SqlStructureConstants.ENAME_DDL_OTHER_BLOCK:
                case SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK:
                case SqlStructureConstants.ENAME_CURSOR_DECLARATION:
                case SqlStructureConstants.ENAME_BEGIN_TRANSACTION:
                case SqlStructureConstants.ENAME_SAVE_TRANSACTION:
                case SqlStructureConstants.ENAME_COMMIT_TRANSACTION:
                case SqlStructureConstants.ENAME_ROLLBACK_TRANSACTION:
                case SqlStructureConstants.ENAME_CONTAINER_OPEN:
                case SqlStructureConstants.ENAME_CONTAINER_CLOSE:
                case SqlStructureConstants.ENAME_WHILE_LOOP:
                case SqlStructureConstants.ENAME_IF_STATEMENT:
                case SqlStructureConstants.ENAME_SELECTIONTARGET:
                case SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT:
                case SqlStructureConstants.ENAME_CTE_WITH_CLAUSE:
                case SqlStructureConstants.ENAME_PERMISSIONS_BLOCK:
                case SqlStructureConstants.ENAME_PERMISSIONS_DETAIL:
                case SqlStructureConstants.ENAME_MERGE_CLAUSE:
                case SqlStructureConstants.ENAME_MERGE_TARGET:
                case SqlStructureConstants.ENAME_CASE_INPUT:
                case SqlStructureConstants.ENAME_BOOLEAN_EXPRESSION:
                case SqlStructureConstants.ENAME_BETWEEN_LOWERBOUND:
                case SqlStructureConstants.ENAME_BETWEEN_UPPERBOUND:
                case SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT:
                case SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT:
                case SqlStructureConstants.ENAME_MERGE_ACTION:
                case SqlStructureConstants.ENAME_PERMISSIONS_TARGET:
                case SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT:
                case SqlStructureConstants.ENAME_DDL_WITH_CLAUSE:
                case SqlStructureConstants.ENAME_MERGE_CONDITION:
                case SqlStructureConstants.ENAME_MERGE_THEN:
                case SqlStructureConstants.ENAME_JOIN_ON_SECTION:
                case SqlStructureConstants.ENAME_CTE_ALIAS:
                case SqlStructureConstants.ENAME_ELSE_CLAUSE:
                case SqlStructureConstants.ENAME_DDL_AS_BLOCK:
                case SqlStructureConstants.ENAME_CURSOR_FOR_BLOCK:
                case SqlStructureConstants.ENAME_TRIGGER_CONDITION:
                case SqlStructureConstants.ENAME_CURSOR_FOR_OPTIONS:
                case SqlStructureConstants.ENAME_CTE_AS_BLOCK:
                case SqlStructureConstants.ENAME_DDL_RETURNS:
                case SqlStructureConstants.ENAME_MERGE_USING:
                case SqlStructureConstants.ENAME_MERGE_WHEN:
                case SqlStructureConstants.ENAME_BETWEEN_CONDITION:
                case SqlStructureConstants.ENAME_BEGIN_END_BLOCK:
                case SqlStructureConstants.ENAME_TRY_BLOCK:
                case SqlStructureConstants.ENAME_CATCH_BLOCK:
                case SqlStructureConstants.ENAME_CASE_STATEMENT:
                case SqlStructureConstants.ENAME_CASE_WHEN:
                case SqlStructureConstants.ENAME_CASE_THEN:
                case SqlStructureConstants.ENAME_CASE_ELSE:
                case SqlStructureConstants.ENAME_AND_OPERATOR:
                case SqlStructureConstants.ENAME_OR_OPERATOR:
                    //these are all containers, and therefore have no impact on obfuscated output.
                    ProcessSqlNodeList(contentElement.Children, state);
                    break;

                case SqlStructureConstants.ENAME_DDLDETAIL_PARENS:
				case SqlStructureConstants.ENAME_FUNCTION_PARENS:
				case SqlStructureConstants.ENAME_IN_PARENS:
				case SqlStructureConstants.ENAME_DDL_PARENS:
                case SqlStructureConstants.ENAME_EXPRESSION_PARENS:
                case SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS:
                    state.SpaceExpected = false;
                    state.AddOutputContent("(");
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.SpaceExpected = false;
                    state.SpaceExpectedForAnsiString = false;
                    state.AddOutputContent(")");
                    break;

                case SqlStructureConstants.ENAME_WHITESPACE:
                    //do nothing
                    break;

                case SqlStructureConstants.ENAME_COMMENT_MULTILINE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("/*" + contentElement.TextValue + "*/");
                    }
                    break;
                case SqlStructureConstants.ENAME_COMMENT_SINGLELINE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("--" + contentElement.TextValue.Replace("\r", "").Replace("\n", ""));
                        state.BreakExpected = true;
                    }
                    break;

                case SqlStructureConstants.ENAME_COMMENT_SINGLELINE_CSTYLE:
                    if (PreserveComments)
                    {
                        state.SpaceExpected = false;
                        state.AddOutputContent("//" + contentElement.TextValue.Replace("\r", "").Replace("\n", ""));
                        state.BreakExpected = true;
                    }
                    break;

                case SqlStructureConstants.ENAME_BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.BreakExpected = true;
                    break;

                case SqlStructureConstants.ENAME_STRING:
                    state.SpaceIfExpectedForAnsiString();
                    state.SpaceExpected = false;
                    state.AddOutputContent("'" + contentElement.TextValue.Replace("'", "''") + "'");
                    state.SpaceExpectedForAnsiString = true;
                    break;

                case SqlStructureConstants.ENAME_NSTRING:
                    state.AddOutputContent("N'" + contentElement.TextValue.Replace("'", "''") + "'");
                    state.SpaceExpectedForAnsiString = true;
                    break;

                case SqlStructureConstants.ENAME_BRACKET_QUOTED_NAME:
                    state.SpaceExpected = false;
                    state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
                    break;

                case SqlStructureConstants.ENAME_QUOTED_STRING:
                    state.SpaceExpected = false;
                    state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
                    break;

                case SqlStructureConstants.ENAME_COMMA:
                case SqlStructureConstants.ENAME_PERIOD:
                case SqlStructureConstants.ENAME_SEMICOLON:
                case SqlStructureConstants.ENAME_SCOPERESOLUTIONOPERATOR:
                case SqlStructureConstants.ENAME_ASTERISK:
                case SqlStructureConstants.ENAME_EQUALSSIGN:
                case SqlStructureConstants.ENAME_OTHEROPERATOR:
                    state.SpaceExpected = false;
                    state.AddOutputContent(contentElement.TextValue);
                    break;

                case SqlStructureConstants.ENAME_COMPOUNDKEYWORD:
                    state.AddOutputContent(FormatKeyword(contentElement.GetAttributeValue(SqlStructureConstants.ANAME_SIMPLETEXT)));
                    state.SpaceExpected = true;
                    break;


                case SqlStructureConstants.ENAME_LABEL:
                    state.AddOutputContent(contentElement.TextValue);
                    state.BreakExpected = true;
                    break;

                case SqlStructureConstants.ENAME_OTHERKEYWORD:
                case SqlStructureConstants.ENAME_ALPHAOPERATOR:
                case SqlStructureConstants.ENAME_DATATYPE_KEYWORD:
                case SqlStructureConstants.ENAME_PSEUDONAME:
                case SqlStructureConstants.ENAME_BINARY_VALUE:
                    state.AddOutputContent(FormatKeyword(contentElement.TextValue));
                    state.SpaceExpected = true;
                    break;

                case SqlStructureConstants.ENAME_NUMBER_VALUE:
                    state.AddOutputContent(FormatKeyword(contentElement.TextValue));
                    if (!contentElement.TextValue.ToLowerInvariant().Contains("e"))
                    {
                        state.SpaceExpectedForE = true;
                        if (contentElement.TextValue.Equals("0"))
                            state.SpaceExpectedForX = true;
                    }
                    break;

                case SqlStructureConstants.ENAME_MONETARY_VALUE:
                    if (!contentElement.TextValue.Substring(0, 1).Equals("$"))
                        state.SpaceExpected = false;

                    state.AddOutputContent(contentElement.TextValue);

                    if (contentElement.TextValue.Length == 1)
                        state.SpaceExpectedForPlusMinus = true;
                    break;

                case SqlStructureConstants.ENAME_OTHERNODE:
                case SqlStructureConstants.ENAME_FUNCTION_KEYWORD:
                    state.AddOutputContent(contentElement.TextValue);
                    state.SpaceExpected = true;
                    break;

                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }
        }

        private string FormatKeyword(string keyword)
        {
            string outputKeyword;
            if (!KeywordMapping.TryGetValue(keyword.ToUpperInvariant(), out outputKeyword))
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

                keywordCharArray[i] = _currentlyUppercase ? keywordCharArray[i].ToUpperInvariant() : keywordCharArray[i].ToLowerInvariant();
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
