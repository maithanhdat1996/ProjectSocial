CREATE PROCEDURE [dbo].[User_Update]
	-- Add the parameters for the stored procedure here
	@Id nvarchar(128),
	@Name nvarchar(256),
	@Gender bit,
	@Birthdate datetime,
	@Aboutme nvarchar(500),
	@Website nvarchar(250)

AS
BEGIN
	UPDATE AspNetUsers
	SET Name = @Name, Gender = @Gender, Birthdate = @Birthdate, Aboutme = @Aboutme, Website = @Website
	WHERE Id = @Id
END