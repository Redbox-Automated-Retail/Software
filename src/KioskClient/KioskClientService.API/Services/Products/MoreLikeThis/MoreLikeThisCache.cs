using System.Collections.Concurrent;
using System.Collections.Generic;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public class MoreLikeThisCache : IMoreLikeThisCache
{
	private ConcurrentDictionary<long, List<long>> _moreLikeThisItems;

	private IMoreLikeThisData _moreLikeThisData;

	private ILogger<MoreLikeThisCache> _logger;

	public MoreLikeThisCache(IMoreLikeThisData moreLikeThisData, ILogger<MoreLikeThisCache> logger)
	{
		_moreLikeThisData = moreLikeThisData;
		_logger = logger;
		MoreLikeThisDataFileContent fileContent = _moreLikeThisData.GetFileContent();
		_moreLikeThisItems = fileContent.Items;
	}

	public void AddToCache(long productGroupId, List<long> moreLikeThisTitles)
	{
		_moreLikeThisItems.TryAdd(productGroupId, moreLikeThisTitles);
	}

	public void AddItemsToCache(ConcurrentDictionary<long, List<long>> itemsToAdd)
	{
		foreach (KeyValuePair<long, List<long>> item in itemsToAdd)
		{
			_moreLikeThisItems.TryAdd(item.Key, item.Value);
		}
		_logger.LogInfoWithSource("Successfully updated MoreLikeThisCache with new items. New Items: " + itemsToAdd.ToJson(), "AddItemsToCache");
	}

	public void RefreshCache(ConcurrentDictionary<long, List<long>> moreLikeThisMap)
	{
		_moreLikeThisItems.Clear();
		_moreLikeThisItems = moreLikeThisMap;
	}

	public bool ContainsItem(long productGroupId)
	{
		return _moreLikeThisItems.ContainsKey(productGroupId);
	}

	public int TotalCount()
	{
		return _moreLikeThisItems?.Count ?? 0;
	}

	public List<long> Get(long productGroupId)
	{
		if (!_moreLikeThisItems.ContainsKey(productGroupId))
		{
			return new List<long>();
		}
		return _moreLikeThisItems[productGroupId];
	}

	public ConcurrentDictionary<long, List<long>> GetAllPairs()
	{
		return _moreLikeThisItems;
	}
}
