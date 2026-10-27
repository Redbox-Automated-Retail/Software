using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Products.ProductReelsApi;

public class ProductReelsApiClient : IProductReelsApiClient
{
	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<ProductReelsApiClient> _logger;

	public ProductReelsApiClient(IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IHttpService httpService, ILogger<ProductReelsApiClient> logger)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_httpService = httpService;
		_logger = logger;
		if (_kioskConfiguration.KioskClientDefault.ProductReelsApiTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientDefault.ProductReelsApiTimeout;
		}
	}

	public async Task<ConcurrentDictionary<long, List<long>>> MoreLikeThisBulkAsync(List<long> productGroupIds)
	{
		ConcurrentDictionary<long, List<long>> result = new ConcurrentDictionary<long, List<long>>();
		List<string> list = new List<string>();
		foreach (long productGroupId in productGroupIds)
		{
			list.Add(productGroupId.ToString());
		}
		var requestObject = new
		{
			productIds = list
		};
		HttpResponseMessage response = await _httpService.SendRequest(_kioskConfiguration.KioskClientDefault.ProductReelsApiUrl, "products/reels/bulkmorelikethis", requestObject, HttpMethod.Post, null, logResponseContent: false);
		if (response == null)
		{
			_logger.LogErrorWithSource("Call to bulkmorelikethis returned no response!", "MoreLikeThisBulkAsync");
		}
		else
		{
			string text = ((response.Content != null) ? (await response.Content.ReadAsStringAsync()) : string.Empty);
			string text2 = text;
			if (response.IsSuccessStatusCode)
			{
				List<BulkMoreLikeThisResponse> list2;
				try
				{
					list2 = JsonConvert.DeserializeObject<List<BulkMoreLikeThisResponse>>(text2);
				}
				catch (JsonReaderException ex)
				{
					_logger.LogCriticalWithSource(ex, "Exception deserializing bulkmorelikethis list!", "MoreLikeThisBulkAsync");
					list2 = new List<BulkMoreLikeThisResponse>();
				}
				foreach (BulkMoreLikeThisResponse item in list2)
				{
					if (item.MoreLikeThisTitles == null || !item.MoreLikeThisTitles.Any())
					{
						continue;
					}
					List<long> list3 = new List<long>();
					foreach (string moreLikeThisTitle in item.MoreLikeThisTitles)
					{
						if (long.TryParse(moreLikeThisTitle, out var result2) && result2 > 0)
						{
							list3.Add(result2);
						}
					}
					if (long.TryParse(item.ProductGroupId, out var result3))
					{
						result.TryAdd(result3, list3);
					}
				}
			}
			else
			{
				_logger.LogErrorWithSource($"Call to bulkMoreLikeThis returned status: {response.StatusCode}, content: {text2}", "MoreLikeThisBulkAsync");
			}
		}
		return result;
	}
}
