using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace KioskClientService.API.Services;

public class StoreService : IStoreService
{
	private ILogger<StoreService> _logger;

	private long _kioskId;

	private string _market;

	private string _banner;

	public string _dataPath;

	public Country Country { get; } = Country.US;

	public long KioskId
	{
		get
		{
			try
			{
				if (_kioskId == 0L)
				{
					string valueName = "ID";
					string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object registryValue = Registry.GetValue(keyName, valueName, null);
					if (registryValue == null)
					{
						_logger.LogInformation("Kiosk ID is not set in the registry at {0}\\{1}", keyName, valueName);
					}
					else
					{
						try
						{
							_kioskId = Convert.ToInt32(registryValue);
						}
						catch (FormatException)
						{
							_logger.LogError($"Store ID key value: '{registryValue}' is not a valid number");
						}
					}
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetKioskId");
			}
			return _kioskId;
		}
	}

	public string Market
	{
		get
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_market))
				{
					string valueName = "Market";
					string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object registryValue = Registry.GetValue(keyName, valueName, null);
					if (registryValue == null)
					{
						_logger.LogInformation("Market is not set in the registry at {0}\\{1}", keyName, valueName);
						return null;
					}
					_market = ((string)registryValue).Trim();
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetMarket");
			}
			return _market;
		}
	}

	public string Banner
	{
		get
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_banner))
				{
					string valueName = "Banner";
					string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object registryValue = Registry.GetValue(keyName, valueName, null);
					if (registryValue == null)
					{
						_logger.LogInformation("Banner is not set in the registry at {0}\\{1}", keyName, valueName);
						return null;
					}
					_banner = ((string)registryValue).Trim();
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetMarket");
			}
			return _banner;
		}
	}

	public string DataPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_dataPath))
			{
				return _dataPath;
			}
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			string path = Path.Combine(folderPath, "Redbox\\KioskClient");
			_dataPath = Path.GetFullPath(path);
			return _dataPath;
		}
	}

	public string RunningPath => Path.GetDirectoryName(typeof(StoreService).Assembly.Location);

	public StoreService(ILogger<StoreService> logger)
	{
		_logger = logger;
	}

	private string GetDefaultCultureByCountry()
	{
		if (Country != Country.Canada)
		{
			return "en-US";
		}
		return "en-CA";
	}

	private string GetDefaultAlternateCultureByCountry()
	{
		if (Country != Country.Canada)
		{
			return "es-MX";
		}
		return "fr-CA";
	}

	private bool IsCultureValid(string culture)
	{
		return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Any((CultureInfo c) => c.Name.Equals(culture, StringComparison.CurrentCultureIgnoreCase));
	}
}
