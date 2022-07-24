/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
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
            Assert.Multiple(() =>
            {
                Assert.That(options.IndentString, Is.EqualTo("\t"));
                Assert.That(options.SpacesPerTab, Is.EqualTo(4));
                Assert.That(options.MaxLineWidth, Is.EqualTo(999));
                Assert.That(options.ExpandCommaLists, Is.True);
                Assert.That(options.TrailingCommas, Is.False);
                Assert.That(options.SpaceAfterExpandedComma, Is.False);
                Assert.That(options.ExpandBooleanExpressions, Is.True);
                Assert.That(options.ExpandCaseStatements, Is.True);
                Assert.That(options.ExpandBetweenConditions, Is.True);
                Assert.That(options.BreakJoinOnSections, Is.False);
                Assert.That(options.UppercaseKeywords, Is.True);
                Assert.That(options.HTMLColoring, Is.False);
                Assert.That(options.KeywordStandardization, Is.False);
            });
        }

        [Test]
        public void Options_Deserialize_TrailingCommasTrue()
        {
            var options = new TSqlStandardFormatterOptions("TrailingCommas=True");

            Assert.That(options.TrailingCommas, Is.True);
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
            Assert.Multiple(() =>
            {
                Assert.That(options.IndentString, Is.EqualTo("    "));
                Assert.That(options.SpacesPerTab, Is.EqualTo(2));
                Assert.That(options.MaxLineWidth, Is.EqualTo(100));
                Assert.That(options.ExpandCommaLists, Is.False);
                Assert.That(options.TrailingCommas, Is.True);
                Assert.That(options.SpaceAfterExpandedComma, Is.True);
                Assert.That(options.ExpandBooleanExpressions, Is.False);
                Assert.That(options.ExpandCaseStatements, Is.False);
                Assert.That(options.ExpandBetweenConditions, Is.False);
                Assert.That(options.BreakJoinOnSections, Is.True);
                Assert.That(options.UppercaseKeywords, Is.False);
                Assert.That(options.HTMLColoring, Is.True);
                Assert.That(options.KeywordStandardization, Is.True);
            });
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
            Assert.Multiple(() =>
            {
                Assert.That(actual.IndentString, Is.EqualTo(expected.IndentString));
                Assert.That(actual.SpacesPerTab, Is.EqualTo(expected.SpacesPerTab));
                Assert.That(actual.MaxLineWidth, Is.EqualTo(expected.MaxLineWidth));
                Assert.That(actual.ExpandCommaLists, Is.EqualTo(expected.ExpandCommaLists));
                Assert.That(actual.TrailingCommas, Is.EqualTo(expected.TrailingCommas));
                Assert.That(actual.SpaceAfterExpandedComma, Is.EqualTo(expected.SpaceAfterExpandedComma));
                Assert.That(actual.ExpandBooleanExpressions, Is.EqualTo(expected.ExpandBooleanExpressions));
                Assert.That(actual.ExpandCaseStatements, Is.EqualTo(expected.ExpandCaseStatements));
                Assert.That(actual.ExpandBetweenConditions, Is.EqualTo(expected.ExpandBetweenConditions));
                Assert.That(actual.BreakJoinOnSections, Is.EqualTo(expected.BreakJoinOnSections));
                Assert.That(actual.UppercaseKeywords, Is.EqualTo(expected.UppercaseKeywords));
                Assert.That(actual.HTMLColoring, Is.EqualTo(expected.HTMLColoring));
                Assert.That(actual.KeywordStandardization, Is.EqualTo(expected.KeywordStandardization));
            });
        }
    }
}
