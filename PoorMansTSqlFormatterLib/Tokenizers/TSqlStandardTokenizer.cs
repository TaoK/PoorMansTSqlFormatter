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
            SqlTokenizationType? currentTokenType;
            StringBuilder currentTokenValue = new StringBuilder();
            bool errorFound = false;

            tokenContainerDoc.AppendChild(tokenContainerDoc.CreateElement(Interfaces.XmlConstants.ENAME_SQLTOKENS_ROOT));
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
                        case SqlTokenizationType.WhiteSpace:
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

                        case SqlTokenizationType.SingleHyphen:
                            if (currentCharacter == '-')
                            {
                                currentTokenType = SqlTokenizationType.SingleLineComment;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.SingleSlash:
                            if (currentCharacter == '*')
                            {
                                currentTokenType = SqlTokenizationType.BlockComment;
                            }
                            else if (currentCharacter == '=')
                            {
                                currentTokenType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('-');
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append('/');
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.SingleLineComment:
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

                        case SqlTokenizationType.BlockComment:
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

                        case SqlTokenizationType.OtherNode:
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

                        case SqlTokenizationType.SingleN:
                            if (currentCharacter == '\'')
                            {
                                currentTokenType = SqlTokenizationType.NString;
                            }
                            else
                            {
                                currentTokenType = SqlTokenizationType.OtherNode;
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
                                    currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
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
                                    currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                }
                            }
                            else
                            {
                                currentTokenValue.Append(currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.SingleLT:
                            currentTokenValue.Append('<');
                            currentTokenType = SqlTokenizationType.OtherOperator;
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

                        case SqlTokenizationType.SingleGT:
                            currentTokenValue.Append('>');
                            currentTokenType = SqlTokenizationType.OtherOperator;
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

                        case SqlTokenizationType.SingleExclamation:
                            currentTokenValue.Append('!');
                            if (currentCharacter == '=' || currentCharacter == '<' || currentCharacter == '>')
                            {
                                currentTokenType = SqlTokenizationType.OtherOperator;
                                currentTokenValue.Append(currentCharacter);
                            }
                            else
                            {
                                currentTokenType = SqlTokenizationType.OtherNode;
                                currentTokenType = CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
                                currentTokenType = StartToken(currentTokenValue, currentCharacter);
                            }
                            break;

                        case SqlTokenizationType.OpenParens:
                        case SqlTokenizationType.CloseParens:
                        case SqlTokenizationType.Comma:
                        case SqlTokenizationType.Period:
                        case SqlTokenizationType.SemiColon:
                        case SqlTokenizationType.Asterisk:
                        case SqlTokenizationType.OtherOperator:
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
                if (currentTokenType.Value == SqlTokenizationType.BlockComment
                    || currentTokenType.Value == SqlTokenizationType.String
                    || currentTokenType.Value == SqlTokenizationType.NString
                    || currentTokenType.Value == SqlTokenizationType.QuotedIdentifier
                    )
                    errorFound = true;

                CompleteToken(currentTokenType.Value, tokenContainer, currentTokenValue);
            }

            if (errorFound)
            {
                tokenContainerDoc.DocumentElement.SetAttribute(Interfaces.XmlConstants.ANAME_ERRORFOUND, "1");
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

        private static SqlTokenizationType? StartToken(StringBuilder currentNodeValue, char currentCharacter)
        {
            SqlTokenizationType? currentNodeType;

            if (IsWhitespace(currentCharacter))
            {
                currentNodeType = SqlTokenizationType.WhiteSpace;
                currentNodeValue.Append(currentCharacter);
            }
            else if (currentCharacter == '-')
            {
                currentNodeType = SqlTokenizationType.SingleHyphen;
            }
            else if (currentCharacter == '/')
            {
                currentNodeType = SqlTokenizationType.SingleSlash;
            }
            else if (currentCharacter == 'N')
            {
                currentNodeType = SqlTokenizationType.SingleN;
            }
            else if (currentCharacter == '\'')
            {
                currentNodeType = SqlTokenizationType.String;
            }
            else if (currentCharacter == '[')
            {
                currentNodeType = SqlTokenizationType.QuotedIdentifier;
            }
            else if (currentCharacter == '(')
            {
                currentNodeType = SqlTokenizationType.OpenParens;
            }
            else if (currentCharacter == ')')
            {
                currentNodeType = SqlTokenizationType.CloseParens;
            }
            else if (currentCharacter == ',')
            {
                currentNodeType = SqlTokenizationType.Comma;
            }
            else if (currentCharacter == '.')
            {
                currentNodeType = SqlTokenizationType.Period;
            }
            else if (currentCharacter == ';')
            {
                currentNodeType = SqlTokenizationType.SemiColon;
            }
            else if (currentCharacter == '*')
            {
                currentNodeType = SqlTokenizationType.Asterisk;
            }
            else if (currentCharacter == '>')
            {
                currentNodeType = SqlTokenizationType.SingleGT;
            }
            else if (currentCharacter == '<')
            {
                currentNodeType = SqlTokenizationType.SingleLT;
            }
            else if (currentCharacter == '!')
            {
                currentNodeType = SqlTokenizationType.SingleExclamation;
            }
            else if (IsOperatorCharacter(currentCharacter))
            {
                currentNodeType = SqlTokenizationType.OtherOperator;
                currentNodeValue.Append(currentCharacter);
            }
            else
            {
                currentNodeType = SqlTokenizationType.OtherNode;
                currentNodeValue.Append(currentCharacter);
            }
            return currentNodeType;
        }

        private static SqlTokenizationType? CompleteToken(SqlTokenizationType thisType, XmlElement tokenContainer, StringBuilder currentValue)
        {
            string elementName = "";
            string elementValue = "";

            switch (thisType)
            {
                case SqlTokenizationType.BlockComment:
                    elementName = Interfaces.XmlConstants.ENAME_COMMENT_MULTILINE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.OtherNode:
                    elementName = Interfaces.XmlConstants.ENAME_OTHERNODE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.SingleLineComment:
                    elementName = Interfaces.XmlConstants.ENAME_COMMENT_SINGLELINE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.SingleHyphen:
                case SqlTokenizationType.SingleSlash:
                    elementName = Interfaces.XmlConstants.ENAME_OTHEROPERATOR;
                    elementValue = "/";
                    break;

                case SqlTokenizationType.WhiteSpace:
                    elementName = Interfaces.XmlConstants.ENAME_WHITESPACE;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.SingleN:
                    elementName = Interfaces.XmlConstants.ENAME_OTHERNODE;
                    elementValue = "N";
                    break;

                case SqlTokenizationType.SingleExclamation:
                    elementName = Interfaces.XmlConstants.ENAME_OTHERNODE;
                    elementValue = "!";
                    break;

                case SqlTokenizationType.NString:
                    elementName = Interfaces.XmlConstants.ENAME_NSTRING;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.String:
                    elementName = Interfaces.XmlConstants.ENAME_STRING;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.QuotedIdentifier:
                    elementName = Interfaces.XmlConstants.ENAME_QUOTED_IDENTIFIER;
                    elementValue = currentValue.ToString();
                    break;

                case SqlTokenizationType.OpenParens:
                    elementName = Interfaces.XmlConstants.ENAME_PARENS_OPEN;
                    elementValue = "";
                    break;

                case SqlTokenizationType.CloseParens:
                    elementName = Interfaces.XmlConstants.ENAME_PARENS_CLOSE;
                    elementValue = "";
                    break;

                case SqlTokenizationType.Comma:
                    elementName = Interfaces.XmlConstants.ENAME_COMMA;
                    elementValue = "";
                    break;

                case SqlTokenizationType.Period:
                    elementName = Interfaces.XmlConstants.ENAME_PERIOD;
                    elementValue = "";
                    break;

                case SqlTokenizationType.SemiColon:
                    elementName = Interfaces.XmlConstants.ENAME_SEMICOLON;
                    elementValue = "";
                    break;

                case SqlTokenizationType.Asterisk:
                    elementName = Interfaces.XmlConstants.ENAME_ASTERISK;
                    elementValue = "";
                    break;

                case SqlTokenizationType.OtherOperator:
                    elementName = Interfaces.XmlConstants.ENAME_OTHEROPERATOR;
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

        public enum SqlTokenizationType
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
