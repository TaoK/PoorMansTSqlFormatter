use master;
go
DECLARE @Test Int
DECLARE Test$Me SCROLL Cursor 
FOR
SELECT 1 AS One FOR read only


OPEN Test$Me

FETCH NEXT FROM Test$Me 
INTO @Test

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'I was here!'
FETCH NEXT FROM Test$Me
INTO @Test
END
CLOSE Test$Me
DEALLOCATE Test$Me
go


CREATE TABLE #CursorFodder (One int, Two int)

SET NOCOUNT ON
INSERT INTO #CursorFodder VALUES (1, 2)
SET NOCOUNT OFF

DECLARE @Test2 Int
DECLARE Test$Me_Again_François Cursor OPTIMISTIC
FOR SELECT One FROM #CursorFodder 
FOR UPDATE of One


OPEN Test$Me_Again_François

FETCH NEXT FROM Test$Me_Again_François
INTO @Test2

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'I was here!'
FETCH NEXT FROM Test$Me_Again_François
INTO @Test2
END
CLOSE Test$Me_Again_François
DEALLOCATE Test$Me_Again_François

DROP TABLE #CursorFodder 