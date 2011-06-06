CREATE TRIGGER TestTrigger ON TestTable
AFTER INSERT
AS
DECLARE @OneThing tinyint, @AnotherThing int;
IF EXISTS (SELECT *
           FROM SomeTable p 
           JOIN inserted AS i 
           ON p.SomeID = i.SomeID
           JOIN OtherTable AS v 
           ON v.OtherID = p.OtherID
           WHERE v.SomeValue = 5
          )
BEGIN
RAISERROR ('Something bad happened, sorry.', 16, 1);
ROLLBACK TRANSACTION;
RETURN 
END;
