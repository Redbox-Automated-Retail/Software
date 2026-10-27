using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductPricing.Model;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.Pricing;

public class PricingData : IPricingData
{
	private int _inRefresh;

	private object _lock = new object();

	private IPricingDataInstance _stagedInstance;

	private IPricingDataInstance _activeInstance;

	private IKernelService _kernelService;

	private IStoreManager _storeManagerService;

	private long _kioskId;

	private string _runningPath;

	private string _dataPath;

	private string _versionStagedPath;

	private string _versionPendingPath;

	private string _versionActivePath;

	private const string VersionsStagedName = "PricingVersion.staged";

	private const string VersionsPendingName = "PricingVersion.pending";

	private const string VersionsActiveName = "PricingVersion.active";

	private string _pricingStagedPath;

	private string _pricingPendingPath;

	private string _pricingActivePath;

	private const string PricingStagedName = "Pricing.staged";

	private const string PricingPendingName = "Pricing.pending";

	private const string PricingActiveName = "Pricing.active";

	public PricingData(IKernelService kernelService, IStoreManager storeManagerService)
	{
		_kernelService = kernelService;
		_storeManagerService = storeManagerService;
		_kioskId = _storeManagerService.KioskId;
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		_runningPath = service.RunningPath;
		_dataPath = service.DataPath;
		_versionStagedPath = Path.Combine(_dataPath, "PricingVersion.staged");
		_versionPendingPath = Path.Combine(_dataPath, "PricingVersion.pending");
		_versionActivePath = Path.Combine(_dataPath, "PricingVersion.active");
		_pricingStagedPath = Path.Combine(_dataPath, "Pricing.staged");
		_pricingPendingPath = Path.Combine(_dataPath, "Pricing.pending");
		_pricingActivePath = Path.Combine(_dataPath, "Pricing.active");
	}

	public IPricingDataInstance GetActiveInstance(bool force = false)
	{
		if (force)
		{
			CheckForStagedAndLoad();
		}
		lock (_lock)
		{
			ActivateIfNeeded();
			if (_activeInstance == null)
			{
				_activeInstance = LoadActiveInstance();
			}
			return _activeInstance;
		}
	}

	public void LoadStaged()
	{
		Task.Run(delegate
		{
			CheckForStagedAndLoad();
		});
	}

	private void CheckForStagedAndLoad()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("PricingData.CheckForStagedAndLoad - exiting in method");
			return;
		}
		try
		{
			LogHelper.Instance.Log("PricingData.CheckForStagedAndLoad");
			LoadStagedFiles();
		}
		finally
		{
			_inRefresh = 0;
		}
	}

	private void LoadStagedFiles()
	{
		try
		{
			if (_stagedInstance != null || !HasStaged())
			{
				return;
			}
			LogHelper.Instance.Log("PricingData.LoadStagedFiles - HasStaged loading instance");
			IPricingDataInstance pricingDataInstance = LoadStagedInstance();
			if (pricingDataInstance == null)
			{
				return;
			}
			lock (_lock)
			{
				DeletePendingFiles();
				LogHelper.Instance.Log("PricingData.LoadStagedFiles - writing pending pricing files");
				_stagedInstance = pricingDataInstance;
				GetKioskPricingUsingStrings instance = GetKioskPricingUsingStrings.ToGetKioskPricingUsingStrings(_stagedInstance.KioskPricing);
				File.WriteAllBytes(_versionPendingPath, Encoding.ASCII.GetBytes(new PricingVersionData
				{
					Version = _stagedInstance.VersionTimeStamp
				}.ToJson()));
				File.WriteAllBytes(_pricingPendingPath, Encoding.ASCII.GetBytes(instance.ToJson()));
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingData.LoadStagedFiles - Unhandled exception occurred", e);
		}
	}

	private bool HasStaged()
	{
		if (File.Exists(_versionStagedPath))
		{
			return true;
		}
		if (File.Exists(_pricingStagedPath))
		{
			return true;
		}
		return false;
	}

	private void ActivateIfNeeded()
	{
		try
		{
			if (_stagedInstance != null)
			{
				DeleteActiveFiles();
				LogHelper.Instance.Log("PricingData.ActivateIfNeeded - Activating pending pricing files");
				File.Move(_versionPendingPath, _versionActivePath);
				File.Move(_pricingPendingPath, _pricingActivePath);
				DeleteStagedFiles();
				_activeInstance = _stagedInstance;
				_stagedInstance = null;
				SendStatistic();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingData.ActivateIfNeeded - Unhandled exception occurred", e);
		}
	}

	private void SendStatistic()
	{
		try
		{
			if (_kernelService != null)
			{
				string text = ((_activeInstance == null) ? new DateTime(2000, 1, 1) : _activeInstance.VersionTimeStamp).ToString("yyyy/MM/dd HH:mm:ss");
				LogHelper.Instance.Log("PricingData.SendStatistic - Sending PricingData new version: {0}.", text);
				_kernelService.ExecuteChunk(string.Format("kernel.healthsvcs_enqueuestatistic('{0}','{1}')", "PricingData", text));
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingData.SendStatistic - Unhandled exception occurred", e);
		}
	}

	private IPricingDataInstance LoadStagedInstance()
	{
		try
		{
			LogHelper.Instance.Log("PricingData.LoadStagedInstance - loading Pricing version file.");
			PricingVersionData pricingVersionData = LoadVersionData(staged: true);
			LogHelper.Instance.Log("PricingData.LoadStagedInstance - loading Pricing file.");
			GetKioskPricing pricing = LoadKioskPricingData(staged: true);
			LogHelper.Instance.Log("PricingData.LoadStagedInstance - Finished loading pricing files.");
			if (pricingVersionData == null)
			{
				LogHelper.Instance.LogError("PricingData.LoadStagedInstance", "Version cannot be null");
				return null;
			}
			if (pricing == null)
			{
				LogHelper.Instance.LogError("PricingData.LoadStagedInstance", "Pricing cannot be null");
				return null;
			}
			PricingDataInstance pricingDataInstance = new PricingDataInstance
			{
				VersionTimeStamp = pricingVersionData.Version
			};
			LogHelper.Instance.Log(pricingVersionData.ToJson());
			if (pricing.KioskPricing.ContainsKey(_kioskId))
			{
				KioskPricing kioskPricing = pricing.KioskPricing[_kioskId];
				GetKioskPricing gkp = new GetKioskPricing
				{
					KioskPricing = new SortedDictionary<long, KioskPricing>(),
					DefaultRentalPricing = new SortedDictionary<long, DefaultRentalPricing>(),
					RentalPricing = new SortedDictionary<long, RentalPricing>(),
					PurchasePricing = new SortedDictionary<long, PurchasePricing>()
				};
				gkp.KioskPricing[_kioskId] = kioskPricing;
				kioskPricing.DefaultRentalPricing.ForEach(delegate(long x)
				{
					gkp.DefaultRentalPricing[x] = pricing.DefaultRentalPricing[x];
				});
				List<long> removeList = new List<long>();
				kioskPricing.RentalPricing.ForEach(delegate(long x)
				{
					if (pricing.RentalPricing.ContainsKey(x))
					{
						gkp.RentalPricing[x] = pricing.RentalPricing[x];
					}
					else
					{
						removeList.Add(x);
					}
				});
				removeList.ForEach(delegate(long x)
				{
					kioskPricing.RentalPricing.Remove(x);
				});
				kioskPricing.PurchasePricing.ForEach(delegate(long x)
				{
					gkp.PurchasePricing[x] = pricing.PurchasePricing[x];
				});
				pricingDataInstance.KioskPricing = gkp;
				pricingDataInstance.DefaultRentalPricing = gkp.DefaultRentalPricing.Select((KeyValuePair<long, DefaultRentalPricing> x) => x.Value).ToList();
				pricingDataInstance.RentalPricing = gkp.RentalPricing.Select((KeyValuePair<long, RentalPricing> x) => x.Value).ToList();
				pricingDataInstance.PurchasePricing = gkp.PurchasePricing.Select((KeyValuePair<long, PurchasePricing> x) => x.Value).ToList();
				return pricingDataInstance;
			}
			LogHelper.Instance.LogError("PricingData.LoadStagedInstance", "Staged pricing file does not contain data for this kiosk");
			DeleteStagedFiles();
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingData.LoadStagedInstance - Unhandled exception occurred", e);
			return null;
		}
	}

	private IPricingDataInstance LoadActiveInstance()
	{
		try
		{
			LogHelper.Instance.Log("PricingData.LoadActiveInstance - loading Pricing version file.");
			PricingVersionData pricingVersionData = LoadVersionData(staged: false);
			LogHelper.Instance.Log("PricingData.LoadActiveInstance - loading Pricing file.");
			GetKioskPricing getKioskPricing = LoadKioskPricingData(staged: false);
			LogHelper.Instance.Log("PricingData.LoadActiveInstance - Finished loading pricing files.");
			if (pricingVersionData == null)
			{
				LogHelper.Instance.LogError("PricingData.LoadActiveInstance", "Version cannot be null");
				return null;
			}
			if (getKioskPricing == null)
			{
				LogHelper.Instance.LogError("PricingData.LoadActiveInstance", "Pricing cannot be null");
				return null;
			}
			PricingDataInstance pricingDataInstance = new PricingDataInstance
			{
				VersionTimeStamp = pricingVersionData.Version
			};
			LogHelper.Instance.Log(pricingVersionData.ToJson());
			if (getKioskPricing.KioskPricing.ContainsKey(_kioskId))
			{
				pricingDataInstance.KioskPricing = getKioskPricing;
				pricingDataInstance.DefaultRentalPricing = getKioskPricing.DefaultRentalPricing.Select((KeyValuePair<long, DefaultRentalPricing> x) => x.Value).ToList();
				pricingDataInstance.RentalPricing = getKioskPricing.RentalPricing.Select((KeyValuePair<long, RentalPricing> x) => x.Value).ToList();
				pricingDataInstance.PurchasePricing = getKioskPricing.PurchasePricing.Select((KeyValuePair<long, PurchasePricing> x) => x.Value).ToList();
				return pricingDataInstance;
			}
			LogHelper.Instance.LogError("PricingData.LoadActiveInstance", "Staged pricing file does not contain data for this kiosk");
			DeleteActiveFiles();
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("$PricingData.LoadActiveInstance - Unhandled exception occurred", e);
			return null;
		}
	}

	private GetKioskPricing LoadKioskPricingData(bool staged)
	{
		try
		{
			string text = (staged ? _pricingStagedPath : _pricingActivePath);
			LogHelper.Instance.Log("PricingData.LoadKioskPricingData file: " + text);
			if (!File.Exists(text))
			{
				LogHelper.Instance.Log($"PricingData.LoadKioskPricingData(staged={staged}) - Pricing file does not exist: {text}");
				return null;
			}
			return DeserializePricing(text);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingData.LoadKioskPricingData - Unhandled exception occurred", e);
			SafeDelete(staged ? _pricingStagedPath : _pricingActivePath);
			return null;
		}
	}

	private PricingVersionData LoadVersionData(bool staged)
	{
		try
		{
			string text = (staged ? _versionStagedPath : _versionActivePath);
			LogHelper.Instance.Log("PricingData.LoadVersionData file: " + text);
			if (!File.Exists(text))
			{
				LogHelper.Instance.Log($"PricingData.LoadVersionData(staged={staged}) - Pricing file does not exist: {text}");
				return null;
			}
			return Encoding.ASCII.GetString(File.ReadAllBytes(text)).ToObject<PricingVersionData>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"PricingData.LoadVersionData(staged:{staged}) - Unhandled exception occurred", e);
			SafeDelete(staged ? _versionStagedPath : _versionActivePath);
			return null;
		}
	}

	private GetKioskPricing DeserializePricing(string file)
	{
		return GetKioskPricingUsingStrings.ToGetKioskPricing(Encoding.ASCII.GetString(File.ReadAllBytes(file)).ToObject<GetKioskPricingUsingStrings>());
	}

	private void DeleteStagedFiles()
	{
		SafeDelete(_versionStagedPath);
		SafeDelete(_pricingStagedPath);
	}

	private void DeletePendingFiles()
	{
		SafeDelete(_versionPendingPath);
		SafeDelete(_pricingPendingPath);
	}

	private void DeleteActiveFiles()
	{
		SafeDelete(_versionActivePath);
		SafeDelete(_pricingActivePath);
	}

	private void SafeDelete(string fileName)
	{
		try
		{
			if (File.Exists(fileName))
			{
				LogHelper.Instance.Log("PricingData.SafeDelete - file ({0})", fileName);
				File.Delete(fileName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"PricingData.SafeDelete - file ({fileName})", e);
		}
	}
}
