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

namespace PoorMansTSqlFormatterLib.Tokenizers
{
    internal class SimplifiedStringReader
    {
        private char[] inputChars;
        private int nextCharIndex = 0;

        public SimplifiedStringReader(string inputString)
        {
            this.inputChars = inputString.ToCharArray();
        }

        internal int Read()
        {
            int nextChar = Peek();
            nextCharIndex++;
            return nextChar;
        }

        internal int Peek()
        {
            if (nextCharIndex < inputChars.Length)
                return inputChars[nextCharIndex];
            else
                return -1;
        }

        //note, intentional difference between index (zero-based) and position (1-based). Zero here means "nothing output yet".
        internal long LastCharacterPosition
        {
            get
            {
                if (nextCharIndex <= inputChars.Length)
                    return nextCharIndex;
                else
                    return inputChars.Length;
            }
        }
    }
}