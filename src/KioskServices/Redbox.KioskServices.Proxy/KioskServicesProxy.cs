using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Text;
using System.Web;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.ComponentModel;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.Proxy;

public class KioskServicesProxy
{
	internal const int DefaultKioskServicesTimeout = 30000;

	internal const string DefaultKioskServicesUrl = "https://bl2.accessredbox.net/KioskServices/MessageService.rem";

	private HttpChannel m_channel;

	public static KioskServicesProxy Instance => Singleton<KioskServicesProxy>.Instance;

	public KioskServicesProtocol Protocol
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesProtocol", KioskServicesProtocol.Remoting);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesProtocol", value);
			RegisterChannel();
		}
	}

	public int Timeout
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesTimeout", 30000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesTimeout", value);
			RegisterChannel();
		}
	}

	public string Url
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesURL", "https://bl2.accessredbox.net/KioskServices/MessageService.rem");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesURL", value);
		}
	}

	public event EventHandler CommunicationActivity;

	public IDictionary<string, object> ExecuteMessage(object message)
	{
		return ExecuteMessage(message.ToJson());
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
		if (action != null)
		{
			CommunicationActivity += delegate(object sender, EventArgs eventArgs)
			{
				action(sender, eventArgs);
			};
		}
	}

	public IDictionary<string, object> ExecuteMessage(string jsonRequest)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(jsonRequest))
		{
			return new Dictionary<string, object> { ["Success"] = false };
		}
		try
		{
			IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
			Dictionary<string, object> dictionary = jsonRequest.ToObject<Dictionary<string, object>>();
			service?.ObfuscateDictionary(dictionary);
			if (dictionary.ContainsKey("OptInItems") && dictionary["OptInItems"] != null)
			{
				foreach (object item in dictionary["OptInItems"] as ArrayList)
				{
					Dictionary<string, object> dictionary2 = item as Dictionary<string, object>;
					if (dictionary2.ContainsKey("OptInMethodData") && dictionary2["OptInMethodData"] != null)
					{
						dictionary2["OptInMethodData"] = service?.ObfuscateMessageData(dictionary2["OptInMethodData"].ToString(), 'x', 1, 0);
					}
				}
			}
			if (dictionary.ContainsKey("CreditCard"))
			{
				Dictionary<string, object> dictionary3 = (Dictionary<string, object>)dictionary["CreditCard"];
				service?.ObfuscateDictionary(dictionary3);
				if (dictionary3.ContainsKey("EmvTags") && dictionary3["EmvTags"] != null)
				{
					IDictionary<string, object> obj = (IDictionary<string, object>)dictionary3["EmvTags"];
					Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
					foreach (KeyValuePair<string, object> item2 in obj)
					{
						dictionary4.Add(item2.Key, new string('x', ((string)item2.Value).Length));
					}
					dictionary3["EmvTags"] = dictionary4;
				}
			}
			if (dictionary.ContainsKey("Image"))
			{
				ArrayList arrayList = dictionary["Image"] as ArrayList;
				List<object> list = arrayList?.ToArray().ToList();
				string arg = ((arrayList != null) ? string.Join(string.Empty, list.Take(Math.Min(10, arrayList.Count))) : string.Empty);
				dictionary["Image"] = $"{arg} ... {list?.Count ?? 0}";
			}
			string text = dictionary.ToJson();
			string text2 = text.ToPrettyJson();
			LogHelper.Instance.Log("KioskServicesProxy JSON request: {0}", (text2.Length <= 10000) ? text2 : text);
			string text3 = PerformRemoteMessage(jsonRequest);
			dictionary = text3.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
			service?.ObfuscateDictionary(dictionary);
			if (dictionary.ContainsKey("CustomerProfile"))
			{
				Dictionary<string, object> workDictionary = (Dictionary<string, object>)dictionary["CustomerProfile"];
				service?.ObfuscateDictionary(workDictionary);
			}
			string source = dictionary.ToJson();
			LogHelper.Instance.Log($"KioskServicesProxy JSON response: {source.ToPrettyJson()}");
			LogHelper.Instance.Log("KioskServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(text3))
			{
				CommunicationActivity?.BeginInvoke(true, EventArgs.Empty, null, null);
				return text3.ToDictionary();
			}
			return new Dictionary<string, object> { ["Success"] = false };
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesProxy.ExecuteMessage.", ex);
			Dictionary<string, object> result = new Dictionary<string, object>
			{
				["Success"] = false,
				["ExceptionRaised"] = true
			};
			if (ex is WebException { Response: null })
			{
				CommunicationActivity?.BeginInvoke(false, EventArgs.Empty, null, null);
			}
			return result;
		}
	}

	public Dictionary<string, string> GetCurrentVersions()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			dictionary["Kiosk Engine"] = service["EngineVersion"];
			dictionary["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			dictionary["Hal Client"] = service["HALVersion"] ?? "Not Found";
		}
		IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		dictionary["Device Service"] = service2?.DeviceServiceVersion?.ToString() ?? "Not Found";
		dictionary["Device Service Client"] = service2.DeviceServiceClientVersion?.ToString() ?? "Not Found";
		IKernelExtensionService service3 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		if (service3 != null)
		{
			foreach (IKernelExtension extension in service3.Extensions)
			{
				dictionary[extension.Name] = extension.Version;
			}
		}
		IResourceBundleService service4 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service4 != null)
		{
			foreach (IBundleSpecifier bundle in service4.Bundles)
			{
				dictionary[bundle.Name] = bundle.Version.ToString();
			}
		}
		return dictionary;
	}

	internal void RegisterChannel()
	{
		if (m_channel != null)
		{
			UnregisterChannel();
			LogHelper.Instance.Log("Unregistering remoting channel");
		}
		if (Protocol == KioskServicesProtocol.Remoting)
		{
			ListDictionary properties = new ListDictionary
			{
				{ "port", 0 },
				{ "timeout", Timeout }
			};
			LogHelper.Instance.Log("Registering remoting channel with timeout {0}", Timeout);
			m_channel = new HttpChannel(properties, new BinaryClientFormatterSinkProvider(), new BinaryServerFormatterSinkProvider());
			ChannelServices.RegisterChannel(m_channel, ensureSecurity: false);
		}
	}

	private KioskServicesProxy()
	{
		RegisterChannel();
	}

	private string PerformRemoteMessage(string jsonRequest)
	{
		LogHelper.Instance.Log("KioskServicesProxy remote URL: {0}", Url);
		if (Protocol == KioskServicesProtocol.REST)
		{
			string arg = HttpUtility.UrlEncode(jsonRequest);
			string text = $"payload={arg}";
			WebRequest webRequest = WebRequest.Create(Url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.ContentLength = text.Length;
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
			requestStream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
		}
		IMessageService messageService = (IMessageService)Activator.GetObject(typeof(IMessageService), Url);
		if (messageService == null)
		{
			LogHelper.Instance.Log("...KioskServicesProxy.PerformRemoteMessage unable to connect.", LogEntryType.Error);
			return null;
		}
		LogHelper.Instance.Log("...Making remote call using .NET Remoting channel.");
		return messageService.Process(jsonRequest);
	}

	private void UnregisterChannel()
	{
		if (m_channel != null || Protocol == KioskServicesProtocol.Remoting)
		{
			ChannelServices.UnregisterChannel(m_channel);
			m_channel = null;
		}
	}
}
