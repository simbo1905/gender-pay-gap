﻿CREATE TABLE [dbo].[User]
(
	[UserId] BIGINT NOT NULL,
	[UserRef] VARCHAR(50) NULL,
	[AuthProviderId] INT,
	[AuthUserTokenId] VARCHAR(50),
    [UserType] VARCHAR(20) NULL, 
    [Title] VARCHAR(20) NULL, 
    [Firstname] VARCHAR(50) NULL, 
    [Lastname] VARCHAR(50) NULL, 
	[Address1] VARCHAR(255) NULL, 
    [Address2] VARCHAR(255) NULL, 
    [Address3] VARCHAR(255) NULL, 
    [TownCity] VARCHAR(255) NULL, 
    [County] VARCHAR(255) NULL, 
    [CountryCode] VARCHAR(10) NULL, 
    [PostCode] VARCHAR(20) NULL, 
    [WorkPhone1] VARCHAR(30) NULL, 
    [WorkPhone2] VARCHAR(30) NULL, 
    [HomePhone] VARCHAR(30) NULL, 
    [Mobile] VARCHAR(30) NULL, 
    [Fax] VARCHAR(30) NULL, 
    [Web] VARCHAR(500) NULL, 
    [Email1] VARCHAR(500) NULL,
    [Email2] VARCHAR(500) NULL,
    [CurrentStatus] VARCHAR(20) NOT NULL, 
    [CurrentStatusDate] SMALLDATETIME NOT NULL, 
    [CurrentStatusDetails] VARCHAR(512) NULL,
    [Created] SMALLDATETIME NULL DEFAULT GETDATE(), 
    [Modified] SMALLDATETIME NULL DEFAULT GETDATE()
    PRIMARY KEY ([UserId]), 
)
