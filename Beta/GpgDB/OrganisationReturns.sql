CREATE TABLE [dbo].[OrganisationGPGReturns]
(

	--[Id] INT NOT NULL PRIMARY KEY

	[OrganisationId] BIGINT NOT NULL, 
	[ReturnId] BIGINT NOT NULL, 
    [Created] SMALLDATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [PK_VacancySchools] PRIMARY KEY ([OrganisationId], [ReturnId]), 
    CONSTRAINT [FK_VacancySchools_School] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisation]([OrganisationId]),
    CONSTRAINT [FK_VacancySchools_Vacancy] FOREIGN KEY ([ReturnId]) REFERENCES [Return]([ReturnId])
)