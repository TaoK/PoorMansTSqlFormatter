BULK INSERT SomeDB.SomeSchema.SomeTable
   FROM 'c:\SomeFolder\SomeFile.txt'
   WITH 
      (
         FIELDTERMINATOR =' |',
         ROWTERMINATOR =' |\n'
      )