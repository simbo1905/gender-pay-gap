CREATE TABLE [dbo].[UserStatuses]
(
	[UserStatusId] BIGINT NOT NULL,
	[UserId] BIGINT NOT NULL, 
    [StatusId] TINYINT NOT NULL, 
    [StatusDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [StatusMessage] VARCHAR(512) NULL, 
	[ByUserId] BIGINT NULL,
	CONSTRAINT [FK_UserStatuses_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
	CONSTRAINT [FK_UserStatuses_ByUser] FOREIGN KEY ([ByUserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [PK_UserStatuses] PRIMARY KEY ([UserStatusId])	
)