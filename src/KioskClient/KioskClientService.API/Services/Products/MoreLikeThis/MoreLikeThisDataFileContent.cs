using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public class MoreLikeThisDataFileContent
{
	public DateTime ExpireTime { get; set; }

	public ConcurrentDictionary<long, List<long>> Items { get; set; }
}
