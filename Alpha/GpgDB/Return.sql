CREATE TABLE [dbo].[Return] (
    [ReturnId]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [DiffMeanHourlyPayPercent]    DECIMAL (18)  NOT NULL,
    [DiffMedianHourlyPercent]     DECIMAL (18)  NOT NULL,
    [DiffMeanBonusPercent]        DECIMAL (18)  NOT NULL,
    [DiffMedianBonusPercent]      DECIMAL (18)  NOT NULL,
    [MaleMedianBonusPayPercent]   DECIMAL (18)  NOT NULL,
    [FemaleMedianBonusPayPercent] DECIMAL (18)  NOT NULL,
    [MaleLowerPayBand]            DECIMAL (18)  NOT NULL,
    [FemaleLowerPayBand]          DECIMAL (18)  NOT NULL,
    [MaleMiddlePayBand]           DECIMAL (18)  NOT NULL,
    [FemaleMiddlePayBand]         DECIMAL (18)  NOT NULL,
    [MaleUpperPayBand]            DECIMAL (18)  NOT NULL,
    [FemaleUpperPayBand]          DECIMAL (18)  NOT NULL,
    [MaleUpperQuartilePayBand]    DECIMAL (18)  NOT NULL,
    [FemaleUpperQuartilePayBand]  DECIMAL (18)  NOT NULL,
    [CompanyLinkToGPGInfo]        VARCHAR (50)  NOT NULL,
    [CurrentStatus]               VARCHAR (20)  NULL,
    [CurrentStatusDate]           SMALLDATETIME NULL,
    [CurrentStatusDetails]        VARCHAR (512) NULL,
    [Created]                     SMALLDATETIME NULL,
    [Modified]                    SMALLDATETIME NULL,
    [JobTitle] VARCHAR(50) NOT NULL, 
    [FirstName] VARCHAR(50) NOT NULL, 
    [LastName] VARCHAR(50) NOT NULL, 
    PRIMARY KEY CLUSTERED ([ReturnId] ASC)
);




GO
