
-- EMA_Constraints Update Min and Max Error message for telephone number.sql

Go
update [EMA_Constraints] SET [MaxErrorMessage] = 'Telephone number should be 11 digits.' , [MinErrorMessage]='Telephone number should be 11 digits.' Where Qid = 1014
Go