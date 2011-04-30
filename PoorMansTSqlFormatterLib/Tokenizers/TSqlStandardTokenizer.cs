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
using System.Text;
using System.Xml;
using System.IO;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Tokenizers
{
    public class TSqlStandardTokenizer : ISqlTokenizer
    {
        /*
         * TODO:
         *  - WILL NEED TO RESEARCH QUOTED_IDENTIFIER
         *  - Future Extensions
         *    - Scope Resolution Operator (and/or colons in general?)
         *    - Compound operators (SQL 2008), those that end in equals.
         */

        public Interfaces.ITokenList TokenizeSQL(string inputSQL)
        {
            TokenList tokenContainer = new TokenList();
            StringReader inputReader = new StringReader(inputSQL);
            SqlTokenizationType? currentTokenizationType;
            StringBuilder currentTokenValue = new StringBuilder();

            currentTokenizationType = null;
            currentTokenValue.Length = 0;

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
                                int nextCharInt = inputReader.Peek();
                                if (nextCharInt == (int)'/')
                                {
                                    inputReader.Read();
                                    CompleteToken(ref currentTokenizationType, tokenContainer, currentTokenValue);
                                }
                                else
                                {
                                    currentTokenValue.Append(currentCharacter);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.OtherNode:
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
                                currentTokenizationType = SqlTokenizationType.OtherNode;
                                currentTokenValue.Append('N');
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.NString:
                        case SqlTokenizationType.String:
                            if (currentCharacter == '\'')
                            {
                                int nextCharInt = inputReader.Peek();
                                if (nextCharInt == (int)'\'')
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

                        case SqlTokenizationType.QuotedIdentifier:
                            if (currentCharacter == ']')
                            {
                                int nextCharInt = inputReader.Peek();
                                if (nextCharInt == (int)']')
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

                        case SqlTokenizationType.SingleGT:
                            currentTokenValue.Append('>');
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
                    || currentTokenizationType.Value == SqlTokenizationType.QuotedIdentifier
                    )
                    tokenContainer.HasErrors = true;

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
                || currentCharacter == '\''
                || currentCharacter == ','
                || currentCharacter == '.'
                || currentCharacter == '['
                || currentCharacter == '('
                || currentCharacter == ')'
                || currentCharacter == '!'
                || currentCharacter == ';'
                );
        }

        private static bool IsOperatorCharacter(char currentCharacter)
        {
            //characters that pop you out of a regular "word" context (maybe into a new word)
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
            else if (currentCharacter == '[')
            {
                currentTokenizationType = SqlTokenizationType.QuotedIdentifier;
            }
            else if (currentCharacter == '(')
            {
                tokenContainer.Add(new Token(SqlTokenType.OpenParens, ""));
            }
            else if (currentCharacter == ')')
            {
                tokenContainer.Add(new Token(SqlTokenType.CloseParens, ""));
            }
            else if (currentCharacter == ',')
            {
                tokenContainer.Add(new Token(SqlTokenType.Comma, ""));
            }
            else if (currentCharacter == '.')
            {
                tokenContainer.Add(new Token(SqlTokenType.Period, ""));
            }
            else if (currentCharacter == ';')
            {
                tokenContainer.Add(new Token(SqlTokenType.Semicolon, ""));
            }
            else if (currentCharacter == '*')
            {
                tokenContainer.Add(new Token(SqlTokenType.Asterisk, ""));
            }
            else if (currentCharacter == '>')
            {
                currentTokenizationType = SqlTokenizationType.SingleGT;
            }
            else if (currentCharacter == '<')
            {
                currentTokenizationType = SqlTokenizationType.SingleLT;
            }
            else if (currentCharacter == '!')
            {
                currentTokenizationType = SqlTokenizationType.SingleExclamation;
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

                case SqlTokenizationType.SingleLineComment:
                    tokenContainer.Add(new Token(SqlTokenType.SingleLineComment, currentValue.ToString()));
                    break;

                case SqlTokenizationType.SingleHyphen:
                    tokenContainer.Add(new Token(SqlTokenType.OtherOperator, "-"));
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

                case SqlTokenizationType.NString:
                    tokenContainer.Add(new Token(SqlTokenType.NationalString, currentValue.ToString()));
                    break;

                case SqlTokenizationType.String:
                    tokenContainer.Add(new Token(SqlTokenType.String, currentValue.ToString()));
                    break;

                case SqlTokenizationType.QuotedIdentifier:
                    tokenContainer.Add(new Token(SqlTokenType.QuotedIdentifier, currentValue.ToString()));
                    break;

                case SqlTokenizationType.OtherOperator:
                    tokenContainer.Add(new Token(SqlTokenType.OtherOperator, currentValue.ToString()));
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
            BlockComment,
            String,
            NString,
            QuotedIdentifier,
            OtherOperator,

            //temporary types
            SingleHyphen,
            SingleSlash,
            SingleN,
            SingleGT,
            SingleLT,
            SingleExclamation
        }

    }
}
