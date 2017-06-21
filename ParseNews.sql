USE [IRBackendDB]

IF NOT EXISTS (SELECT TOP 1 * FROM sys.database_principals WHERE name = 'SqlPublic')
BEGIN
	CREATE USER [SqlPublic] FOR LOGIN [SqlPublic]
	ALTER ROLE [db_datareader] ADD MEMBER [SqlPublic]
END
GO


USE [EuroInvestorDB]
GO

IF EXISTS (SELECT * FROM sys.objects
	WHERE object_id = OBJECT_ID(N'ParseXMLNews') AND type IN (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[ParseXMLNews]
END
GO

/****** Object:  StoredProcedure [dbo].[ParseXMLNews]    Script Date: 3/31/2017 10:21:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ParseXMLNews]
	@XML XML,
	@ResultCode int OUTPUT,
	@ErrorMessage nvarchar(max) OUTPUT
	WITH RECOMPILE
AS
BEGIN

	SET NOCOUNT ON;
	DECLARE @stime datetime;
	SET @stime = GETDATE();

	DECLARE @CodeSuccess int, @CodeDuplicate int, @CodeSkip int, @CodeInstrumentNotFound int, @CodeFail int;
	SELECT @CodeSuccess = 0, @CodeDuplicate = 1, @CodeSkip = 2, @CodeInstrumentNotFound = 3, @CodeFail = 4;
	SET @ErrorMessage = NULL;
	
	-- Three input points, the news itself (@TInput), the participating companies (@Instruments), and the metaData (@MetaData)

	DECLARE @NewsId INT

	DECLARE @TInput TABLE
	(
		Timestamp datetime,
		Headline nvarchar(255),
		Filename varchar(255),
		Text nvarchar(max),
		Language varchar(2),
		TimestampAllowedPublishTime datetime, 
		exchangeId integer, InstrumentID int,
		Symbol varchar(20),
		ProviderCode int,
		ProviderName nvarchar(255),
		SourceID int -- This is our identifier for source 
	);

	DECLARE @Instruments TABLE
	(
		Symbol varchar(20),
		exchangeMIC varchar(4),
		exchangeId integer, 
		InstrumentID int,
		CompanyName varchar(100) COLLATE Latin1_General_CI_AS -- This is needed to avoid a collation error
	);

	DECLARE @MetaData TABLE
	(
		type varchar(20),
		extDataType int,
		value nVarChar(max)
	);

	DECLARE @TAttachments Table
	(
		Url varchar(255),
		Filename varchar(255)
	);
	

	INSERT INTO
		@TAttachments(Url, Filename)
		SELECT 
			Stock.value('Url[1]','varchar(255)') AS Url,
			Stock.value('Filename[1]','varchar(255)') AS Filename
			FROM @XML.nodes('News/Attachments/Attachment')Catalog(Stock)	

	INSERT INTO 
		@Instruments(Symbol, exchangeMIC, CompanyName)
		Select 
			Stock.value('Symbol[1]','varchar(20)') AS Symbol,
  			Stock.value('Exchange[1]','varchar(4)') AS exchangeMIC,
			Stock.value('Name[1]','varchar(100)') AS CompanyName
			FROM @XML.nodes('News/CompanyCodes/CompanyCode')Catalog(Stock) 

	INSERT INTO 
		@TInput(Headline,Text,Language,Filename,Timestamp,TimestampAllowedPublishTime,ProviderCode,Symbol)
		SELECT 
			Stock.value('Headline[1]','nvarchar(255)') AS Headline,
  			Stock.value('Body[1]','nvarchar(max)') AS Text,
			Stock.value('Language[1]','varchar(2)') AS Language,
			Stock.value('Filename[1]','varchar(255)') AS Filename,
  			Stock.value('PublicationDate[1]','datetime') AS Timestamp,
			@stime AS TimestampAllowedPublishTime,
			Stock.value('ProviderCode[1]','int') AS ProviderCode,	--MIC code of the news service 
			Stock.value('Symbol[1]','varchar(20)') AS Symbol
			FROM @XML.nodes('News')Catalog(Stock)

	INSERT INTO  
		@MetaData(type, value)
		Select 
			Stock.value('Type[1]','varchar(20)') AS type,
  			Stock.value('Value[1]','nvarchar(max)') AS value
			FROM @XML.nodes('News/MetaData/Meta')Catalog(Stock) 

	PRINT '***'
	PRINT ' XML read-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60))


	-- Convert MIC to Exchange 
	UPDATE @Instruments 
		SET ExchangeId = (SELECT [ExchangeID] 
			FROM [EuroInvestorStockDB].[dbo].[MicExchange] WITH(NOLOCK) 
			WHERE [Mic]= SI.exchangeMIC)
		FROM @Instruments AS SI
	PRINT ' Reading the symbol-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60)) 

	--take care of E: and E1: case 
	UPDATE @Instruments 
		SET Symbol = (
				IIF (
						-- E: case
						EXISTS (SELECT TOP 1 [Symbol] FROM [EuroInvestorStockDB].[dbo].Instrument WHERE Symbol = CONCAT('E:',SI.Symbol) AND ExchangeId = SI.ExchangeId ),
						CONCAT('E:',SI.Symbol),
						
						-- E1: case
						IIF(
								EXISTS(SELECT TOP 1 [Symbol] FROM [EuroInvestorStockDB].[dbo].Instrument WHERE Symbol = CONCAT('E1:',SI.Symbol) AND ExchangeId = SI.ExchangeId), 
								CONCAT('E1:',SI.Symbol), 
								SI.Symbol
							)
					)
		)  
		FROM @Instruments AS SI 
		
		
	-- Maybe these two can be merged? Getting the instrumentID for each stock
	UPDATE @Instruments 
		SET InstrumentID = (SELECT TOP 1 ID FROM [EuroInvestorStockDB].[dbo].Instrument WHERE Symbol = SI.Symbol AND ExchangeId = SI.ExchangeId)
		FROM @Instruments AS SI 

	-- Set Instrument and Symbol for IRSH news
	UPDATE @Instruments 
		SET InstrumentID = (SELECT TOP 1 InstrumentID 
				FROM [IRBackendDB].[dbo].NewsWhiteListByCompanyName WITH(NOLOCK)
				WHERE CompanyName = SI.CompanyName)
		FROM @Instruments AS SI 
		WHERE exchangeMIC = 'XLON' AND Symbol = 'IRSH'
	UPDATE @Instruments 
		SET Symbol = (SELECT TOP 1 Symbol 
				FROM [EuroInvestorStockDB].[dbo].Instrument WITH(NOLOCK)
				WHERE ID = SI.InstrumentID)
		FROM @Instruments AS SI 
		WHERE exchangeMIC = 'XLON' AND Symbol = 'IRSH'

	-- If all instruments have no instrument ID then we skip inserting the news to the database
	IF NOT EXISTS (SELECT TOP 1 * FROM @Instruments WHERE InstrumentID IS NOT NULL)
		BEGIN
			PRINT 'No instruments found' 
			SET @ResultCode = @CodeInstrumentNotFound;
			RETURN;
		END;

	-- Find the sourceID, need to build up the table NewsWireMapping
	UPDATE @TInput 
		SET SourceID = (SELECT TOP 1 EI FROM NewsWireMapping WHERE Q4 = SI.providercode)
		FROM @TInput AS SI 
		
	-- If Source is not found, we skip inserting the news to the database
	IF EXISTS (SELECT TOP 1 * FROM @TInput WHERE SourceID IS NULL)
		BEGIN
			PRINT 'Skip news' 
			SET @ResultCode = @CodeSkip;
			RETURN;
		END;

	-- Insert into NewsStories

	BEGIN TRY
	BEGIN TRANSACTION;  


		INSERT INTO 
			NewsStories (StoryIdExt,Headline,text,Language,timestamp,isHtml,TimestampAllowedPublishTime,SourceID)
			SELECT 
				IP.Filename AS StoryIdExt,
				IP.Headline AS Headline,
				IP.Text AS Text,
				IP.language AS language,
				IP.timestamp AS timestamp, 
				1 AS IsHtml,
				-- We don't get Oslo NewsWeb attachments from our news provider. A seperate service is made (OsloNewsWebAttachmentDownloader) that takes all news that are newer than GETDATE() and append any missing attachments from newsweb.no directly.
				IIF(IP.SourceID = 1016, DATEADD(MINUTE, 10, IP.TimestampAllowedPublishTime), IP.TimestampAllowedPublishTime) AS TimestampAllowedPublishtime, --IP.TimestampAllowedPublishTime AS TimestampAllowedPublishtime,
				Ip.SourceID AS SourceID
				FROM @TInput AS IP
				-- There exists an index on NewsStories based on StoryIdExt and Language combined 
				WHERE (NOT EXISTS (SELECT top 1 * FROM NewsStories WITH(NOLOCK) WHERE StoryIdExt = IP.Filename AND language = IP.language))
		
			SET @NewsId = SCOPE_IDENTITY();

			IF (@@ROWCOUNT = 0) PRINT 'Did not update NewsStories' 
			ELSE PRINT 'insertion to NewsStories-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60 )) 

		IF (@NewsId IS NULL)
		BEGIN
			PRINT 'Duplicate news' 
			SET @ResultCode = @CodeDuplicate;
			COMMIT;
			RETURN;
		END;

		INSERT INTO
			NewsStoriesStockRef(StoryID,StockID,StockRefSymbol)
			SELECT 
				@NewsId AS StoryID,
				IP.InstrumentID AS StockID,
				Ip.symbol AS StockRefSymbol
				FROM @Instruments AS IP
				WHERE IP.InstrumentID IS NOT NULL;

			IF (@@ROWCOUNT = 0) PRINT 'Did not update NewsStoriesStockRef' 
			ELSE PRINT 'insertion to NewsStoriesStockRef-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60 )) 


		UPDATE @MetaData 
			SET ExtDataType = ( SELECT TOP 1 ExtDataType FROM NewsStoriesExtDataQ4 WHERE Q4Data = SI.type)
			FROM @MetaData AS SI 


		INSERT INTO
			NewsStoriesExtData(StoryID,extDataType,Data)
			SELECT 
				@NewsId AS StoryID,
				IP.extDataType AS extDataType,
				IP.value AS Data
				FROM @MetaData AS IP
				WHERE IP.extDataType IS NOT NULL

			IF (@@ROWCOUNT = 0) PRINT 'Did not update NewsStoriesExtData' 
			ELSE PRINT 'insertion to NewsStoriesExtData-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60 )) 


		INSERT INTO
			NewsStoriesAttachments(StoryID, AttachmentURL, Filename)
			SELECT
				@NewsId AS StoryID,
				IP.Url AS AttachmentURL,
				IP.Filename AS Filename
				FROM @TAttachments AS IP WHERE IP.Url IS NOT NULL 

			IF (@@ROWCOUNT = 0) PRINT 'Did not update Attachments' 
			ELSE PRINT 'time to finish insertion to Attachments-->' + CONVERT(varchar(50),(CAST (GETDATE()-@stime AS float)*24*60*60 )) 
			COMMIT;
			PRINT 'successfully entered NewsID ' + CONVERT(varchar(50),@NewsId)

	END TRY


	BEGIN CATCH

		ROLLBACK
		PRINT 'Transaction Canceled. Reasons:'
		PRINT ERROR_MESSAGE()
		SET @ResultCode = @CodeFail;
		SET @ErrorMessage = 
			'Number: '		+ CONVERT(nvarchar(10),ERROR_NUMBER()) + 
			', Severity: '	+ CONVERT(nvarchar(10),ERROR_SEVERITY()) + 
			', State: '		+ CONVERT(nvarchar(10),ERROR_STATE()) + 
			', Procedure: '	+ ERROR_PROCEDURE() + 
			', Line: '		+ CONVERT(nvarchar(10),ERROR_LINE()) + ',' + CHAR(13) + 
			'Message: '		+ ERROR_MESSAGE();
		RETURN;

	END CATCH

	SET @ResultCode = @CodeSuccess;

END;
