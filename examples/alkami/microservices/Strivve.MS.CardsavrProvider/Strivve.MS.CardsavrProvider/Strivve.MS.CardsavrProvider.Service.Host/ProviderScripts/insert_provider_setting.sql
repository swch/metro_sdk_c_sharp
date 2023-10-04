WHILE @@TRANCOUNT > 0 ROLLBACK
SET XACT_ABORT ON
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
 
USE DeveloperDynamic
  
BEGIN TRAN

declare @providerId table (id bigint); declare @itemIdtable table (id bigint);declare @itemSettings TABLE (ItemID int, Name nvarchar(max), Value nvarchar(max));DECLARE @sUser NVARCHAR(500) = SUSER_NAME(); DECLARE @itemSettingOutput TABLE(Action NVARCHAR(max), NewItemSettingId BIGINT, NewItemId NVARCHAR(max), NewName NVARCHAR(max), NewValue NVARCHAR(MAX), NewCreatedUtc DATETIME, NewVersion NVARCHAR(max), NewLastUpdate DateTime, OldItemSettingId BIGINT, OldItemId NVARCHAR(max), OldName NVARCHAR(max), OldValue NVARCHAR(MAX), OldCreatedUtc DATETIME, OldVersion NVARCHAR(max), OldLastUpdate DateTime);

/****************************************************
	START EDIT
****************************************************/
declare @providerTypeName nvarchar(30) = 'Strivve'
declare @providerTypeDisplayName nvarchar(max) = 'Strivve'
declare @providerTypeDescription nvarchar(max) = 'Strivve'

declare @providerName nvarchar(50) = 'Strivve.MS.CardsavrProvider'
declare @providerDescription nvarchar(100) = 'Strivve CardsavrProvider'
declare @providerAssemblyInfo nvarchar(max) = ''
/* Identify the Ticket Number Here for audit reasons */
DECLARE @ticket nvarchar(20) = 'SDKCustom'
/****************************************************
	STOP EDIT                                                 
****************************************************/
if not exists (select * from core.ProviderType where name = @providerTypeName) begin insert into core.ProviderType (Name, DisplayName, Description, CreateDate, ViewPage)  select @providerTypeName, @providerTypeDisplayName, @providerTypeDescription, getutcdate(), null; end; if not exists (select * from core.provider p join core.providertype pt on pt.id = p.ProviderTypeID where pt.name = @providerTypeName and p.Name = @providerName) begin insert into core.provider (ProviderTypeID, Name, Description, AssemblyInfo, CreateDate) output inserted.id into @providerId select pt.id, @providerName, @providerDescription, @providerAssemblyInfo, GETUTCDATE() from core.ProviderType pt where name = @providerTypeName; end else begin insert into @providerId (id) select p.id from core.provider p join core.providertype pt on pt.id = p.ProviderTypeID where pt.name = @providerTypeName and p.Name = @providerName; end /*select * from @providerId -- will always only be one row from the above*/; if not exists (select * from core.item i join core.bank b on b.id = i.SecondaryId and i.ItemType = 'Connector' join @providerId t on t.id = i.ParentId) begin insert into core.item (ItemType, ParentId, SecondaryId, Name, CreatedUtc, Version, Deleted) output inserted.id into @itemIdtable select 'Connector', t.id, (select id from core.bank), @providerName, GETUTCDATE(), '1.0.0.0', 0  from @providerId t; end else begin insert into @itemIdtable select i.id from core.item i join core.bank b on b.id = i.SecondaryId and i.ItemType = 'Connector' join @providerId t on t.id = i.ParentId; end;DECLARE @itemID INT = (select top 1 ID from @itemIdtable);


--ROLLBACK TRAN
COMMIT TRAN
  
select * from core.item where [Name] = 'Strivve.MS.CardsavrProvider';
SET TRANSACTION ISOLATION LEVEL READ COMMITTED
WHILE @@TRANCOUNT > 0 ROLLBACK
	