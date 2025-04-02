 IF EXISTS (SELECT * FROM sysobjects WHERE NAME = 'EMA_UpdateCustDetails')
BEGIN
	DROP PROCEDURE [dbo].[EMA_UpdateCustDetails]
END
GO
 
-- =============================================
-- Author:	<Author,Yogesh Mali>
-- Create date: <Create Date,,>
-- Description:	<Description,,> EMA_UpdateCustDetails '14737331SSUVP','MR',''
-- =============================================
Create PROCEDURE [dbo].[EMA_UpdateCustDetails]
	    @CustId VARCHAR(20) = N'', 
		@Title varchar(25)=N'',
		@Message varchar(MAX) output
AS
BEGIN
	
	IF exists (SELECT * FROM customer WHERE custid = @CustId)
		 Update customer SET Title = @Title Where custid = @CustId
	ELSE 
	Begin
		Set @Message='Record not found'
		select @Message
	End
END