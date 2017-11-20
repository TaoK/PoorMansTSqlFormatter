select 1 
from /*[noformat]*/ (select ABC FROM XYZ WHERE Whatever Applies) /*[/noformat]*/
where somethingelse applies

select 1 
from /*[minify]*/ (select ABC FROM XYZ WHERE Whatever Applies) /*[/minify]*/
where somethingelse applies

select /*[noformat]*/1E/*[/noformat]*/
from somewhere
where somethingelse applies

select 1 /* sometimes [noformat] is a red herring*//*[/noformat]*/
from somewhere --[noformat] nothing here
--[/noformat]
where somethingelse applies

-- I use [NOFORMAT] here because I like this section to remain dense despite my usually-spread-out formatting standards
DECLARE @something int = (select 1 from 2 where 3)
DECLARE @somethingelse int = (select 1 from 2 where 3)
declare @athirdthing int = (select 1 from 2 where 3)
--The end is [/noFORMAT]

select 'gimmespaceman' --...it's worth noting that if a statement is completely "minified" (or "nofomatted"), 
-- then we don't know it was a statement at all, and we fail to make the statement breaks before the NEXT one.

--Sometimes, I like to [Minify] all the way to the end of the file
create table abc (a int, b float, c varchar)
execute myprochere 
select somestuff from there 
DECLARE @something int = (select 1 from 2 where 3)
DECLARE @somethingelse int = (select 1 from 2 where 3)
declare @athirdthing int = (select 1 from 2 where 3)
