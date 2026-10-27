using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Dual;
using Redbox.Rental.Model.KioskClientService.Kiosk;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Dual;

internal class KioskClientServiceDual : BaseKioskClientService, IKioskClientServiceDual
{
	private const string _apiPath = "api/dual";

	private ConcurrentDictionary<long, string> _inStockInOther = new ConcurrentDictionary<long, string>();

	private NearbyKiosk DualKiosk => ServiceLocator.Instance.GetService<IKioskClientService>()?.Kiosk?.DualKiosk;

	private bool ShowDualInStock => ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession.Browse.ShowDualInStock ?? false;

	public void Refresh()
	{
		LogHelper.Instance.Log("KCS.Dual.KioskClientServiceDual.Refresh");
		if (DualKiosk != null && DualKiosk.KioskId > 0)
		{
			Refresh(DualKiosk.KioskId);
		}
	}

	public void LoadInStockInOther()
	{
		LogHelper.Instance.Log("KCS.Dual.KioskClientServiceDual.LoadInStockInOther");
		List<long> list = new List<long>();
		if (DualKiosk != null && ShowDualInStock)
		{
			List<long> outOfStockTitleIds = GetOutOfStockTitleIds();
			list = ((outOfStockTitleIds.Count > 0) ? InStockInOther(outOfStockTitleIds) : null)?.InStock ?? new List<long>();
		}
		_inStockInOther.Clear();
		list.ForEach(delegate(long titleId)
		{
			_inStockInOther.TryAdd(titleId, null);
		});
	}

	public void UpdateTitles()
	{
		LogHelper.Instance.Log("KCS.Dual.KioskClientServiceDual.UpdateTitles");
		List<long> list = new List<long>();
		if (DualKiosk != null && DualKiosk.KioskId > 0 && ShowDualInStock)
		{
			List<long> outOfStockTitleIds = GetOutOfStockTitleIds();
			if (outOfStockTitleIds.Count > 0)
			{
				list = UpdateTitles(DualKiosk.KioskId, outOfStockTitleIds)?.TitlesCount?.Where((KeyValuePair<long, int> tc) => tc.Value > 0).Select((KeyValuePair<long, int> tc) => tc.Key).ToList() ?? new List<long>();
			}
		}
		_inStockInOther.Clear();
		list.ForEach(delegate(long titleId)
		{
			_inStockInOther.TryAdd(titleId, null);
		});
	}

	public bool IsInStockInOther(long titleId)
	{
		return _inStockInOther.ContainsKey(titleId);
	}

	public IRefreshResponse Refresh(long dualKioskId)
	{
		RefreshResponse refreshResponse = new RefreshResponse();
		try
		{
			var anon = new
			{
				DualKioskId = dualKioskId
			};
			LogHelper.Instance.Log("KioskClientServiceDual.Refresh - sending request of " + anon.ToJson());
			WebResult<RefreshResponse> webResult = ExecuteWebRequest<RefreshResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/dual/refresh"), anon, "POST", Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Dual.RefreshTimeout);
			LogHelper.Instance.Log("KioskClientServiceDual.Refresh - response: " + webResult.ToJson());
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceDual.Refresh - api call successful");
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceDual.Refresh - " + text);
				refreshResponse.Error = text;
			}
		}
		catch (Exception e)
		{
			string text2 = "KioskClientServiceDual.Refresh - An unhandled exception was raised!";
			LogHelper.Instance.Log(text2, e);
			refreshResponse.Error = text2;
		}
		return refreshResponse;
	}

	public async Task<IRefreshResponse> RefreshAsync(long dualKioskId)
	{
		return await Task.Run(() => Refresh(dualKioskId));
	}

	public IUpdateTitlesResponse UpdateTitles(long dualKioskId, IEnumerable<long> titleIds)
	{
		UpdateTitlesResponse updateTitlesResponse = new UpdateTitlesResponse();
		try
		{
			var anon = new { dualKioskId, titleIds };
			LogHelper.Instance.Log("KioskClientServiceDual.UpdateTitle - sending request of " + anon.ToJson());
			WebResult<UpdateTitlesResponse> webResult = ExecuteWebRequest<UpdateTitlesResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/dual/updatetitles"), anon, "POST", Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Dual.UpdateTitlesTimeout);
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceDual.UpdateTitles - api call successful");
				updateTitlesResponse.TitlesCount = webResult.Response.TitlesCount;
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceDual.UpdateTitles - " + text);
				updateTitlesResponse.Error = text;
			}
		}
		catch (Exception e)
		{
			string text2 = "KioskClientServiceDual.UpdateTitles - An unhandled exception was raised!";
			LogHelper.Instance.Log(text2, e);
			updateTitlesResponse.Error = text2;
		}
		return updateTitlesResponse;
	}

	public async Task<IUpdateTitlesResponse> UpdateTitlesAsync(long dualKioskId, IEnumerable<long> titleIds)
	{
		return await Task.Run(() => UpdateTitles(dualKioskId, titleIds));
	}

	public IInStockInOtherResponse InStockInOther(IEnumerable<long> titleIds)
	{
		InStockInOtherResponse inStockInOtherResponse = new InStockInOtherResponse();
		try
		{
			var anon = new { titleIds };
			LogHelper.Instance.Log("KioskClientServiceDual.InStockInOther - sending request of " + anon.ToJson());
			WebResult<InStockInOtherResponse> webResult = ExecuteWebRequest<InStockInOtherResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/dual/instockinother"), anon, "POST", Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Dual.InStockInOtherTimeout);
			LogHelper.Instance.Log("KioskClientServiceDual.InStockInOther - response: " + webResult.ToJson());
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceDual.InStockInOther - api call successful");
				inStockInOtherResponse.InStock.AddRange(webResult.Response.InStock);
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceDual.InStockInOther - " + text);
				inStockInOtherResponse.Error = text;
			}
		}
		catch (Exception e)
		{
			string text2 = "KioskClientServiceDual.InStockInOther - An unhandled exception was raised!";
			LogHelper.Instance.Log(text2, e);
			inStockInOtherResponse.Error = text2;
		}
		return inStockInOtherResponse;
	}

	public async Task<IInStockInOtherResponse> InStockInOtherAsync(IEnumerable<long> titleIds)
	{
		return await Task.Run(() => InStockInOther(titleIds));
	}

	private List<long> GetOutOfStockTitleIds()
	{
		List<long> result = new List<long>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			service.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, TitleType.All, BrowseSort.Alpha, null, null, null, null, isADAView: true, null, null, out var products);
			result = (products?.Where((IKioskProduct kp) => kp is ITitleProduct).Select((IKioskProduct kp) => kp as ITitleProduct).ToList())?.Where((ITitleProduct tp) => !tp.InStock).Select((ITitleProduct tp) => tp.ProductId).ToList() ?? new List<long>();
		}
		return result;
	}
}
