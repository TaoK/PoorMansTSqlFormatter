--Data type synonyms are overviewed at the following MSDN page:
-- http://msdn.microsoft.com/en-us/library/ms177566.aspx

DECLARE @Int1 National Character
DECLARE @Table1 TABLE (Col1 National Character, Col2 National Character Varying(20))
SELECT @Int1 = 9


INSERT INTO @Table1 SELECT 1, 2
SELECT Col1 + @Int1, Col2 FROM @Table1
GO

CREATE PROC TestTypes @Arg1 AS national text
AS 
SELECT CONVERT(Double precision,10) AS SomeCol
INTO #SomeTempTable

SELECT * FROM #SomeTempTable
Exec tempdb..sp_help 'tempdb..#SomeTempTable'
DROP TABle #SomeTempTable
GO
TestTypes 'test'
DROP PROC TestTypes

--Also 2008-specific datatypes here, from MSDN sample:
-- http://msdn.microsoft.com/en-us/library/bb630289.aspx

SELECT 
     CAST('2007-05-08 12:35:29. 1234567 +12:15' AS time(7)) AS 'time' 
    ,CAST('2007-05-08 12:35:29. 1234567 +12:15' AS date) AS 'date' 
    ,CAST('2007-05-08 12:35:29.123' AS smalldatetime) AS 
        'smalldatetime' 
    ,CAST('2007-05-08 12:35:29.123' AS datetime) AS 'datetime' 
    ,CAST('2007-05-08 12:35:29.1234567+12:15' AS datetime2(7)) AS 
        'datetime2'
    ,CAST('2007-05-08 12:35:29.1234567 +12:15' AS datetimeoffset(7)) AS 
        'datetimeoffset'
    ,CAST('2007-05-08 12:35:29.1234567+12:15' AS datetimeoffset(7)) AS
        'datetimeoffset IS08601';

--HierarchyID Stuff:
-- http://msdn.microsoft.com/en-us/library/bb677237.aspx

CREATE TABLE #EmployeeDemo (EmployeeID int, LoginID varchar(200), ManagerID int);
INSERT INTO #EmployeeDemo 
VALUES (1, 'zarifin', Null), 
       (2, 'tplate', 1),
       (3, 'hjensen', 1),
       (4, 'schai', 2),
       (5, 'elang', 2),
       (6, 'gsmits', 2),
       (7, 'sdavis', 3),
       (8, 'norint', 3),
       (9, 'jwang', 4),
       (10, 'malexander', 4);
       
       
CREATE TABLE #NewOrg
(
  OrgNode hierarchyid,
  EmployeeID int,
  LoginID nvarchar(50),
  ManagerID int
CONSTRAINT PK_#NewOrg_OrgNode
  PRIMARY KEY CLUSTERED (OrgNode)
);
GO

CREATE TABLE #Children 
   (
    EmployeeID int,
    ManagerID int,
    Num int
);
GO

CREATE CLUSTERED INDEX tmpind ON #Children(ManagerID, EmployeeID);
GO

INSERT #Children (EmployeeID, ManagerID, Num)
SELECT EmployeeID, ManagerID,
  ROW_NUMBER() OVER (PARTITION BY ManagerID ORDER BY ManagerID) 
FROM #EmployeeDemo
GO

WITH paths(path, EmployeeID) 
AS (
-- This section provides the value for the root of the hierarchy
SELECT hierarchyid::GetRoot() AS OrgNode, EmployeeID 
FROM #Children AS C 
WHERE ManagerID IS NULL 

UNION ALL 
-- This section provides values for all nodes except the root
SELECT 
CAST(p.path.ToString() + CAST(C.Num AS varchar(30)) + '/' AS hierarchyid), 
C.EmployeeID
FROM #Children AS C 
JOIN paths AS p 
   ON C.ManagerID = P.EmployeeID 
)
INSERT #NewOrg (OrgNode, O.EmployeeID, O.LoginID, O.ManagerID)
SELECT P.path, O.EmployeeID, O.LoginID, O.ManagerID
FROM #EmployeeDemo AS O 
JOIN Paths AS P 
   ON O.EmployeeID = P.EmployeeID
GO

SELECT OrgNode.ToString() AS LogicalNode, * 
FROM #NewOrg 
ORDER BY LogicalNode;
GO

DROP TABLE #Children
DROP TABLE #EmployeeDemo 
DROP TABLE #NewOrg
GO

--Geography Stuff:
-- http://msdn.microsoft.com/en-us/library/cc280766.aspx

CREATE TABLE #SpatialTable 
    ( id int IDENTITY (1,1),
    GeogCol1 geography, 
    GeogCol2 AS GeogCol1.STAsText() );
GO

INSERT INTO #SpatialTable (GeogCol1)
VALUES (geography::STGeomFromText('LINESTRING(-122.360 47.656, -122.343 47.656 )', 4326));

INSERT INTO #SpatialTable (GeogCol1)
VALUES (geography::STGeomFromText('POLYGON((-122.358 47.653 , -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))', 4326));
GO

DECLARE @geog1 geography;
DECLARE @geog2 geography;
DECLARE @result geography;

SELECT @geog1 = GeogCol1 FROM #SpatialTable WHERE id = 1;
SELECT @geog2 = GeogCol1 FROM #SpatialTable WHERE id = 2;
SELECT @result = @geog1.STIntersection(@geog2);
SELECT @result.STAsText();

DROP TABLE #SpatialTable 
GO

SELECT Count(N) FROM CounterTable