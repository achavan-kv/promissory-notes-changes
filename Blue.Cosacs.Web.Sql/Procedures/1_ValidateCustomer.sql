
/****** Object:  StoredProcedure [dbo].[ValidateCustomer]    Script Date: 11/19/2018 1:28:35 PM ******/
IF EXISTS (SELECT * FROM sysobjects WHERE NAME = 'ValidateCustomer')
BEGIN
	DROP PROCEDURE [dbo].[ValidateCustomer]
END
GO

/****** Object:  StoredProcedure [dbo].[ValidateCustomer]    Script Date: 11/19/2018 1:28:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[ValidateCustomer]
	@IdNumber char(30),
	@IdType char(4),
	@PhoneNumber VARCHAR(20),
	@LastName_IN VARCHAR(20),
	@DOB_IN datetime
AS
BEGIN

	DECLARE @ReturnIdNumber char(30) = '';
	DECLARE @ReturnIdType char(4) = '';

	DECLARE @Message VARCHAR(MAX) = '';
	DECLARE @custid varchar(20) = '';
	DECLARE @LastName varchar(60) = '';
	DECLARE @FirstName varchar(30) = '';
	DECLARE @EmailId varchar(60) = '';
	DECLARE @DOB datetime = '';
	DECLARE @Status INT = 0;


	DECLARE @tempTable TABLE (custID varchar(20));

	INSERT INTO @tempTable SELECT custid FROM [dbo].[custtel] tel WHERE  tel.tellocn ='M' AND (REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
																	= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
	INSERT INTO @tempTable SELECT custid FROM [dbo].[customer] cust WHERE UPPER(LTRIM(RTRIM(cust.name))) = UPPER(LTRIM(RTRIM(@LastName_IN)))

	INSERT INTO @tempTable SELECT custid FROM [dbo].[customer] cust WHERE convert(varchar, cust.dateborn, 23) = convert(varchar, @DOB_IN, 23)

	
	
	IF ((@PhoneNumber != '' AND @PhoneNumber IS NOT NULL) AND (ISNULL(@LastName_IN,'') <> '') AND (@DOB_IN != '' AND @DOB_IN IS NOT NULL))
	
		BEGIN
		-----------------------------------------
			IF(
			(SELECT COUNT(*)
				  
					FROM [dbo].[customer] cust
					inner JOIN [dbo].[custtel] tel ON tel.custid = cust.custid
					inner JOIN [dbo].[custaddress] adr ON adr.custid = cust.custid
					WHERE cust.custid in(SELECT custID FROM @tempTable) 
					AND
					(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
						= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
					AND  (UPPER(LTRIM(RTRIM(cust.name))) = UPPER(@LastName_IN) OR ISNULL(LTRIM(RTRIM(cust.name)), '') = '')
					AND convert(varchar, cust.dateborn, 23) = convert(varchar, @DOB_IN, 23)
					and adr.addtype = 'H' and tel.tellocn = 'M'
			)>1
			)
			BEGIN
				PRINT 'Multiple for PhoneNumber'
				SET @Message += 'Multiple users found';		
				SET @Status = 406;
			END
			ELSE
			BEGIN
				SELECT @custid = cust.[custid]
				  ,@LastName = cust.[name]
				  ,@FirstName = cust.[firstname]
				  ,@DOB = cust.[dateborn]
				  ,@EmailId = adr.Email
				  ,@ReturnIdNumber = cust.[IdNumber]
				  ,@ReturnIdType = cust.[IdType]
				  
			  FROM [dbo].[customer] cust
			  LEFT OUTER JOIN [dbo].[custtel] tel ON tel.custid = cust.custid
			  LEFT OUTER JOIN [dbo].[custaddress] adr ON adr.custid = cust.custid
			  WHERE cust.custid in(SELECT custID FROM @tempTable) 
			  AND
				(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
					= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
			  AND  (UPPER(LTRIM(RTRIM(cust.name))) = UPPER(@LastName_IN) OR ISNULL(LTRIM(RTRIM(cust.name)), '') = '')
			  AND convert(varchar, cust.dateborn, 23) = convert(varchar, @DOB_IN, 23)
			  --AND  (LTRIM(RTRIM(cust.dateborn)) = @DOB_IN OR ISNULL(LTRIM(RTRIM(cust.dateborn)), '') = '')

				IF(@custid != '' AND @custid != '0')
			
				BEGIN
					SET @Message += 'User Exists with customer : ' + @custid;
					SET @Status = 200;
				END
			ELSE
				BEGIN
					SELECT @custid = cust.[custid]
				  ,@LastName = cust.[name]
				  ,@FirstName = cust.[firstname]
				  ,@DOB = cust.[dateborn]
				  ,@EmailId = adr.Email
				  ,@ReturnIdNumber = cust.[IdNumber]
				  ,@ReturnIdType = cust.[IdType]
				  
				  FROM [dbo].[customer] cust
				  LEFT OUTER JOIN [dbo].[custtel] tel ON tel.custid = cust.custid
				  LEFT OUTER JOIN [dbo].[custaddress] adr ON adr.custid = cust.custid
				  WHERE cust.custid in(SELECT custID FROM @tempTable)
				  AND
					(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
						= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
				  AND  
					UPPER(LTRIM(RTRIM(cust.name))) != UPPER(@LastName_IN)
				  AND  
					convert(varchar, cust.dateborn, 23) != convert(varchar, @DOB_IN, 23)
					--LTRIM(RTRIM(cust.dateborn)) != @DOB_IN
					

				  IF(@custid != '' AND @custid != '0')
				  
					BEGIN
						SET @Message = 'No user found';
						SET @Status = 404;
					END
				  ELSE
					BEGIN
						SELECT @custid = cust.[custid]
					  ,@LastName = cust.[name]
					  ,@FirstName = cust.[firstname]
					  ,@DOB = cust.[dateborn]
					  ,@EmailId = adr.Email
					  ,@ReturnIdNumber = cust.[IdNumber]
					  ,@ReturnIdType = cust.[IdType]
				  
					  FROM [dbo].[customer] cust
					  LEFT OUTER JOIN [dbo].[custtel] tel ON tel.custid = cust.custid
					  LEFT OUTER JOIN [dbo].[custaddress] adr ON adr.custid = cust.custid
					  WHERE cust.custid in(SELECT custID FROM @tempTable) 
					  AND
						(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
							= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
					  AND  
						(
							UPPER(LTRIM(RTRIM(cust.name))) != UPPER(@LastName_IN)
							OR  
							convert(varchar, cust.dateborn, 23) != convert(varchar, @DOB_IN, 23)
							--(LTRIM(RTRIM(cust.dateborn)) != @DOB_IN OR ISNULL(LTRIM(RTRIM(cust.dateborn)), '') = '')
						)

					  IF(@custid != '' AND @custid != '0')
					  
						BEGIN
							select @Message = [value] from CountryMaintenance where codename = 'EMMA406ErrorMessage'
							SET @Status = 406;
						END
					  ELSE 
						BEGIN
							SELECT @custid = cust.[custid]
						  ,@LastName = cust.[name]
						  ,@FirstName = cust.[firstname]
						  ,@DOB = cust.[dateborn]
						  ,@EmailId = adr.Email
						  ,@ReturnIdNumber = cust.[IdNumber]
						  ,@ReturnIdType = cust.[IdType]
				  
						  FROM [dbo].[customer] cust
						  LEFT OUTER JOIN [dbo].[custtel] tel ON tel.custid = cust.custid
						  LEFT OUTER JOIN [dbo].[custaddress] adr ON adr.custid = cust.custid
						  WHERE cust.custid in(SELECT custID FROM @tempTable) 
						  AND
							(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(tel.telno)) , '-', ''),' ',''),'+','')			
								!= REPLACE(REPLACE( REPLACE(LTRIM(RTRIM(@PhoneNumber)), '-', ''),' ',''),'+',''))
						  AND  
							UPPER(LTRIM(RTRIM(cust.name))) = UPPER(@LastName_IN)
						  AND  
							convert(varchar, cust.dateborn, 23) = convert(varchar, @DOB_IN, 23)

							IF(@custid != '' AND @custid != '0')
					  
								BEGIN
									select @Message = [value] from CountryMaintenance where codename = 'EMMA406ErrorMessage'
									SET @Status = 406;
								END
							ELSE
							BEGIN
								SET @Message = 'No user found';
								SET @Status = 404;
							END
						END
						
					END 
				END
			END
		END
	
	
	SELECT 
		@ReturnIdNumber 'IdNumber'
		, CASE 
			WHEN ISNULL(LTRIM(RTRIM(@ReturnIdType)), '') <> '' 
			THEN 
				CASE WHEN LTRIM(RTRIM(@ReturnIdType)) = 'P' THEN 'PP' ELSE
					CASE WHEN LTRIM(RTRIM(@ReturnIdType)) = 'D' THEN 'DP' ELSE
						CASE WHEN LTRIM(RTRIM(@ReturnIdType)) = 'I' THEN 'NI' ELSE LTRIM(RTRIM(@ReturnIdType)) 
						END 
					END 
				END
			ELSE '' 
		  END'IdType'
		, @custid 'CustomerId'
		, @LastName 'LastName'
		, @FirstName 'FirstName'
		, @DOB 'DateOfBirth'
		, @EmailId 'EmailId'
		, @Message 'Message'
		, @Status 'StatusCode' 

END	 
