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
	private Uri _kioskDataServicesBaseUri;

	private string _kioskDataServicesKey;

	public static KioskDataServicesProxy Instance => Singleton<KioskDataServicesProxy>.Instance;

	private Uri KioskDataServicesBaseUri
	{
		get
		{
			LoadUriAndKey();
			return _kioskDataServicesBaseUri;
		}
	}

	private string KioskDataServicesKey
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
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			string text = Encoding.ASCII.GetString(payload);
			string text2 = text.ToPrettyJson();
			LogHelper.Instance.Log("KioskDataServicesProxy JSON request: type " + messageType + "; body: " + ((text2.Length <= 10000) ? text2 : text));
			string text3 = PerformRemoteMessage(apiUri, text, KioskDataServicesKey);
			LogHelper.Instance.Log("KioskDataServicesProxy JSON response: type " + messageType + "; response: " + text3.ToPrettyJson());
			LogHelper.Instance.Log($"KioskDataServicesProxy call execution time: {executionTimer.Elapsed}");
			if (!string.IsNullOrEmpty(text3))
			{
				dictionary = text3.ToDictionary();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskDataServicesProxy.ExecuteMessage.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}

	private string PerformRemoteMessage(string apiUri, string jsonRequest, string authorizationKey)
	{
		if (Uri.TryCreate(KioskDataServicesBaseUri, apiUri, out var result))
		{
			LogHelper.Instance.Log("KioskDataServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = jsonRequest.Length;
			webRequest.Headers.Add("Authorization: Bearer " + authorizationKey);
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service != null)
			{
				webRequest.Headers.Add("x-redbox-kioskid", service.KioskId.ToString());
				LogHelper.Instance.Log(string.Format("Added {0} header where KioskId = '{1}'", "x-redbox-kioskid", service.KioskId));
			}
			else
			{
				LogHelper.Instance.Log("Could not add x-redbox-kioskid header; Store Manager is null.");
			}
			string sessionIdFromBody = GetSessionIdFromBody(jsonRequest);
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

	private string GetSessionIdFromBody(string body)
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
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		if (service == null)
		{
			return;
		}
		Uri uri = null;
		string kioskDataServicesUrl = service.KioskDataServicesUrl;
		string kioskDataServicesKey = service.KioskDataServicesKey;
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
