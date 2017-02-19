CREATE TABLE [dbo].[OrganisationStatuses]
(
--	[Id] INT NOT NULL PRIMARY KEY

	[OrganisationStatusId] BIGINT NOT NULL,
	[OrganisationId] BIGINT NOT NULL, 
    [StatusId] TINYINT NOT NULL, 
    [StatusDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [StatusMessage] VARCHAR(512) NULL, 
	[ByUserId] BIGINT NULL,
	CONSTRAINT [FK_OrganisationStatuses_School] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisation]([OrganisationId]),
	CONSTRAINT [FK_OrganisationStatuses_User] FOREIGN KEY ([ByUserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [PK_OrganisationStatus] PRIMARY KEY ([OrganisationStatusId])	
)
