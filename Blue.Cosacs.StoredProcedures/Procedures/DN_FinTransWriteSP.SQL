IF EXISTS (SELECT 'A' FROM SYS.PROCEDURES WHERE NAME = 'DN_FinTransWriteSP')
	DROP PROC DN_FinTransWriteSP
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET NOCOUNT OFF; 
Go

CREATE PROCEDURE 	[dbo].[DN_FinTransWriteSP]
			@origbr smallint,
			@branchno smallint,
			@acctno varchar(12),
			@transrefno int,
			@datetrans datetime,
			@transtypecode varchar(3),
			@empeeno int,
			@transupdated char(1),
			@transprinted char(1),
			@transvalue money,
			@bankcode varchar(6),
			@bankacctno varchar(20),   --IP - 09/02/10 - CR1064 Merged - Malaysia Enhancements (CR1072)
			@chequeno varchar(16),
			@ftnotes varchar(4),  
			@paymethod smallint, 
			@runno int,
			@source varchar(10),
			@agrmtno int,
			@cashierTotID INT = null,
			@return int OUTPUT
AS
	declare    @count SMALLINT
	declare	@fintransId INT				
   --preventing duplicate key on insert removing as handled in trigger
  
   
	SET 	@return = 0			--initialise return code

   if @transtypecode in ('DEL','GRT','REB','REP','RPO', 'RDL')
      return 0

	IF @transtypecode = 'DEL'
	BEGIN 
		SET @datetrans = CONVERT(SMALLDATETIME, CONVERT(VARCHAR(10), @datetrans, 105), 105)
	END

   if @ftnotes =''  --marking as .Net if blank so that we can always identify that this definitely posted by application
     set @ftnotes ='dnet'

	IF NOT EXISTS (SELECT 1 FROM fintrans WHERE acctno =@acctno AND datetrans = @datetrans AND branchno = @branchno	AND transrefno = @transrefno) 
		INSERT
		INTO		fintrans
				(origbr, branchno, acctno, transrefno, datetrans, transtypecode,
				empeeno, transupdated, transprinted, transvalue, bankcode, 
				bankacctno, chequeno, ftnotes, paymethod, runno, source, agrmtno)
		VALUES	(@origbr, @branchno, @acctno, @transrefno, @datetrans, @transtypecode,
				@empeeno, @transupdated, @transprinted, @transvalue, @bankcode, 
				@bankacctno, @chequeno, @ftnotes, @paymethod, @runno, @source, @agrmtno)
	ELSE 
		UPDATE fintrans SET transvalue = transvalue + @transvalue 
		WHERE acctno =@acctno AND datetrans = @datetrans AND branchno = @branchno
		AND transrefno = @transrefno

			
	-- now update CashierTotalBreakdown with FintransID if Shortage or Overage
	select @fintransId = @@IDENTITY	
	if @transtypecode in('SHO','OVE') and @cashierTotID is not null
		UPDATE dbo.CashierTotalsBreakdown
		set FintransId=@fintransId
		Where cashiertotalid=@cashierTotID and paymethod=@paymethod

	IF (@@error != 0)
		SET @return = @@error

	--Details	: Added below code for only CLA outstanding balance.
	-- Calling proceduer to insert payment component into new payment details table 

IF EXISTS (SELECT 'A' FROM ACCT WHERE acctno = @acctno and isAmortized =1 and IsAmortizedOutStandingBal =1)
BEGIN
	DECLARE @TransvalueCredit Money,
			@CDAcctNo INT
	IF(@transtypecode = 'PAY')
	BEGIN
		--SET @TransvalueCredit = @transvalue*-1 -- Changing the sign from -ve to =ve debit operation because transvalue of PAY is already -ve.
		-- Calling proceduer to insert payment component into new payment details table 
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('PAY',1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @transvalue, 'PAY',@CDAcctNo,0		--Debit (-ve entry)
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('BNS',-1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @transvalue, 'BNS',@CDAcctNo,0		--Credit (-ve entry)
		SET @TransvalueCredit = @transvalue*-1
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('PAY',-1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @TransvalueCredit, 'PAY',@CDAcctNo,0	--Credit (+ve entry)
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('BNS',1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @TransvalueCredit, 'BNS',@CDAcctNo,0	--Credit (+ve entry)
		SET @TransvalueCredit =0;
	END
	IF  (@transtypecode = 'FEE')
	BEGIN
		-- Calling proceduer to insert Late Fee component into new payment details table 
		SET @TransvalueCredit = @transvalue*-1
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('FCL',1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @transvalue, 'FCL',@CDAcctNo,0		--Debit,1301
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('FCL',-1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @TransvalueCredit, 'FCL',@CDAcctNo,0	--Credit,9256
		SET @TransvalueCredit =0
	END
	IF  (@transtypecode = 'CLD' )
	BEGIN
		-- Calling proceduer to insert Loan and disburshment component into new payment details table 
		SET @TransvalueCredit = @transvalue*-1
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('CLD',1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @transvalue, 'CLD',@CDAcctNo,0		--Debit,5176
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('CLD',-1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @TransvalueCredit, 'CLD',@CDAcctNo,0	--Credit, 2980
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('BHP',1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @transvalue, 'BHP',@CDAcctNo,0		--Debit, 1301
		SELECT @CDAcctNo = dbo.fn_CLAGetTranstypeAcctNo('BHP',-1)
		EXEC CLAmortizationInsertNewPayMentDetails @acctno, @TransvalueCredit, 'BHP',@CDAcctNo,0		--Credit, 5176
		SET @TransvalueCredit =0
	END
END