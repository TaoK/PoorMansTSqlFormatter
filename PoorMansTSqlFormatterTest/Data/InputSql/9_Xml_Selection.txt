USE master
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fn_hexadecimal_to_varbin]') and OBJECTPROPERTY(id, N'IsFunction') = 1)
drop function [dbo].[fn_hexadecimal_to_varbin]
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE FUNCTION fn_hexadecimal_to_varbin (@hexvalue varchar(max))
RETURNS varbinary(max)
AS
BEGIN
	/* 
	From MS Article: http://blogs.msdn.com/b/sqltips/archive/2008/07/02/converting-from-hex-string-to-varbinary-and-vice-versa.aspx
	*/
	DECLARE @OutValue varbinary(max)
	select @OutValue = cast('' as xml).value('xs:hexBinary( substring(sql:variable("@hexvalue"), sql:column("t.pos")) )', 'varbinary(max)')
	from (select case substring(@hexvalue, 1, 2) when '0x' then 3 else 0 end) as t(pos)
	RETURN @OutValue
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

