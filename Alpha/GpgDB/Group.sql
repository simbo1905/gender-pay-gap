CREATE TABLE [dbo].[Group]
(
	[GroupId] BIGINT NOT NULL PRIMARY KEY, 
	OrganisationId BIGINT NULL,
	[GroupRef] VARCHAR(50) NULL,
	[UserId] BIGINT NOT NULL, 
	[GroupParentId] BIGINT  , 
    [GroupName] VARCHAR(255) NOT NULL, 
    [GroupDescription] VARCHAR(512) NULL, 
    [Created] SMALLDATETIME NOT NULL DEFAULT GETDATE(), 
    [Modified] SMALLDATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [FK_Group_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]),
	CONSTRAINT [FK_Group_Organisation] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId])
)
