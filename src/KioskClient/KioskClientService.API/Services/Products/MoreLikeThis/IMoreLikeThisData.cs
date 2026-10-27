using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public interface IMoreLikeThisData
{
	DateTime FileContentExpireTime { get; }

	MoreLikeThisDataFileContent GetFileContent();

	void RefreshAll(ConcurrentDictionary<long, List<long>> moreLikeThisTitleMapping, DateTime nextFullRefreshDate);

	void RefreshAllWithCurrentExpireTime(ConcurrentDictionary<long, List<long>> moreLikeThisTitleMapping);
}
