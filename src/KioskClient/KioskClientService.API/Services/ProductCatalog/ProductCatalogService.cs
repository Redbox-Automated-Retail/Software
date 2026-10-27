using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.ProductCatalog;

internal class ProductCatalogService : IProductCatalogService
{
	private readonly ILogger<IProductCatalogService> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly IStoreService _store;

	private IPersistentDataCacheService _persistentDataCacheService;

	private string _cacheProductFileName => "ProductCatalog";

	public ProductCatalogService(ILogger<IProductCatalogService> logger, IStoreService store, IProxyApi proxy, IPersistentDataCacheService persistentDataCacheService)
	{
		_logger = logger;
		_store = store;
		_proxyApi = proxy;
		_persistentDataCacheService = persistentDataCacheService;
	}

	public async Task<ApiBaseResponse> RemoveTitleCache(long titleId)
	{
		try
		{
			bool flag = await RemoveProductCache(titleId);
			return new ApiBaseResponse
			{
				StatusCode = (flag ? HttpStatusCode.OK : HttpStatusCode.NotFound)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception", "RemoveTitleCache");
			return new ProductByTitleIdsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = "Internal Service Error"
			};
		}
	}

	public async Task<ProductByTitleIdsResponse> ProductsByTitleIds(ProductByTitleIdsRequest request)
	{
		_ = 1;
		try
		{
			ProductByTitleIdsResponse response = new ProductByTitleIdsResponse
			{
				StatusCode = HttpStatusCode.OK
			};
			await UpdateMissing(request.TitleIds);
			ProductByTitleIdsResponse productByTitleIdsResponse = response;
			productByTitleIdsResponse.Products = await GetProducts(request.TitleIds);
			return response;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception", "ProductsByTitleIds");
			return new ProductByTitleIdsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = "Internal Service Error"
			};
		}
	}

	private async Task<List<Product>> GetProducts(List<long> titleIds)
	{
		PersistentDataWrapper<ProductData> persistentDataWrapper = await _persistentDataCacheService.Read<ProductData>(_cacheProductFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			return new List<Product>();
		}
		List<Product> list = (from p in persistentDataWrapper.Data.Products
			join t in titleIds on p.Product.TitleId equals t
			select p.Product).ToList();
		_logger.LogInfoWithSource($"GetProducts returning: {list.Count} cached products out of {titleIds.Count} requested", "GetProducts");
		return list;
	}

	private async Task UpdateMissing(List<long> titleIds)
	{
		List<ProductWrapper> wrapperProducts = (await _persistentDataCacheService.Read<ProductData>(_cacheProductFileName, useCache: true))?.Data?.Products ?? new List<ProductWrapper>();
		List<ProductWrapper> list = wrapperProducts.Where((ProductWrapper wp) => wp.DownloadedOn < DateTime.Now.AddDays(-7.0)).ToList();
		if (list.Count > 0)
		{
			_logger.LogInfoWithSource($"Removing: {list.Count} products because they where downloaded more than 7 days ago.", "UpdateMissing");
			list.ForEach(delegate(ProductWrapper wp)
			{
				wrapperProducts.Remove(wp);
			});
		}
		List<long> second = (from p in wrapperProducts
			join t in titleIds on p.Product.TitleId equals t
			select t).ToList();
		List<long> list2 = titleIds.Except(second).ToList();
		_logger.LogInfoWithSource($"Found: {list2.Count} products", "UpdateMissing");
		if (list2.Count != 0)
		{
			List<ProductWrapper> list3 = (await GetProductsByTitleIds(list2)).ToProductWraper();
			list3.AddRange(wrapperProducts);
			await _persistentDataCacheService.Write(new ProductData
			{
				Products = list3.OrderBy((ProductWrapper p) => p.Product.TitleId).ToList()
			}, _cacheProductFileName);
		}
	}

	private async Task<List<Product>> GetProductsByTitleIds(List<long> titleIds)
	{
		_logger.LogInfoWithSource("Requesting " + string.Join(",", titleIds) + " from proxy", "GetProductsByTitleIds");
		APIResponse<ProductByTitleIdsResponse> aPIResponse = await _proxyApi.PostProductsByTitleIds(_store.KioskId, Guid.NewGuid(), titleIds);
		if (aPIResponse.IsSuccessStatusCode)
		{
			return aPIResponse.Response.Products;
		}
		return new List<Product>();
	}

	private async Task<bool> RemoveProductCache(long titleId)
	{
		_logger.LogInfoWithSource($"Removing title: {titleId} from cache", "RemoveProductCache");
		PersistentDataWrapper<ProductData> persistentDataWrapper = await _persistentDataCacheService.Read<ProductData>(_cacheProductFileName, useCache: true);
		List<ProductWrapper> source = persistentDataWrapper?.Data?.Products ?? new List<ProductWrapper>();
		if (!source.Any())
		{
			_logger.LogInfoWithSource($"Title: {titleId} not found in cache because there is no cache", "RemoveProductCache");
			return false;
		}
		ProductWrapper productWrapper = source.FirstOrDefault((ProductWrapper p) => p.Product.TitleId == titleId);
		if (productWrapper == null)
		{
			_logger.LogInfoWithSource($"Title: {titleId} not found in cache, skipping removal", "RemoveProductCache");
			return false;
		}
		persistentDataWrapper.Data.Products.Remove(productWrapper);
		await _persistentDataCacheService.Write(persistentDataWrapper.Data, _cacheProductFileName);
		return true;
	}
}
