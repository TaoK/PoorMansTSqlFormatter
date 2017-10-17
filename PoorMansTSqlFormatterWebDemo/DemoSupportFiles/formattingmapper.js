var PageFormattingMapper = function() {
  var GetFormatterForOptions = function(options, enableHtml, errorHandler) {
    switch (options.formattingType) {
      case 'standard':
        var stdOptions = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions();
        stdOptions.IndentString = options.indent;
        stdOptions.SpacesPerTab = options.spacesPerTab;
        stdOptions.MaxLineWidth = options.maxLineWidth;
        stdOptions.NewStatementLineBreaks = options.statementBreaks;
        stdOptions.NewClauseLineBreaks = options.clauseBreaks;
        stdOptions.ExpandCommaLists = options.expandCommaLists == "true";
        stdOptions.TrailingCommas = options.trailingCommas == "true";
        stdOptions.SpaceAfterExpandedComma = options.spaceAfterExpandedComma == "true";
        stdOptions.ExpandBooleanExpressions = options.expandBooleanExpressions == "true";
        stdOptions.ExpandCaseStatements = options.expandCaseStatements == "true";
        stdOptions.ExpandBetweenConditions = options.expandBetweenConditions == "true";
        stdOptions.ExpandInLists = options.expandInLists == "true";
        stdOptions.BreakJoinOnSections = options.breakJoinOnSections == "true";
        stdOptions.UppercaseKeywords = options.uppercaseKeywords == "true";
        stdOptions.HTMLColoring = enableHtml && options.coloring == "true";
        stdOptions.KeywordStandardization = options.keywordStandardization == "true";
        return new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter.$ctor1(stdOptions);
      case 'identity':
        return new PoorMansTSqlFormatterLib.Formatters.TSqlIdentityFormatter.$ctor1(enableHtml);
      case 'obfuscation':
        return new PoorMansTSqlFormatterLib.Formatters.TSqlObfuscatingFormatter.$ctor1(
          options.randomizeKeywordCase == "true",
          enableHtml && options.randomizeColor == "true",
          options.randomizeLineLengths == "true",
          options.preserveComments == "true",
          options.enableKeywordSubstitution == "true"
        );
      default:
        errorHandler('Invalid options - formattingType unrecognized: ' + options.formattingType);
    }
  }

  return { GetFormatterForOptions: GetFormatterForOptions }
}();