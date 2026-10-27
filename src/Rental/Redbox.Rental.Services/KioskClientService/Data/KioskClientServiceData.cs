using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Data;

namespace Redbox.Rental.Services.KioskClientService.Data;

internal class KioskClientServiceData : BaseKioskClientService, IKioskClientServiceData
{
	private class PingDataResponse : BaseResponse
	{
		public override string ToString()
		{
			return this.ToJson().ToPrettyJson();
		}
	}

	private const int _timeout = 10000;

	private const string _apiPath = "api/data";

	public async Task KioskVersions(int kioskId, IDictionary<string, string> versions)
	{
		try
		{
			DateTime now = DateTime.Now;
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = kioskId,
				CreatedOn = now.ToUniversalTime(),
				CreatedOnLocal = now.ToString(),
				Versions = versions
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskVersions - sending request: " + ToPrettyJson(anon));
			WebResult<PingDataResponse> webResult = await ExecuteAsyncWebRequest<PingDataResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskversions"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskVersions - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskVersions", webResult);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskVersions!", e);
		}
	}

	public async Task KioskStatistics(int kioskId, IDictionary<string, string> statistics)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key in statistics.Keys)
		{
			if (key.IsNotInFilteredList())
			{
				dictionary.Add(key, statistics[key]);
			}
		}
		try
		{
			DateTime now = DateTime.Now;
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = kioskId,
				CreatedOn = now.ToUniversalTime(),
				CreatedOnLocal = now.ToString(),
				Statistics = dictionary
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskStatistics - sending request: " + ToPrettyJson(anon));
			WebResult<PingDataResponse> webResult = await ExecuteAsyncWebRequest<PingDataResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskstatistics"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskStatistics - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskStatistics", webResult);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskStatistics!", e);
		}
	}

	public async Task KioskProperties(int kioskId, IDictionary<string, string> properties)
	{
		try
		{
			DateTime now = DateTime.Now;
			var anon = new
			{
				MessageId = Guid.NewGuid(),
				KioskId = kioskId,
				CreatedOn = now.ToUniversalTime(),
				CreatedOnLocal = now.ToString(),
				Properties = properties
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskProperties - sending request: " + ToPrettyJson(anon));
			WebResult<PingDataResponse> webResult = await ExecuteAsyncWebRequest<PingDataResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskproperties"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskProperties - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskProperties", webResult);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskProperties!", e);
		}
	}

	public async Task<BaseResponse> KioskAlert(Guid messageId, int kioskId, int alertType, string subAlertType, string message, DateTime dateTime)
	{
		BaseResponse response = new BaseResponse
		{
			Success = false
		};
		try
		{
			var anon = new
			{
				UniqueId = messageId,
				KioskId = kioskId,
				CreatedOn = dateTime.ToUniversalTime(),
				CreatedOnLocal = dateTime.ToString(),
				AlertType = alertType,
				SubAlertType = subAlertType,
				AlertMessage = message
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskAlert - sending request: " + ToPrettyJson(anon));
			WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskalert"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskAlert - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskAlert", webResult);
			if (webResult.Response == null)
			{
				response.AddError("(E001)", "No response received from KioskClientService");
			}
			else
			{
				response.Success = webResult.Response.Success;
				response.Errors.AddRange(webResult.Response.Errors);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskAlert!", ex);
			response.Success = false;
			response.AddError("(E000)", ex.Message);
		}
		return response;
	}

	public async Task<BaseResponse> AuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source)
	{
		BaseResponse response = new BaseResponse
		{
			Success = false
		};
		try
		{
			var anon = new
			{
				MessageId = messageId,
				KioskId = kioskId,
				LogDate = logDate.ToUniversalTime(),
				CreatedOnLocal = logDate.ToString(),
				EventType = eventType,
				UserName = userName.CheckNotAvailable(),
				Message = message,
				Source = source,
				SendToTable = true
			};
			LogHelper.Instance.Log("KioskClientServiceData.AuditEvent - sending request: " + ToPrettyJson(anon));
			WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/auditevent"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.AuditEvent - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.AuditEvent", webResult);
			if (webResult.Response == null)
			{
				response.AddError("(E001)", "No response received from KioskClientService");
			}
			else
			{
				response.Success = webResult.Response.Success;
				response.Errors.AddRange(webResult.Response.Errors);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.AuditEvent!", ex);
			response.Success = false;
			response.AddError("(E000)", ex.Message);
		}
		return response;
	}

	public async Task<BaseResponse> KioskEvents(Guid messageId, long kioskId, DateTime createdOn, IList<KioskEvent> events)
	{
		BaseResponse response = new BaseResponse
		{
			Success = false
		};
		try
		{
			List<object> kdsEvents = new List<object>();
			events.ForEach(delegate(KioskEvent e)
			{
				kdsEvents.Add(new
				{
					Id = e.Id,
					KioskEventType = KioskEventType.GetName(e.KioskEventType),
					KioskEventSubType = KioskEventSubType.GetName(e.KioskEventSubType),
					Success = e.Success,
					EventTime = e.EventTime,
					Elapsed = e.Elapsed,
					RelatedId = e.RelatedId,
					Data = e.Data
				});
			});
			var anon = new
			{
				MessageId = messageId,
				KioskId = kioskId,
				CreatedOn = createdOn.ToUniversalTime(),
				CreatedOnLocal = createdOn.ToString(),
				KioskEventsData = JsonConvert.SerializeObject(kdsEvents).CompressString()
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskEvents - sending request: " + ToPrettyJson(anon));
			WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskevents"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskEvents - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskEvents", webResult);
			if (webResult.Response == null)
			{
				response.AddError("(E001)", "No response received from KioskClientService");
			}
			else
			{
				response.Success = webResult.Response.Success;
				response.Errors.AddRange(webResult.Response.Errors);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskEvents!", ex);
			response.Success = false;
			response.AddError("(E000)", ex.Message);
		}
		return response;
	}

	public async Task<BaseResponse> KioskFunctionCheck(Guid messageId, long kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver)
	{
		BaseResponse response = new BaseResponse
		{
			Success = false
		};
		try
		{
			string text = reportTime.ToString();
			var anon = new
			{
				MessageId = messageId,
				KioskId = kioskId,
				CreatedOn = reportTime.ToUniversalTime(),
				CreatedOnLocal = text,
				ReportTime = text,
				UserName = userName,
				InitTest = initTest,
				TrackTest = trackTest,
				VendDoor = vendDoor,
				VerticalSlot = verticalSlot,
				CameraDriver = cameraDriver,
				SnapDecode = snapDecode,
				TouchScreenDriver = touchScreenDriver
			};
			LogHelper.Instance.Log("KioskClientServiceData.KioskFunctionCheck - sending request: " + ToPrettyJson(anon));
			WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/data/kioskfunctioncheck"), anon, "POST", 10000);
			LogHelper.Instance.Log($"KioskClientServiceData.KioskFunctionCheck - response: {webResult.Response}");
			LogResponse("KioskClientServiceData.KioskFunctionCheck", webResult);
			if (webResult.Response == null)
			{
				response.AddError("(E001)", "No response received from KioskClientService");
			}
			else
			{
				response.Success = webResult.Response.Success;
				response.Errors.AddRange(webResult.Response.Errors);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceData.KioskFunctionCheck!", ex);
			response.Success = false;
			response.AddError("(E000)", ex.Message);
		}
		return response;
	}

	private static string ToPrettyJson(object source)
	{
		return JsonConvert.SerializeObject(source).ToPrettyJson();
	}
}
