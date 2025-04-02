 
  IF EXISTS (SELECT * FROM sysobjects WHERE NAME = 'EMA_UpdateAcctTermsType')
BEGIN
	DROP PROCEDURE [dbo].[EMA_UpdateAcctTermsType]
END
GO
 

-- =============================================
-- Author:	<Author,Yogesh Mali>
-- Create date: <Create Date,,>
-- Description:	<Description,,> EMA_UpdateAcctTermsType '184000022071',''
-- =============================================
CREATE PROCEDURE [dbo].[EMA_UpdateAcctTermsType]
	    @AcctNo VARCHAR(20) = N'', 
		@Message varchar(MAX) output
AS
BEGIN
	
	IF exists (SELECT * FROM acct WHERE acctno = @AcctNo)
		 Update acct SET termstype = 'UA' Where acctNo = @AcctNo
	ELSE 
	Begin
		Set @Message='Record not found'
		select @Message
	End
END