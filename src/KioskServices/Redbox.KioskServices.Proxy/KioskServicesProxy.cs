using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
#if NETFRAMEWORK
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
#else
using System.Threading.Tasks;
using Redbox.Remoting.Nrbf;
#endif
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

	// Placeholder (RFC 2606 .invalid, never resolves): the real URL comes from the registry value, which the backend's
	// kiosk profile (rb-reprovision) sets. Redbox's default was https://bl2.accessredbox.net/KioskServices/MessageService.rem.
	internal const string DefaultKioskServicesUrl = "https://kioskservices.kiosk-backend.invalid/KioskServices/MessageService.rem";

#if NETFRAMEWORK
	private HttpChannel m_channel;
#endif

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
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesURL", DefaultKioskServicesUrl);
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
			IObfuscationService obfuscationService = ServiceLocator.Instance.GetService<IObfuscationService>();
			// A copy of the request with card data, opt-in contacts and images masked, for the log only; the request
			// itself is sent as it is.
			Dictionary<string, object> logRequest = jsonRequest.ToObject<Dictionary<string, object>>();
			obfuscationService?.ObfuscateDictionary(logRequest);
			if (logRequest.ContainsKey("OptInItems") && logRequest["OptInItems"] != null)
			{
				foreach (object item in logRequest["OptInItems"] as ArrayList)
				{
					Dictionary<string, object> optInItem = item as Dictionary<string, object>;
					if (optInItem.ContainsKey("OptInMethodData") && optInItem["OptInMethodData"] != null)
					{
						optInItem["OptInMethodData"] = obfuscationService?.ObfuscateMessageData(optInItem["OptInMethodData"].ToString(), 'x', 1, 0);
					}
				}
			}
			if (logRequest.ContainsKey("CreditCard"))
			{
				Dictionary<string, object> creditCard = (Dictionary<string, object>)logRequest["CreditCard"];
				obfuscationService?.ObfuscateDictionary(creditCard);
				if (creditCard.ContainsKey("EmvTags") && creditCard["EmvTags"] != null)
				{
					IDictionary<string, object> emvTags = (IDictionary<string, object>)creditCard["EmvTags"];
					Dictionary<string, object> maskedTags = new Dictionary<string, object>();
					foreach (KeyValuePair<string, object> tag in emvTags)
					{
						maskedTags.Add(tag.Key, new string('x', ((string)tag.Value).Length));
					}
					creditCard["EmvTags"] = maskedTags;
				}
			}
			if (logRequest.ContainsKey("Image"))
			{
				ArrayList image = logRequest["Image"] as ArrayList;
				List<object> imageItems = image?.ToArray().ToList();
				string imageStart = ((image != null) ? string.Join(string.Empty, imageItems.Take(Math.Min(10, image.Count))) : string.Empty);
				logRequest["Image"] = $"{imageStart} ... {imageItems?.Count ?? 0}";
			}
			string logJson = logRequest.ToJson();
			string prettyLogJson = logJson.ToPrettyJson();
			LogHelper.Instance.Log("KioskServicesProxy JSON request: {0}", (prettyLogJson.Length <= 10000) ? prettyLogJson : logJson);
			string jsonResponse = PerformRemoteMessage(jsonRequest);
			Dictionary<string, object> logResponse = jsonResponse.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
			obfuscationService?.ObfuscateDictionary(logResponse);
			if (logResponse.ContainsKey("CustomerProfile"))
			{
				Dictionary<string, object> customerProfile = (Dictionary<string, object>)logResponse["CustomerProfile"];
				obfuscationService?.ObfuscateDictionary(customerProfile);
			}
			string logResponseJson = logResponse.ToJson();
			LogHelper.Instance.Log($"KioskServicesProxy JSON response: {logResponseJson.ToPrettyJson()}");
			LogHelper.Instance.Log("KioskServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(jsonResponse))
			{
				RaiseCommunicationActivity(true);
				return jsonResponse.ToDictionary();
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
				RaiseCommunicationActivity(false);
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
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		dictionary["Device Service"] = deviceServiceClientHelper?.DeviceServiceVersion?.ToString() ?? "Not Found";
		dictionary["Device Service Client"] = deviceServiceClientHelper.DeviceServiceClientVersion?.ToString() ?? "Not Found";
		IKernelExtensionService kernelExtensionService = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		if (kernelExtensionService != null)
		{
			foreach (IKernelExtension extension in kernelExtensionService.Extensions)
			{
				dictionary[extension.Name] = extension.Version;
			}
		}
		IResourceBundleService resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (resourceBundleService != null)
		{
			foreach (IBundleSpecifier bundle in resourceBundleService.Bundles)
			{
				dictionary[bundle.Name] = bundle.Version.ToString();
			}
		}
		return dictionary;
	}

	// Raised asynchronously, as the original BeginInvoke did (delegate BeginInvoke does not exist on .NET).
	private void RaiseCommunicationActivity(bool success)
	{
#if NETFRAMEWORK
		CommunicationActivity?.BeginInvoke(success, EventArgs.Empty, null, null);
#else
		EventHandler handler = CommunicationActivity;
		if (handler != null)
		{
			Task.Run(() => handler(success, EventArgs.Empty));
		}
#endif
	}

	internal void RegisterChannel()
	{
#if !NETFRAMEWORK
		// .NET has no .NET Remoting channels; Remoting-mode calls go through RemotingHttpClient in PerformRemoteMessage.
	}
#else
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
#endif

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
			// Content-Length is the UTF-8 byte count (it was the string length, which cut off any non-ASCII body).
			byte[] body = Encoding.UTF8.GetBytes($"payload={arg}");
			WebRequest webRequest = WebRequest.Create(Url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.ContentLength = body.Length;
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			using (Stream requestStream = webRequest.GetRequestStream())
			{
				requestStream.Write(body, 0, body.Length);
			}
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
		}
#if !NETFRAMEWORK
		// The same binary .NET Remoting call over HTTP that HttpChannel made, without .NET Remoting (Redbox.Remoting.Nrbf).
		LogHelper.Instance.Log("...Making remote call using .NET Remoting over HTTP (binary formatter).");
		return (string)RemotingHttpClient.Invoke(Url, typeof(IMessageService).AssemblyQualifiedName, "Process", new object[] { jsonRequest }, TimeSpan.FromMilliseconds(Timeout));
#else
		IMessageService messageService = (IMessageService)Activator.GetObject(typeof(IMessageService), Url);
		if (messageService == null)
		{
			LogHelper.Instance.Log("...KioskServicesProxy.PerformRemoteMessage unable to connect.", LogEntryType.Error);
			return null;
		}
		LogHelper.Instance.Log("...Making remote call using .NET Remoting channel.");
		return messageService.Process(jsonRequest);
#endif
	}

	private void UnregisterChannel()
	{
#if !NETFRAMEWORK
	}
#else
		if (m_channel != null || Protocol == KioskServicesProtocol.Remoting)
		{
			ChannelServices.UnregisterChannel(m_channel);
			m_channel = null;
		}
	}
#endif
}
