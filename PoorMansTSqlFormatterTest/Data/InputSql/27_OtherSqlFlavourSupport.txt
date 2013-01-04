--DB2 and SQLite both support a double-pipe string concatenation operator that 
-- could never exist in T-SQL, so parsing as an operator makes the formatter
-- more versatile without affecting core T-SQL support:

SELECT 'One String'||'another string'

--Similar SQLite operators: bit shifts and c-style equality operator

SELECT 101 << 1, 101 >> 1
WHERE 1 == 1

--NexusDB C-Style single-line comment

// Another (not valid T-SQL) comment

--NexusDB Parameters / DB2 or PostgreSQL Host Variables

SELECT :MagicValue
