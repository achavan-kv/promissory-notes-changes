-- Script Comment	: EMA_Constraints Add regex constraints for Dependent, Years at location, Net Income, Telephone Number

GO
SET IDENTITY_INSERT [dbo].[EMA_Constraints] ON 
GO
IF NOT EXISTS(select * from [EMA_Constraints] where [QId] = 1010)
BEGIN
	INSERT [dbo].[EMA_Constraints] ([Id], [QId], [Max], [Min], [Regex], [MaxErrorMessage], [MinErrorMessage], [RegexErrorMessage]) VALUES (2, 1010, 0, 9, N'^[0-9]?$', N'Dependent should be less than or equal to 9.', N'Dependent should be greater than 0.', N'answer does not match format')
END
GO
IF NOT EXISTS(select * from [EMA_Constraints] where [QId] = 1050)
BEGIN
	INSERT [dbo].[EMA_Constraints] ([Id], [QId], [Max], [Min], [Regex], [MaxErrorMessage], [MinErrorMessage], [RegexErrorMessage]) VALUES (3, 1050, 0, 99, N'^\d{1,2}(\.5)?$', N'Entered years should be less than 99 years.', N'Entered years should be greater than 0 years.', N'answer does not match format')
END
GO
IF NOT EXISTS(select * from [EMA_Constraints] where [QId] = 1019)
BEGIN
	INSERT [dbo].[EMA_Constraints] ([Id], [QId], [Max], [Min], [Regex], [MaxErrorMessage], [MinErrorMessage], [RegexErrorMessage]) VALUES (4, 1019, 500, 6000000000, N'^(([5-9]\d{2}|[1-9]\d{3,9})|6000000000)$', N'Net income should be less than 6000000000.', N'Net income should be greater than 500.', N'Answer does not match format')
END
GO
IF NOT EXISTS(select * from [EMA_Constraints] where [QId] = 1014)
BEGIN
	INSERT [dbo].[EMA_Constraints] ([Id], [QId], [Max], [Min], [Regex], [MaxErrorMessage], [MinErrorMessage], [RegexErrorMessage]) VALUES (5, 1014, 11, 11, N'(?:(\+1)[ -])?\(?(\d{4})\)?[ -]?(\d{3})[ -]?(\d{4})', N'Telephone number should be 11 characters.', N'Telephone number should be 11 characters.', N'Phone does not match format')
END
GO
SET IDENTITY_INSERT [dbo].[EMA_Constraints] OFF