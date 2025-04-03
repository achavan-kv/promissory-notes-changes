IF OBJECT_ID('dbo.uspGetCashloanDetails', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.uspGetCashloanDetails;
END
GO
	
CREATE PROCEDURE dbo.uspGetCashloanDetails
    @CustId VARCHAR(20),
    @AcctNo VARCHAR(12),
	@return INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
	SET @return = 0;

    BEGIN TRY
        SELECT 
		c.CodeDescript AS Occupation,
		p.dctext1 AS DocumentNumber1,
		p.dctext2 AS DocumentNumber2,
		p.dctext3 AS DocumentNumber3,
		cl.DatePrinted,
		a.DateAgrmt,
		'[Placeholder]' AS DocumentType,
		'[Placeholder]' AS DailyLateInterest,
		'[Placeholder]' AS LateFee,
		'[Placeholder]' AS ProcessingFee,
		'[Placeholder]' AS InsuranceCompany,
		'[Placeholder]' AS PolicyNumber,
		'[Placeholder]' AS PolicyPreminumPercentage,
		'[Placeholder]' AS LoanStartDate,
		'[Placeholder]' AS TotalInterest,
		0 AS IsCompany
		FROM Proposal p
		INNER JOIN Code c ON c.Code = p.Occupation
		LEFT JOIN CashLoan cl ON cl.CustId = p.CustId AND cl.AcctNo = p.AcctNo
		LEFT JOIN Agreement a ON a.AcctNo = p.AcctNo
		WHERE p.CustId = @CustId
		  AND p.AcctNo = @AcctNo;

        ---- Instalment Plan
        --SELECT InstalNo, InstalAmount, DateFirst
        --FROM InstalPlan 
        --WHERE AcctNo = @AcctNo;

        ---- Interest Rate based on TermsType & ScoringBand
        --SELECT ih.Intrate
        --FROM CashLoan cl
        --INNER JOIN IntrateHistory ih ON ih.TermsType = cl.TermsType
        --INNER JOIN Proposal p ON ih.Band = p.ScoringBand
        --WHERE cl.CustId = @CustId
        --  AND cl.AcctNo = @AcctNo
        --  AND p.CustId = @CustId
        --  AND p.AcctNo = @AcctNo;
    END TRY
    BEGIN CATCH
        SET @return = ERROR_NUMBER();
    END CATCH
END
