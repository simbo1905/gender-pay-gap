CREATE TABLE [dbo].[OrganisationAddress]
(
	--[Id] INT NOT NULL PRIMARY KEY

	[OrganisationID]	BIGINT NOT NULL, 
    [Type] VARCHAR(255) NULL, 
   -- [Easting] int NULL, 
   -- [Northing] int NULL, 
    [Address1] VARCHAR(255) NULL, 
    [Address2] VARCHAR(255) NULL, 
    [Address3] VARCHAR(255) NULL, 
    [TownCity] VARCHAR(255) NULL, 
    [County] VARCHAR(255) NULL, 
    [CountryCode] VARCHAR(10) NULL, 
    [PostCode] VARCHAR(20) NULL, 
    [Created] SMALLDATETIME NULL DEFAULT GETDATE(), 
    [Modified] SMALLDATETIME NULL DEFAULT GETDATE(), 
    CONSTRAINT [FK_Address_Organisation] FOREIGN KEY ([OrganisationID]) REFERENCES [Organisation]([OrganisationID]),
    PRIMARY KEY ([OrganisationID])
)
