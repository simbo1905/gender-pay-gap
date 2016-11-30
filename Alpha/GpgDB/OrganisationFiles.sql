CREATE TABLE [dbo].[OrganisationFiles]
(
--	[Id] INT NOT NULL PRIMARY KEY

	[FileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[OrganisationId] BIGINT NOT NULL,
	[FileType] VARCHAR(255) NOT NULL, 
	[FileSize] int,
	[Filename] varchar(255),
	[ContentType] varchar(255),
	[Created] SMALLDATETIME NULL DEFAULT GETDATE(), 
    [Modified] SMALLDATETIME NULL DEFAULT GETDATE(), 
    CONSTRAINT [FK_Files_Organisation] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisation]([OrganisationId]) 
)
