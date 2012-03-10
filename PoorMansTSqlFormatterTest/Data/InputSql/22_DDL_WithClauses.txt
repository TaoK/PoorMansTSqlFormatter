
IF OBJECT_ID ('dbo.DummyProc', 'P') IS NOT NULL 
    DROP PROCEDURE dbo.DummyProc;
GO
CREATE PROCEDURE dbo.DummyProc WITH RECOMPILE, encryption, execute as caller
AS
SELECT user_name();
GO


CREATE PROC ClrProc
    @SomeParameter MyType
WITH EXECUTE AS self
AS EXTERNAL NAME SomeAssembly.SomeClass.SomeMethod
GO
CREATE QUEUE TestQueue WITH STATUS=OFF ;
go
CREATE QUEUE TestQueue
    WITH STATUS=ON,
    ACTIVATION (
        PROCEDURE_NAME = SomeProc,
        MAX_QUEUE_READERS = 5,
        EXECUTE AS 'PerplexedUser' ) ;

GO

CREATE INDEX [IX_Table1] ON [dbo].[Table1] 
(
                [Col1] ASC
)
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON ) ON [PRIMARY]
GO