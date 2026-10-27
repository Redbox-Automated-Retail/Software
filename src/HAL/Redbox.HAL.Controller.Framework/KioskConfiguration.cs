using System;
using System.IO;
using System.Xml.Serialization;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class KioskConfiguration
{
	private bool _loaded;

	private static XmlSerializer _serializer = new XmlSerializer(typeof(KioskConfiguration));

	private const string FILE_NAME = "Kiosk.xml";

	private const string IRHardwareInstallDateName = "IRHardwareInstallDate";

	public static KioskConfiguration Instance => Singleton<KioskConfiguration>.Instance;

	public DateTime? IRHardwareInstallDate { get; set; }

	private string FilePath
	{
		get
		{
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			if (service != null)
			{
				return service.RuntimePath("Kiosk.xml");
			}
			return Path.Combine(Path.GetDirectoryName(typeof(KioskConfiguration).Assembly.Location), "Kiosk.xml");
		}
	}

	public void Initialize(ErrorList errors)
	{
		if (!Load())
		{
			errors.Add(Error.NewError("KCL001", "KioskConfiguration Initialize failed to load.", ""));
		}
	}

	public void SetConfig(string name, string value, bool saveValue = false)
	{
		LogHelper.Instance.Log($"KioskConfiguration.SetConfig - name: {name}, value: {value}, save: {saveValue}");
		if (name == "IRHardwareInstallDate")
		{
			IRHardwareInstallDate = ((!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var result)) ? new DateTime?(result) : ((DateTime?)null));
		}
		else
		{
			LogHelper.Instance.Log($"KioskConfiguration.SetConfig doesn't support: {name} - {value}");
		}
		if (saveValue)
		{
			Save();
		}
	}

	private void Save()
	{
		LogHelper.Instance.Log("KioskConfiguration.Save - " + FilePath);
		try
		{
			using StreamWriter textWriter = new StreamWriter(FilePath);
			_serializer.Serialize(textWriter, Instance);
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Exception when Saving KioskConfiguration: {arg}");
		}
	}

	public bool Load()
	{
		try
		{
			_loaded = false;
			if (File.Exists(FilePath))
			{
				KioskConfiguration kioskConfiguration = null;
				using (StreamReader textReader = new StreamReader(FilePath))
				{
					kioskConfiguration = _serializer.Deserialize(textReader) as KioskConfiguration;
				}
				if (kioskConfiguration == null)
				{
					return false;
				}
				IRHardwareInstallDate = kioskConfiguration.IRHardwareInstallDate;
				_loaded = true;
			}
			else
			{
				_loaded = true;
				Save();
			}
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Exception when Loading KioskConfiguration: {arg}");
			_loaded = false;
		}
		finally
		{
			LogHelper.Instance.Log($"KioskConfiguration.Load File: {FilePath} - Success: {_loaded.ToString()}");
		}
		return _loaded;
	}

	private KioskConfiguration()
	{
	}
}
