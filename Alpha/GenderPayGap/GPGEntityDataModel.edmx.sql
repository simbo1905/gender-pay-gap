
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/24/2016 16:26:04
-- Generated from EDMX file: C:\Cadence\GenderPayGap\Alpha\GenderPayGap\GPGEntityDataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [GpgDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Address_Organisation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationAddresses] DROP CONSTRAINT [FK_Address_Organisation];
GO
IF OBJECT_ID(N'[dbo].[FK_Files_Organisation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationFiles] DROP CONSTRAINT [FK_Files_Organisation];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganisationStatuses_School]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationStatuses] DROP CONSTRAINT [FK_OrganisationStatuses_School];
GO
IF OBJECT_ID(N'[dbo].[FK_ReturnHit_Account]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReturnHits] DROP CONSTRAINT [FK_ReturnHit_Account];
GO
IF OBJECT_ID(N'[dbo].[FK_VacancySchools_School]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationGPGReturns] DROP CONSTRAINT [FK_VacancySchools_School];
GO
IF OBJECT_ID(N'[dbo].[FK_VacancySchools_Vacancy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationGPGReturns] DROP CONSTRAINT [FK_VacancySchools_Vacancy];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganisationStatuses_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganisationStatuses] DROP CONSTRAINT [FK_OrganisationStatuses_User];
GO
IF OBJECT_ID(N'[dbo].[FK_ReturnHit_Vacancy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReturnHits] DROP CONSTRAINT [FK_ReturnHit_Vacancy];
GO
IF OBJECT_ID(N'[dbo].[FK_ReturnStatuses_Vacancy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReturnStatuses] DROP CONSTRAINT [FK_ReturnStatuses_Vacancy];
GO
IF OBJECT_ID(N'[dbo].[FK_ReturnStatuses_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReturnStatuses] DROP CONSTRAINT [FK_ReturnStatuses_User];
GO
IF OBJECT_ID(N'[dbo].[FK_UserStatuses_ByUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserStatuses] DROP CONSTRAINT [FK_UserStatuses_ByUser];
GO
IF OBJECT_ID(N'[dbo].[FK_UserStatuses_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserStatuses] DROP CONSTRAINT [FK_UserStatuses_User];
GO
IF OBJECT_ID(N'[dbo].[FK_UserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AuthTokens] DROP CONSTRAINT [FK_UserId];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[C__RefactorLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[C__RefactorLog];
GO
IF OBJECT_ID(N'[dbo].[Organisations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Organisations];
GO
IF OBJECT_ID(N'[dbo].[OrganisationAddresses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganisationAddresses];
GO
IF OBJECT_ID(N'[dbo].[OrganisationFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganisationFiles];
GO
IF OBJECT_ID(N'[dbo].[OrganisationGPGReturns]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganisationGPGReturns];
GO
IF OBJECT_ID(N'[dbo].[OrganisationStatuses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganisationStatuses];
GO
IF OBJECT_ID(N'[dbo].[Returns]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Returns];
GO
IF OBJECT_ID(N'[dbo].[ReturnHits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnHits];
GO
IF OBJECT_ID(N'[dbo].[ReturnStatuses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnStatuses];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[UserStatuses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserStatuses];
GO
IF OBJECT_ID(N'[dbo].[AuthTokens]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AuthTokens];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'C__RefactorLog'
CREATE TABLE [dbo].[C__RefactorLog] (
    [OperationKey] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Organisations'
CREATE TABLE [dbo].[Organisations] (
    [OrganisationId] bigint  NOT NULL,
    [GroupId] bigint  NOT NULL,
    [URN] varchar(50)  NULL,
    [OrganisationName] varchar(250)  NOT NULL,
    [OrganisationType] varchar(255)  NULL,
    [OrganisationPhase] varchar(30)  NULL,
    [OrganisationPolicy] varchar(255)  NULL,
    [OrganisationDescription] varchar(max)  NULL,
    [Capacity] int  NULL,
    [Population] int  NULL,
    [Phone] varchar(30)  NULL,
    [Email] varchar(500)  NULL,
    [Web] varchar(500)  NULL,
    [CurrentStatus] varchar(20)  NOT NULL,
    [CurrentStatusDate] datetime  NOT NULL,
    [CurrentStatusDetails] varchar(512)  NULL,
    [Created] datetime  NULL,
    [Modified] datetime  NULL
);
GO

-- Creating table 'OrganisationAddresses'
CREATE TABLE [dbo].[OrganisationAddresses] (
    [OrganisationID] bigint  NOT NULL,
    [Type] varchar(255)  NULL,
    [Address1] varchar(255)  NULL,
    [Address2] varchar(255)  NULL,
    [Address3] varchar(255)  NULL,
    [TownCity] varchar(255)  NULL,
    [County] varchar(255)  NULL,
    [CountryCode] varchar(10)  NULL,
    [PostCode] varchar(20)  NULL,
    [Created] datetime  NULL,
    [Modified] datetime  NULL
);
GO

-- Creating table 'OrganisationFiles'
CREATE TABLE [dbo].[OrganisationFiles] (
    [FileId] uniqueidentifier  NOT NULL,
    [OrganisationId] bigint  NOT NULL,
    [FileType] varchar(255)  NOT NULL,
    [FileSize] int  NULL,
    [Filename] varchar(255)  NULL,
    [ContentType] varchar(255)  NULL,
    [Created] datetime  NULL,
    [Modified] datetime  NULL
);
GO

-- Creating table 'OrganisationGPGReturns'
CREATE TABLE [dbo].[OrganisationGPGReturns] (
    [OrganisationId] bigint  NOT NULL,
    [ReturnId] bigint  NOT NULL,
    [Created] datetime  NOT NULL
);
GO

-- Creating table 'OrganisationStatuses'
CREATE TABLE [dbo].[OrganisationStatuses] (
    [OrganisationStatusId] bigint  NOT NULL,
    [OrganisationId] bigint  NOT NULL,
    [StatusId] tinyint  NOT NULL,
    [StatusDate] datetime  NOT NULL,
    [StatusMessage] varchar(512)  NULL,
    [ByUserId] bigint  NULL
);
GO

-- Creating table 'Returns'
CREATE TABLE [dbo].[Returns] (
    [ReturnId] bigint  NOT NULL,
    [DiffMeanHourlyPayPercent] decimal(18,0)  NOT NULL,
    [DiffMedianHourlyPercent] decimal(18,0)  NOT NULL,
    [DiffMeanBonusPercent] decimal(18,0)  NOT NULL,
    [DiffMedianBonusPercent] decimal(18,0)  NOT NULL,
    [MaleMedianBonusPayPercent] decimal(18,0)  NOT NULL,
    [FemaleMedianBonusPayPercent] decimal(18,0)  NOT NULL,
    [MaleLowerPayBand] decimal(18,0)  NOT NULL,
    [FemaleLowerPayBand] decimal(18,0)  NOT NULL,
    [MaleMiddlePayBand] decimal(18,0)  NOT NULL,
    [FemaleMiddlePayBand] decimal(18,0)  NOT NULL,
    [MaleUpperPayBand] decimal(18,0)  NOT NULL,
    [FemaleUpperPayBand] decimal(18,0)  NOT NULL,
    [MaleUpperQuartilePayBand] decimal(18,0)  NOT NULL,
    [FemaleUpperQuartilePayBand] decimal(18,0)  NOT NULL,
    [CompanyLinkToGPGInfo] varchar(50)  NOT NULL,
    [CurrentStatus] varchar(20)  NULL,
    [CurrentStatusDate] datetime  NULL,
    [CurrentStatusDetails] varchar(512)  NULL,
    [Created] datetime  NULL,
    [Modified] datetime  NULL
);
GO

-- Creating table 'ReturnHits'
CREATE TABLE [dbo].[ReturnHits] (
    [ReturnHitId] bigint  NOT NULL,
    [ReturnId] bigint  NOT NULL,
    [ByAccountId] bigint  NOT NULL,
    [HitType] tinyint  NULL,
    [Source] varchar(255)  NULL,
    [Created] datetime  NOT NULL
);
GO

-- Creating table 'ReturnStatuses'
CREATE TABLE [dbo].[ReturnStatuses] (
    [ReturnStatusId] bigint  NOT NULL,
    [ReturnId] bigint  NOT NULL,
    [StatusId] tinyint  NOT NULL,
    [StatusDate] datetime  NOT NULL,
    [StatusMessage] varchar(512)  NOT NULL,
    [ByUserId] bigint  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserId] bigint  NOT NULL,
    [UserRef] varchar(50)  NULL,
    [UserType] varchar(20)  NULL,
    [Title] varchar(20)  NULL,
    [Firstname] varchar(50)  NULL,
    [Lastname] varchar(50)  NULL,
    [Address1] varchar(255)  NULL,
    [Address2] varchar(255)  NULL,
    [Address3] varchar(255)  NULL,
    [TownCity] varchar(255)  NULL,
    [County] varchar(255)  NULL,
    [CountryCode] varchar(10)  NULL,
    [PostCode] varchar(20)  NULL,
    [WorkPhone1] varchar(30)  NULL,
    [WorkPhone2] varchar(30)  NULL,
    [HomePhone] varchar(30)  NULL,
    [Mobile] varchar(30)  NULL,
    [Fax] varchar(30)  NULL,
    [Web] varchar(500)  NULL,
    [Email1] varchar(500)  NULL,
    [Email2] varchar(500)  NULL,
    [CurrentStatus] varchar(20)  NOT NULL,
    [CurrentStatusDate] datetime  NOT NULL,
    [CurrentStatusDetails] varchar(512)  NULL,
    [Created] datetime  NULL,
    [Modified] datetime  NULL
);
GO

-- Creating table 'UserStatuses'
CREATE TABLE [dbo].[UserStatuses] (
    [UserStatusId] bigint  NOT NULL,
    [UserId] bigint  NOT NULL,
    [StatusId] tinyint  NOT NULL,
    [StatusDate] datetime  NOT NULL,
    [StatusMessage] varchar(512)  NULL,
    [ByUserId] bigint  NULL
);
GO

-- Creating table 'AuthTokens'
CREATE TABLE [dbo].[AuthTokens] (
    [AuthTokenId] int IDENTITY(1,1) NOT NULL,
    [AuthUserTokenId] nvarchar(max)  NOT NULL,
    [AuthProviderId] int  NOT NULL,
    [UserUserId] bigint  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [OperationKey] in table 'C__RefactorLog'
ALTER TABLE [dbo].[C__RefactorLog]
ADD CONSTRAINT [PK_C__RefactorLog]
    PRIMARY KEY CLUSTERED ([OperationKey] ASC);
GO

-- Creating primary key on [OrganisationId] in table 'Organisations'
ALTER TABLE [dbo].[Organisations]
ADD CONSTRAINT [PK_Organisations]
    PRIMARY KEY CLUSTERED ([OrganisationId] ASC);
GO

-- Creating primary key on [OrganisationID] in table 'OrganisationAddresses'
ALTER TABLE [dbo].[OrganisationAddresses]
ADD CONSTRAINT [PK_OrganisationAddresses]
    PRIMARY KEY CLUSTERED ([OrganisationID] ASC);
GO

-- Creating primary key on [FileId] in table 'OrganisationFiles'
ALTER TABLE [dbo].[OrganisationFiles]
ADD CONSTRAINT [PK_OrganisationFiles]
    PRIMARY KEY CLUSTERED ([FileId] ASC);
GO

-- Creating primary key on [OrganisationId], [ReturnId] in table 'OrganisationGPGReturns'
ALTER TABLE [dbo].[OrganisationGPGReturns]
ADD CONSTRAINT [PK_OrganisationGPGReturns]
    PRIMARY KEY CLUSTERED ([OrganisationId], [ReturnId] ASC);
GO

-- Creating primary key on [OrganisationStatusId] in table 'OrganisationStatuses'
ALTER TABLE [dbo].[OrganisationStatuses]
ADD CONSTRAINT [PK_OrganisationStatuses]
    PRIMARY KEY CLUSTERED ([OrganisationStatusId] ASC);
GO

-- Creating primary key on [ReturnId] in table 'Returns'
ALTER TABLE [dbo].[Returns]
ADD CONSTRAINT [PK_Returns]
    PRIMARY KEY CLUSTERED ([ReturnId] ASC);
GO

-- Creating primary key on [ReturnHitId] in table 'ReturnHits'
ALTER TABLE [dbo].[ReturnHits]
ADD CONSTRAINT [PK_ReturnHits]
    PRIMARY KEY CLUSTERED ([ReturnHitId] ASC);
GO

-- Creating primary key on [ReturnStatusId], [StatusMessage] in table 'ReturnStatuses'
ALTER TABLE [dbo].[ReturnStatuses]
ADD CONSTRAINT [PK_ReturnStatuses]
    PRIMARY KEY CLUSTERED ([ReturnStatusId], [StatusMessage] ASC);
GO

-- Creating primary key on [UserId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [UserStatusId] in table 'UserStatuses'
ALTER TABLE [dbo].[UserStatuses]
ADD CONSTRAINT [PK_UserStatuses]
    PRIMARY KEY CLUSTERED ([UserStatusId] ASC);
GO

-- Creating primary key on [AuthTokenId] in table 'AuthTokens'
ALTER TABLE [dbo].[AuthTokens]
ADD CONSTRAINT [PK_AuthTokens]
    PRIMARY KEY CLUSTERED ([AuthTokenId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [OrganisationID] in table 'OrganisationAddresses'
ALTER TABLE [dbo].[OrganisationAddresses]
ADD CONSTRAINT [FK_Address_Organisation]
    FOREIGN KEY ([OrganisationID])
    REFERENCES [dbo].[Organisations]
        ([OrganisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [OrganisationId] in table 'OrganisationFiles'
ALTER TABLE [dbo].[OrganisationFiles]
ADD CONSTRAINT [FK_Files_Organisation]
    FOREIGN KEY ([OrganisationId])
    REFERENCES [dbo].[Organisations]
        ([OrganisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Files_Organisation'
CREATE INDEX [IX_FK_Files_Organisation]
ON [dbo].[OrganisationFiles]
    ([OrganisationId]);
GO

-- Creating foreign key on [OrganisationId] in table 'OrganisationStatuses'
ALTER TABLE [dbo].[OrganisationStatuses]
ADD CONSTRAINT [FK_OrganisationStatuses_School]
    FOREIGN KEY ([OrganisationId])
    REFERENCES [dbo].[Organisations]
        ([OrganisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganisationStatuses_School'
CREATE INDEX [IX_FK_OrganisationStatuses_School]
ON [dbo].[OrganisationStatuses]
    ([OrganisationId]);
GO

-- Creating foreign key on [ByAccountId] in table 'ReturnHits'
ALTER TABLE [dbo].[ReturnHits]
ADD CONSTRAINT [FK_ReturnHit_Account]
    FOREIGN KEY ([ByAccountId])
    REFERENCES [dbo].[Organisations]
        ([OrganisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReturnHit_Account'
CREATE INDEX [IX_FK_ReturnHit_Account]
ON [dbo].[ReturnHits]
    ([ByAccountId]);
GO

-- Creating foreign key on [OrganisationId] in table 'OrganisationGPGReturns'
ALTER TABLE [dbo].[OrganisationGPGReturns]
ADD CONSTRAINT [FK_VacancySchools_School]
    FOREIGN KEY ([OrganisationId])
    REFERENCES [dbo].[Organisations]
        ([OrganisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ReturnId] in table 'OrganisationGPGReturns'
ALTER TABLE [dbo].[OrganisationGPGReturns]
ADD CONSTRAINT [FK_VacancySchools_Vacancy]
    FOREIGN KEY ([ReturnId])
    REFERENCES [dbo].[Returns]
        ([ReturnId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_VacancySchools_Vacancy'
CREATE INDEX [IX_FK_VacancySchools_Vacancy]
ON [dbo].[OrganisationGPGReturns]
    ([ReturnId]);
GO

-- Creating foreign key on [ByUserId] in table 'OrganisationStatuses'
ALTER TABLE [dbo].[OrganisationStatuses]
ADD CONSTRAINT [FK_OrganisationStatuses_User]
    FOREIGN KEY ([ByUserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganisationStatuses_User'
CREATE INDEX [IX_FK_OrganisationStatuses_User]
ON [dbo].[OrganisationStatuses]
    ([ByUserId]);
GO

-- Creating foreign key on [ReturnId] in table 'ReturnHits'
ALTER TABLE [dbo].[ReturnHits]
ADD CONSTRAINT [FK_ReturnHit_Vacancy]
    FOREIGN KEY ([ReturnId])
    REFERENCES [dbo].[Returns]
        ([ReturnId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReturnHit_Vacancy'
CREATE INDEX [IX_FK_ReturnHit_Vacancy]
ON [dbo].[ReturnHits]
    ([ReturnId]);
GO

-- Creating foreign key on [ReturnId] in table 'ReturnStatuses'
ALTER TABLE [dbo].[ReturnStatuses]
ADD CONSTRAINT [FK_ReturnStatuses_Vacancy]
    FOREIGN KEY ([ReturnId])
    REFERENCES [dbo].[Returns]
        ([ReturnId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReturnStatuses_Vacancy'
CREATE INDEX [IX_FK_ReturnStatuses_Vacancy]
ON [dbo].[ReturnStatuses]
    ([ReturnId]);
GO

-- Creating foreign key on [ByUserId] in table 'ReturnStatuses'
ALTER TABLE [dbo].[ReturnStatuses]
ADD CONSTRAINT [FK_ReturnStatuses_User]
    FOREIGN KEY ([ByUserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReturnStatuses_User'
CREATE INDEX [IX_FK_ReturnStatuses_User]
ON [dbo].[ReturnStatuses]
    ([ByUserId]);
GO

-- Creating foreign key on [ByUserId] in table 'UserStatuses'
ALTER TABLE [dbo].[UserStatuses]
ADD CONSTRAINT [FK_UserStatuses_ByUser]
    FOREIGN KEY ([ByUserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserStatuses_ByUser'
CREATE INDEX [IX_FK_UserStatuses_ByUser]
ON [dbo].[UserStatuses]
    ([ByUserId]);
GO

-- Creating foreign key on [UserId] in table 'UserStatuses'
ALTER TABLE [dbo].[UserStatuses]
ADD CONSTRAINT [FK_UserStatuses_User]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserStatuses_User'
CREATE INDEX [IX_FK_UserStatuses_User]
ON [dbo].[UserStatuses]
    ([UserId]);
GO

-- Creating foreign key on [UserUserId] in table 'AuthTokens'
ALTER TABLE [dbo].[AuthTokens]
ADD CONSTRAINT [FK_UserId]
    FOREIGN KEY ([UserUserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserId'
CREATE INDEX [IX_FK_UserId]
ON [dbo].[AuthTokens]
    ([UserUserId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------