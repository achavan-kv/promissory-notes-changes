GO
SET IDENTITY_INSERT [dbo].[CreditAppQuestionnaire] ON
GO
IF NOT EXISTS(select * from [CreditAppQuestionnaire] where [QuestionId] = 1054)
BEGIN
	INSERT [dbo].[CreditAppQuestionnaire] ([QuestionId], [Question], [InputType], [InputCategory], [CategorySection], [IsMandatory], [IsActive], [Category], [Sequence]) VALUES (1054, N'Upload your Additional Income proof', N'IMAGE', N'NONE', N'DOCUMENTS', 1, 1, NULL, 24)
END
GO
IF NOT EXISTS(select * from [CreditAppQuestionnaire] where [QuestionId] = 1055)
BEGIN
	INSERT [dbo].[CreditAppQuestionnaire] ([QuestionId], [Question], [InputType], [InputCategory], [CategorySection], [IsMandatory], [IsActive], [Category], [Sequence]) VALUES (1055, N'Enter your Home Address Instructions', N'TEXTBOX', N'NONE', N'Personal', 1, 1, NULL, 3)
END
GO
SET IDENTITY_INSERT [dbo].[CreditAppQuestionnaire] OFF


GO
UPDATE CreditAppQuestionnaire SET IsActive = 0, [Sequence]= 999 Where QuestionId = 1001
Go
update CreditAppQuestionnaire SET Question = 'Enter your Reference 2 Contact', InputType = 'CONTACT', InputCategory= 'PHONE', CategorySection = 'Reference2', 
IsMandatory = 1, IsActive = 1 where questionid = 1035
Go
update CreditAppQuestionnaire SET [Sequence]= 1, isactive = 1, IsMandatory = 1 Where QuestionId= 1002
Go
update CreditAppQuestionnaire SET [Sequence]= 2 Where QuestionId= 1004
Go
update CreditAppQuestionnaire SET [Sequence]= 3 Where QuestionId= 1055
Go
update CreditAppQuestionnaire SET [Sequence]= 4 Where QuestionId= 1046
Go
update CreditAppQuestionnaire SET [Sequence]= 5 Where QuestionId= 1010
Go
update CreditAppQuestionnaire SET [Sequence]= 6 Where QuestionId= 1009
Go
update CreditAppQuestionnaire SET [Sequence]= 7 Where QuestionId= 1011
Go
update CreditAppQuestionnaire SET [Sequence]= 8 Where QuestionId= 1012
Go
update CreditAppQuestionnaire SET [Sequence]= 9 Where QuestionId= 1047
Go
update CreditAppQuestionnaire SET [Sequence]= 10 Where QuestionId= 1048
Go
update CreditAppQuestionnaire SET [Sequence]= 11 Where QuestionId= 1013
Go
update CreditAppQuestionnaire SET [Sequence]= 12 Where QuestionId= 1014
Go
update CreditAppQuestionnaire SET [Sequence]= 13 Where QuestionId= 1015
Go
update CreditAppQuestionnaire SET [Sequence]= 14 Where QuestionId= 1049
Go
update CreditAppQuestionnaire SET [Sequence]= 15 Where QuestionId= 1050
Go
update CreditAppQuestionnaire SET [Sequence]= 16 Where QuestionId= 1019
Go
update CreditAppQuestionnaire SET [Sequence]= 17 Where QuestionId= 1052
Go
update CreditAppQuestionnaire SET [Sequence]= 18 Where QuestionId= 1028
Go
update CreditAppQuestionnaire SET [Sequence]= 19 Where QuestionId= 1035
Go
update CreditAppQuestionnaire SET [Sequence]= 20 Where QuestionId= 1042
Go
update CreditAppQuestionnaire SET [Sequence]= 21 Where QuestionId= 1053
Go
update CreditAppQuestionnaire SET [Sequence]= 22 Where QuestionId= 1043
Go
update CreditAppQuestionnaire SET [Sequence]= 23 Where QuestionId= 1044
Go
update CreditAppQuestionnaire SET [Sequence]= 24 Where QuestionId= 1054
Go
