using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.KioskServices.Proxy;

public class HealthServicesProxy : IHealthServices
{
	private const int DefaultPingPeriod = 300000;

	private const string DefaultSmtpServer = "corpsmtp01.rb.local";

	private const string DefaultRecipients = "";

	private bool m_wasFirstPingSent;

	private Timer m_pingTimer;

	private readonly object m_syncLock = new object();

	private readonly Dictionary<string, string> m_statistics = new Dictionary<string, string>();

	private readonly Dictionary<string, string> m_properties = new Dictionary<string, string>();

	private int m_working;

	public static IHealthServices Instance => Singleton<HealthServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "LiveCall", value);
		}
	}

	public int PingPeriod
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "PingPeriod", 300000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "PingPeriod", value);
			StopPingTimer();
			StartPingTimer();
		}
	}

	public bool IsServerBusy
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "IsServerBusy", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "IsServerBusy", value);
		}
	}

	public string SmtpServer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SmtpServer", "corpsmtp01.rb.local");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SmtpServer", value);
		}
	}

	public string Recipients
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "Recipients", "");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "Recipients", value);
		}
	}

	public string RemoteServicesStatus
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "RemoteServicesStatus", "Normal");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "RemoteServicesStatus", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		bool num = m_working == 0;
		if (num)
		{
			ResetMinutesInMaintenanceMode();
		}
		return num;
	}

	public void Ping(string storeNumber)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		Dictionary<string, string> versions = CollectVersions();
		Dictionary<string, string> statistics;
		Dictionary<string, string> properties;
		lock (m_syncLock)
		{
			statistics = m_statistics.ShallowCopy();
			m_statistics.Clear();
			properties = m_properties.ShallowCopy();
			m_properties.Clear();
		}
		ThreadPool.QueueUserWorkItem(async delegate
		{
			WorkingIncrement();
			try
			{
				if (LiveCall)
				{
					LogHelper.Instance.Log($"Live call for KioskClientServiceHealthListener.Ping, original message id: {messageId}");
					await Task.WhenAll(SendKioskVersions(kioskId, versions), SendKioskStatistics(kioskId, statistics), SendKioskProperties(kioskId, properties));
					UpdateFirstPingSent();
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to KioskClientServiceHealthListener.Ping", LogEntryType.Info);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.Ping.", e);
			}
			finally
			{
				WorkingDecrement();
			}
		});
	}

	private async Task SendKioskProperties(int kioskId, IDictionary<string, string> properties)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskPropertiesToKioskDataService)
		{
			if (properties.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskProperties(kioskId, properties);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskProperties() - bypassing live call due to no properties information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskProperties() - bypassing live call due to configuration flag being off");
		}
	}

	private async Task SendKioskVersions(int kioskId, IDictionary<string, string> versions)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskVersionsToKioskDataService)
		{
			if (versions.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskVersions(kioskId, versions);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskVersions() - bypassing live call due to no version information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskVersions() - bypassing live call due to configuration flag being off");
		}
	}

	private async Task SendKioskStatistics(int kioskId, IDictionary<string, string> statistics)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskStatisticsToKioskDataService)
		{
			if (statistics.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskStatistics(kioskId, statistics);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskStatistics() - bypassing live call due to no statistics information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskStatistics() - bypassing live call due to configuration flag being off");
		}
	}

	public void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action = null, bool skipQueue = false)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskAlertsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				Redbox.KioskEngine.ComponentModel.ErrorList errors = null;
				try
				{
					WorkingIncrement();
					if (LiveCall)
					{
						LogHelper.Instance.Log($"Enqueue call to KioskClientService.SendAlert, message id: {messageId}");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskAlert(messageId, kioskId, alertType.ToAlertTypeNumeric(), subType, message, dateTime);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.SendAlert successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.SendAlert failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskClientService.SendAlert", LogEntryType.Info);
						SendEmail($"Test Alert: {alertType}, Kiosk ID: {kioskId}, Date Time: {dateTime}", message);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServicesHealthListener.SendAlert.", e);
				}
				finally
				{
					try
					{
						LogHelper.Instance.Log("Invoking action from KioskClientServicesHealthListener.SendAlert");
						action?.Invoke();
						if (callback != null)
						{
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
									{
										remoteServiceResult.AddError(item.Code, item.Description, item.Details);
									}
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
								{
									Callback = callback,
									Result = remoteServiceResult,
									Message = "Invoking callback from KioskServicesHealthListener.SendAlert."
								});
							}
						}
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendAlert - bypassing kiosk alert to cloud due to configuration flag being off");
		}
	}

	public void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendAuditEventsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				WorkingIncrement();
				Redbox.KioskEngine.ComponentModel.ErrorList errors = null;
				try
				{
					if (LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to remote KioskClientServiceHealthListener.SendAuditEvent endpoint");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.AuditEvent(messageId, kioskId, eventType, logDate, userName, message, source);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.AuditEvent successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.AuditEvent failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskClientServiceHealthListener.SendAuditEvent", LogEntryType.Info);
						SendEmail($"Test Audit Event: {eventType}, Kiosk ID: {kioskId}, Date Time: {logDate}, User Name: {userName}, Source: {source}", message);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.SendAuditEvent.", e);
				}
				finally
				{
					try
					{
						if (callback != null)
						{
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									remoteServiceResult.Errors.AddRange(errors);
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
								{
									Callback = callback,
									Result = remoteServiceResult,
									Message = "Invoking callback from KioskClientServiceHealthListener.SendAuditEvent."
								});
							}
						}
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendAuditEvent - bypassing kiosk audit event to cloud due to configuration flag being off");
		}
	}

	public void SendKioskFunctionCheck(string storeNumber, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskFunctionChecksToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				Redbox.KioskEngine.ComponentModel.ErrorList errors = null;
				try
				{
					WorkingIncrement();
					if (LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to KioskServicesHealthListener.SendKioskFunctionCheck");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskFunctionCheck(messageId, kioskId, userName, reportTime, verticalSlot, initTest, vendDoor, trackTest, snapDecode, touchScreenDriver, cameraDriver);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.KioskFunctionCheck successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.KioskFunctionCheck failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskServicesHealthListener.SendKioskFunctionCheck", LogEntryType.Info);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesHealthListener.SendKioskFunctionCheck!", e);
				}
				finally
				{
					try
					{
						if (callback != null)
						{
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									remoteServiceResult.Errors.AddRange(errors);
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
								{
									Callback = callback,
									Result = remoteServiceResult,
									Message = "Invoking callback from KioskServicesHealthListener.SendKioskFunctionCheck."
								});
							}
							else
							{
								dotNetCallBack?.Invoke();
							}
						}
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendKioskFunctionCheck - bypassing kiosk events to cloud due to configuration flag being off");
		}
	}

	public void SendKioskEvents(long kioskId, List<KioskEvent> events)
	{
		Guid messageId = Guid.NewGuid();
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskEventsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				WorkingIncrement();
				try
				{
					LogHelper.Instance.Log("Enqueue call to remote KioskClientServiceHealthListener.SendKioskEvents");
					BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskEvents(messageId, kioskId, DateTime.Now, events);
					if (baseResponse.Success)
					{
						LogHelper.Instance.Log("Call to KioskClientService.KioskEvents successful");
					}
					else
					{
						LogHelper.Instance.Log($"Call to KioskClientService.KioskEvents failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.SendKioskEvents.", e);
				}
				finally
				{
					WorkingDecrement();
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendKioskEvents - bypassing kiosk events to cloud due to configuration flag being off");
		}
	}

	public void StopPingTimer()
	{
		if (m_pingTimer != null)
		{
			m_pingTimer.Dispose();
			m_pingTimer = null;
		}
	}

	public void StartPingTimer()
	{
		if (m_pingTimer != null)
		{
			return;
		}
		m_pingTimer = new Timer(delegate
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			try
			{
				if (m_pingTimer != null)
				{
					m_pingTimer.Change(-1, -1);
				}
				Ping(service.GetValue<string>("Store", "ID"));
			}
			finally
			{
				if (m_pingTimer != null)
				{
					m_pingTimer.Change(PingPeriod, PingPeriod);
				}
			}
		}, null, 0, PingPeriod);
	}

	public void EnqueueStatistic(string name, string value)
	{
		lock (m_syncLock)
		{
			if (m_statistics.ContainsKey(name))
			{
				m_statistics[name] = value;
			}
			else
			{
				m_statistics.Add(name, value);
			}
		}
	}

	public void EnqueueProperty(string name, string value)
	{
		lock (m_syncLock)
		{
			m_properties.Add(name, value);
		}
	}

	public void SendEmail(string subject, string message)
	{
		if (!string.IsNullOrEmpty(Recipients))
		{
			MailMessage message2 = new MailMessage("deddie@redbox.com", Recipients, subject, message);
			new SmtpClient(SmtpServer).Send(message2);
		}
		else
		{
			LogHelper.Instance.Log("> recipient list for simulated SendAlert was empty, email was not sent.", LogEntryType.Info);
		}
	}

	public void UpdateOnlineStatus(IDictionary<string, object> response)
	{
		bool flag = response.ContainsKey("ServerBusy") && (bool)response["ServerBusy"];
		IsServerBusy = flag;
		RemoteServicesStatus = (flag ? "Busy" : "Normal");
	}

	public void UpdateOnlineStatus(bool isServerBusy)
	{
		IsServerBusy = IsServerBusy;
		RemoteServicesStatus = (isServerBusy ? "Busy" : "Normal");
	}

	internal void UpdateFirstPingSent()
	{
		m_wasFirstPingSent = true;
	}

	private HealthServicesProxy()
	{
	}

	public void ResetMinutesInMaintenanceMode()
	{
		string lastShownView = ServiceLocator.Instance.GetService<IViewService>().GetLastShownView();
		if (!string.IsNullOrEmpty(lastShownView) && lastShownView.Equals("maintenance_view"))
		{
			EnqueueStatistic("MinutesInMaintenance", "0");
		}
	}

	private Dictionary<string, string> CollectVersions()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!m_wasFirstPingSent)
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			dictionary["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			dictionary["Hal Client"] = service["HALVersion"] ?? "Not Found";
			dictionary["Kiosk Engine"] = service["EngineVersion"];
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			dictionary["Device Service"] = service2?.DeviceServiceVersion?.ToString() ?? "Not Found";
			dictionary["Device Service Client"] = service2?.DeviceServiceClientVersion?.ToString() ?? "Not Found";
			foreach (IBundleSpecifier bundle in ServiceLocator.Instance.GetService<IResourceBundleService>().Bundles)
			{
				dictionary[bundle.Name] = bundle.Version.ToString();
			}
		}
		return dictionary;
	}

	private int VerifyStoreNumber(string number)
	{
		if (!string.IsNullOrEmpty(number) && string.Compare(number, "unconfigured", ignoreCase: true) == 0)
		{
			IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
			if (service != null)
			{
				number = service.GetObject<string>("NullKioskId") ?? "1";
			}
		}
		return int.Parse(number);
	}

	private void WorkingDecrement()
	{
		Interlocked.Decrement(ref m_working);
	}

	private void WorkingIncrement()
	{
		Interlocked.Increment(ref m_working);
	}
}
