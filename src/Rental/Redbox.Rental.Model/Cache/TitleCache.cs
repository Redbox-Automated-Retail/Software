using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Cache;

public class TitleCache
{
	private Dictionary<string, BarcodeProduct> _barcodeProducts = new Dictionary<string, BarcodeProduct>();

	public long ProductId { get; set; }

	public bool IsComingSoon { get; set; }

	public bool IsAvailable { get; set; }

	public string ProductFamilyName { get; set; }

	public bool InStock { get; set; }

	public string ProductTypeName { get; set; }

	public int OfrCount { get; set; }

	public int OnHandCount { get; set; }

	public bool ThinnedOut { get; set; }

	public int LastMaxOfr { get; set; }

	public DateTime LastMaxOfrDate { get; set; }

	public int CurrentMaxOfr { get; set; }

	public DateTime CreatedDate { get; set; }

	public DateTime? LastDateOutOfStock { get; set; }

	public DateTime? SoldOut { get; set; }

	public Dictionary<string, BarcodeProduct> BarcodeProducts => _barcodeProducts;

	public TitleCache()
	{
		LastMaxOfr = 1;
		CreatedDate = DateTime.Now;
		LastMaxOfrDate = DateTime.Now;
	}
}
