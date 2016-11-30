﻿CREATE TABLE [dbo].[Organisation]
(
	[OrganisationId]	BIGINT NOT NULL, 
	[GroupId] BIGINT NOT NULL, 
	[URN] VARCHAR(50) NULL ,
    [OrganisationName] VARCHAR(250) NOT NULL, 
    [OrganisationType] VARCHAR(255) NULL, 
    [OrganisationPhase] VARCHAR(30) NULL, 
    [OrganisationPolicy] VARCHAR(255) NULL, 
    [OrganisationDescription] VARCHAR(MAX) NULL, 
    --[SchoolTrust] VARCHAR(255) NULL, 
    --[PupilAgeMin] TINYINT NULL, 
    --[PupilAgeMax] TINYINT NULL, 
    --[Gender] VARCHAR(20) NULL, 
    [Capacity] INT NULL, 
    [Population] INT NULL, 
    --[HeadName] VARCHAR(30) NULL, 
    [Phone] VARCHAR(30) NULL, 
    [Email] VARCHAR(500) NULL,
    [Web] VARCHAR(500) NULL, 
    [CurrentStatus] VARCHAR(20) NOT NULL, 
    [CurrentStatusDate] SMALLDATETIME NOT NULL, 
    [CurrentStatusDetails] VARCHAR(512) NULL,
    [Created] SMALLDATETIME NULL DEFAULT GETDATE(), 
    [Modified] SMALLDATETIME NULL,
    CONSTRAINT [FK_Organisation_Group] FOREIGN KEY ([GroupId]) REFERENCES [Group]([GroupId]),
    PRIMARY KEY ([OrganisationId])
)
