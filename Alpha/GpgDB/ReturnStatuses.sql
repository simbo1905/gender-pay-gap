CREATE TABLE [dbo].[ReturnStatuses]
(
	[ReturnStatusId] BIGINT NOT NULL,
	[ReturnId] BIGINT NOT NULL, 
    [StatusId] TINYINT NOT NULL, 
    [StatusDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [StatusMessage] VARCHAR(512) NULL, 
	[ByUserId] BIGINT NULL,
    CONSTRAINT [FK_ReturnStatuses_Vacancy] FOREIGN KEY ([ReturnId]) REFERENCES [Return]([ReturnId]),
	CONSTRAINT [FK_ReturnStatuses_User] FOREIGN KEY ([ByUserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [PK_ReturnStatuses] PRIMARY KEY ([ReturnStatusId])	
)