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

namespace PoorMansTSqlFormatterLib.Tokenizers
{
    public class TSqlStandardTokenizer : Interfaces.ISqlTokenizer
    {
        /*
         * TODO:
         *  - WILL NEED TO RESEARCH QUOTED_IDENTIFIER
         *  - Future Extensions
         *    - Scope Resolution Operator (and/or colons in general?)
         *    - Compound operators (SQL 2008)
         */

        public XmlDocument TokenizeSQL(string inputSQL)
        {
            XmlDocument tokenContainerDoc = new XmlDocument();
            XmlElement tokenContainer;
            StringReader inputReader = new StringReader(inputSQL);
            SqlTokenType? currentTokenType;
            StringBuilder currentTokenValue = new StringBuilder();
            bool errorFound = false;

            tokenContainerDoc.AppendChild(tokenContainerDoc.CreateElement(Interfaces.Constants.ENAME_SQLTOKENS_ROOT));
            tokenContainer = tokenContainerDoc.DocumentElement;
            currentTokenType = null;
            currentTokenValue.Length = 0;

            int currentCharInt = inputReader.Read();
            while (currentCharInt >= 0)
            {
                char currentCharacter = (char)currentCharInt;
                if (currentTokenType == null)
                {
                    currentTokenType = StartToken(currentTokenValue, currentCharacter);
                }
                else
                {
                    switch (currentTokenType.Value)
                    {
                        case SqlTokenType.WhiteSpace:
                            if (IsWhitespace(currentCharacter))
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleHyphen:
                            if (currentCharacter == '-')
                            {
                                currentTokenType = SqlTokenType.SingleLineComment;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenType = SqlTokenType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleSlash:
                            if (currentCharacter == '*')
                            {
                                currentTokenType = SqlTokenType.BlockComment;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenType = SqlTokenType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenType.OtherOperator;
                                currentTokenValue.Append('/');
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleLineComment:
                            if (currentCharacter == (char)13 || currentCharacter == (char)10)
                            {
                                currentTokenValue.Append(currentCharacter);

                                int nextCharInt = inputReader.Peek();
                                if (currentCharacter == (char)13 && nextCharInt == 10)
                                    currentTokenValue.Append((char)inputReader.Read());

                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenType.BlockComment:
                            if (currentCharacter == '*')
                            {
                                int nextCharInt = inputReader.Peek();
                                if (nextCharInt == (int)'/')
                                {
                                    inputReader.Read();
                                    currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
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

                        case SqlTokenType.OtherNode:
                            if (IsNonWordCharacter(currentCharacter))
                            {
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleN:
                            if (currentCharacter == '\'')
                            {
                                currentTokenType = SqlTokenType.NString;
                            }
                            else
                            {
                                currentTokenType = SqlTokenType.OtherNode;
                                currentTokenValue.Append('N');
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenType.NString:
                        case SqlTokenType.String:
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
                                    currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenType.QuotedIdentifier:
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
                                    currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleLT:
                            currentTokenValue.Append('<');
                            currentTokenType = SqlTokenType.OtherOperator;
                            if (currentCharacter == '=' || currentCharacter == '>')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleGT:
                            currentTokenValue.Append('>');
                            currentTokenType = SqlTokenType.OtherOperator;
                            if (currentCharacter == '=')
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.SingleExclamation:
                            currentTokenValue.Append('!');
                            if (currentCharacter == '=' || currentCharacter == '<' || currentCharacter == '>')
                            {
                                currentTokenType = SqlTokenType.OtherOperator;
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenType.OtherNode;
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenType.OpenParens:
                        case SqlTokenType.CloseParens:
                        case SqlTokenType.Comma:
                        case SqlTokenType.Period:
                        case SqlTokenType.SemiColon:
                        case SqlTokenType.Asterisk:
                        case SqlTokenType.OtherOperator:
                            currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                            currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            break;

                        default:
                            throw new Exception("In-progress node unrecognized!");
                    }
                }

                currentCharInt = inputReader.Read();
            }


            if (currentTokenType != null)
            {
                if (currentTokenType.Value == SqlTokenType.BlockComment
                    || currentTokenType.Value == SqlTokenType.String
                    || currentTokenType.Value == SqlTokenType.NString
                    || currentTokenType.Value == SqlTokenType.QuotedIdentifier
                    )
                    errorFound = true;

                CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
            }

            if (errorFound)
            {
                tokenContainerDoc.DocumentElement.SetAttribute(Interfaces.Constants.ANAME_ERRORFOUND, "1");
            }

            return tokenContainerDoc;
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

        private static SqlTokenType? StartToken(StringBuilder currentNodeValue, char currentCharacter)
        {
            SqlTokenType? currentNodeType;

            if (IsWhitespace(currentCharacter))
            {
                currentNodeType = SqlTokenType.WhiteSpace;
                currentNodeValue.Append(currentCharacter);
            }
            else if (currentCharacter == '-')
            {
                currentNodeType = SqlTokenType.SingleHyphen;
            }
            else if (currentCharacter == '/')
            {
                currentNodeType = SqlTokenType.SingleSlash;
            }
            else if (currentCharacter == 'N')
            {
                currentNodeType = SqlTokenType.SingleN;
            }
            else if (currentCharacter == '\'')
            {
                currentNodeType = SqlTokenType.String;
            }
            else if (currentCharacter == '[')
            {
                currentNodeType = SqlTokenType.QuotedIdentifier;
            }
            else if (currentCharacter == '(')
            {
                currentNodeType = SqlTokenType.OpenParens;
            }
            else if (currentCharacter == ')')
            {
                currentNodeType = SqlTokenType.CloseParens;
            }
            else if (currentCharacter == ',')
            {
                currentNodeType = SqlTokenType.Comma;
            }
            else if (currentCharacter == '.')
            {
                currentNodeType = SqlTokenType.Period;
            }
            else if (currentCharacter == ';')
            {
                currentNodeType = SqlTokenType.SemiColon;
            }
            else if (currentCharacter == '*')
            {
                currentNodeType = SqlTokenType.Asterisk;
            }
            else if (currentCharacter == '>')
            {
                currentNodeType = SqlTokenType.SingleGT;
            }
            else if (currentCharacter == '<')
            {
                currentNodeType = SqlTokenType.SingleLT;
            }
            else if (currentCharacter == '!')
            {
                currentNodeType = SqlTokenType.SingleExclamation;
            }
            else if (IsOperatorCharacter(currentCharacter))
            {
                currentNodeType = SqlTokenType.OtherOperator;
                currentNodeValue.Append(currentCharacter);
            }
            else
            {
                currentNodeType = SqlTokenType.OtherNode;
                currentNodeValue.Append(currentCharacter);
            }
            return currentNodeType;
        }

        private static SqlTokenType? CompleteToken(SqlTokenType thisType, XmlElement tokenContainer, StringBuilder currentValue)
        {
            string elementName = "";
            string elementValue = "";

            switch (thisType)
            {
                case SqlTokenType.BlockComment:
                    elementName = Interfaces.Constants.ENAME_COMMENT_MULTILINE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.OtherNode:
                    elementName = Interfaces.Constants.ENAME_OTHERNODE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.SingleLineComment:
                    elementName = Interfaces.Constants.ENAME_COMMENT_SINGLELINE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.SingleHyphen:
                case SqlTokenType.SingleSlash:
                    elementName = Interfaces.Constants.ENAME_OTHEROPERATOR;
                    elementValue = "/";
                    break;

                case SqlTokenType.WhiteSpace:
                    elementName = Interfaces.Constants.ENAME_WHITESPACE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.SingleN:
                    elementName = Interfaces.Constants.ENAME_OTHERNODE;
                    elementValue = "N";
                    break;

                case SqlTokenType.SingleExclamation:
                    elementName = Interfaces.Constants.ENAME_OTHERNODE;
                    elementValue = "!";
                    break;

                case SqlTokenType.NString:
                    elementName = Interfaces.Constants.ENAME_NSTRING;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.String:
                    elementName = Interfaces.Constants.ENAME_STRING;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.QuotedIdentifier:
                    elementName = Interfaces.Constants.ENAME_QUOTED_IDENTIFIER;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenType.OpenParens:
                    elementName = Interfaces.Constants.ENAME_PARENS_OPEN;
                    elementValue = "";
                    break;

                case SqlTokenType.CloseParens:
                    elementName = Interfaces.Constants.ENAME_PARENS_CLOSE;
                    elementValue = "";
                    break;

                case SqlTokenType.Comma:
                    elementName = Interfaces.Constants.ENAME_COMMA;
                    elementValue = "";
                    break;

                case SqlTokenType.Period:
                    elementName = Interfaces.Constants.ENAME_PERIOD;
                    elementValue = "";
                    break;

                case SqlTokenType.SemiColon:
                    elementName = Interfaces.Constants.ENAME_SEMICOLON;
                    elementValue = "";
                    break;

                case SqlTokenType.Asterisk:
                    elementName = Interfaces.Constants.ENAME_ASTERISK;
                    elementValue = "";
                    break;

                case SqlTokenType.OtherOperator:
                    elementName = Interfaces.Constants.ENAME_OTHEROPERATOR;
                    elementValue = currentValue.ToString();
                    break;

                default:
                    throw new Exception("Unrecognized SQL Node Type");
            }

            XmlElement newNode = tokenContainer.OwnerDocument.CreateElement(elementName);
            newNode.InnerText = elementValue;
            tokenContainer.AppendChild(newNode);

            currentValue.Length = 0;
            return null;
        }

        public enum SqlTokenType
        {
            //actual tokens:
            WhiteSpace,
            OtherNode,
            SingleLineComment,
            BlockComment,
            String,
            NString,
            QuotedIdentifier,
            OpenParens,
            CloseParens,
            Comma,
            Period,
            SemiColon,
            Asterisk,
            OtherOperator,

            //temp types:
            SingleHyphen,
            SingleSlash,
            SingleN,
            SingleGT,
            SingleLT,
            SingleExclamation
        }

    }
}
