

-- Script Comment	: CountryMaintenance  Add parameter in Basic Details for EMMA 406 error message

IF NOT EXISTS(select * from CountryMaintenance where codename = 'EMMA406ErrorMessage')
BEGIN

    INSERT INTO CountryMaintenance (CountryCode, ParameterCategory, Name, Value, Type, Precision, OptionCategory, OptionListName, Description, CodeName)
    SELECT CountryCode, '01', 'EMMA B,C,F scenario(406 ResponseCode)', 
	CASE WHEN CountryCode = 'J' THEN 'Oh no! Seems like we’ve encountered an issue! Please contact 1-866-447-2474 or email jamaicaemmasupport@unicomer.com for further assistance.'
	     WHEN CountryCode = 'T' THEN 'Oh no! Seems like we’ve encountered an issue! Please contact 1-868-800-4222 or email trinidademmasupport@unicomer.com for further assistance.'
		 else 'Oh no! Seems like we’ve encountered an issue! Please contact Xcontactcenternumber or email Xemmasupport@unicomer.com for further assistance.' END
	, 'text', 0, '', '', 'Enter error message to populate in EMMA for Contact to customer care error message', 'EMMA406ErrorMessage'
    FROM Country

END
 
 

 
GO