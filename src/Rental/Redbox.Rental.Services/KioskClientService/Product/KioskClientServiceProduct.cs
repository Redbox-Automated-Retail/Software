using System;
using System.Net;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Product;

namespace Redbox.Rental.Services.KioskClientService.Product;

public class KioskClientServiceProduct : BaseKioskClientService, IKioskClientServiceProduct
{
	private const string _apiPath = "api/product";

	public IProductByTitleIdsResponse ProductsByTitleIds(IProductByTitleIdsRequest request)
	{
		ProductByTitleIdsResponse productByTitleIdsResponse = new ProductByTitleIdsResponse();
		try
		{
			LogHelper.Instance.Log("KioskClientServiceProduct.ProductsByTitleIds - sending request of " + request.ToJson());
			WebResult<ProductByTitleIdsResponse> webResult = ExecuteWebRequest<ProductByTitleIdsResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/product/bytitleids"), request, "POST", 2000);
			LogHelper.Instance.Log("KioskClientServiceProduct.ProductsByTitleIds - response: " + webResult.ToJson());
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceProduct.ProductsByTitleIds - api call successful");
				productByTitleIdsResponse.Products.AddRange(webResult.Response.Products);
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceProduct.ProductsByTitleIds - " + text);
				productByTitleIdsResponse.Error = text;
			}
		}
		catch (Exception e)
		{
			string text2 = "KioskClientServiceProduct.ProductsByTitleIds - An unhandled exception was raised!";
			LogHelper.Instance.Log(text2, e);
			productByTitleIdsResponse.Error = text2;
		}
		return productByTitleIdsResponse;
	}
}
