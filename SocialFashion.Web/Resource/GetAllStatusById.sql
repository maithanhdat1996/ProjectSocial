USE [SocialFashion]
GO

/****** Object:  StoredProcedure [dbo].[GetAllStatusById]    Script Date: 12/28/2016 3:38:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[GetAllStatusById]', 'P') IS NOT NULL
DROP PROC [dbo].[GetAllStatusById]
GO

CREATE PROCEDURE [dbo].[GetAllStatusById]
	-- Add the parameters for the stored procedure here
	@UserId nvarchar(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from Status where ((UserId = '1' or UserId in (select SenderId from Fan where RequestId = '1') or
UserId in (select RequestId from Fan where SenderId = '1')) and Privacy = 1) or (Privacy = 0) 
END

GO
