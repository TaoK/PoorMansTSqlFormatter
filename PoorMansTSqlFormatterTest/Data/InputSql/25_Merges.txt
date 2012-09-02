--Samples adapted from MSDN: http://msdn.microsoft.com/en-us/library/bb510625.aspx
-- Note: some of these samples require compatibility level 100 (SQL 2008)

--MERGE with OUTPUT INTO
CREATE TABLE #MatchReasons (Name NVarChar(50), ReasonType NVarChar(50));
DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));

MERGE INTO #MatchReasons AS Target
USING (VALUES ('Name1','Reason1'), ('Name2', 'Reason2'), ('Name3', 'Reason3'))
       AS Source (NewName, NewReasonType)
ON Target.Name = Source.NewName
and 1 = 1 
WHEN MATCHED THEN
	UPDATE SET ReasonType = Source.NewReasonType
WHEN NOT MATCHED BY TARGET THEN
	INSERT (Name, ReasonType) VALUES (NewName, NewReasonType)
OUTPUT $action INTO @SummaryOfChanges;

SELECT Change, COUNT(*) AS CountPerChange
FROM @SummaryOfChanges
GROUP BY Change;

DROP TABLE #MatchReasons


--INSERT INTO, MERGE and OUTPUT
CREATE TABLE #UpdatedInventory
    (ProductID INT NOT NULL, LocationID int, NewQty int, PreviousQty int);
CREATE TABLE #ReferenceInventory
    (ProductID INT NOT NULL, LocationID int, Quantity int);
GO
INSERT INTO #ReferenceInventory
SELECT 1, 1, 100
UNION SELECT 2, 5, 1000
go
SELECT * FROM #ReferenceInventory
go
INSERT INTO #UpdatedInventory
SELECT ProductID, LocationID, NewQty, PreviousQty 
FROM
(    MERGE #ReferenceInventory AS RI
     USING (SELECT 1, 100 UNION SELECT 2, 500) AS src (ProductID, OrderQty)
     ON RI.ProductID = src.ProductID
    WHEN MATCHED AND RI.Quantity - src.OrderQty > 0 
        THEN UPDATE SET RI.Quantity = RI.Quantity - src.OrderQty
    WHEN MATCHED AND RI.Quantity - src.OrderQty <= 0 
        THEN DELETE
    OUTPUT $action, Deleted.ProductID, Deleted.LocationID, Inserted.Quantity AS NewQty, Deleted.Quantity AS PreviousQty)
 AS Changes (Action, ProductID, LocationID, NewQty, PreviousQty) 
WHERE Action IN ('UPDATE', 'DELETE');
GO
SELECT * FROM #UpdatedInventory
SELECT * FROM #ReferenceInventory
DROP TABLE #UpdatedInventory
DROP TABLE #ReferenceInventory
GO


--same sample with OUTPUT INTO instead (can't use a WHERE clause though, this way)
CREATE TABLE #UpdatedInventory
    (ProductID INT NOT NULL, LocationID int, NewQty int, PreviousQty int);
CREATE TABLE #ReferenceInventory
    (ProductID INT NOT NULL, LocationID int, Quantity int);
GO
INSERT INTO #ReferenceInventory
SELECT 1, 1, 100
UNION SELECT 2, 5, 1000
go
SELECT * FROM #ReferenceInventory
go
    MERGE #ReferenceInventory AS RI
     USING (SELECT 1, 100 UNION SELECT 2, 500) AS src (ProductID, OrderQty)
     ON RI.ProductID = src.ProductID
    WHEN MATCHED AND RI.Quantity - src.OrderQty > 0 
        THEN UPDATE SET RI.Quantity = RI.Quantity - src.OrderQty
    WHEN MATCHED AND RI.Quantity - src.OrderQty <= 0 
        THEN DELETE
    OUTPUT Deleted.ProductID, Deleted.LocationID, Inserted.Quantity AS NewQty, Deleted.Quantity AS PreviousQty
		INTO #UpdatedInventory
	OPTION (FAST 10)
	;
		
GO
SELECT * FROM #UpdatedInventory
SELECT * FROM #ReferenceInventory
DROP TABLE #UpdatedInventory
DROP TABLE #ReferenceInventory
GO
--Variable initializer using Subquery; not directly related to MERGE, but also 2008-specific
DECLARE @SomeValue int=(select the thing from the place where tis found)
DECLARE @SomeOtherValue int= Convert(int, '1')
SELECT 1 as two
SELECT (SELECT 1) AS t
