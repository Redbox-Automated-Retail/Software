using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Jayrock.Json.Conversion;
using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskDataServices.Proxy;

internal class KioskDataServicesProxy
{
	private Uri? _kioskDataServicesBaseUri;

	private string? _kioskDataServicesKey;

	public static KioskDataServicesProxy Instance => Singleton<KioskDataServicesProxy>.Instance;

	private Uri? KioskDataServicesBaseUri
	{
		get
		{
			LoadUriAndKey();
			return _kioskDataServicesBaseUri;
		}
	}

	private string? KioskDataServicesKey
	{
		get
		{
			LoadUriAndKey();
			return _kioskDataServicesKey;
		}
	}

	public IDictionary<string, object> AddKioskSessionJson(byte[] payload)
	{
		return ExecuteMessage("AddKioskSessionJson", "api/KioskData/AddKioskSessionJson", payload);
	}

	private IDictionary<string, object> ExecuteMessage(string messageType, string apiUri, byte[] payload)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		Dictionary<string, object> responseData = new Dictionary<string, object> { { "success", false } };
		try
		{
			string json = Encoding.ASCII.GetString(payload);
			string prettyJson = json.ToPrettyJson();
			LogHelper.Instance.Log("KioskDataServicesProxy JSON request: type " + messageType + "; body: " + ((prettyJson.Length <= 10000) ? prettyJson : json));
			string? responseJson = PerformRemoteMessage(apiUri, json, KioskDataServicesKey);
			LogHelper.Instance.Log("KioskDataServicesProxy JSON response: type " + messageType + "; response: " + responseJson.ToPrettyJson());
			LogHelper.Instance.Log($"KioskDataServicesProxy call execution time: {executionTimer.Elapsed}");
			if (!string.IsNullOrEmpty(responseJson))
			{
				responseData = responseJson.ToDictionary();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskDataServicesProxy.ExecuteMessage.", e);
			responseData["ExceptionRaised"] = true;
		}
		return responseData;
	}

	private string? PerformRemoteMessage(string apiUri, string jsonRequest, string? authorizationKey)
	{
		if (Uri.TryCreate(KioskDataServicesBaseUri, apiUri, out var result))
		{
			LogHelper.Instance.Log("KioskDataServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = jsonRequest.Length;
			webRequest.Headers.Add("Authorization: Bearer " + authorizationKey);
			IStoreManager storeManager = ServiceLocator.Instance.GetService<IStoreManager>();
			if (storeManager != null)
			{
				webRequest.Headers.Add("x-redbox-kioskid", storeManager.KioskId.ToString());
				LogHelper.Instance.Log(string.Format("Added {0} header where KioskId = '{1}'", "x-redbox-kioskid", storeManager.KioskId));
			}
			else
			{
				LogHelper.Instance.Log("Could not add x-redbox-kioskid header; Store Manager is null.");
			}
			string? sessionIdFromBody = GetSessionIdFromBody(jsonRequest);
			if (!string.IsNullOrWhiteSpace(sessionIdFromBody))
			{
				webRequest.Headers.Add("x-redbox-activityid", sessionIdFromBody);
			}
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(Encoding.UTF8.GetBytes(jsonRequest), 0, jsonRequest.Length);
			requestStream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
		}
		LogHelper.Instance.Log($"Unable to create Uri from {KioskDataServicesBaseUri} and {apiUri}");
		return null;
	}

	private string? GetSessionIdFromBody(string body)
	{
		try
		{
			return ((AddKioskSessionRequest)JsonConvert.Import(typeof(AddKioskSessionRequest), body)).SessionId;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable get sessionId from body: " + body, e);
			return null;
		}
	}

	private void LoadUriAndKey()
	{
		if (!(_kioskDataServicesBaseUri == null) && _kioskDataServicesKey != null)
		{
			return;
		}
		IConfiguration configuration = ServiceLocator.Instance.GetService<IConfiguration>();
		if (configuration == null)
		{
			return;
		}
		Uri? uri = null;
		string kioskDataServicesUrl = configuration.KioskDataServicesUrl;
		string kioskDataServicesKey = configuration.KioskDataServicesKey;
		if (!string.IsNullOrEmpty(kioskDataServicesUrl))
		{
			kioskDataServicesUrl = kioskDataServicesUrl.TrimEnd('/') + "/";
			try
			{
				uri = new Uri(kioskDataServicesUrl);
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("KioskDataServicesProxy.LoadUriAndKey() - exception creating Uri!", e);
			}
		}
		if (uri != null && !string.IsNullOrEmpty(kioskDataServicesKey))
		{
			_kioskDataServicesBaseUri = uri;
			_kioskDataServicesKey = kioskDataServicesKey;
		}
	}

	private KioskDataServicesProxy()
	{
	}
}
