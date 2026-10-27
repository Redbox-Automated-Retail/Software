using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Products;

namespace Redbox.Rental.Services.KioskClientService.Products;

public class KioskClientServiceProductGroups : BaseKioskClientService, IKioskClientServiceProductGroups
{
	private const string _controllerPrefix = "api/productgroups";

	public List<long> GetMoreLikeThis(long productGroupId)
	{
		List<long> result = new List<long>();
		if (productGroupId > 0)
		{
			LogHelper.Instance.Log($"Getting more like this for productGroupId: {productGroupId}");
			WebResult<GetMoreLikeThisResponse> webResult = ExecuteWebRequest<GetMoreLikeThisResponse>(new Uri(string.Format("{0}/{1}/{2}/morelikethis", base.KioskClientServiceBaseUrl, "api/productgroups", productGroupId)), null, "GET", 500);
			LogResponse("KioskClientServiceProductGroups", webResult);
			result = webResult?.Response?.MoreLikeThis;
		}
		else
		{
			LogHelper.Instance.Log($"Not making KCS call to get more like this because given productGroupId was less than 1. Given ProductGroupId: {productGroupId}");
		}
		return result;
	}

	public void RefreshAllMLTData(List<long> productGroupIds)
	{
		Task.Run(() => ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/productgroups/morelikethis/refresh"), productGroupIds, "POST", 180000));
	}

	public ProductGroupSortOrder GetSortOrder()
	{
		LogHelper.Instance.Log("KCS ProductGroups.GetSortOrder");
		WebResult<GetSortOrderResponse> webResult = ExecuteWebRequest<GetSortOrderResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/productgroups/sortorder"), null, "GET", 500);
		LogResponse("KCS ProductGroups.GetSortOrder", webResult);
		if (webResult == null || webResult.Response == null)
		{
			LogHelper.Instance.Log("KCS.GetSortOrder returned null");
			return null;
		}
		if (!webResult.Response.Success)
		{
			LogHelper.Instance.Log($"KCS.GetSortOrder not ok {webResult.Response.StatusCode}");
			return null;
		}
		ProductGroupSortOrder productGroupSortOrder = webResult?.Response?.ProductGroupSortOrder;
		if (productGroupSortOrder == null)
		{
			LogHelper.Instance.Log("KCS.GetSortOrder ProductGroupSortOrder is null");
			return null;
		}
		LogHelper.Instance.Log($"KCS SortOrderTest Retrieved Type: {productGroupSortOrder.SortOrderType}, Count: {productGroupSortOrder.SortOrders?.Count}");
		return productGroupSortOrder;
	}
}
