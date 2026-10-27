using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Installer;

namespace Redbox.Rental.Services.StoreServices;

public class StoreServices : IStoreServices
{
	protected sealed class State : IState
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	protected sealed class Banner : IBanner
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	protected sealed class Store : IStore
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
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["states"] = new List<IState>();
			try
			{
				Interlocked.Increment(ref m_working);
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
					List<IState> list = remoteServiceResult.Properties["states"] as List<IState>;
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

	public void GetPendingBanners(int stateId, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["banners"] = new List<IBanner>();
			try
			{
				Interlocked.Increment(ref m_working);
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
					List<IBanner> list = remoteServiceResult.Properties["banners"] as List<IBanner>;
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

	public void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["stores"] = new List<IStore>();
			try
			{
				Interlocked.Increment(ref m_working);
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
					List<IStore> list = remoteServiceResult.Properties["stores"] as List<IStore>;
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

	public void GetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
				LogHelper.Instance.Log("Live call to KioskClientService GetOpsMarket, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
				OpsMarketResponse opsMarketForKiosk = service.Installer.GetOpsMarketForKiosk(int.Parse(storeNumber));
				remoteServiceResult.Success = opsMarketForKiosk.Success;
				remoteServiceResult.Properties["is_online"] = true;
				remoteServiceResult.Properties["ops_market"] = opsMarketForKiosk.OpsMarket;
				remoteServiceResult.Properties["kaseya_server"] = opsMarketForKiosk.KaseyaServer;
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
