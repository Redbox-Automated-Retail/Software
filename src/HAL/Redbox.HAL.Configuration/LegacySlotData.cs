using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class LegacySlotData : IConfigurationFile, IDisposable
{
	private bool Disposed;

	public SystemConfigurations Type => SystemConfigurations.SlotData;

	public string Path => "c:\\Gamp";

	public string FileName => "SlotData.dat";

	public string FullSourcePath { get; private set; }

	public List<PlatterConfig> SlotData { get; private set; }

	public KioskConfiguration KioskConfig { get; private set; }

	public void ImportFrom(IConfigurationFile config, ErrorList errors)
	{
		throw new NotImplementedException();
	}

	public ConversionResult ConvertTo(KioskConfiguration newConfig, ErrorList errors)
	{
		if (SlotData.Count == 0)
		{
			return ConversionResult.InvalidFile;
		}
		if (KioskConfiguration.R504 == KioskConfig)
		{
			return ConversionResult.UnsupportedConversion;
		}
		if (KioskConfiguration.R630 == KioskConfig && (!ConvertToVMZ() || !WriteSlotData()))
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
			SlotData.Clear();
		}
	}

	public bool WriteSlotData()
	{
		if (SlotData.Count == 0)
		{
			return false;
		}
		using (StreamWriter streamWriter = new StreamWriter(FullSourcePath))
		{
			foreach (PlatterConfig slotDatum in SlotData)
			{
				streamWriter.WriteLine(slotDatum.Data.ToString());
			}
		}
		return true;
	}

	public LegacySlotData()
		: this(removeZeroes: true)
	{
	}

	public LegacySlotData(bool removeZeroes)
	{
		FullSourcePath = System.IO.Path.Combine(Path, FileName);
		SlotData = new List<PlatterConfig>();
		if (File.Exists(FullSourcePath))
		{
			ReadLegacyFile(removeZeroes);
		}
		KioskConfig = KioskConfiguration.None;
		if (SlotData.Count > 0)
		{
			if (SlotData[1].Type == PlatterType.Sparse)
			{
				KioskConfig = KioskConfiguration.R504;
				return;
			}
			PlatterConfig platterConfig = SlotData[SlotData.Count - 1];
			KioskConfig = ((platterConfig.Type == PlatterType.Qlm) ? KioskConfiguration.R630 : KioskConfiguration.R717);
		}
	}

	private bool ConvertToVMZ()
	{
		if (SlotData.Count == 0)
		{
			return false;
		}
		PlatterConfig platterConfig = SlotData[SlotData.Count - 1];
		PlatterConfig platterConfig2 = SlotData[SlotData.Count - 2];
		platterConfig.Data.SegmentOffsets = platterConfig2.Data.SegmentOffsets;
		KioskConfig = KioskConfiguration.R717;
		return true;
	}

	private void ReadLegacyFile(bool removeZeroes)
	{
		int lineNumber = 1;
		bool invalid = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string line in File.ReadAllLines(FullSourcePath))
		{
			List<int> row = new List<int>();
			int result;
			for (int j = 0; j < line.Length; j++)
			{
				if (char.IsWhiteSpace(line[j]) || line[j] == ',')
				{
					if (stringBuilder.Length > 0)
					{
						if (!int.TryParse(stringBuilder.ToString(), out result))
						{
							result = 0;
							LogHelper.Instance.Log(LogEntryType.Error, "The value {0} in slotdata.dat (line {1}) is invalid", stringBuilder.ToString(), lineNumber);
							invalid = true;
						}
						else
						{
							row.Add(result);
						}
						stringBuilder.Remove(0, stringBuilder.Length);
					}
				}
				else
				{
					stringBuilder.Append(line[j]);
				}
			}
			if (stringBuilder.Length > 0)
			{
				if (!int.TryParse(stringBuilder.ToString(), out result))
				{
					result = 0;
					LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {lineNumber}) is invalid", LogEntryType.Error);
					invalid = true;
				}
				else
				{
					row.Add(result);
				}
				stringBuilder.Remove(0, stringBuilder.Length);
			}
			lineNumber++;
			if (row.Count > 0)
			{
				SlotData.Add(PlatterConfig.Get(new PlatterData(row, removeZeroes)));
			}
		}
		if (invalid)
		{
			SlotData.Clear();
		}
	}
}
