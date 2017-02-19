CREATE TABLE [dbo].[ReturnHit]
(
	[ReturnHitId] BIGINT NOT NULL PRIMARY KEY, 
    [ReturnId] BIGINT NOT NULL,
    [ByAccountId] BIGINT NOT NULL,
	[HitType] TINYINT,
	[Source] VARCHAR(255),
    [Created] SMALLDATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [FK_ReturnHit_Vacancy] FOREIGN KEY ([ReturnId]) REFERENCES [Return]([ReturnId]),
    CONSTRAINT [FK_ReturnHit_Account] FOREIGN KEY ([ByAccountId]) REFERENCES [Organisation]([OrganisationId])
)
