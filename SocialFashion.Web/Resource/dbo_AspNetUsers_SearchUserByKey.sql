USE [SocialFashion]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[AspNetUsers_SearchUserByKey]', 'P') IS NOT NULL
DROP PROC [dbo].[AspNetUsers_SearchUserByKey]
GO

CREATE PROCEDURE [dbo].[AspNetUsers_SearchUserByKey] 
	@Keyword nvarchar(256)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM dbo.AspNetUsers 
	WHERE dbo.AspNetUsers.Name LIKE '%'+@Keyword+'%'
	OR dbo.AspNetUsers.UserName LIKE '%'+@Keyword+'%'
END
