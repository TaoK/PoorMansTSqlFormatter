--Taken from SQL Server Central question of the day, Feb 11 2009:
-- http://www.sqlservercentral.com/questions/T-SQL/65712/
--Discussion here:
-- http://www.sqlservercentral.com/Forums/Topic654391-1181-1.aspx
-- (interestingly, most online formatting tools get this wrong - afaik GuDu is the only 
--   other one that doesn't. Query analyser didn't get this right either, but SSMS does.)
--  
PRINT '1' -- /* ;PRINT '2' */ ;PRINT '3' /*
PRINT '4' --*/
--/*
PRINT '5'
--*/
/*
PRINT '6'
--/* point here is that 7 is still commented, because T-SQL supports nested multiline comments.
*/
PRINT '7'
--*/
PRINT '8'
