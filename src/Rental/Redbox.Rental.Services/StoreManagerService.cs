using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class StoreManagerService : IStoreManager
{
	private Country _country;

	private IConfigurationService _configService;

	private long _kioskId;

	private string _market;

	private string _banner;

	private bool _kioskSellThru = true;

	public string _applicationDataPath;

	public Country Country => _country;

	public long KioskId
	{
		get
		{
			try
			{
				if (_kioskId == 0L)
				{
					string text = "ID";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, "");
					if (value == null || string.IsNullOrEmpty(value.ToString()))
					{
						LogHelper.Instance.Log("Kiosk ID is not set in the registry at {0}\\{1}", text2, text);
						return 0L;
					}
					_kioskId = Convert.ToInt32(value);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception occurred in StoreManagerService.GetKioskId", e);
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
					string text = "Market";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, null);
					if (value == null)
					{
						LogHelper.Instance.Log("Market is not set in the registry at {0}\\{1}", text2, text);
						return null;
					}
					_market = ((string)value).Trim();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception occurred in StoreManagerService.GetMarket", e);
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
					string text = "Banner";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, null);
					if (value == null)
					{
						LogHelper.Instance.Log("Banner is not set in the registry at {0}\\{1}", text2, text);
						return null;
					}
					_banner = ((string)value).Trim();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception occurred in StoreManagerService.GetMarket", e);
			}
			return _banner;
		}
	}

	public bool SellThru
	{
		get
		{
			return _kioskSellThru;
		}
		set
		{
			_kioskSellThru = value;
		}
	}

	public bool CanSellUsed
	{
		get
		{
			if (!SellThru)
			{
				return false;
			}
			if (!GetSellThruOfUsed())
			{
				return false;
			}
			return true;
		}
	}

	public bool CanSellUsedMovies => CanSellUsed;

	public bool CanSellUsedGames
	{
		get
		{
			if (!CanSellUsed)
			{
				return false;
			}
			return GetGamesSellThru();
		}
	}

	public decimal PurchasePrice { get; set; }

	public string ApplicationDataPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_applicationDataPath))
			{
				return _applicationDataPath;
			}
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Redbox\\KioskEngine");
			_applicationDataPath = Path.GetFullPath(path);
			return _applicationDataPath;
		}
	}

	public string RunningPath => Path.GetDirectoryName(typeof(StoreManagerService).Assembly.Location);

	public StoreManagerService(Country country, IConfigurationService configService)
	{
		_country = country;
		_configService = configService;
	}

	public string GetDefaultCulture()
	{
		string text = GetDefaultCultureByCountry();
		if (_configService.TryGetValue<string>("system", "Culture", "DefaultCulture", out var value))
		{
			LogHelper.Instance.Log("Found default culture in configuration file: {0}", value);
			if (IsCultureValid(value))
			{
				text = value;
			}
			else
			{
				LogHelper.Instance.Log("Culture found in configuration file is not valid: {0}", value);
			}
		}
		else
		{
			LogHelper.Instance.Log("Default culture was not found in configuration file, using: {0}", text);
		}
		return text;
	}

	public string GetAlternateCulture()
	{
		string text = GetDefaultAlternateCultureByCountry();
		if (_configService.TryGetValue<string>("system", "Culture", "AlternateCulture", out var value))
		{
			LogHelper.Instance.Log("Found alternate culture in configuration file: {0}", value);
			if (IsCultureValid(value))
			{
				text = value;
			}
			else
			{
				LogHelper.Instance.Log("Alternate Culture found in configuration file is not valid: {0}", value);
			}
		}
		else
		{
			LogHelper.Instance.Log("Alternate culture was not found in configuration file, using: {0}", text);
		}
		string defaultCulture = GetDefaultCulture();
		if (text == defaultCulture)
		{
			text = ((defaultCulture == GetDefaultCultureByCountry()) ? GetDefaultAlternateCultureByCountry() : GetDefaultCultureByCountry());
			LogHelper.Instance.Log("Alternate culture cannot equal the default.  Setting alternate culter to {0}", text);
		}
		return text;
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

	public int GetSellThruDays()
	{
		int result = 30;
		if (_configService.TryGetValue<int>("system", "General", "SellThruDays", out var value))
		{
			result = value;
		}
		return result;
	}

	private bool GetSellThruOfUsed()
	{
		bool result = true;
		if (_configService.TryGetValue<bool>("system", "General", "SellThruOfUsed", out var value))
		{
			result = value;
		}
		return result;
	}

	private bool GetGamesSellThru()
	{
		bool result = true;
		if (_configService.TryGetValue<bool>("system", "General", "GamesSellThru", out var value))
		{
			result = value;
		}
		return result;
	}
}
