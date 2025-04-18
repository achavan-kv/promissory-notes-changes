IF EXISTS (SELECT * FROM sysobjects WHERE NAME = 'EMA_GetCustomerDOB')
BEGIN
	DROP PROCEDURE [dbo].[EMA_GetCustomerDOB]
END
GO
 
-- =============================================
-- Author:	<Author,Yogesh Mali>
-- Create date: <Create Date,,>
-- Description:	<Description,,> EMA_GetCustomerDOB '14737331SSUVP',''
-- =============================================
CREATE PROCEDURE [dbo].[EMA_GetCustomerDOB]
	    @CustId VARCHAR(20) = N'', 
		@Message varchar(MAX) output
AS
BEGIN
	
	IF exists (SELECT * FROM customer WHERE custid = @CustId)
		 select CONVERT(varchar, dateborn, 101) as 'dateborn' from customer where custid = @CustId 
	ELSE 
	Begin
		Set @Message='Record not found'
		select @Message
	End
END