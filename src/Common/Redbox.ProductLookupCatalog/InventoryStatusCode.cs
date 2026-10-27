namespace Redbox.ProductLookupCatalog;

public enum InventoryStatusCode : byte
{
	Known,
	WrongTitle,
	Damaged,
	Thinned,
	PR1,
	PR2,
	Fraud,
	Rebalancing,
	Redeployment
}
