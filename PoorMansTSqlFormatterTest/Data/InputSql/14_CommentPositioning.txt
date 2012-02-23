if exists (select * from dbo.sysobjects where id = object_id('MyTestProc') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure MyTestProc
GO

CREATE PROCEDURE MyTestProc
WITH 
RECOMPILE
AS
BEGIN
/*

This is where some explanation of the proc's purpose and usage might go.

*/

	--Set some initial stuff
	SET NOCOUNT ON 
	SET XACT_ABORT ON

	--declare some variables (keep them grouped for clarity)
	DECLARE @Var1 Int
	DECLARE @Var2 Int
	DECLARE @Var3 nvarchar(50)

	--set a variable with SET (groupable)
	SET @Var3 = 'Empty'
	--then set a variable value with SELECT (not groupable)
	SELECT @DaysAfterDelivery = Max(Convert(Int, ParameterValue)) FROM CompanyOrderNotification (NOLOCK) WHERE CID = @CID AND NotificationID = 43 AND DATALENGTH(Notificationtext) > 0 AND IsNumeric(ParameterValue) = 1 --attach a comment to the end of this

	--and place another comment before the cursor
	DECLARE SomeCursor CURSOR READ_ONLY FAST_FORWARD 
	FOR
	SELECT [Some], Things, here
	FROM SomeTable
	LEFT JOIN [Some Other Table] (NOLOCK) 
		ON sometable.Col1 = [Some Other Table].Col1
		AND sometable.Col2 = [Some Other Table].Col2
		
		OPEN SomeCursor
		
		FETCH NEXT FROM SomeCursor
		INTO @Var1, @Var2, @Var3
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			--another comment explaining something
			PRINT 'Var 1 is ' + Convert(NVarChar(10), @Var1)

			/* A multiline comment on its own 
			but separated from the previous statement */
			
			/* a multiline comment attached */
			PRINT 'Var 2 is ' --mid-statement comment
							+ Cast(@Var2 AS NVarChar(10))

			FETCH NEXT FROM SomeCursor
			INTO @Var1, @Var2, @Var3
		END /* comment explaining what ends here */
		
		CLOSE SomeCursor
		DEALLOCATE SomeCursor


	SET XACT_ABORT OFF
	SET NOCOUNT OFF
	
END
GO

SELECT 1, 2, 3, 
/*******************    
Multi-line Block Comment - earlier and later content should be on new line, respecting the linebreaks in the source
*******************/ 
     4 



SELECT 1,
			---------------------    
			-- multiple single-line comments
			---------------------     
			2, 3 -- single-line comment before a comma
			, CASE 1 WHEN 1 THEN 4 ELSE 1 END 
