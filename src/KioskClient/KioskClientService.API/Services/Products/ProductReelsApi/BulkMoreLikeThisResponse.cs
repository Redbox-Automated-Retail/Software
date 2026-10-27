using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Products.ProductReelsApi;

public class BulkMoreLikeThisResponse
{
	[JsonProperty("id")]
	public string ProductGroupId { get; set; }

	[JsonProperty("productIds")]
	public List<string> MoreLikeThisTitles { get; set; }
}
