using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class LegacySystemData : IConfigurationFile, IDisposable
{
	private bool Disposed;

	public SystemConfigurations Type => SystemConfigurations.SystemData;

	public string Path => "c:\\Gamp";

	public string FileName => "SystemData.dat";

	public string FullSourcePath { get; private set; }

	public IDictionary<string, int> SystemData { get; private set; }

	public KioskConfiguration KioskConfig { get; private set; }

	public void ImportFrom(IConfigurationFile config, ErrorList errors)
	{
		throw new NotImplementedException();
	}

	public ConversionResult ConvertTo(KioskConfiguration newConfig, ErrorList errors)
	{
		if (SystemData.Keys.Count == 0)
		{
			return ConversionResult.InvalidFile;
		}
		if (KioskConfig == KioskConfiguration.R504)
		{
			return ConversionResult.UnsupportedConversion;
		}
		if (KioskConfiguration.R630 == KioskConfig && (!ConvertToVMZ() || !Write()))
		{
			return ConversionResult.Failure;
		}
		return ConversionResult.Success;
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			SystemData.Clear();
		}
	}

	public bool Write()
	{
		if (SystemData.Keys.Count == 0)
		{
			return false;
		}
		using (StreamWriter streamWriter = new StreamWriter(FullSourcePath))
		{
			foreach (string key in SystemData.Keys)
			{
				streamWriter.WriteLine("{0},{1}", key, SystemData[key]);
			}
		}
		return true;
	}

	public LegacySystemData()
	{
		FullSourcePath = System.IO.Path.Combine(Path, FileName);
		SystemData = new Dictionary<string, int>();
		Action<string, int> action = delegate(string key3, int value)
		{
			SystemData[key3] = value;
		};
		OnReadSystemData(action);
		KioskConfig = KioskConfiguration.None;
		if (SystemData.Keys.Count <= 0)
		{
			return;
		}
		string key = "PlatterMaxSlots1";
		if (!SystemData.ContainsKey(key))
		{
			return;
		}
		int num = SystemData[key];
		if (72 == num)
		{
			KioskConfig = KioskConfiguration.R504;
			return;
		}
		string key2 = "QLMDeckNumber";
		if (SystemData.ContainsKey(key2))
		{
			KioskConfig = ((SystemData[key2] == 0) ? KioskConfiguration.R717 : KioskConfiguration.R630);
		}
	}

	private bool ConvertToVMZ()
	{
		if (SystemData.Keys.Count == 0)
		{
			return false;
		}
		for (int i = 1; i <= 9; i++)
		{
			string key = $"PlatterMaxSlots{i}";
			if (SystemData.ContainsKey(key))
			{
				SystemData[key] = 90;
			}
		}
		if (SystemData.ContainsKey("QLMDeckNumber"))
		{
			SystemData["QLMDeckNumber"] = 0;
		}
		if (SystemData.ContainsKey("LastDecknNumber"))
		{
			SystemData["LastDecknNumber"] = 9;
		}
		KioskConfig = KioskConfiguration.R717;
		return true;
	}

	private void OnReadSystemData(Action<string, int> action)
	{
		if (!File.Exists(FullSourcePath))
		{
			return;
		}
		string[] array = File.ReadAllLines(FullSourcePath);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length >= 2 && int.TryParse(array2[1], out var result))
			{
				action(array2[0], result);
			}
		}
	}
}
