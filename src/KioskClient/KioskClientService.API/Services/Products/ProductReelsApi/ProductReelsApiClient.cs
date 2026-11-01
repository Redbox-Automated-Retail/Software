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
		List<string> productIdStrings = new List<string>();
		foreach (long productGroupId in productGroupIds)
		{
			productIdStrings.Add(productGroupId.ToString());
		}
		var requestObject = new
		{
			productIds = productIdStrings
		};
		HttpResponseMessage response = await _httpService.SendRequest(_kioskConfiguration.KioskClientDefault.ProductReelsApiUrl, "products/reels/bulkmorelikethis", requestObject, HttpMethod.Post, null, logResponseContent: false);
		if (response == null)
		{
			_logger.LogErrorWithSource("Call to bulkmorelikethis returned no response!", "MoreLikeThisBulkAsync");
		}
		else
		{
			string rawContent = ((response.Content != null) ? (await response.Content.ReadAsStringAsync()) : string.Empty);
			string responseContent = rawContent;
			if (response.IsSuccessStatusCode)
			{
				List<BulkMoreLikeThisResponse> bulkResponses;
				try
				{
					bulkResponses = JsonConvert.DeserializeObject<List<BulkMoreLikeThisResponse>>(responseContent);
				}
				catch (JsonReaderException ex)
				{
					_logger.LogCriticalWithSource(ex, "Exception deserializing bulkmorelikethis list!", "MoreLikeThisBulkAsync");
					bulkResponses = new List<BulkMoreLikeThisResponse>();
				}
				foreach (BulkMoreLikeThisResponse bulkResponse in bulkResponses)
				{
					if (bulkResponse.MoreLikeThisTitles == null || !bulkResponse.MoreLikeThisTitles.Any())
					{
						continue;
					}
					List<long> similarProductIds = new List<long>();
					foreach (string moreLikeThisTitle in bulkResponse.MoreLikeThisTitles)
					{
						if (long.TryParse(moreLikeThisTitle, out var similarProductId) && similarProductId > 0)
						{
							similarProductIds.Add(similarProductId);
						}
					}
					if (long.TryParse(bulkResponse.ProductGroupId, out var groupId))
					{
						result.TryAdd(groupId, similarProductIds);
					}
				}
			}
			else
			{
				_logger.LogErrorWithSource($"Call to bulkMoreLikeThis returned status: {response.StatusCode}, content: {responseContent}", "MoreLikeThisBulkAsync");
			}
		}
		return result;
	}
}
