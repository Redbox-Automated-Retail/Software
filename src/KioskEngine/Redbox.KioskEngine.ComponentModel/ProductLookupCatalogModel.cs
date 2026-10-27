using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public class ProductLookupCatalogModel
{
	public string OriginMachine { get; set; }

	public string GeneratedOn { get; set; }

	public Dictionary<string, Inventory> Barcodes { get; set; }
}
