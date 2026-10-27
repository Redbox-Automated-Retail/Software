IF EXISTS (
	SELECT 1
	FROM [database schema]
	WHERE [foreignReference] IN (
		SELECT TOP 1 [objectId]
		FROM [database schema]
		WHERE [typeId] = 1 AND [name] = 'ShoppingCart'
	) AND [typeId] = 3 AND [name] = 'BinNumber'
)
BEGIN
	ALTER TABLE ShoppingCart DROP COLUMN [BinNumber];
END