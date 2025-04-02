IF OBJECT_ID('Merchandising.AutoCancelPurchaseOrders') IS NOT NULL
	DROP PROCEDURE Merchandising.AutoCancelPurchaseOrders
GO 

-- =============================================================================
-- Author:		Abhijeet Gawali 
-- Create date: 
-- Description:	This stored procedure is used to auto cancel purchase order.
-- Version:		001 - Removed EstimatedDeliveryDate condition check and included in [Merchandising].[AutoExpirePurchaseOrders]
-- =============================================================================

CREATE PROCEDURE [Merchandising].[AutoCancelPurchaseOrders]		
	@CancelDate DATETIME,
	@DaysTillCancel INT
AS

BEGIN
	DECLARE @ids TABLE (id INT); 

	;WITH ProductCount
	AS
	(
		SELECT 
			SUM(grp.QuantityReceived) AS quantity, 
			pop.PurchaseOrderId AS id
		FROM Merchandising.PurchaseOrderProduct pop
		INNER JOIN Merchandising.GoodsReceiptProduct grp 
			ON pop.Id = grp.PurchaseOrderProductId
		GROUP BY pop.PurchaseOrderId
	)
	
	UPDATE p
	SET P.Status = 
		CASE 
			WHEN pc.quantity > 0 THEN 'Completed' 
			ELSE 'Cancelled' 
		END
	OUTPUT INSERTED.Id INTO @ids
	FROM Merchandising.PurchaseOrder p
	LEFT JOIN ProductCount pc 
		ON pc.Id = p.Id
	WHERE p.Status = 'Expired'
	-- Code commented by Abhijeet
	--AND NOT EXISTS 
	--(
	--	SELECT 1
	--	FROM Merchandising.PurchaseOrderProduct pop  
	--	WHERE pop.EstimatedDeliveryDate >= DATEADD(DAY, -@DaysTillCancel, @CancelDate)
	--		AND p.Id = pop.PurchaseOrderId
	--)
	AND EXISTS 
	(
		SELECT 1
		FROM Merchandising.PurchaseOrderProduct pop  
		WHERE p.Id = pop.PurchaseOrderId
	)


	;WITH GR
	AS 
	(
		SELECT 
			SUM(rp.QuantityReceived + rp.QuantityCancelled) AS Received, 
			PurchaseOrderProductId
		FROM Merchandising.GoodsReceiptProduct rp
		GROUP BY PurchaseOrderProductId
	)

	UPDATE pop
	SET pop.QuantityCancelled = pop.QuantityOrdered - ISNULL(GR.Received, 0)
	FROM Merchandising.PurchaseOrderProduct pop
	INNER JOIN Merchandising.PurchaseOrder po
		ON po.Id = pop.PurchaseOrderId
	INNER JOIN @ids i
		ON i.id = po.Id
	LEFT JOIN GR
		ON gr.PurchaseOrderProductId = pop.Id
	WHERE po.Status = 'Cancelled'

	SELECT id
	FROM @ids
END
