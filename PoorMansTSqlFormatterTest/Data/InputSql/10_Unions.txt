SELECT *
FROM (((SELECT A AS TheOnlyColumn FROM (Select 'Test' as a UNION SELECT 'Test2') AS something))
UNION
((SELECT 'More'))) AS TestDerivedTable

IF 1=1 and 2 BETWEEN 1 AND (SELECT TOP 1 FirstValue FROM (SELECT 2 AS FirstValue UNION ALL SELECT 3) AS FictitiousTable ORDER BY 1)
	SELECT 'Yes!'


SELECT *
FROM (((SELECT A AS TheOnlyColumn FROM (Select 'Test' as a UNION SELECT 'Test2') AS something))
EXCEPT
((SELECT 'Test'))) AS TestDerivedTable

SELECT *
FROM (((SELECT A AS TheOnlyColumn FROM (Select 'Test' as a UNION SELECT 'Test2') AS something))
INTERSECT
((SELECT 'Test'))) AS TestDerivedTable
