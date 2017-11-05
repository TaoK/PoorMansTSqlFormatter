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
using System.Text;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Tokenizers
{
    public class TSqlStandardTokenizer : ISqlTokenizer
    {
        private class TokenizationState
        {
            public TokenList TokenContainer { get; private set; } = new TokenList();
            public SimplifiedStringReader InputReader { get; private set; } = null;
            public SqlTokenizationType? CurrentTokenizationType { get; set; } = null;
            public StringBuilder CurrentTokenValue { get; set; } = new StringBuilder();
            public int CommentNesting { get; set; } = 0;
            public int CurrentCharInt { get; set; } = -1;
            public char CurrentChar
            {
                get
                {
                    if (CurrentCharInt < 0)
                        throw new InvalidOperationException("No character has been read from the stream");

                    if (!HasUnprocessedCurrentCharacter)
                        throw new InvalidOperationException("The current character has already been consumed");

                    return (char)CurrentCharInt;
                }
            }
            public long? RequestedMarkerPosition { get; private set; }
            public bool HasUnprocessedCurrentCharacter { get; set; }

            public TokenizationState(string inputSQL, long? requestedMarkerPosition)
            {
                if (requestedMarkerPosition > inputSQL.Length)
                    throw new ArgumentException("Requested marker position cannot be beyond the end of the input string", "requestedMarkerPosition");

                InputReader = new SimplifiedStringReader(inputSQL);
                RequestedMarkerPosition = requestedMarkerPosition;
            }

            internal void ReadNextCharacter()
            {
                if (HasUnprocessedCurrentCharacter)
                    throw new Exception("Unprocessed character detected!");

                CurrentCharInt = InputReader.Read();

                if (CurrentCharInt >= 0)
                    HasUnprocessedCurrentCharacter = true;
            }

            internal void ConsumeCurrentCharacterIntoToken()
            {
                if (!HasUnprocessedCurrentCharacter)
                    throw new Exception("No current character to consume!");

                CurrentTokenValue.Append(CurrentChar);
                HasUnprocessedCurrentCharacter = false;
            }

            internal void HasRemainingCharacters()
            {
                if (!HasUnprocessedCurrentCharacter)
                    throw new Exception("No current character to consume!");

                CurrentTokenValue.Append(CurrentChar);
                HasUnprocessedCurrentCharacter = false;
            }


            internal void DiscardNextCharacter()
            {
                ReadNextCharacter();
                HasUnprocessedCurrentCharacter = false;
            }

        }

        public ITokenList TokenizeSQL(string inputSQL)
        {
            return TokenizeSQL(inputSQL, null);
        }

        public ITokenList TokenizeSQL(string inputSQL, long? requestedMarkerPosition)
        {
            var state = new TokenizationState(inputSQL, requestedMarkerPosition);

            state.ReadNextCharacter();
            while (state.HasUnprocessedCurrentCharacter)
            {
                if (state.CurrentTokenizationType == null)
                {
                    ProcessOrOpenToken(state);
                    state.ReadNextCharacter();
                    continue;
                }

                switch (state.CurrentTokenizationType.Value)
                {
                    case SqlTokenizationType.WhiteSpace:
                        if (IsWhitespace(state.CurrentChar))
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SinglePeriod:
                        if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
                            state.CurrentTokenValue.Append('.');
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            state.CurrentTokenValue.Append('.');
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleZero:
                        if (state.CurrentChar == 'x' || state.CurrentChar == 'X')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.BinaryValue;
                            state.CurrentTokenValue.Append('0');
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.Number;
                            state.CurrentTokenValue.Append('0');
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar == '.')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
                            state.CurrentTokenValue.Append('0');
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            state.CurrentTokenValue.Append('0');
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.Number:
                        if (state.CurrentChar == 'e' || state.CurrentChar == 'E')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.FloatValue;
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar == '.')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.DecimalValue:
                        if (state.CurrentChar == 'e' || state.CurrentChar == 'E')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.FloatValue;
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.FloatValue:
                        if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if ((state.CurrentChar == '-' || state.CurrentChar == '+') && state.CurrentTokenValue.ToString().ToUpper().EndsWith("E"))
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.BinaryValue:
                        if ((state.CurrentChar >= '0' && state.CurrentChar <= '9')
                            || (state.CurrentChar >= 'A' && state.CurrentChar <= 'F')
                            || (state.CurrentChar >= 'a' && state.CurrentChar <= 'f')
                            )
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleDollar:
                        state.CurrentTokenValue.Append('$');

                        if ((state.CurrentChar >= 'A' && state.CurrentChar <= 'Z')
                            || (state.CurrentChar >= 'a' && state.CurrentChar <= 'z')
                            )
                            state.CurrentTokenizationType = SqlTokenizationType.PseudoName;
                        else
                            state.CurrentTokenizationType = SqlTokenizationType.MonetaryValue;

                        state.ConsumeCurrentCharacterIntoToken();
                        break;

                    case SqlTokenizationType.MonetaryValue:
                        if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar == '-' && state.CurrentTokenValue.Length == 1)
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else if (state.CurrentChar == '.' && !state.CurrentTokenValue.ToString().Contains("."))
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleHyphen:
                        if (state.CurrentChar == '-')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.SingleLineComment;
                            state.HasUnprocessedCurrentCharacter = false; //DISCARDING the hyphen because of weird standard
                        }
                        else if (state.CurrentChar == '=')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            state.CurrentTokenValue.Append('-');
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            state.CurrentTokenValue.Append('-');
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleSlash:
                        if (state.CurrentChar == '*')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.BlockComment;
                            state.HasUnprocessedCurrentCharacter = false; //DISCARDING the asterisk because of weird standard
                            state.CommentNesting++;
                        }
                        else if (state.CurrentChar == '/')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.SingleLineCommentCStyle;
                            state.HasUnprocessedCurrentCharacter = false; //DISCARDING the slash because of weird standard
                        }
                        else if (state.CurrentChar == '=')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            state.CurrentTokenValue.Append('/');
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            state.CurrentTokenValue.Append('/');
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleLineComment:
                    case SqlTokenizationType.SingleLineCommentCStyle:
                        if (state.CurrentChar == (char)13 || state.CurrentChar == (char)10)
                        {
                            int nextCharInt = state.InputReader.Peek();
                            if (state.CurrentChar == (char)13 && nextCharInt == 10)
                            {
                                state.ConsumeCurrentCharacterIntoToken();
                                state.ReadNextCharacter();
                            }
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        break;

                    case SqlTokenizationType.BlockComment:
                        if (state.CurrentChar == '*')
                        {
                            if (state.InputReader.Peek() == (int)'/')
                            {
                                state.CommentNesting--;
                                if (state.CommentNesting > 0)
                                {
                                    state.ConsumeCurrentCharacterIntoToken();
                                    state.ReadNextCharacter();
                                    state.ConsumeCurrentCharacterIntoToken();
                                }
                                else
                                {
                                    state.HasUnprocessedCurrentCharacter = false; //discarding the asterisk
                                    state.ReadNextCharacter();
                                    //TODO: DANGER DANGER why do "contained" token types have this inconsistent handling where the delimiters are not in the value???
                                    SwallowOutstandingCharacterAndCompleteToken(state);
                                }
                            }
                            else
                            {
                                state.ConsumeCurrentCharacterIntoToken();
                            }
                        }
                        else
                        {
                            if (state.CurrentChar == '/' && state.InputReader.Peek() == (int)'*')
                            {
                                state.ConsumeCurrentCharacterIntoToken();
                                state.ReadNextCharacter();
                                state.ConsumeCurrentCharacterIntoToken();
                                state.CommentNesting++;
                            }
                            else
                            {
                                state.ConsumeCurrentCharacterIntoToken();
                            }
                        }
                        break;

                    case SqlTokenizationType.OtherNode:
                    case SqlTokenizationType.PseudoName:
                        if (IsNonWordCharacter(state.CurrentChar))
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        else
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        break;

                    case SqlTokenizationType.SingleN:
                        if (state.CurrentChar == '\'')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.NString;
                            state.HasUnprocessedCurrentCharacter = false; //DISCARDING the apostrophe because of weird standard
                        }
                        else
                        {
							if (IsNonWordCharacter(state.CurrentChar))
							{
                                CompleteTokenAndProcessNext(state);
                            }
                            else
							{
								state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
								state.CurrentTokenValue.Append('N');
                                state.ConsumeCurrentCharacterIntoToken();
                            }
                        }
                        break;

                    case SqlTokenizationType.NString:
                    case SqlTokenizationType.String:
                        if (state.CurrentChar == '\'')
                        {
                            if (state.InputReader.Peek() == (int)'\'')
                            {
                                //add the character (once)
                                state.ConsumeCurrentCharacterIntoToken();

                                //throw away the second character... because (for some reason?) we're storing the effective value" rather than the raw token...
                                state.DiscardNextCharacter();
                            }
                            else
                            {
                                //TODO: DANGER DANGER why do "contained" token types have this inconsistent handling where the delimiters are not in the value???
                                SwallowOutstandingCharacterAndCompleteToken(state);
                            }
                        }
                        else
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        break;

                    case SqlTokenizationType.QuotedString:
                        if (state.CurrentChar == '"')
                        {
                            if (state.InputReader.Peek() == (int)'"')
                            {
                                //add the character (once)
                                state.ConsumeCurrentCharacterIntoToken();

                                //throw away the second character... because (for some reason?) we're storing the effective value" rather than the raw token...
                                state.DiscardNextCharacter();
                            }
                            else
                            {
                                //TODO: DANGER DANGER why do "contained" token types have this inconsistent handling where the delimiters are not in the value???
                                SwallowOutstandingCharacterAndCompleteToken(state);
                            }
                        }
                        else
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        break;

                    case SqlTokenizationType.BracketQuotedName:
                        if (state.CurrentChar == ']')
                        {
                            if (state.InputReader.Peek() == (int)']')
                            {
                                //add the character (once)
                                state.ConsumeCurrentCharacterIntoToken();

                                //throw away the second character... because (for some reason?) we're storing the effective value" rather than the raw token...
                                state.DiscardNextCharacter();
                            }
                            else
                            {
                                //TODO: DANGER DANGER why do "contained" token types have this inconsistent handling where the delimiters are not in the value???
                                SwallowOutstandingCharacterAndCompleteToken(state);
                            }
                        }
                        else
                        {
                            state.ConsumeCurrentCharacterIntoToken();
                        }
                        break;

                    case SqlTokenizationType.SingleLT:
                        state.CurrentTokenValue.Append('<');
                        state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                        if (state.CurrentChar == '=' || state.CurrentChar == '>' || state.CurrentChar == '<')
                        {
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleGT:
                        state.CurrentTokenValue.Append('>');
                        state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                        if (state.CurrentChar == '=' || state.CurrentChar == '>')
                        {
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleAsterisk:
                        state.CurrentTokenValue.Append('*');
                        if (state.CurrentChar == '=')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleOtherCompoundableOperator:
                        state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                        if (state.CurrentChar == '=')
                        {
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SinglePipe:
                        state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                        state.CurrentTokenValue.Append('|');
                        if (state.CurrentChar == '=' || state.CurrentChar == '|')
                        {
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleEquals:
                        state.CurrentTokenValue.Append('=');
                        if (state.CurrentChar == '=')
                        {
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    case SqlTokenizationType.SingleExclamation:
                        state.CurrentTokenValue.Append('!');
                        if (state.CurrentChar == '=' || state.CurrentChar == '<' || state.CurrentChar == '>')
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
                            AppendCharAndCompleteToken(state);
                        }
                        else
                        {
                            state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
                            CompleteTokenAndProcessNext(state);
                        }
                        break;

                    default:
                        throw new Exception("In-progress node unrecognized!");
                }

                state.ReadNextCharacter();
            }


            if (state.CurrentTokenizationType != null)
            {
                if (state.CurrentTokenizationType.Value == SqlTokenizationType.BlockComment
                    || state.CurrentTokenizationType.Value == SqlTokenizationType.String
                    || state.CurrentTokenizationType.Value == SqlTokenizationType.NString
                    || state.CurrentTokenizationType.Value == SqlTokenizationType.QuotedString
                    || state.CurrentTokenizationType.Value == SqlTokenizationType.BracketQuotedName
                    )
                    state.TokenContainer.HasUnfinishedToken = true;

                SwallowOutstandingCharacterAndCompleteToken(state);
            }

            return state.TokenContainer;
        }

        private static bool IsWhitespace(char targetCharacter)
        {
            return (targetCharacter == ' '
                || targetCharacter == '\t'
                || targetCharacter == (char)10
                || targetCharacter == (char)13
                );
        }

        private static bool IsNonWordCharacter(char currentCharacter)
        {
            //characters that pop you out of a regular "word" context (maybe into a new word)
            return (IsWhitespace(currentCharacter)
                || IsOperatorCharacter(currentCharacter)
                || (IsCurrencyPrefix(currentCharacter) && currentCharacter != '$')
                || currentCharacter == '\''
                || currentCharacter == '"'
                || currentCharacter == ','
                || currentCharacter == '.'
                || currentCharacter == '['
                || currentCharacter == '('
                || currentCharacter == ')'
                || currentCharacter == '!'
                || currentCharacter == ';'
                || currentCharacter == ':'
                );
        }

        private static bool IsCompoundableOperatorCharacter(char currentCharacter)
        {
            //operator characters that can be compounded by a subsequent "equals" sign
            return (currentCharacter == '/'
                || currentCharacter == '-'
                || currentCharacter == '+'
                || currentCharacter == '*'
                || currentCharacter == '%'
                || currentCharacter == '&'
                || currentCharacter == '^'
                || currentCharacter == '<'
				|| currentCharacter == '>'
				|| currentCharacter == '|'
				);
        }

        private static bool IsOperatorCharacter(char currentCharacter)
        {
            //operator characters
            return (currentCharacter == '/'
                || currentCharacter == '-'
                || currentCharacter == '+'
                || currentCharacter == '%'
                || currentCharacter == '*'
                || currentCharacter == '&'
                || currentCharacter == '|'
                || currentCharacter == '^'
                || currentCharacter == '='
                || currentCharacter == '<'
                || currentCharacter == '>'
                || currentCharacter == '~'
                );
        }

        private static bool IsCurrencyPrefix(char currentCharacter)
        {
            //symbols that SQL Server recognizes as currency prefixes - these also happen to 
            // be word-breakers, except the dollar. Ref:
            // http://msdn.microsoft.com/en-us/library/ms188688.aspx
            return (currentCharacter == (char)0x0024
                || currentCharacter == (char)0x00A2
                || currentCharacter == (char)0x00A3
                || currentCharacter == (char)0x00A4
                || currentCharacter == (char)0x00A5
                || currentCharacter == (char)0x09F2
                || currentCharacter == (char)0x09F3
                || currentCharacter == (char)0x0E3F
                || currentCharacter == (char)0x17DB
                || currentCharacter == (char)0x20A0
                || currentCharacter == (char)0x20A1
                || currentCharacter == (char)0x20A2
                || currentCharacter == (char)0x20A3
                || currentCharacter == (char)0x20A4
                || currentCharacter == (char)0x20A5
                || currentCharacter == (char)0x20A6
                || currentCharacter == (char)0x20A7
                || currentCharacter == (char)0x20A8
                || currentCharacter == (char)0x20A9
                || currentCharacter == (char)0x20AA
                || currentCharacter == (char)0x20AB
                || currentCharacter == (char)0x20AC
                || currentCharacter == (char)0x20AD
                || currentCharacter == (char)0x20AE
                || currentCharacter == (char)0x20AF
                || currentCharacter == (char)0x20B0
                || currentCharacter == (char)0x20B1
                || currentCharacter == (char)0xFDFC
                || currentCharacter == (char)0xFE69
                || currentCharacter == (char)0xFF04
                || currentCharacter == (char)0xFFE0
                || currentCharacter == (char)0xFFE1
                || currentCharacter == (char)0xFFE5
                || currentCharacter == (char)0xFFE6
                );
        }

        private static void CompleteTokenAndProcessNext(TokenizationState state)
        {
            CompleteToken(state, true);
            ProcessOrOpenToken(state);
        }

        private static void AppendCharAndCompleteToken(TokenizationState state)
        {
            state.ConsumeCurrentCharacterIntoToken();
            CompleteToken(state, false);
        }

        private static void SwallowOutstandingCharacterAndCompleteToken(TokenizationState state)
        {
            //this is for cases where we *know* we are swallowing the "current character" (not putting it in the output)
            state.HasUnprocessedCurrentCharacter = false;
            CompleteToken(state, false);
        }

        private static void ProcessOrOpenToken(TokenizationState state)
        {
            if (state.CurrentTokenizationType != null)
                throw new Exception("Cannot start a new Token: existing Tokenization Type is not null");

            if (!state.HasUnprocessedCurrentCharacter)
                throw new Exception("Cannot start a new Token: no (outstanding) current character specified!");

            //start a new value.
            state.CurrentTokenValue.Length = 0;

            if (IsWhitespace(state.CurrentChar))
            {
                state.CurrentTokenizationType = SqlTokenizationType.WhiteSpace;
                state.ConsumeCurrentCharacterIntoToken();
            }
            else if (state.CurrentChar == '-')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleHyphen;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '$')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleDollar;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '/')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleSlash;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == 'N')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleN;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later except N-string case
            }
            else if (state.CurrentChar == '\'')
            {
                state.CurrentTokenizationType = SqlTokenizationType.String;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing
            }
            else if (state.CurrentChar == '"')
            {
                state.CurrentTokenizationType = SqlTokenizationType.QuotedString;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing
            }
            else if (state.CurrentChar == '[')
            {
                state.CurrentTokenizationType = SqlTokenizationType.BracketQuotedName;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing
            }
            else if (state.CurrentChar == '(')
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.OpenParens);
            }
            else if (state.CurrentChar == ')')
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.CloseParens);
            }
            else if (state.CurrentChar == ',')
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.Comma);
            }
            else if (state.CurrentChar == '.')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SinglePeriod;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '0')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleZero;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar >= '1' && state.CurrentChar <= '9')
            {
                state.CurrentTokenizationType = SqlTokenizationType.Number;
                state.ConsumeCurrentCharacterIntoToken();
            }
            else if (IsCurrencyPrefix(state.CurrentChar))
            {
                state.CurrentTokenizationType = SqlTokenizationType.MonetaryValue;
                state.ConsumeCurrentCharacterIntoToken();
            }
            else if (state.CurrentChar == ';')
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.Semicolon);
            }
            else if (state.CurrentChar == ':')
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.Colon);
            }
            else if (state.CurrentChar == '*')
            {
				state.CurrentTokenizationType = SqlTokenizationType.SingleAsterisk;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '=')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleEquals;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '<')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleLT;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '>')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleGT;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '!')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleExclamation;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (state.CurrentChar == '|')
            {
                state.CurrentTokenizationType = SqlTokenizationType.SinglePipe;
                state.HasUnprocessedCurrentCharacter = false; //purposefully swallowing, will be reinserted later
            }
            else if (IsCompoundableOperatorCharacter(state.CurrentChar))
            {
                state.CurrentTokenizationType = SqlTokenizationType.SingleOtherCompoundableOperator;
                state.ConsumeCurrentCharacterIntoToken();
            }
            else if (IsOperatorCharacter(state.CurrentChar))
            {
                SaveCurrentCharToNewToken(state, SqlTokenType.OtherOperator);
            }
            else
            {
                state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
                state.ConsumeCurrentCharacterIntoToken();
            }
        }

        private static void CompleteToken(TokenizationState state, bool nextCharRead)
        {
            if (state.CurrentTokenizationType == null)
                throw new Exception("Cannot complete Token, as there is no current Tokenization Type");

            switch (state.CurrentTokenizationType)
            {
				case SqlTokenizationType.BlockComment:
                    SaveToken(state, SqlTokenType.MultiLineComment, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.OtherNode:
                    SaveToken(state, SqlTokenType.OtherNode, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.PseudoName:
                    SaveToken(state, SqlTokenType.PseudoName, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleLineComment:
                    SaveToken(state, SqlTokenType.SingleLineComment, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleLineCommentCStyle:
                    SaveToken(state, SqlTokenType.SingleLineCommentCStyle, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleHyphen:
                    SaveToken(state, SqlTokenType.OtherOperator, "-");
                    break;

                case SqlTokenizationType.SingleDollar:
                    SaveToken(state, SqlTokenType.MonetaryValue, "$");
                    break;

                case SqlTokenizationType.SingleSlash:
                    SaveToken(state, SqlTokenType.OtherOperator, "/");
                    break;

                case SqlTokenizationType.WhiteSpace:
                    SaveToken(state, SqlTokenType.WhiteSpace, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleN:
                    SaveToken(state, SqlTokenType.OtherNode, "N");
                    break;

                case SqlTokenizationType.SingleExclamation:
                    SaveToken(state, SqlTokenType.OtherNode, "!");
                    break;

				case SqlTokenizationType.SinglePipe:
                    SaveToken(state, SqlTokenType.OtherNode, "|");
					break;

				case SqlTokenizationType.SingleGT:
                    SaveToken(state, SqlTokenType.OtherOperator, ">");
					break;

				case SqlTokenizationType.SingleLT:
                    SaveToken(state, SqlTokenType.OtherOperator, "<");
					break;

				case SqlTokenizationType.NString:
                    SaveToken(state, SqlTokenType.NationalString, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.String:
                    SaveToken(state, SqlTokenType.String, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.QuotedString:
                    SaveToken(state, SqlTokenType.QuotedString, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.BracketQuotedName:
                    SaveToken(state, SqlTokenType.BracketQuotedName, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.OtherOperator:
                case SqlTokenizationType.SingleOtherCompoundableOperator:
                    SaveToken(state, SqlTokenType.OtherOperator, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleZero:
                    SaveToken(state, SqlTokenType.Number, "0");
                    break;

                case SqlTokenizationType.SinglePeriod:
                    SaveToken(state, SqlTokenType.Period, ".");
                    break;

                case SqlTokenizationType.SingleAsterisk:
                    SaveToken(state, SqlTokenType.Asterisk, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.SingleEquals:
                    SaveToken(state, SqlTokenType.EqualsSign, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.Number:
                case SqlTokenizationType.DecimalValue:
                case SqlTokenizationType.FloatValue:
                    SaveToken(state, SqlTokenType.Number, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.BinaryValue:
                    SaveToken(state, SqlTokenType.BinaryValue, state.CurrentTokenValue.ToString());
                    break;

                case SqlTokenizationType.MonetaryValue:
                    SaveToken(state, SqlTokenType.MonetaryValue, state.CurrentTokenValue.ToString());
                    break;

                default:
                    throw new Exception("Unrecognized SQL Node Type");
            }

            state.CurrentTokenizationType = null;
        }

        private static void SaveCurrentCharToNewToken(TokenizationState state, SqlTokenType tokenType)
        {
            char charToSave = state.CurrentChar;
            state.HasUnprocessedCurrentCharacter = false; //because we're using it now!
            SaveToken(state, tokenType, charToSave.ToString());
        }

        private static void SaveToken(TokenizationState state, SqlTokenType tokenType, string tokenValue)
        {
            var foundToken = new Token(tokenType, tokenValue);
            state.TokenContainer.Add(foundToken);

            long positionOfLastCharacterInToken = state.InputReader.LastCharacterPosition - (state.HasUnprocessedCurrentCharacter ? 1 : 0);
            if (state.RequestedMarkerPosition != null
                && state.TokenContainer.MarkerToken == null
                && state.RequestedMarkerPosition <= positionOfLastCharacterInToken
                )
            {
                state.TokenContainer.MarkerToken = foundToken;
                //TODO: this is wrong for container types, as commented elsewhere. the marker position will be too high.
                var rawPositionInToken = foundToken.Value.Length - (positionOfLastCharacterInToken - state.RequestedMarkerPosition);
                // temporarily bypass overflow issues without fixing underlying problem
                state.TokenContainer.MarkerPosition = rawPositionInToken > foundToken.Value.Length ? foundToken.Value.Length : rawPositionInToken;
            }
        }

        public enum SqlTokenizationType
        {
            //variable-length types
            WhiteSpace,
            OtherNode,
            SingleLineComment,
            SingleLineCommentCStyle,
            BlockComment,
            String,
            NString,
            QuotedString,
            BracketQuotedName,
            OtherOperator,
            Number,
            BinaryValue,
            MonetaryValue,
            DecimalValue,
            FloatValue,
            PseudoName,

            //temporary types
            SingleAsterisk,
            SingleDollar,
            SingleHyphen,
            SingleSlash,
            SingleN,
            SingleLT,
            SingleGT,
            SingleExclamation,
            SinglePeriod,
            SingleZero,
            SinglePipe,
            SingleEquals,
            SingleOtherCompoundableOperator
        }

    }
}
