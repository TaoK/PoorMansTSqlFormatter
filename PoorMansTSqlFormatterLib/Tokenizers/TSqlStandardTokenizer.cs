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
using System.Text;
using System.Xml;
using System.IO;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Tokenizers
{
    public class TSqlStandardTokenizer : ISqlTokenizer
    {
        public Interfaces.ITokenList TokenizeSQL(string inputSQL)
        {
            TokenList tokenContainer = new TokenList();
            StringReader inputReader = new StringReader(inputSQL);
            SqlTokenizationType? currentTokenizationType;
            StringBuilder currentTokenValue = new StringBuilder();
            int commentNesting;

            currentTokenizationType = null;
            currentTokenValue.Length = 0;
            commentNesting = 0;

            int currentCharInt = inputReader.Read();
            while (currentCharInt >= 0)
            {
                char currentCharacter = (char)currentCharInt;
                if (currentTokenizationType == null)
                {
                    ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                }
                else
                {
                    switch (currentTokenizationType.Value)
                    {
                        case SqlTokenizationType.WhiteSpace:
                            if (IsWhitespace(currentCharacter))
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SinglePeriod:
                            if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenizationType = SqlTokenizationType.DecimalValue;
                                currentTokenValue.Append('.');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenValue.Append('.');
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleZero:
                            if (currentCharacter == 'x' || currentCharacter == 'X')
                            {
                                currentTokenizationType = SqlTokenizationType.BinaryValue;
                                currentTokenValue.Append('0');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenizationType = SqlTokenizationType.Number;
                                currentTokenValue.Append('0');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter == '.')
                            {
                                currentTokenizationType = SqlTokenizationType.DecimalValue;
                                currentTokenValue.Append('0');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenValue.Append('0');
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.Number:
                            if (currentCharacter == 'e' || currentCharacter == 'E')
                            {
                                currentTokenizationType = SqlTokenizationType.FloatValue;
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter == '.')
                            {
                                currentTokenizationType = SqlTokenizationType.DecimalValue;
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.DecimalValue:
                            if (currentCharacter == 'e' || currentCharacter == 'E')
                            {
                                currentTokenizationType = SqlTokenizationType.FloatValue;
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.FloatValue:
                            if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter == '-' && currentTokenValue.ToString().EndsWith("e", StringComparison.OrdinalIgnoreCase))
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.BinaryValue:
                            if ((currentCharacter >= '0' && currentCharacter <= '9')
                                || (currentCharacter >= 'A' && currentCharacter <= 'F')
                                || (currentCharacter >= 'a' && currentCharacter <= 'f')
                                )
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleDollar:
                            currentTokenValue.Append('$');
                            currentTokenValue.Append(currentCharacter);

                            if ((currentCharacter >= 'A' && currentCharacter <= 'Z')
                                || (currentCharacter >= 'a' && currentCharacter <= 'z')
                                )
                                currentTokenizationType = SqlTokenizationType.PseudoName;
                            else
                                currentTokenizationType = SqlTokenizationType.MonetaryValue;

                            break;

                        case SqlTokenizationType.MonetaryValue:
                            if (currentCharacter >= '0' && currentCharacter <= '9')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter == '-' && currentTokenValue.Length == 1)
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else if (currentCharacter == '.' && !currentTokenValue.ToString().Contains("."))
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleHyphen:
                            if (currentCharacter == '-')
                            {
                                currentTokenizationType = SqlTokenizationType.SingleLineComment;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('-');
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleSlash:
                            if (currentCharacter == '*')
                            {
                                currentTokenizationType = SqlTokenizationType.BlockComment;
                                commentNesting++;
                            }
                            else if (currentCharacter == '/')
                            {
                                currentTokenizationType = SqlTokenizationType.SingleLineCommentCStyle;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('/');
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('/');
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleLineComment:
                        case SqlTokenizationType.SingleLineCommentCStyle:
                            if (currentCharacter == (char)13 || currentCharacter == (char)10)
                            {
                                currentTokenValue.Append(currentCharacter);

                                int nextCharInt = inputReader.Peek();
                                if (currentCharacter == (char)13 && nextCharInt == 10)
                                    currentTokenValue.Append((char)inputReader.Read());

                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.BlockComment:
                            if (currentCharacter == '*')
                            {
                                if (inputReader.Peek() == (int)'/')
                                {
                                    commentNesting--;
                                    char nextCharacter = (char)inputReader.Read();
                                    if (commentNesting > 0)
                                    {
                                        currentTokenValue.Append(currentCharacter);
                                        currentTokenValue.Append(nextCharacter);
                                    }
                                    else
                                    {
                                        CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                    }
                                }
                                else
                                {
                                    currentTokenValue.Append(currentCharacter);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);

                                if (currentCharacter == '/' && inputReader.Peek() == (int)'*')
                                {
                                    currentTokenValue.Append((char)inputReader.Read());
                                    commentNesting++;
                                }
                            }
                            break;

                        case SqlTokenizationType.OtherNode:
                        case SqlTokenizationType.PseudoName:
                            if (IsNonWordCharacter(currentCharacter))
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.SingleN:
                            if (currentCharacter == '\'')
                            {
                                currentTokenizationType = SqlTokenizationType.NString;
                            }
                            else
                            {
								if (IsNonWordCharacter(currentCharacter))
								{
									CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
									ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
								}
								else
								{
									currentTokenizationType = SqlTokenizationType.OtherNode;
									currentTokenValue.Append('N');
									currentTokenValue.Append(currentCharacter);
								}
                            }
                            break;

                        case SqlTokenizationType.NString:
                        case SqlTokenizationType.String:
                            if (currentCharacter == '\'')
                            {
                                if (inputReader.Peek() == (int)'\'')
                                {
                                    inputReader.Read();
                                    currentTokenValue.Append(currentCharacter);
                                }
                                else
                                {
                                    CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.QuotedString:
                            if (currentCharacter == '"')
                            {
                                if (inputReader.Peek() == (int)'"')
                                {
                                    inputReader.Read();
                                    currentTokenValue.Append(currentCharacter);
                                }
                                else
                                {
                                    CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.BracketQuotedName:
                            if (currentCharacter == ']')
                            {
                                if (inputReader.Peek() == (int)']')
                                {
                                    inputReader.Read();
                                    currentTokenValue.Append(currentCharacter);
                                }
                                else
                                {
                                    CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.SingleLT:
                            currentTokenValue.Append('<');
                            currentTokenizationType = SqlTokenizationType.OtherOperator;
                            if (currentCharacter == '=' || currentCharacter == '>' || currentCharacter == '<')
                            {
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleGT:
                            currentTokenValue.Append('>');
                            currentTokenizationType = SqlTokenizationType.OtherOperator;
                            if (currentCharacter == '=' || currentCharacter == '>')
                            {
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleAsterisk:
                            currentTokenValue.Append('*');
                            if (currentCharacter == '=')
                            {
                                currentTokenValue.Append(currentCharacter);
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleOtherCompoundableOperator:
                            currentTokenizationType = SqlTokenizationType.OtherOperator;
                            if (currentCharacter == '=')
                            {
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SinglePipe:
                            currentTokenizationType = SqlTokenizationType.OtherOperator;
                            currentTokenValue.Append('|');
                            if (currentCharacter == '=' || currentCharacter == '|')
                            {
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleEquals:
                            currentTokenValue.Append('=');
                            if (currentCharacter == '=')
                            {
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        case SqlTokenizationType.SingleExclamation:
                            currentTokenValue.Append('!');
                            if (currentCharacter == '=' || currentCharacter == '<' || currentCharacter == '>')
                            {
                                currentTokenizationType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append(currentCharacter);
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                currentTokenizationType = SqlTokenizationType.OtherNode;
                                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                ProcessOrOpenToken(ref currentTokenizationType, currentTokenValue, currentCharacter, tokenContainer);
                            }
                            break;

                        default:
                            throw new Exception("In-progress node unrecognized!");
                    }
                }

                currentCharInt = inputReader.Read();
            }


            if (currentTokenizationType != null)
            {
                if (currentTokenizationType.Value == SqlTokenizationType.BlockComment
                    || currentTokenizationType.Value == SqlTokenizationType.String
                    || currentTokenizationType.Value == SqlTokenizationType.NString
                    || currentTokenizationType.Value == SqlTokenizationType.QuotedString
                    || currentTokenizationType.Value == SqlTokenizationType.BracketQuotedName
                    )
                    tokenContainer.HasUnfinishedToken = true;

                CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
            }

            return tokenContainer;
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

        private static void ProcessOrOpenToken(ref SqlTokenizationType? currentTokenizationType, StringBuilder currentNodeValue, char currentCharacter, ITokenList tokenContainer)
        {

            if (currentTokenizationType != null)
                throw new Exception("Cannot start a new Token: existing Tokenization Type is not null");

            //start a new value.
            currentNodeValue.Length = 0;

            if (IsWhitespace(currentCharacter))
            {
                currentTokenizationType = SqlTokenizationType.WhiteSpace;
                currentNodeValue.Append(currentCharacter);
            }
            else if (currentCharacter == '-')
            {
                currentTokenizationType = SqlTokenizationType.SingleHyphen;
            }
            else if (currentCharacter == '$')
            {
                currentTokenizationType = SqlTokenizationType.SingleDollar;
            }
            else if (currentCharacter == '/')
            {
                currentTokenizationType = SqlTokenizationType.SingleSlash;
            }
            else if (currentCharacter == 'N')
            {
                currentTokenizationType = SqlTokenizationType.SingleN;
            }
            else if (currentCharacter == '\'')
            {
                currentTokenizationType = SqlTokenizationType.String;
            }
            else if (currentCharacter == '"')
            {
                currentTokenizationType = SqlTokenizationType.QuotedString;
            }
            else if (currentCharacter == '[')
            {
                currentTokenizationType = SqlTokenizationType.BracketQuotedName;
            }
            else if (currentCharacter == '(')
            {
                tokenContainer.Add(new Token(SqlTokenType.OpenParens, currentCharacter.ToString()));
            }
            else if (currentCharacter == ')')
            {
                tokenContainer.Add(new Token(SqlTokenType.CloseParens, currentCharacter.ToString()));
            }
            else if (currentCharacter == ',')
            {
                tokenContainer.Add(new Token(SqlTokenType.Comma, currentCharacter.ToString()));
            }
            else if (currentCharacter == '.')
            {
                currentTokenizationType = SqlTokenizationType.SinglePeriod;
            }
            else if (currentCharacter == '0')
            {
                currentTokenizationType = SqlTokenizationType.SingleZero;
            }
            else if (currentCharacter >= '1' && currentCharacter <= '9')
            {
                currentTokenizationType = SqlTokenizationType.Number;
                currentNodeValue.Append(currentCharacter);
            }
            else if (IsCurrencyPrefix(currentCharacter))
            {
                currentTokenizationType = SqlTokenizationType.MonetaryValue;
                currentNodeValue.Append(currentCharacter);
            }
            else if (currentCharacter == ';')
            {
                tokenContainer.Add(new Token(SqlTokenType.Semicolon, currentCharacter.ToString()));
            }
            else if (currentCharacter == ':')
            {
                tokenContainer.Add(new Token(SqlTokenType.Colon, currentCharacter.ToString()));
            }
            else if (currentCharacter == '*')
            {
				currentTokenizationType = SqlTokenizationType.SingleAsterisk;
			}
            else if (currentCharacter == '=')
            {
                currentTokenizationType = SqlTokenizationType.SingleEquals;
            }
            else if (currentCharacter == '<')
            {
                currentTokenizationType = SqlTokenizationType.SingleLT;
            }
            else if (currentCharacter == '>')
            {
                currentTokenizationType = SqlTokenizationType.SingleGT;
            }
            else if (currentCharacter == '!')
            {
                currentTokenizationType = SqlTokenizationType.SingleExclamation;
            }
            else if (currentCharacter == '|')
            {
                currentTokenizationType = SqlTokenizationType.SinglePipe;
            }
            else if (IsCompoundableOperatorCharacter(currentCharacter))
            {
                currentTokenizationType = SqlTokenizationType.SingleOtherCompoundableOperator;
                currentNodeValue.Append(currentCharacter);
            }
            else if (IsOperatorCharacter(currentCharacter))
            {
                tokenContainer.Add(new Token(SqlTokenType.OtherOperator, currentCharacter.ToString()));
            }
            else
            {
                currentTokenizationType = SqlTokenizationType.OtherNode;
                currentNodeValue.Append(currentCharacter);
            }
        }

        private static void CompleteToken(ref SqlTokenizationType? currentTokenizationType, ITokenList tokenContainer, StringBuilder currentValue)
        {
            if (currentTokenizationType == null)
                throw new Exception("Cannot complete Token, as there is no current Tokenization Type");

            switch (currentTokenizationType)
            {


				case SqlTokenizationType.BlockComment:
                    tokenContainer.Add(new Token(SqlTokenType.MultiLineComment, currentValue.ToString()));
                    break;

                case SqlTokenizationType.OtherNode:
                    tokenContainer.Add(new Token(SqlTokenType.OtherNode, currentValue.ToString()));
                    break;

                case SqlTokenizationType.PseudoName:
                    tokenContainer.Add(new Token(SqlTokenType.PseudoName, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleLineComment:
                    tokenContainer.Add(new Token(SqlTokenType.SingleLineComment, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleLineCommentCStyle:
                    tokenContainer.Add(new Token(SqlTokenType.SingleLineCommentCStyle, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleHyphen:
                    tokenContainer.Add(new Token(SqlTokenType.OtherOperator, "-"));
                    break;

                case SqlTokenizationType.SingleDollar:
                    tokenContainer.Add(new Token(SqlTokenType.MonetaryValue, "$"));
                    break;

                case SqlTokenizationType.SingleSlash:
                    tokenContainer.Add(new Token(SqlTokenType.OtherOperator, "/"));
                    break;

                case SqlTokenizationType.WhiteSpace:
                    tokenContainer.Add(new Token(SqlTokenType.WhiteSpace, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleN:
                    tokenContainer.Add(new Token(SqlTokenType.OtherNode, "N"));
                    break;

                case SqlTokenizationType.SingleExclamation:
                    tokenContainer.Add(new Token(SqlTokenType.OtherNode, "!"));
                    break;

				case SqlTokenizationType.SinglePipe:
					tokenContainer.Add(new Token(SqlTokenType.OtherNode, "|"));
					break;

				case SqlTokenizationType.SingleGT:
					tokenContainer.Add(new Token(SqlTokenType.OtherOperator, ">"));
					break;

				case SqlTokenizationType.SingleLT:
					tokenContainer.Add(new Token(SqlTokenType.OtherOperator, "<"));
					break;

				case SqlTokenizationType.NString:
                    tokenContainer.Add(new Token(SqlTokenType.NationalString, currentValue.ToString()));
                    break;

                case SqlTokenizationType.String:
                    tokenContainer.Add(new Token(SqlTokenType.String, currentValue.ToString()));
                    break;

                case SqlTokenizationType.QuotedString:
                    tokenContainer.Add(new Token(SqlTokenType.QuotedString, currentValue.ToString()));
                    break;

                case SqlTokenizationType.BracketQuotedName:
                    tokenContainer.Add(new Token(SqlTokenType.BracketQuotedName, currentValue.ToString()));
                    break;

                case SqlTokenizationType.OtherOperator:
                case SqlTokenizationType.SingleOtherCompoundableOperator:
                    tokenContainer.Add(new Token(SqlTokenType.OtherOperator, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleZero:
                    tokenContainer.Add(new Token(SqlTokenType.Number, "0"));
                    break;

                case SqlTokenizationType.SinglePeriod:
                    tokenContainer.Add(new Token(SqlTokenType.Period, "."));
                    break;

                case SqlTokenizationType.SingleAsterisk:
                    tokenContainer.Add(new Token(SqlTokenType.Asterisk, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleEquals:
                    tokenContainer.Add(new Token(SqlTokenType.EqualsSign, currentValue.ToString()));
                    break;

                case SqlTokenizationType.Number:
                case SqlTokenizationType.DecimalValue:
                case SqlTokenizationType.FloatValue:
                    tokenContainer.Add(new Token(SqlTokenType.Number, currentValue.ToString()));
                    break;

                case SqlTokenizationType.BinaryValue:
                    tokenContainer.Add(new Token(SqlTokenType.BinaryValue, currentValue.ToString()));
                    break;

                case SqlTokenizationType.MonetaryValue:
                    tokenContainer.Add(new Token(SqlTokenType.MonetaryValue, currentValue.ToString()));
                    break;


                default:
                    throw new Exception("Unrecognized SQL Node Type");
            }

            currentTokenizationType = null;
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
