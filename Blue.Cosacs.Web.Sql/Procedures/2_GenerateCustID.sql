

/****** Object:  StoredProcedure [dbo].[GenerateCustID]    Script Date: 11/19/2018 1:31:23 PM ******/
IF EXISTS (SELECT * FROM sysobjects 
   WHERE NAME = 'GenerateCustID'
   )
BEGIN
DROP PROCEDURE [dbo].[GenerateCustID]
END
GO

/****** Object:  StoredProcedure [dbo].[GenerateCustID]    Script Date: 11/19/2018 1:31:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Kute
-- Create date: 26-06-2018
-- Description:	To generate random n characters to create CustId.
/*
EXEC GenerateCustID @len = 3, @exclude = '0123456789:;<=>?@O[]`^\/_-'
EXEC GenerateCustID @len = 8, @exclude = '0:;<=>?@O[]`^\/_-'
DECLARE @CustId VARCHAR(20)
EXEC GenerateCustID @IDNumber = '123', @FirstName = 'First', @LastName = 'Last',@len = 8, @exclude = '*0123456789:;<=>?@O[]`^\/_-', @CustId = @CustId OUTPUT
SELECT @CustId
*/
-- =============================================
CREATE PROCEDURE [dbo].[GenerateCustID]
(
	--@DateBorn DATETIME,
	@IDNumber char(30),
	@FirstName VARCHAR(30),
	@MiddleName VARCHAR(30),
	@LastName VARCHAR(60),
	@len INT = NULL,
	@exclude varchar(50) = NULL,
	@DateBorn DATETIME = NULL,
	@CustId VARCHAR(20) OUTPUT
)
AS
BEGIN
    DECLARE @char char
    DECLARE @output varchar(50)
	Declare @strDateBorn nvarchar(8)

	SET @output = LTRIM(RTRIM(REPLACE(CONVERT(CHAR(12),@DateBorn,23), '-','')))
	
	PRINT CONVERT(VARCHAR(MAX), @len)
	IF(@len IS NULL OR @len > 3)
		set @len = 3
 
	SET @output = @output + SUBSTRING(@FirstName, 1, 1) + CASE WHEN @MiddleName <> '' THEN SUBSTRING(@MiddleName, 1, 1) ELSE 'X' END + SUBSTRING(@LastName, 1, 1);

	-- Declare the return variable here
	IF(@exclude IS NULL)
		SET @exclude = '0123456789:;<=>?@O[]`^\/_-'

	Declare @cntCustId int
		Select @cntCustId = COUNT(*) from customer where custid LIKE ''+@output+'%' 

	SET @output = @output + CAST(@cntCustId as varchar(10))
	 
	PRINT CONVERT(VARCHAR(25), UPPER(@output))
	SET @CustId = UPPER(@output)
END


GO

