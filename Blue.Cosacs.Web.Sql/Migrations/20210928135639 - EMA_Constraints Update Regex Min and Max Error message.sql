-- Script Comment	: EMA_Constraints Update Regex Min and Max Error message.sql

Go
Update [EMA_Constraints] SET Regex =  '(?=.*[1-9])(?!99.5)^\d{1,2}(\.5)?$'
,[MaxErrorMessage] = 'Number of years at address should not exceed 99.' 
, [MinErrorMessage]='Number of years at address should not be less than 0.5' where Qid = 1050

Go
update [EMA_Constraints] SET [MaxErrorMessage] = 'Net income should not exceed $600,000,000.' , [MinErrorMessage]='Net income should not be less than $500.' Where Qid = 1019
Go
update [EMA_Constraints] SET [MaxErrorMessage] = 'Dependents should not exceed 9.' , [MinErrorMessage]='Dependents should not be less than 0.' Where Qid = 1010
Go