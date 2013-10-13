IF @OuterIfCondition = 1
IF @InnerIfCondition = 2
SET @InnerIfStatement = 3
ELSE IF @SecondInnerIfCondition = 4
SET @InnerSecondIfStatement = 5
ELSE
SET @InnerElseStatement = 6
ELSE
SET @OuterElseStatement = 7
