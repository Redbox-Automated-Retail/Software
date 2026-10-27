using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
	private readonly LogHelper _logger;

	private readonly IConfigurationService _configuration;

	private readonly IEngineApplication _engine;

	private IStoreManager _storeManager;

	private IMacroService _macroService;

	private IResourceBundleService _bundleService;

	private readonly string _analyticsSessionsPath = "AnalyticsSessions";

	private readonly IDictionary<string, IAnalyticsSession> _analyticsSessions = new Dictionary<string, IAnalyticsSession>();

	private List<IAnalyticsEvent> _queuedAnalyticsEvents = new List<IAnalyticsEvent>();

	public IStoreManager StoreManager
	{
		get
		{
			if (_storeManager == null)
			{
				_storeManager = ServiceLocator.Instance.GetService<IStoreManager>();
			}
			return _storeManager;
		}
	}

	public IMacroService MacroService
	{
		get
		{
			if (_macroService == null)
			{
				_macroService = ServiceLocator.Instance.GetService<IMacroService>();
			}
			return _macroService;
		}
	}

	public IResourceBundleService BundleService
	{
		get
		{
			if (_bundleService == null)
			{
				_bundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
			}
			return _bundleService;
		}
	}

	public IAnalyticsSession CurrentSession { get; set; }

	public List<IAnalyticsEvent> QueuedAnalyticsEvents => _queuedAnalyticsEvents;

	public AnalyticsService(LogHelper logger, IConfigurationService configuration, IEngineApplication engine)
	{
		_logger = logger;
		_configuration = configuration;
		_engine = engine;
		CleanupAnalyticsFiles();
	}

	public IAnalyticsSession GetSession(string id)
	{
		if (SessionExists(id))
		{
			return _analyticsSessions[id];
		}
		_logger.Log("AnalyticsService.GetSession(" + id + ") does not exist");
		return null;
	}

	public IAnalyticsSession Start(string id, AnalyticsConstants.SessionType sessionType)
	{
		try
		{
			_logger.Log("AnalyticsService.Start({0}) requested", id);
			IAnalyticsSession session = GetSession(id);
			if (session != null)
			{
				_logger.Log("Session " + id + " already exists");
				if (session != CurrentSession)
				{
					_logger.Log("AnalyticsService.Start(" + id + ") - a start session was requested that already exists and is not the current session", LogEntryType.Error);
				}
				return session;
			}
			CurrentSession = new AnalyticsSession(id, _logger, _configuration, StoreManager, MacroService, BundleService);
			_analyticsSessions.Add(id, CurrentSession);
			_logger.Log("AnalyticsService.Start({0}) started", id);
			CurrentSession.AddProperty("SessionType", sessionType.ToString());
			return CurrentSession;
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.Start.", e);
			return null;
		}
	}

	public void End(string sessionId, bool isAbandoned)
	{
		try
		{
			_logger.Log("AnalyticsService.End({0}) requested", sessionId);
			if (!SessionExists(sessionId))
			{
				_logger.Log("Session {0} doesn't exist", sessionId);
				return;
			}
			IAnalyticsSession analyticsSession = _analyticsSessions[sessionId];
			IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
			if (service != null)
			{
				if (service.IsADAAccessible)
				{
					analyticsSession.AddProperty("IsADA", service.IsADAAccessible);
				}
				if (service.IsABEOn)
				{
					analyticsSession.AddProperty("IsABE", service.IsABEOn);
				}
			}
			analyticsSession.End();
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.SendJsonKioskAnalyticsToKioskDataService)
			{
				WriteToFile(sessionId, ToJson(sessionId), "json");
				SendToKioskDataService(sessionId);
			}
			else
			{
				LogHelper.Instance.Log("Bypassing Kiosk Data Services message AddKioskSessionJson due to SendJsonKioskAnalyticsToKioskDataService being turned off.");
			}
			Remove(sessionId);
			if (CurrentSession != null && CurrentSession.SessionId == sessionId)
			{
				CurrentSession = null;
			}
			_logger.Log("AnalyticsService.End({0}) ended", sessionId);
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.End.", e);
		}
	}

	public void Remove(string sessionId)
	{
		try
		{
			if (SessionExists(sessionId))
			{
				_logger.Log("AnalyticsService.Remove: removing session {0}.", sessionId);
				_analyticsSessions.Remove(sessionId);
			}
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.Remove.", e);
		}
	}

	public IAnalyticsEvent AddView(string name)
	{
		return CurrentSession?.AddView(name);
	}

	public void AddQueuedViewEvents()
	{
		if (_queuedAnalyticsEvents == null)
		{
			return;
		}
		foreach (IAnalyticsEvent queuedAnalyticsEvent in _queuedAnalyticsEvents)
		{
			AddViewEvent(queuedAnalyticsEvent);
		}
		_queuedAnalyticsEvents.Clear();
	}

	public IAnalyticsEvent AddViewEvent(string action, string name)
	{
		return CurrentSession?.AddViewEvent(action, name);
	}

	public IAnalyticsData AddData(IAnalyticsData analyticsData)
	{
		return CurrentSession?.AddData(analyticsData);
	}

	public IAnalyticsSession AddSessionProperty(string name, object value)
	{
		if (CurrentSession != null)
		{
			CurrentSession.AddProperty(name, value);
		}
		return CurrentSession;
	}

	public IAnalyticsEvent AddEventProperty(string name, object value)
	{
		return CurrentSession?.AddEventProperty(name, value);
	}

	public IAnalyticsSession AddSessionProperty(string name, object value, string sessionId)
	{
		IAnalyticsSession session = GetSession(sessionId);
		session?.AddProperty(name, value);
		return session;
	}

	public IAnalyticsSession AddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId)
	{
		IAnalyticsSession session = GetSession(sessionId);
		return AddAuthTransactionInfo(session, customerProfileNumber, accountNumber, transactionId);
	}

	public IAnalyticsSession AddAuthTransactionInfo(IAnalyticsSession session, string customerProfileNumber, string accountNumber, string transactionId)
	{
		if (session == null)
		{
			session = CurrentSession;
		}
		if (session != null)
		{
			if (!string.IsNullOrWhiteSpace(customerProfileNumber))
			{
				AddSessionProperty("CustomerProfileNumber", customerProfileNumber);
			}
			if (!string.IsNullOrWhiteSpace(accountNumber))
			{
				AddSessionProperty("AccountNumber", accountNumber);
			}
			if (!string.IsNullOrWhiteSpace(transactionId))
			{
				AddSessionProperty("TransactionId", transactionId);
			}
		}
		return session;
	}

	public IAnalyticsSession AddROPFlowFalse()
	{
		CurrentSession?.AddData(new ROPRecommendedProducts
		{
			RecommendationsFlow = false
		});
		return CurrentSession;
	}

	public AnalyticsShoppingCart AddShoppingCart(IRentalShoppingCart sc)
	{
		return CurrentSession?.AddShoppingCart(sc);
	}

	public IAnalyticsEvent AddViewEvent(IAnalyticsEvent evt)
	{
		try
		{
			return CurrentSession?.AddViewEvent(evt);
		}
		catch (Exception e)
		{
			_logger.Log("AnalyticsService.AddViewEvent - An unhandled exception was raised in .", e);
			return null;
		}
	}

	public IAnalyticsEvent AddOrQueueViewEvent(IAnalyticsEvent evt)
	{
		if (CurrentSession != null)
		{
			return AddViewEvent(evt);
		}
		_queuedAnalyticsEvents.Add(evt);
		return evt;
	}

	public IAnalyticsEvent AddButtonPressEvent(string name, string eventSubType = null)
	{
		try
		{
			return CurrentSession?.AddButtonPressEvent(name, eventSubType);
		}
		catch (Exception e)
		{
			_logger.Log("AnalyticsService.AddButtonPressEvent - An unhandled exception was raised", e);
			return null;
		}
	}

	public IAnalyticsEvent AddCheckBoxEvent(string name, bool isChecked)
	{
		try
		{
			return CurrentSession?.AddCheckBoxEvent(name, isChecked);
		}
		catch (Exception e)
		{
			_logger.Log("AnalyticsService.AddCheckBoxEvent - An unhandled exception was raised", e);
			return null;
		}
	}

	public IAnalyticsEvent AddTimeoutEvent(int timeOut)
	{
		return AddTimeoutEvent(ServiceLocator.Instance.GetService<IViewService>()?.GetCurrentViewAnalyticsName(), timeOut);
	}

	public IAnalyticsEvent AddTimeoutEvent(string viewName, int timeOut)
	{
		try
		{
			return AddViewEvent(new TimeoutEvent(viewName, timeOut, ServiceLocator.Instance.GetService<ITextToSpeechService>()?.TTSEnabled ?? false));
		}
		catch (Exception e)
		{
			_logger.Log("AnalyticsService.AddTimeoutEvent - An unhandled exception was raised", e);
			return null;
		}
	}

	public IAnalyticsSession AddCurrentCulture(string culture)
	{
		if (CurrentSession == null || string.IsNullOrEmpty(culture))
		{
			return null;
		}
		return CurrentSession.AddProperty("CurrentCulture", culture);
	}

	public void WriteToFile(string sessionId, string contents, string extension)
	{
		try
		{
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				string text = Path.Combine(service.DataPath, _analyticsSessionsPath);
				if (!Directory.Exists(text))
				{
					_logger.Log("AnalyticsService.WriteToFile: {0} not found, creating directory.", text);
					Directory.CreateDirectory(text);
				}
				string text2 = Path.Combine(text, sessionId + "." + extension);
				_logger.Log("AnalyticsService.WriteToFile: writing file {0}.", text2);
				File.WriteAllText(text2, contents);
			}
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.WriteToFile.", e);
		}
	}

	public void SendToKioskDataService(string sessionId)
	{
		try
		{
			_logger.Log("AnalyticsService.SendToKioskDataService({0}) requested", sessionId);
			if (!SessionExists(sessionId))
			{
				_logger.Log("Session {0} doesn't exist", sessionId);
				return;
			}
			IAnalyticsSession analyticsSession = _analyticsSessions[sessionId];
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			ServiceLocator.Instance.GetService<IQueueService>()?.Enqueue((byte)Redbox.Rental.Services.Configuration.Configuration.Instance.KioskAnalyticsMessagePriority, "AddKioskSessionJson", new AddKioskSessionRequest
			{
				KioskId = (int)(service?.KioskId ?? 0),
				SessionId = sessionId,
				StartTime = analyticsSession.StartTime.ToString("yyyy/MM/dd HH:mm:ss"),
				EndTime = analyticsSession.EndTime.ToString("yyyy/MM/dd HH:mm:ss"),
				IsUnCompressed = false,
				SessionData = ToJson(sessionId, Formatting.None).CompressString()
			});
			_logger.Log("AnalyticsService.SendToKioskDataService({0}) completed", sessionId);
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.SendToKioskDataService.", e);
		}
	}

	public bool SessionExists(string sessionId)
	{
		if (sessionId == null)
		{
			return false;
		}
		return _analyticsSessions.ContainsKey(sessionId);
	}

	private string ToJson(string sessionId, Formatting formatting = Formatting.Indented)
	{
		try
		{
			if (SessionExists(sessionId))
			{
				JsonSerializerSettings settings = new JsonSerializerSettings
				{
					DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFF",
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = formatting
				};
				return JsonConvert.SerializeObject(_analyticsSessions[sessionId], settings);
			}
		}
		catch (Exception e)
		{
			_logger.Log("An unhandled exception was raised in AnalyticsService.ToJson.", e);
		}
		return string.Empty;
	}

	private void CleanupAnalyticsFiles()
	{
		Task.Run(delegate
		{
			try
			{
				_logger.Log("AnalyticsService.CleanupAnalyticsFiles task started");
				string path = Path.Combine(_engine.DataPath, _analyticsSessionsPath);
				if (Directory.Exists(path))
				{
					List<string> first = (from fp in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly)
						where File.GetCreationTime(fp) < DateTime.Now.AddDays(-7.0)
						select fp).ToList();
					List<string> second = (from fp in Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly)
						where File.GetCreationTime(fp) < DateTime.Now.AddDays(-7.0)
						select fp).ToList();
					foreach (string item in first.Union(second))
					{
						if (File.Exists(item))
						{
							_logger.Log("AnalyticsService.Initialize: deleting file {0}.", item);
							File.Delete(item);
						}
					}
				}
				_logger.Log("AnalyticsService.CleanupAnalyticsFiles task complete");
			}
			catch (Exception e)
			{
				_logger.Log("AnalyticsService.CleanupAnalyticsFiles - An unhandled exception was raised", e);
			}
		});
	}
}
