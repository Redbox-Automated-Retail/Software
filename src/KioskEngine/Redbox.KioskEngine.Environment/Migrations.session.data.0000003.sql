IF NOT EXISTS ( SELECT 1 FROM [database schema] WHERE [foreignReference] IN ( SELECT TOP 1 [objectId] FROM [database schema] WHERE [typeId] = 1 AND [name] = 'ShoppingCart' ) AND [typeId] = 3 AND [name] = 'BinNumber' )
BEGIN
	ALTER TABLE ShoppingCart ADD [BinNumber] Char(6) NULL DEFAULT NULL;
END