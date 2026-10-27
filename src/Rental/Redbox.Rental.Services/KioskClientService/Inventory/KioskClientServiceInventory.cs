using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class KioskClientServiceInventory : BaseKioskClientService, IKioskClientServiceInventory
{
	private class KioskClientServiceResult : IKioskClientServiceResult
	{
		public bool Success { get; internal set; }

		public int StatusCode { get; internal set; }

		public Redbox.KioskEngine.ComponentModel.Error Error { get; internal set; }
	}

	private const string _apiPath = "api/inventory";

	public KioskClientServiceInventory()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new KcsUpdateMerchStatusMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskClientServiceInventory.  Unable to find QueueService.");
		}
	}

	public IGetMerchandizingOrdersResponse GetCurrentMerchandizingOrders()
	{
		WebResult<GetMerchandizingOrdersResponse> webResult = ExecuteWebRequest<GetMerchandizingOrdersResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/MerchandizingOrders"), null, "GET", ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceGetMerchandizingOrdersTimeout ?? 60000);
		LogResponse("KioskClientServiceInventory.GetCurrentMerchandizingOrders", webResult);
		return webResult?.Response;
	}

	public IUpdateMerchandizingStatusResponse SendMerchandizingStatusUpdates(List<MerchStatusUpdate> updates)
	{
		if (updates != null && updates.Count > 0)
		{
			WebResult<UpdateMerchandizingStatusResponse> webResult = ExecuteWebRequest<UpdateMerchandizingStatusResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/updatemerchandizingstatus"), updates, "POST", ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceGetMerchandizingOrdersTimeout ?? 60000);
			LogResponse("KioskClientServiceInventory.SendMerchandizingStatusUpdates", webResult);
			return webResult?.Response;
		}
		LogHelper.Instance.Log("Error in KioskClientServiceInventory.SendMerchandizingStatusUpdates. No updates specified.");
		return new UpdateMerchandizingStatusResponse
		{
			Success = false
		};
	}

	public IDictionary<string, object> ProcessQueuedMerchandStatusUpdates(byte[] payload)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			List<MerchStatusUpdate> list = Encoding.ASCII.GetString(payload).ToObject<List<MerchStatusUpdate>>();
			LogHelper.Instance.Log($"KioskClientServiceInventory.ProcessQueuedMerchandStatusUpdates sending {list?.Count ?? 0} status updates.");
			IUpdateMerchandizingStatusResponse updateMerchandizingStatusResponse = SendMerchandizingStatusUpdates(list);
			if (updateMerchandizingStatusResponse != null && updateMerchandizingStatusResponse.Success)
			{
				dictionary["success"] = true;
				dictionary["FailedUpdates"] = updateMerchandizingStatusResponse.FailedBarcodes;
				dictionary["message"] = updateMerchandizingStatusResponse.Error;
			}
			else
			{
				dictionary["FailedUpdates"] = list;
				dictionary["message"] = updateMerchandizingStatusResponse?.Error;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceInventory.ProcessQueuedMerchandStatusUpdates.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}

	public IKioskClientServiceResult Snapshot(string storeNumber, IEnumerable<IInventoryItem> items)
	{
		KioskClientServiceResult kioskClientServiceResult = new KioskClientServiceResult();
		try
		{
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = int.Parse(storeNumber),
				Discs = items
			};
			LogHelper.Instance.Log("KioskClientServiceInventory.Snapshot - sending inventory snapshot request of " + anon.ToJson());
			int timeout = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceInventorySnapshotTimeout ?? 60000;
			WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/sync"), anon, "POST", timeout);
			LogResponse("KioskClientServiceInventory.Snapshot", webResult);
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				if (webResult.Response.Success)
				{
					LogHelper.Instance.Log("KioskClientServiceInventory.Snapshot - Api call successful");
					kioskClientServiceResult.Success = true;
				}
				else
				{
					kioskClientServiceResult.Success = false;
					string text = $"KioskClientServiceInventory.Snapshot - Api call failed server side with the following status code: {webResult.Response.StatusCode}";
					Redbox.Rental.Model.Error error = webResult.Response.Errors.FirstOrDefault();
					if (error != null)
					{
						text = text + ", error: " + error.Code + ", " + error.Message;
						kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, text, string.Empty);
					}
					LogHelper.Instance.Log(text);
				}
			}
			else
			{
				string text2 = $"KioskClientServiceInventory.Snapshot - api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log(text2);
				kioskClientServiceResult.Success = false;
				kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text2, string.Empty);
			}
		}
		catch (Exception ex)
		{
			string text3 = "KioskClientServiceInventory.Snapshot - An unhandled exception was raised!";
			LogHelper.Instance.Log(text3, ex);
			kioskClientServiceResult.Success = false;
			kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text3, ex.Message);
		}
		return kioskClientServiceResult;
	}

	public async Task<IKioskClientServiceResult> SnapshotAsync(string storeNumber, IEnumerable<IInventoryItem> items)
	{
		return await Task.Run(() => Snapshot(storeNumber, items));
	}

	public IKioskClientServiceResult DiscAction(string storeNumber, string userName, string jobName, DateTime jobTimestamp, List<Dictionary<string, string>> actionList)
	{
		KioskClientServiceResult kioskClientServiceResult = new KioskClientServiceResult();
		try
		{
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = int.Parse(storeNumber),
				UserName = userName,
				JobName = jobName,
				ActionDate = jobTimestamp,
				Actions = actionList
			};
			LogHelper.Instance.Log("KioskClientServiceInventory.DiscAction - sending disc action request of " + anon.ToJson());
			int timeout = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceDiscActionTimeout ?? 10000;
			WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/discaction"), anon, "POST", timeout);
			LogResponse("KioskClientServiceInventory.DiscAction", webResult);
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				if (webResult.Response.Success)
				{
					LogHelper.Instance.Log("KioskClientServiceInventory.DiscAction - Api call successful");
					kioskClientServiceResult.Success = true;
				}
				else
				{
					kioskClientServiceResult.Success = false;
					string text = $"KioskClientServiceInventory.DiscAction - Api call failed server side with the following status code: {webResult.Response.StatusCode}";
					Redbox.Rental.Model.Error error = webResult.Response.Errors.FirstOrDefault();
					if (error != null)
					{
						text = text + ", error: " + error.Code + ", " + error.Message;
						kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, text, string.Empty);
					}
					LogHelper.Instance.Log(text);
				}
			}
			else
			{
				string text2 = $"KioskClientServiceInventory.DiscAction - api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log(text2);
				kioskClientServiceResult.Success = false;
				kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text2, string.Empty);
			}
		}
		catch (Exception ex)
		{
			string text3 = "KioskClientServiceInventory.DiscAction - An unhandled exception was raised!";
			LogHelper.Instance.Log(text3, ex);
			kioskClientServiceResult.Success = false;
			kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text3, ex.Message);
		}
		return kioskClientServiceResult;
	}

	public async Task<IKioskClientServiceResult> DiscActionAsync(string storeNumber, string userName, string jobName, DateTime jobTimestamp, List<Dictionary<string, string>> actionList)
	{
		return await Task.Run(() => DiscAction(storeNumber, userName, jobName, jobTimestamp, actionList));
	}

	public IKioskClientServiceResult DiscFraud(string storeNumber, string barcode, DateTime readTime, Dictionary<string, string> properties)
	{
		KioskClientServiceResult kioskClientServiceResult = new KioskClientServiceResult();
		try
		{
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = int.Parse(storeNumber),
				Barcode = barcode,
				TransactionDate = readTime,
				Properties = properties
			};
			LogHelper.Instance.Log("KioskClientServiceInventory.DiscFraud - sending disc fraud request of " + anon.ToJson());
			int timeout = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceDiscFraudTimeout ?? 10000;
			WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/discfraud"), anon, "POST", timeout);
			LogResponse("KioskClientServiceInventory.DiscFraud", webResult);
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				if (webResult.Response.Success)
				{
					LogHelper.Instance.Log("KioskClientServiceInventory.DiscFraud - Api call successful");
					kioskClientServiceResult.Success = true;
				}
				else
				{
					kioskClientServiceResult.Success = false;
					string text = $"KioskClientServiceInventory.DiscFraud - Api call failed server side with the following status code: {webResult.Response.StatusCode}";
					Redbox.Rental.Model.Error error = webResult.Response.Errors.FirstOrDefault();
					if (error != null)
					{
						text = text + ", error: " + error.Code + ", " + error.Message;
						kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, text, string.Empty);
					}
					LogHelper.Instance.Log(text);
				}
			}
			else
			{
				string text2 = $"KioskClientServiceInventory.DiscFraud - api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log(text2);
				kioskClientServiceResult.Success = false;
				kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text2, string.Empty);
			}
		}
		catch (Exception ex)
		{
			string text3 = "KioskClientServiceInventory.DiscFraud - An unhandled exception was raised!";
			LogHelper.Instance.Log(text3, ex);
			kioskClientServiceResult.Success = false;
			kioskClientServiceResult.Error = Redbox.KioskEngine.ComponentModel.Error.NewError("(V000)", text3, ex.Message);
		}
		return kioskClientServiceResult;
	}

	public async Task<IKioskClientServiceResult> DiscFraudAsync(string storeNumber, string barcode, DateTime readDate, Dictionary<string, string> properties)
	{
		return await Task.Run(() => DiscFraud(storeNumber, barcode, readDate, properties));
	}

	public IKioskTitlesCountResponse KioskTitlesCount(long sourceKioskId, List<long> kioskIds, List<long> titleIds)
	{
		KioskTitlesCountResponse kioskTitlesCountResponse = new KioskTitlesCountResponse();
		try
		{
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				SourceKioskId = sourceKioskId,
				KioskIds = kioskIds,
				Titles = titleIds
			};
			LogHelper.Instance.Log("KioskClientServiceInventory.KioskTitlesCount - sending request of " + anon.ToJson());
			WebResult<KioskTitlesCountResponse> webResult = ExecuteWebRequest<KioskTitlesCountResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/inventory/kiosktitlescount"), anon, "POST", Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Inventory.KioskTitlesCountTimeout);
			LogHelper.Instance.Log($"KioskClientServiceInventory.KioskTitlesCount - api response: {webResult}");
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceInventory.KioskTitlesCount - api call successful");
				kioskTitlesCountResponse.KioskTitlesCount.AddRange(webResult.Response.KioskTitlesCount);
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceInventory.KioskTitlesCount - " + text);
				kioskTitlesCountResponse.Error = text;
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("KioskClientServiceInventory.KioskTitlesCount - An unhandled exception was raised!", ex);
			kioskTitlesCountResponse.Error = ex.Message;
		}
		return kioskTitlesCountResponse;
	}

	public async Task<IKioskTitlesCountResponse> KioskTitlesCountAsync(long sourceKioskId, List<long> kioskIds, List<long> titleIds)
	{
		return await Task.Run(() => KioskTitlesCount(sourceKioskId, kioskIds, titleIds));
	}
}
