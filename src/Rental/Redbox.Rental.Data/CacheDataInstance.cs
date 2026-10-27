using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Cache;

namespace Redbox.Rental.Data;

public class CacheDataInstance : ICacheDataInstance
{
	public Dictionary<string, BarcodeProduct> _barcodeProducts = new Dictionary<string, BarcodeProduct>();

	public Dictionary<long, TitleCache> _titles = new Dictionary<long, TitleCache>();

	public Dictionary<string, BarcodeProduct> BarcodeProducts => _barcodeProducts;

	public Dictionary<long, TitleCache> Titles => _titles;

	public TitleCache AddTitleCache(long productId)
	{
		if (!_titles.TryGetValue(Convert.ToInt64(productId), out var value))
		{
			value = new TitleCache
			{
				ProductId = Convert.ToInt64(productId)
			};
			_titles[value.ProductId] = value;
		}
		return value;
	}
}
