using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public interface IMoreLikeThisCache
{
	void AddToCache(long productGroupId, List<long> moreLikeThisTitles);

	void AddItemsToCache(ConcurrentDictionary<long, List<long>> itemsToAdd);

	void RefreshCache(ConcurrentDictionary<long, List<long>> moreLikeThisMap);

	bool ContainsItem(long productGroupId);

	int TotalCount();

	List<long> Get(long productGroupId);

	ConcurrentDictionary<long, List<long>> GetAllPairs();
}
