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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PoorMansTSqlFormatterLib.Formatters;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    class TSqlStandardFormatterOptionsTests
    {

        //Make sure that a new TSqlStandardFormatterOptions defaults to what the default formatting options were circa v1.4.1, 2012-09-02
        [Test]
        public void Options_New_DefaultsToOriginalOptions()
        {
            var options = new TSqlStandardFormatterOptions();

            Assert.AreEqual("\t", options.IndentString);
            Assert.AreEqual(4, options.SpacesPerTab);
            Assert.AreEqual(999, options.MaxLineWidth);
            Assert.IsTrue(options.ExpandCommaLists);
            Assert.IsFalse(options.TrailingCommas);
            Assert.IsFalse(options.SpaceAfterExpandedComma);
            Assert.IsTrue(options.ExpandBooleanExpressions);
            Assert.IsTrue(options.ExpandCaseStatements);
            Assert.IsTrue(options.ExpandBetweenConditions);
            Assert.IsFalse(options.BreakJoinOnSections);
            Assert.IsTrue(options.UppercaseKeywords);
            Assert.IsFalse(options.HTMLColoring);
            Assert.IsFalse(options.KeywordStandardization);
        }

        [Test]
        public void Options_Deserialize_TrailingCommasTrue()
        {
            var options = new TSqlStandardFormatterOptions("TrailingCommas=True");

            Assert.IsTrue(options.TrailingCommas);
        }

        [Test]
        public void Options_Deserialize_OverrideAll()
        {
            var options = new TSqlStandardFormatterOptions(
                "IndentString=    "
                + ",SpacesPerTab=2"
                + ",MaxLineWidth=100"
                + ",ExpandCommaLists=false"
                + ",TrailingCommas=True"
                + ",SpaceAfterExpandedComma=true"
                + ",ExpandBooleanExpressions=False"
                + ",ExpandCaseStatements=false"
                + ",ExpandBetweenConditions=false"
                + ",BreakJoinOnSections=true"
                + ",UppercaseKeywords=false"
                + ",HTMLColoring=true"
                + ",KeywordStandardization=true"
            );

            Assert.AreEqual("    ", options.IndentString);
            Assert.AreEqual(2, options.SpacesPerTab);
            Assert.AreEqual(100, options.MaxLineWidth);
            Assert.IsFalse(options.ExpandCommaLists);
            Assert.IsTrue(options.TrailingCommas);
            Assert.IsTrue(options.SpaceAfterExpandedComma);
            Assert.IsFalse(options.ExpandBooleanExpressions);
            Assert.IsFalse(options.ExpandCaseStatements);
            Assert.IsFalse(options.ExpandBetweenConditions);
            Assert.IsTrue(options.BreakJoinOnSections);
            Assert.IsFalse(options.UppercaseKeywords);
            Assert.IsTrue(options.HTMLColoring);
            Assert.IsTrue(options.KeywordStandardization);
        }

        [Test]
        public void Options_Deserialize_RoundTrip()
        {
            var expected = new TSqlStandardFormatterOptions
                {
                    IndentString = "  ",
                    SpacesPerTab = 2,
                    MaxLineWidth = 100,
                    ExpandCommaLists = false,
                    TrailingCommas = true,
                    SpaceAfterExpandedComma = true,
                    ExpandBooleanExpressions = false,
                    ExpandCaseStatements = false,
                    ExpandBetweenConditions = false,
                    BreakJoinOnSections = true,
                    UppercaseKeywords = false,
                    HTMLColoring = true,
                    KeywordStandardization = true
                };

            var serializedString = expected.ToSerializedString();

            var actual = new TSqlStandardFormatterOptions(serializedString);

            Assert.AreEqual(expected.IndentString, actual.IndentString);
            Assert.AreEqual(expected.SpacesPerTab, actual.SpacesPerTab);
            Assert.AreEqual(expected.MaxLineWidth, actual.MaxLineWidth);
            Assert.AreEqual(expected.ExpandCommaLists, actual.ExpandCommaLists);
            Assert.AreEqual(expected.TrailingCommas, actual.TrailingCommas);
            Assert.AreEqual(expected.SpaceAfterExpandedComma, actual.SpaceAfterExpandedComma);
            Assert.AreEqual(expected.ExpandBooleanExpressions, actual.ExpandBooleanExpressions);
            Assert.AreEqual(expected.ExpandCaseStatements, actual.ExpandCaseStatements);
            Assert.AreEqual(expected.ExpandBetweenConditions, actual.ExpandBetweenConditions);
            Assert.AreEqual(expected.BreakJoinOnSections, actual.BreakJoinOnSections);
            Assert.AreEqual(expected.UppercaseKeywords, actual.UppercaseKeywords);
            Assert.AreEqual(expected.HTMLColoring, actual.HTMLColoring);
            Assert.AreEqual(expected.KeywordStandardization, actual.KeywordStandardization);

        }

    }
}
