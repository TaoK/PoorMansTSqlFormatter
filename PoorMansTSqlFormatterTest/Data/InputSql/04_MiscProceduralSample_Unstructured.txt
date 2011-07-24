Create Table #ValueTable (Id nvarchar(5))
declare @MonthIDs nvarchar(4000)
declare @WorkingString AS nvarchar(4000)
select @MonthIDs = '3,5,6,8,11'
select @WorkingString = @MonthIDs
WHILE len(@WorkingString) > 0
BEGIN 
--Printing the current temp string and the position of the first comma in it
--Print @WorkingString
--Print patindex('%,%',@WorkingString)
-- Inserting the string before the first comma
Insert #ValueTable(Id)
values(substring(@WorkingString,1,patindex('%,%',@WorkingString)-1))
--resetting the temporary string to start after the first comma in the previous temp string
select @WorkingString = substring(@WorkingString,patindex('%,%',@WorkingString)+1,Len(@WorkingString))
--Checking if there is no more commas 
IF patindex('%,%',@WorkingString)<=0
Begin
--Inserting the last ID
Insert #ValueTable(Id)
values(@WorkingString)
BREAK
End
ELSE
CONTINUE
END

-- Getting the Employee Data
select *
from HumanResources.Employee e
join #ValueTable t
on month(e.BirthDate)= t.Id

drop table #ValueTable 