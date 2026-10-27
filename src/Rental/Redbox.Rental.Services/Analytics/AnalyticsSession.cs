using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.Analytics;

public class AnalyticsSession : IAnalyticsSession
{
	private readonly LogHelper _logger;

	private readonly IConfigurationService _configuration;

	private readonly IStoreManager _storeManager;

	private readonly IMacroService _macroService;

	private readonly IResourceBundleService _bundleService;

	public string SessionId { get; set; }

	public DateTime StartTime { get; set; }

	public DateTime EndTime { get; set; }

	public long KioskId { get; set; }

	public List<IAnalyticsEvent> Events { get; } = new List<IAnalyticsEvent>();

	public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

	public List<ABTest> ABTests { get; } = new List<ABTest>();

	public AnalyticsShoppingCart ShoppingCart { get; set; }

	private IAnalyticsEvent _currentViewEvent { get; set; }

	public AnalyticsSession(string sessionId, LogHelper logger, IConfigurationService configuration, IStoreManager storeManager, IMacroService macroService, IResourceBundleService bundleService)
	{
		StartTime = DateTime.Now;
		SessionId = sessionId;
		_logger = logger;
		_configuration = configuration;
		_storeManager = storeManager;
		_macroService = macroService;
		_bundleService = bundleService;
		Initialize();
	}

	public void End()
	{
		try
		{
			EndTime = DateTime.Now;
			LoadABTests();
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddView(string name)
	{
		try
		{
			_currentViewEvent = new ViewShownEvent(name);
			Events.Add(_currentViewEvent);
			return _currentViewEvent;
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddEventProperty(string name, object value)
	{
		try
		{
			_currentViewEvent?.AddProperty(name, value);
			return _currentViewEvent;
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddViewEvent(string action, string name)
	{
		try
		{
			return _currentViewEvent?.AddEvent(new ViewEventData(action, name));
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddViewEvent(IAnalyticsEvent evt)
	{
		try
		{
			return _currentViewEvent?.AddEvent(evt);
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddButtonPressEvent(string name, string eventSubType = null)
	{
		try
		{
			return AddViewEvent(new ButtonPressEvent(name, eventSubType));
		}
		finally
		{
		}
	}

	public IAnalyticsEvent AddCheckBoxEvent(string name, bool isChecked)
	{
		try
		{
			return AddViewEvent(new CheckBoxEvent(name, isChecked));
		}
		finally
		{
		}
	}

	public IAnalyticsData AddData(IAnalyticsData analyticsData)
	{
		try
		{
			return _currentViewEvent?.AddData(analyticsData);
		}
		finally
		{
		}
	}

	public IAnalyticsSession AddProperty(string name, object value)
	{
		try
		{
			if (value == null)
			{
				if (Properties.ContainsKey(name))
				{
					Properties.Remove(name);
				}
			}
			else
			{
				Properties[name] = value;
			}
			return this;
		}
		finally
		{
		}
	}

	public AnalyticsShoppingCart AddShoppingCart(IRentalShoppingCart sc)
	{
		try
		{
			AnalyticsShoppingCart analyticsShoppingCart = AnalyticsShoppingCart.ToShoppingCart(sc);
			if (analyticsShoppingCart.Items.Count > 0 || analyticsShoppingCart.SubscriptionItems.Count > 0)
			{
				ShoppingCart = analyticsShoppingCart;
			}
			return analyticsShoppingCart;
		}
		finally
		{
		}
	}

	private void Initialize()
	{
		try
		{
			KioskId = _storeManager.KioskId;
			Properties["Market"] = _storeManager.Market;
			Properties["Banner"] = _storeManager.Banner;
			Properties["DefaultCulture"] = _storeManager.GetDefaultCulture();
			if (_macroService["ProductName"] != null)
			{
				Properties["Application"] = _macroService["ProductName"];
			}
			if (_macroService["ProductVersion"] != null)
			{
				Properties["ApplicationVersion"] = _macroService["ProductVersion"];
			}
			if (_bundleService.Filter["culture"] != null)
			{
				Properties["CurrentCulture"] = _bundleService.Filter["culture"];
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("AnalyticsSession.Initialize - An unhandled exception was raised", e);
		}
		finally
		{
		}
	}

	private void LoadABTests()
	{
		try
		{
			string outOfStockTitleDetails = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.ABTesting.OutOfStockTitleDetails;
			string name = "OutOfStockTitleDetails";
			if (!string.IsNullOrEmpty(outOfStockTitleDetails) && outOfStockTitleDetails != "OFF")
			{
				ABTests.Add(new ABTest
				{
					Name = name,
					Value = outOfStockTitleDetails
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("AnalyticsSession.LoadABTests - An unhandled exception was raised", e);
		}
	}

	[Conditional("DEBUG")]
	private void WriteJsonFile()
	{
		try
		{
			if (!Redbox.Rental.Services.Configuration.Configuration.Instance.SendJsonKioskAnalyticsToKioskDataService)
			{
				return;
			}
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFF",
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			};
			string contents = JsonConvert.SerializeObject(this, settings);
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				string text = Path.Combine(service.DataPath, "AnalyticsSessions");
				if (!Directory.Exists(text))
				{
					_logger.Log("AnalyticsService.WriteJsonFile: {0} not found, creating directory.", text);
					Directory.CreateDirectory(text);
				}
				string text2 = Path.Combine(text, "Analytics.json");
				_logger.Log("AnalyticsService.WriteJsonFile: writing file {0}.", text2);
				File.WriteAllText(text2, contents);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("AnalyticsSession.WriteJsonFile - An unhandled exception was raised", e);
		}
	}
}
