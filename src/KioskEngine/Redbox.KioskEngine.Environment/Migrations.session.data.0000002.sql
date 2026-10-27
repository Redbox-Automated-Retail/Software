IF NOT EXISTS ( SELECT 1 FROM [database schema] WHERE [typeid] = 1 AND [name] = 'SessionError')
	CREATE TABLE SessionError(
		ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		SessionID uniqueidentifier NOT NULL,
		Code varchar(32) NULL,
		Description varchar(1024) NULL,
		Details varchar(2048) NULL,
		IsWarning bit NOT NULL DEFAULT 0,
		CreatedOn datetime NOT NULL DEFAULT 'GETDATE()',
		CONSTRAINT FK_SessionError_Session FOREIGN KEY(SessionID) REFERENCES Session(ID) ON DELETE CASCADE);

--IF NOT EXISTS( SELECT 1 FROM [database schema] WHERE [foreignReference] IN (SELECT TOP 1 [objectId] FROM [database schema] WHERE [typeId] = 1 AND [name] = 'ShoppingCartLineItem') AND [typeId] = 3 AND [name] = 'ProductFamily')
--  ALTER TABLE ShoppingCartLineItem ADD [ProductFamily] VarChar(32) NOT NULL DEFAULT 'Movies'

--IF NOT EXISTS( SELECT 1 FROM [database schema] WHERE [foreignReference] IN (SELECT TOP 1 [objectId] FROM [database schema] WHERE [typeId] = 1 AND [name] = 'ShoppingCartLineItem') AND [typeId] = 3 AND [name] = 'ProductType')
--  ALTER TABLE ShoppingCartLineItem ADD [ProductType] VarChar(32) NOT NULL DEFAULT 'DVD'

DECLARE @productfamily as bit
set @productfamily = 0
IF NOT EXISTS( SELECT 1 FROM [database schema] WHERE [foreignReference] IN (SELECT TOP 1 [objectId] FROM [database schema] WHERE [typeId] = 1 AND [name] = 'ShoppingCartLineItem') AND [typeId] = 3 AND [name] = 'ProductFamily')
  SET @productfamily = 1

IF @productfamily = 1
BEGIN  
  ALTER TABLE ShoppingCartLineItem ADD [ProductFamily] VarChar(32) DEFAULT 'Movies';
END

IF @productfamily = 1
BEGIN  
  UPDATE ShoppingCartLineItem SET [ProductFamily] = 'Movies';
  ALTER TABLE ShoppingCartLineItem ALTER COLUMN [ProductFamily] VarChar(32) NOT NULL DEFAULT 'Movies';
END

DECLARE @producttype as bit
set @producttype = 0

IF NOT EXISTS( SELECT 1 FROM [database schema] WHERE [foreignReference] IN (SELECT TOP 1 [objectId] FROM [database schema] WHERE [typeId] = 1 AND [name] = 'ShoppingCartLineItem') AND [typeId] = 3 AND [name] = 'ProductType')
  SET @producttype = 1

IF @producttype = 1
BEGIN
  ALTER TABLE ShoppingCartLineItem ADD [ProductType] VarChar(32) DEFAULT 'DVD'
END

IF @producttype = 1
BEGIN
  UPDATE ShoppingCartLineItem SET [ProductType] = 'DVD';
  ALTER TABLE ShoppingCartLineItem ALTER COLUMN [ProductType] VarChar(32) NOT NULL DEFAULT 'DVD'
END