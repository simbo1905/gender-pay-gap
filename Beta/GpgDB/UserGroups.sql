CREATE TABLE [dbo].[UserGroups]
(
	[UserId] BIGINT NOT NULL , 
    [GroupId] BIGINT NOT NULL, 
    [Created] NCHAR(10) NULL DEFAULT GETDATE(), 
    PRIMARY KEY ([GroupId], [UserId]), 
    CONSTRAINT [FK_UserGroups_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]),
    CONSTRAINT [FK_UserGroups_Group] FOREIGN KEY ([GroupId]) REFERENCES [Group]([GroupId])
)
