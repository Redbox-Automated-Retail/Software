using System.Collections.Generic;

namespace Redbox.Rental.Model.Cache;

public interface ICacheDataInstance
{
	Dictionary<string, BarcodeProduct> BarcodeProducts { get; }

	Dictionary<long, TitleCache> Titles { get; }

	TitleCache AddTitleCache(long productId);
}
