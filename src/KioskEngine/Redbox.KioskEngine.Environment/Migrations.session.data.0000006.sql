IF NOT EXISTS ( 
	SELECT	1 
	FROM	[database schema] 
	WHERE	[foreignReference] IN ( 
		SELECT TOP 1 [objectId] 
		FROM	[database schema] 
		WHERE	[typeId] = 1 AND [name] = 'ShoppingCartLineItem' ) 
			AND [typeId] = 3 
			AND [name] = 'SourceType' )
BEGIN
	ALTER TABLE ShoppingCartLineItem ADD [SourceType] int DEFAULT 3
END

IF EXISTS ( 
	SELECT	1 
	FROM	[database schema] 
	WHERE	[foreignReference] IN ( 
		SELECT TOP 1 [objectId] 
		FROM	[database schema] 
		WHERE	[typeId] = 1 AND [name] = 'ShoppingCartLineItem' ) 
			AND [typeId] = 3 
			AND [name] = 'SourceType' )
BEGIN
	UPDATE ShoppingCartLineItem SET [SourceType] = 3 WHERE [SourceType] is NULL
	ALTER TABLE ShoppingCartLineItem ALTER COLUMN [SourceType] int NOT NULL DEFAULT 3
END