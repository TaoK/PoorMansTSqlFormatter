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
            var expected = new TSqlStandardFormatterOptions();
            expected.IndentString = "  ";
            expected.SpacesPerTab = 2;
            expected.MaxLineWidth = 100;
            expected.ExpandCommaLists = false;
            expected.TrailingCommas = true;
            expected.SpaceAfterExpandedComma = true;
            expected.ExpandBooleanExpressions = false;
            expected.ExpandCaseStatements = false;
            expected.ExpandBetweenConditions = false;
            expected.BreakJoinOnSections = true;
            expected.UppercaseKeywords = false;
            expected.HTMLColoring = true;
            expected.KeywordStandardization = true;

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
