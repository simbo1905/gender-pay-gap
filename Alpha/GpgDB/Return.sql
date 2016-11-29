CREATE TABLE [dbo].[Return]
(

	[ReturnId] BIGINT NOT NULL PRIMARY KEY, 
	
    [DiffMeanHourlyPayPercent] DECIMAL NOT NULL, 
    [DiffMedianHourlyPercent] DECIMAL NOT NULL, 
    [DiffMeanBonusPercent] DECIMAL NOT NULL, 
    [DiffMedianBonusPercent] DECIMAL NOT NULL, 

    [MaleMedianBonusPayPercent] DECIMAL NOT NULL, 
    [FemaleMedianBonusPayPercent] DECIMAL NOT NULL, 
    [MaleLowerPayBand] DECIMAL NOT NULL, 
    [FemaleLowerPayBand] DECIMAL NOT NULL, 
    [MaleMiddlePayBand] DECIMAL NOT NULL, 
    [FemaleMiddlePayBand] DECIMAL NOT NULL, 
    [MaleUpperPayBand] DECIMAL NOT NULL, 
    [FemaleUpperPayBand] DECIMAL NOT NULL, 
    [MaleUpperQuartilePayBand] DECIMAL NOT NULL, 
    [FemaleUpperQuartilePayBand] DECIMAL NOT NULL, 
    [CompanyLinkToGPGInfo] VARCHAR(50) NOT NULL,

    [CurrentStatus] VARCHAR(20) NULL, 
    [CurrentStatusDate] SMALLDATETIME NULL, 
    [CurrentStatusDetails] VARCHAR(512) NULL,
    [Created] SMALLDATETIME NULL, 
    [Modified] SMALLDATETIME NULL, 
    --CONSTRAINT [FK_Return_Group] FOREIGN KEY ([GroupId]) REFERENCES [GroupId]([GroupIdId])
)

