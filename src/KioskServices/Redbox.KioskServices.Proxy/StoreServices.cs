using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Installer;

namespace Redbox.KioskServices.Proxy;

internal class StoreServices : Redbox.KioskServices.Proxy.ComponentModel.IStoreServices
{
	protected sealed class State : Redbox.KioskServices.Proxy.ComponentModel.IState
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	protected sealed class Banner : Redbox.KioskServices.Proxy.ComponentModel.IBanner
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	protected sealed class Store : Redbox.KioskServices.Proxy.ComponentModel.IStore
	{
		public int Id { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string ZipCode { get; set; }

		public string DueTime { get; set; }

		public string Address { get; set; }

		public string MarketName { get; set; }

		public string KaseyaMarketName { get; set; }
	}

	protected int m_working;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "LiveCall", value);
		}
	}

	public bool RunInitialSync
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "RunInitialSync", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "RunInitialSync", value);
		}
	}

	public bool IsStandAlone
	{
		get
		{
			return ServiceLocator.Instance.GetService<IApplicationState>().IsStandAlone;
		}
		set
		{
			ServiceLocator.Instance.GetService<IApplicationState>().IsStandAlone = value;
		}
	}

	public string StoreNumberString
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().StoreNumberString();
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "ID", value);
		}
	}

	public int StoreNumberInteger
	{
		get
		{
			if (!int.TryParse(ServiceLocator.Instance.GetService<IMachineSettingsStore>().StoreNumberString(), out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public string Environment
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Store", "Environment", null);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "Environment", value);
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				service.Filter["environment"] = value;
			}
		}
	}

	public string Application
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Store", "Application", null);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "Application", value);
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				service.Filter["application"] = value;
			}
		}
	}

	public string RebootTime
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			string text = service.GetValue<string>("Store", "RebootTime", null);
			if (string.IsNullOrEmpty(text))
			{
				string s = IdHelper.GeneratePseudoUniqueId(1, "12345");
				string s2 = IdHelper.GeneratePseudoUniqueId(2, "012345");
				text = $"{int.Parse(s):00}:{int.Parse(s2):00}";
				service.SetValue("Store", "RebootTime", text);
			}
			return text;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "RebootTime", value);
		}
	}

	public string[] PendingStates
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingStates", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingStates", value);
		}
	}

	public string[] PendingBanners
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingBanners", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingBanners", value);
		}
	}

	public string[] PendingStores
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingStores", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingStores", value);
		}
	}

	public string InitialSyncType
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "InitialSyncType", "Foreground");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "InitialSyncType", value);
		}
	}

	public bool SynchronizeTimeToServer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "SynchronizeTimeToServer", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "SynchronizeTimeToServer", value);
		}
	}

	private bool IsServicesOnline
	{
		get
		{
			IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
			if (service != null && !service.IsServerBusy)
			{
				return !IsStandAlone;
			}
			return false;
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public string GetEnvironment()
	{
		return Environment;
	}

	public string GetApplication()
	{
		return Application;
	}

	public DateTime? GetRebootTime()
	{
		if (!DateTime.TryParse(RebootTime, out var result))
		{
			return null;
		}
		return result;
	}

	public bool ShouldRunInitialSync()
	{
		return RunInitialSync;
	}

	public string GetInitialSyncType()
	{
		return InitialSyncType;
	}

	public void SetRunInitialSync(bool flag)
	{
		RunInitialSync = flag;
	}

	public void GetPendingStates(RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInstallerApisToKioskClientService)
		{
			KCSGetPendingStates(completeCallback);
		}
		else
		{
			KSGetPendingStates(completeCallback);
		}
	}

	public void GetPendingBanners(int stateId, RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInstallerApisToKioskClientService)
		{
			KCSGetPendingBanners(stateId, completeCallback);
		}
		else
		{
			KSGetPendingBanners(stateId, completeCallback);
		}
	}

	public void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInstallerApisToKioskClientService)
		{
			KCSGetPendingStores(bannerId, stateId, completeCallback);
		}
		else
		{
			KSGetPendingStores(bannerId, stateId, completeCallback);
		}
	}

	public void GetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInstallerApisToKioskClientService)
		{
			KCSGetOpsMarket(storeNumber, completeCallback);
		}
		else
		{
			KSGetOpsMarket(storeNumber, completeCallback);
		}
	}

	internal StoreServices()
	{
	}

	private void KCSGetPendingStates(RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["states"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IState>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
					LogHelper.Instance.Log("Live call to KioskClientService GetPendingStates, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
					PendingStatesResponse pendingStates = service.Installer.GetPendingStates();
					if (pendingStates == null)
					{
						LogHelper.Instance.Log("ProxyService GetPendingStates api returned a null response");
						remoteServiceResult.Success = false;
					}
					else
					{
						remoteServiceResult.Success = pendingStates.Success;
						List<Redbox.KioskServices.Proxy.ComponentModel.IState> list = remoteServiceResult.Properties["states"] as List<Redbox.KioskServices.Proxy.ComponentModel.IState>;
						pendingStates.States.ForEach(delegate(Redbox.Rental.Model.KioskClientService.Installer.State s)
						{
							list.Add(new State
							{
								Id = s.Id,
								Name = s.Description
							});
						});
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ProxyServiceStoreServices.GetPendingStates.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ProxyServiceStoreServices.GetPendingStates.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from ProxyServiceStoreServices.GetPendingStates."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KSGetPendingStates(RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["states"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IState>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (IsServicesOnline)
					{
						LogHelper.Instance.Log("Live call to KioskServices GetPendingStates, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingStates",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<Redbox.KioskServices.Proxy.ComponentModel.IState> list = new List<Redbox.KioskServices.Proxy.ComponentModel.IState>();
						if (dictionary.ContainsKey("States"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["States"])
							{
								list.Add(new State
								{
									Id = (int)item["Id"],
									Name = (item.ContainsKey("Description") ? ((string)item["Description"]) : string.Empty)
								});
							}
						}
						remoteServiceResult.Properties["states"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingStates.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingStates.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingStates.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingStates."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KCSGetPendingBanners(int stateId, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["banners"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IBanner>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
					LogHelper.Instance.Log("Live call to KioskClientService GetPendingBanners, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
					PendingBannersResponse pendingBannersForState = service.Installer.GetPendingBannersForState(stateId);
					if (pendingBannersForState == null)
					{
						LogHelper.Instance.Log("ProxyService GetPendingBanners api returned a null response");
						remoteServiceResult.Success = false;
					}
					else
					{
						remoteServiceResult.Success = pendingBannersForState.Success;
						List<Redbox.KioskServices.Proxy.ComponentModel.IBanner> list = remoteServiceResult.Properties["banners"] as List<Redbox.KioskServices.Proxy.ComponentModel.IBanner>;
						pendingBannersForState.Banners.ForEach(delegate(Redbox.Rental.Model.KioskClientService.Installer.Banner b)
						{
							list.Add(new Banner
							{
								Id = b.Id,
								Name = b.Name
							});
						});
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingBanners.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingBanners.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingBanners."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KSGetPendingBanners(int stateId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["banners"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IBanner>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (IsServicesOnline)
					{
						LogHelper.Instance.Log("Live call to KioskServices GetPendingBanners, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingBanners",
							MessageId = Guid.NewGuid(),
							StateId = stateId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<Redbox.KioskServices.Proxy.ComponentModel.IBanner> list = new List<Redbox.KioskServices.Proxy.ComponentModel.IBanner>();
						if (dictionary.ContainsKey("Banners"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["Banners"])
							{
								list.Add(new Banner
								{
									Id = (int)item["Id"],
									Name = (string)item["Name"]
								});
							}
						}
						remoteServiceResult.Properties["banners"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingBanners.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingBanners.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingBanners.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingBanners."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KCSGetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["stores"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IStore>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
					LogHelper.Instance.Log("Live call to KioskClientService GetPendingStores, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
					PendingKiosksResponse pendingKiosksForStateAndBanner = service.Installer.GetPendingKiosksForStateAndBanner(stateId, bannerId);
					if (pendingKiosksForStateAndBanner == null)
					{
						LogHelper.Instance.Log("ProxyService GetPendingKiosksForStateAndBanner api returned a null response");
						remoteServiceResult.Success = false;
					}
					else
					{
						remoteServiceResult.Success = pendingKiosksForStateAndBanner.Success;
						List<Redbox.KioskServices.Proxy.ComponentModel.IStore> list = remoteServiceResult.Properties["stores"] as List<Redbox.KioskServices.Proxy.ComponentModel.IStore>;
						pendingKiosksForStateAndBanner.PendingKiosks.ForEach(delegate(PendingKiosk k)
						{
							list.Add(new Store
							{
								Id = k.Id,
								City = k.City,
								State = k.State,
								ZipCode = k.ZipCode,
								DueTime = k.DueTime,
								Address = k.Address,
								MarketName = k.MarketName,
								KaseyaMarketName = k.KaseyaMarketName
							});
						});
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingStores.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingStores.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingStores."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KSGetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["stores"] = new List<Redbox.KioskServices.Proxy.ComponentModel.IStore>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (IsServicesOnline)
					{
						LogHelper.Instance.Log("Live call to KioskServices GetPendingStores, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingKiosks",
							MessageId = Guid.NewGuid(),
							BannerId = bannerId,
							StateId = stateId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<Redbox.KioskServices.Proxy.ComponentModel.IStore> list = new List<Redbox.KioskServices.Proxy.ComponentModel.IStore>();
						if (dictionary.ContainsKey("PendingKiosks"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["PendingKiosks"])
							{
								list.Add(new Store
								{
									Id = (int)item["Id"],
									City = (string)item["City"],
									State = (string)item["State"],
									ZipCode = (string)item["ZipCode"],
									DueTime = (string)item["DueTime"],
									Address = (string)item["Address"],
									MarketName = (string)item["MarketName"],
									KaseyaMarketName = (string)item["KaseyaMarketName"]
								});
							}
						}
						remoteServiceResult.Properties["stores"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingStores.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingStores.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingStores.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingStores."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KCSGetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
					LogHelper.Instance.Log("Live call to KioskClientService GetOpsMarket, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
					OpsMarketResponse opsMarketForKiosk = service.Installer.GetOpsMarketForKiosk(int.Parse(storeNumber));
					remoteServiceResult.Success = opsMarketForKiosk.Success;
					remoteServiceResult.Properties["is_online"] = true;
					remoteServiceResult.Properties["ops_market"] = opsMarketForKiosk.OpsMarket;
					remoteServiceResult.Properties["kaseya_server"] = opsMarketForKiosk.KaseyaServer;
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetOpsMarket.", LogEntryType.Info);
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["is_online"] = false;
					remoteServiceResult.Properties["ops_market"] = "UnNamed";
					remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetOpsMarket.", e);
				remoteServiceResult.Success = false;
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["ops_market"] = "UnNamed";
				remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetOpsMarket."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KSGetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (IsServicesOnline)
					{
						LogHelper.Instance.Log("Live call to KioskServices GetOpsMarket, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetOpsMarket",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(storeNumber)
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["ops_market"] = (dictionary.ContainsKey("OpsMarket") ? dictionary["OpsMarket"] : "UnNamed");
						remoteServiceResult.Properties["kaseya_server"] = (dictionary.ContainsKey("KaseyaServer") ? dictionary["KaseyaServer"] : "Kaseya6");
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetOpsMarket.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["ops_market"] = "UnNamed";
						remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetOpsMarket.", LogEntryType.Info);
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["is_online"] = false;
					remoteServiceResult.Properties["ops_market"] = "UnNamed";
					remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetOpsMarket.", e);
				remoteServiceResult.Success = false;
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["ops_market"] = "UnNamed";
				remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetOpsMarket."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}
}
