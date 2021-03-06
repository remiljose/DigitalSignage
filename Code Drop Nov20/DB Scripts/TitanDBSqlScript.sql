USE [master]
GO
/****** Object:  Database [CCTitanDB]    Script Date: 20-Nov-18 10:22:34 PM ******/
CREATE DATABASE [CCTitanDB]
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CCTitanDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CCTitanDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CCTitanDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CCTitanDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CCTitanDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CCTitanDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [CCTitanDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CCTitanDB] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [CCTitanDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CCTitanDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CCTitanDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CCTitanDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CCTitanDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CCTitanDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CCTitanDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CCTitanDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CCTitanDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CCTitanDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CCTitanDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CCTitanDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CCTitanDB] SET ALLOW_SNAPSHOT_ISOLATION ON 
GO
ALTER DATABASE [CCTitanDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CCTitanDB] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [CCTitanDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CCTitanDB] SET RECOVERY FULL 
GO
ALTER DATABASE [CCTitanDB] SET  MULTI_USER 
GO
ALTER DATABASE [CCTitanDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CCTitanDB] SET DB_CHAINING OFF 
GO
USE [CCTitanDB]
GO
/****** Object:  StoredProcedure [dbo].[SP_AutoAlertProcessor]    Script Date: 20-Nov-18 10:22:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_AutoAlertProcessor]

AS
BEGIN  
  
   DECLARE @AlertMinTime datetime;
   DECLARE @AlertMaxTime datetime;
   DECLARE @Interval int;
   DECLARE @CurntInterval int;
   DECLARE @Count int;
   DECLARE @GatewayID varchar(100);
   DECLARE @BeaconID varchar(100);
   DECLARE @ShipmentID varchar(100);
   DECLARE @ShipmasterID int;
   DECLARE @BeaconTmpProcess Table
       (
	         [BeaconID] varchar(100)
			,[GatewayID] varchar(100)
			,[Processed] tinyint  
				
		   );
   INSERT INTO @BeaconTmpProcess  SELECT DISTINCT [BeaconID],[GatewayID],0  from [Raw_Alert] WHERE [Status] = 'Raw'
   --select * from @BeaconTmpProcess
   -----Process
   WHILE (SELECT Count(*) From @BeaconTmpProcess Where  Processed = 0 ) > 0
    BEGIN  -----Loop Starts
	
	   SELECT TOP 1 @BeaconID = BeaconID ,@GatewayID = GatewayID  FROM @BeaconTmpProcess Where   Processed = 0 
	   SELECT TOP 1 @AlertMinTime=[CURRENTSYSTEMTIME] FROM [dbo].[Raw_Alert] where [BeaconID]=@BeaconID and [Status] = 'Raw' order by [CURRENTSYSTEMTIME] ASC
	   --select @AlertMinTime
	   SELECT TOP 1 @AlertMaxTime=[CURRENTSYSTEMTIME] FROM [dbo].[Raw_Alert] where [BeaconID]=@BeaconID and [Status] = 'Raw' order by [CURRENTSYSTEMTIME] DESC
	   ---select @AlertMaxTime 
	    Select distinct @ShipmasterID=ShipmasterID,@ShipmentID=ShipmentID  from [dbo].[VW_Gateway_Pallet_ShipmentAssociation] where MacId=@GatewayID

	    SELECT @Interval= DATEDIFF(minute,  @AlertMinTime,    @AlertMaxTime);
		SELECT @CurntInterval= DATEDIFF(minute,  @AlertMaxTime,   CURRENT_TIMESTAMP);

		-- SELECT @Interval,@CurntInterval
	   IF (@Interval >10 and @CurntInterval <10)
		 Begin
		 --select 'In Alert'
			if ((select count(BeaconID) from [dbo].[Alert_Process] where [BeaconID]=@BeaconID and [Status]='Active')=0)
				Begin
					--select 'Alert-if not exists as status active then insert'
					INSERT INTO [dbo].[Alert_Process]([ShipmentID],[GatewayID] ,[BeaconID],[Temperature],[Humidity],[AlertType],[LocationLattitude],[LocationLongitude]
				   ,[Acknowledge] ,[Ack_Notes],[User],[Status],[TimeStamp],[ShipmasterID],[AlertStartTime])
					Select Top 1 @ShipmentID,[GatewayID],[BeaconID],[Temperature],[Humidity],'Alert',[LocationLattitude],[LocationLongitude],'','','Job','Active',CURRENT_TIMESTAMP,@ShipmasterID,@AlertMinTime 
					FROM [dbo].[Raw_Alert] where [BeaconID]=@BeaconID and [Status] = 'Raw' order by [CURRENTSYSTEMTIME] ASC
				end --End Count Condition
		 End --End Interval Condition
	   Else
		Begin
			 ---select 'In non-Alert'
			if ((select count(BeaconID) from [dbo].[Alert_Process] where [BeaconID]=@BeaconID and [Status]='Active')=1)
				Begin
					---select 'NonAlert -if exists as status not closed then update as closed'
					Update [Alert_Process] set [AlertEndTime]=@AlertMaxTime, [Status]='Closed' where [BeaconID]=@BeaconID and [GatewayID]=@GatewayID and [Status]='Active'
					Update [Raw_Alert] set [Status] = 'Processed'  where [BeaconID]=@BeaconID and [GatewayID]=@GatewayID  and [Status] = 'Raw'
				End --End Count Condition
		End  --End Else Interval Condition

	 UPDATE @BeaconTmpProcess Set Processed = 1 Where  BeaconID=@BeaconID    --Set Processed = 1
   END -----Loop Ends

END --- SP ENd
GO
/****** Object:  StoredProcedure [dbo].[Sp_GetAssociation]    Script Date: 20-Nov-18 10:22:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE  [dbo].[Sp_GetAssociation]

@ShipmentId varchar(100)
 AS
 BEGIN -- Begin Procedure


DECLARE @ObjectTreeTmp Table
       (
	         [parent] int
			,[child] int
			,[level] int
			,[AssociatedType] Varchar(100)		
		  );
 DECLARE @parent int = 0;
SELECT DISTINCT  @parent=pallet_id FROM VW_Gateway_Pallet_ShipmentAssociation WHERE [shipmentId]= @ShipmentId;
--select @parent;
WITH cte AS
(
  select null parent, @parent child, 0 as level,'Pallet' as AssociatedType
   union
  SELECT  a.[Object_Beac_Id], a.[Associated_Object_Id] , 1 as level,a.[AssociatedType]
    FROM Shipping_Association a
   WHERE a.[Object_Beac_Id] = @parent
   UNION ALL
  SELECT a.[Object_Beac_Id], a.[Associated_Object_Id] , c.level +    1,a.[AssociatedType]
    FROM Shipping_Association a JOIN cte c ON a.[Object_Beac_Id] = c.child
)
INSERT into @ObjectTreeTmp SELECT distinct parent, child , level,AssociatedType
  FROM cte
 ORDER by level, parent;

 SELECT  tmp.child as Id,bec.[ObjectId],tmp.AssociatedType as [Type] ,tmp.parent, tmp.[level] FROM @ObjectTreeTmp tmp
 INNER JOIN Beacon_Object_Info bec ON tmp.child=bec.[Beacon_Obj_Id]  ORDER by tmp.level, tmp.parent;

END --End Procedure
GO
/****** Object:  StoredProcedure [dbo].[Sp_GetBeacons]    Script Date: 20-Nov-18 10:22:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Sp_GetBeacons]
 
 @MacId varchar(100)

 AS
 BEGIN

 DECLARE @parent int = 0;
 --DECLARE @MacId varchar(100)='2345673'
 DECLARE @ObjectTreeTmp Table
       (
	         [parent] int
			,[child] int
			,[level] int
			,[AssociatedType] Varchar(100)		
		   );

SELECT DISTINCT  @parent=pallet_id FROM VW_Gateway_Pallet_ShipmentAssociation WHERE [MacId]=@MacId;
--select @parent;
WITH cte AS
(
  select null parent, @parent child, 0 as level,'Pallet' as AssociatedType
   union
  SELECT  a.[Object_Beac_Id], a.[Associated_Object_Id] , 1 as level,a.[AssociatedType]
    FROM Shipping_Association a
   WHERE a.[Object_Beac_Id] = @parent
   UNION ALL
  SELECT a.[Object_Beac_Id], a.[Associated_Object_Id] , c.level +    1,a.[AssociatedType]
    FROM Shipping_Association a JOIN cte c ON a.[Object_Beac_Id] = c.child
)
INSERT into @ObjectTreeTmp SELECT distinct parent, child , level,AssociatedType
  FROM cte
 ORDER by level, parent;

 SELECT [BeaconId],[TemperatureUpperLimit],[TemperatureLowerLimit],[HumidityUpperLimit],[HumidityLowerLimit] 
 FROM Beacon_Object_Info WHERE [Beacon_Obj_Id] in (select child from @ObjectTreeTmp)
 
 END

GO
/****** Object:  StoredProcedure [dbo].[Sp_GetBeacons_Json]    Script Date: 20-Nov-18 10:22:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Sp_GetBeacons_Json](
 
 @MacId varchar(100),
 @jsonOutput NVARCHAR(MAX) OUTPUT
 )
 AS
 BEGIN

 DECLARE @parent int = 0;
 --DECLARE @MacId varchar(100)='2345673'
 DECLARE @ObjectTreeTmp Table
       (
	         [parent] int
			,[child] int
			,[level] int
			,[AssociatedType] Varchar(100)		
		   );

SELECT DISTINCT  @parent=pallet_id FROM VW_Gateway_Pallet_ShipmentAssociation WHERE [MacId]=@MacId;
--select @parent;
WITH cte AS
(
  select null parent, @parent child, 0 as level,'Pallet' as AssociatedType
   union
  SELECT  a.[Object_Beac_Id], a.[Associated_Object_Id] , 1 as level,a.[AssociatedType]
    FROM Shipping_Association a
   WHERE a.[Object_Beac_Id] = @parent
   UNION ALL
  SELECT a.[Object_Beac_Id], a.[Associated_Object_Id] , c.level +    1,a.[AssociatedType]
    FROM Shipping_Association a JOIN cte c ON a.[Object_Beac_Id] = c.child
)
INSERT into @ObjectTreeTmp SELECT distinct parent, child , level,AssociatedType
  FROM cte
 ORDER by level, parent;

 SET @jsonOutput = (SELECT [BeaconId],[ObjectId],[ObjectType],[TemperatureUpperLimit],[TemperatureLowerLimit],[HumidityUpperLimit],[HumidityLowerLimit] 
 FROM Beacon_Object_Info WHERE [Beacon_Obj_Id] in (select child from @ObjectTreeTmp)
 --FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
 FOR JSON AUTO, Root('Beacons'))

 
 END
GO
/****** Object:  Table [dbo].[Alert_Process]    Script Date: 20-Nov-18 10:22:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Alert_Process](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ShipmentID] [varchar](100) NULL,
	[GatewayID] [varchar](100) NOT NULL,
	[BeaconID] [varchar](100) NULL,
	[Temperature] [float] NULL,
	[Humidity] [float] NULL,
	[AlertType] [varchar](100) NULL,
	[LocationLattitude] [float] NULL,
	[LocationLongitude] [float] NULL,
	[Acknowledge] [bit] NULL,
	[Ack_Notes] [varchar](100) NULL,
	[User] [varchar](100) NULL,
	[Status] [varchar](100) NULL,
	[TimeStamp] [time](7) NULL,
	[ShipmasterID] [int] NULL,
	[AlertStartTime] [datetime] NULL,
	[AlertEndTime] [datetime] NULL,
	[TemperatureAlert] [bit] NULL,
	[ShockAlert] [bit] NULL,
	[HumidityAlert] [bit] NULL,
	[TamperAlert] [bit] NULL,
	[BlockChainStatus] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Beacon_Object_Info]    Script Date: 20-Nov-18 10:22:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Beacon_Object_Info](
	[Beacon_Obj_Id] [int] IDENTITY(1,1) NOT NULL,
	[BeaconId] [nvarchar](100) NOT NULL,
	[ObjectId] [nvarchar](100) NOT NULL,
	[ObjectType] [nvarchar](100) NOT NULL,
	[TemperatureUpperLimit] [float] NULL,
	[TemperatureLowerLimit] [float] NULL,
	[HumidityUpperLimit] [float] NULL,
	[HumidityLowerLimit] [float] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[CreatedDateTime] [datetime] NULL,
	[UpdatedBy] [nvarchar](100) NULL,
	[UpdatedDateTime] [datetime] NULL,
	[CONTENT] [nvarchar](100) NULL,
	[ShipMasterId] [int] NULL,
	[TemperatureAlertThreshold] [int] NULL,
	[HumidityAlertThreshold] [int] NULL,
 CONSTRAINT [PK_Object] PRIMARY KEY CLUSTERED 
(
	[Beacon_Obj_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[DeviceInfo]    Script Date: 20-Nov-18 10:22:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeviceInfo](
	[DeviceId] [int] IDENTITY(1,1) NOT NULL,
	[MacId] [nvarchar](100) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL,
	[Status] [nvarchar](250) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[CreatedDateTime] [datetime] NULL,
	[UpdatedBy] [nvarchar](100) NULL,
	[UpdatedDateTime] [datetime] NULL,
 CONSTRAINT [PK_Device] PRIMARY KEY CLUSTERED 
(
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Gateway_Beacon_List]    Script Date: 20-Nov-18 10:22:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Gateway_Beacon_List](
	[Slno-ID] [int] IDENTITY(1,1) NOT NULL,
	[BeaconId] [nvarchar](100) NOT NULL,
	[ObjectId] [nvarchar](100) NOT NULL,
	[ObjectType] [nvarchar](100) NOT NULL,
	[TemperatureUpperLimit] [float] NULL,
	[TemperatureLowerLimit] [float] NULL,
	[HumidityUpperLimit] [float] NULL,
	[HumidityLowerLimit] [float] NULL,
	[GatewayMAcID] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Slno-ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GPS_Data]    Script Date: 20-Nov-18 10:22:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GPS_Data](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TrackerId] [nvarchar](100) NOT NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_GPS_Data] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Pallet_Gateway_Association]    Script Date: 20-Nov-18 10:22:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pallet_Gateway_Association](
	[Pallet_Gateway_Id] [int] IDENTITY(1,1) NOT NULL,
	[ShipMasterId] [int] NULL,
	[Pallet_Id] [int] NULL,
	[Gateway_Mac_Id] [int] NULL,
	[Tracker_Id] [int] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[CreatedTime] [datetime] NULL,
	[UpdatedBy] [nvarchar](100) NULL,
	[UpdatedTime] [datetime] NULL,
 CONSTRAINT [PK_Pallet_Gateway_Association] PRIMARY KEY CLUSTERED 
(
	[Pallet_Gateway_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Raw_Alert]    Script Date: 20-Nov-18 10:22:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Raw_Alert](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GatewayID] [varchar](100) NOT NULL,
	[BeaconID] [varchar](100) NULL,
	[Temperature] [float] NULL,
	[Humidity] [float] NULL,
	[ShockAlert] [bit] NULL,
	[TamperAlert] [bit] NULL,
	[TemperatureAlert] [bit] NULL,
	[HumidityAlert] [bit] NULL,
	[LocationLattitude] [float] NULL,
	[LocationLongitude] [float] NULL,
	[Status] [varchar](100) NULL,
	[AlertDatetime] [datetime] NULL,
	[CURRENTSYSTEMTIME] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Shipment_Product]    Script Date: 20-Nov-18 10:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Shipment_Product](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ShipMasterID] [int] NULL,
	[ProductID] [varchar](100) NULL,
	[Product] [varchar](100) NULL,
	[Quantity] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Shipping_Association]    Script Date: 20-Nov-18 10:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Shipping_Association](
	[ShipBoxAssociateID] [int] IDENTITY(1,1) NOT NULL,
	[ObjectType] [varchar](100) NULL,
	[AssociatedType] [varchar](100) NULL,
	[CreatedBy] [varchar](100) NOT NULL,
	[CreatedTime] [datetime] NULL,
	[UpdatedBy] [varchar](100) NULL,
	[UpdatedTime] [datetime] NULL,
	[ShippingId] [nvarchar](50) NULL,
	[ShipMasterID] [int] NULL,
	[Object_Beac_Id] [int] NULL,
	[Associated_Object_Id] [int] NULL,
	[Status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ShipBoxAssociateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Shipping_Master]    Script Date: 20-Nov-18 10:22:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Shipping_Master](
	[ShipmasterID] [int] IDENTITY(1,1) NOT NULL,
	[ShipmentID] [varchar](100) NOT NULL,
	[TemperatureBreach] [int] NULL,
	[TamperBreach] [int] NULL,
	[HumidityBreach] [int] NULL,
	[VibrationBreach] [int] NULL,
	[ShipmentStatus] [varchar](100) NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[CreatedTime] [datetime] NULL,
	[UpdatedBy] [varchar](20) NULL,
	[UpdatedTime] [datetime] NULL,
	[SourceLoc] [varchar](100) NULL,
	[DestinationLoc] [varchar](100) NULL,
	[LogisticPartner] [varchar](100) NULL,
	[DateofShipment] [date] NULL,
	[DeliveryDate] [date] NULL,
	[InvoiceDocRef] [varchar](100) NULL,
	[PONumber] [varchar](100) NULL,
	[BlockchainStatus] [varchar](100) NULL,
	[TransactionHash] [varchar](250) NULL,
	[GatewayCount] [int] NULL,
	[PalletCount] [int] NULL,
	[CartonCount] [int] NULL,
	[BoxCount] [int] NULL,
	[ProductCount] [int] NULL,
	[IsActive] [bit] NULL,
	[BeaconCount] [int] NULL,
	[CurrrentLat] [float] NULL,
	[CurrrentLong] [float] NULL,
	[UnreachableDevice] [int] NULL,
 CONSTRAINT [PK_Shipping_Master] PRIMARY KEY CLUSTERED 
(
	[ShipmasterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TestAlert]    Script Date: 20-Nov-18 10:22:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestAlert](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[MacId] [nvarchar](100) NULL,
	[temperature] [numeric](18, 7) NULL,
	[sensorId] [nvarchar](50) NULL,
	[Alert] [nvarchar](50) NULL,
 CONSTRAINT [PK_TestAlert] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[UserRole_Mapping]    Script Date: 20-Nov-18 10:22:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserRole_Mapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](100) NOT NULL,
	[Email] [varchar](100) NULL,
	[_Role] [varchar](100) NULL,
	[_Status] [varchar](100) NULL,
	[CreatedBy] [varchar](100) NOT NULL,
	[CreatedTime] [datetime] NULL,
	[UpdatedBy] [varchar](100) NULL,
	[UpdatedTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[VW_Gateway_Pallet_ShipmentAssociation]    Script Date: 20-Nov-18 10:22:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[VW_Gateway_Pallet_ShipmentAssociation] 
AS
SELECT PG.ShipmasterID,SHM.ShipmentID,PG.Pallet_id,BEC.[ObjectId],BEC.[ObjectType],PG.Gateway_Mac_Id,D.[MacId],D.[Type],
PG.Tracker_Id,SHA.[Associated_Object_Id],SHA.[AssociatedType]
FROM Pallet_Gateway_Association PG
INNER JOIN [Shipping_Master] SHM
ON PG.ShipmasterID=SHM.ShipmasterID
INNER JOIN [DeviceInfo] D
ON PG.Gateway_Mac_Id=D.DeviceId
INNER JOIN [Beacon_Object_Info] BEC
ON PG.Pallet_id=BEC.[Beacon_Obj_Id]
INNER JOIN [Shipping_Association] SHA
ON PG.Pallet_id=SHA.[Object_Beac_Id]
WHERE SHM.[IsActive]=1
AND PG.ShipmasterID =SHA.ShipmasterID

GO
ALTER TABLE [dbo].[Alert_Process]  WITH CHECK ADD FOREIGN KEY([ShipmasterID])
REFERENCES [dbo].[Shipping_Master] ([ShipmasterID])
GO
USE [master]
GO
ALTER DATABASE [CCTitanDB] SET  READ_WRITE 
GO
