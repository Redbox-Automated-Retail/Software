using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class GampHelper
{
	internal const int DenseQuadrants = 6;

	internal const int SparseQuadrants = 12;

	internal const int DenseSlotsPerQuadrant = 15;

	internal const int SparseSlotsPerQuadrant = 6;

	internal const int DefaultSellThru = 915;

	internal const decimal DenseSlotWidth = 166.6667m;

	internal const decimal QlmSlotWidth = 177.7m;

	internal const string SlotData = "C:\\gamp\\SlotData.dat";

	internal const string SystemData = "C:\\gamp\\SystemData.dat";

	public int GetPlatterSlots()
	{
		List<List<int>> list = new GampHelper().ReadLegacySlotDataFile(removeZeroes: false);
		if (list.Count == 0)
		{
			return -1;
		}
		if (list[0].Count == 7)
		{
			return 90;
		}
		if (list[0][7] != 0)
		{
			return 72;
		}
		return 90;
	}

	public IDictionary<string, int> ReadLegacySystemDataFile()
	{
		string path = "C:\\gamp\\SystemData.dat";
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (!File.Exists(path))
		{
			return dictionary;
		}
		string[] lines = File.ReadAllLines(path);
		for (int i = 0; i < lines.Length; i++)
		{
			string[] fields = lines[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (fields.Length >= 2 && int.TryParse(fields[1], out var number))
			{
				dictionary[fields[0]] = number;
			}
		}
		return dictionary;
	}

	public GampBackupResult WriteSystemData(IDictionary<string, int> data, bool createBackup)
	{
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		GampBackupResult gampBackupResult = new GampBackupResult();
		if (createBackup)
		{
			gampBackupResult.BackupFile = runtimeService.CreateBackup("C:\\gamp\\SystemData.dat", BackupAction.Move);
		}
		using (StreamWriter streamWriter = new StreamWriter("C:\\gamp\\SystemData.dat"))
		{
			foreach (string key in data.Keys)
			{
				streamWriter.WriteLine("{0},{1}", key, data[key]);
			}
		}
		gampBackupResult.Success = true;
		return gampBackupResult;
	}

	public GampBackupResult WriteSlotDataFile(List<List<int>> data, bool createBackup)
	{
		GampBackupResult gampBackupResult = new GampBackupResult();
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		if (createBackup)
		{
			gampBackupResult.BackupFile = runtimeService.CreateBackup("C:\\gamp\\SlotData.dat", BackupAction.Move);
		}
		using (StreamWriter streamWriter = new StreamWriter("C:\\gamp\\SlotData.dat"))
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (List<int> datum in data)
			{
				foreach (int item in datum)
				{
					stringBuilder.Append(item);
					stringBuilder.Append(",");
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				streamWriter.WriteLine(stringBuilder.ToString());
				stringBuilder.Capacity = 512;
				stringBuilder.Length = 0;
			}
		}
		gampBackupResult.Success = true;
		return gampBackupResult;
	}

	public List<List<int>> ReadLegacySlotDataFile(bool removeZeroes)
	{
		string path = "C:\\gamp\\SlotData.dat";
		List<List<int>> rows = new List<List<int>>();
		if (!File.Exists(path))
		{
			return rows;
		}
		int lineNumber = 1;
		bool hasInvalidValue = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string line in File.ReadAllLines(path))
		{
			List<int> row = new List<int>();
			int number;
			for (int j = 0; j < line.Length; j++)
			{
				if (char.IsWhiteSpace(line[j]) || line[j] == ',')
				{
					if (stringBuilder.Length > 0)
					{
						if (!int.TryParse(stringBuilder.ToString(), out number))
						{
							number = 0;
							LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {lineNumber}) is invalid", LogEntryType.Error);
							hasInvalidValue = true;
						}
						else
						{
							row.Add(number);
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
				if (!int.TryParse(stringBuilder.ToString(), out number))
				{
					number = 0;
					LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {lineNumber}) is invalid", LogEntryType.Error);
					hasInvalidValue = true;
				}
				else
				{
					row.Add(number);
				}
				stringBuilder.Remove(0, stringBuilder.Length);
			}
			lineNumber++;
			if (row.Count > 0)
			{
				rows.Add(row);
			}
		}
		if (hasInvalidValue)
		{
			rows.Clear();
		}
		else if (removeZeroes)
		{
			foreach (List<int> slotRow in rows)
			{
				slotRow.RemoveAll((int each) => each == 0);
			}
		}
		return rows;
	}

	public List<int> ComputeDenseQuadrants(decimal? startOffset)
	{
		List<int> offsets = new List<int>();
		decimal? offset = startOffset;
		offsets.Add((int)offset.Value);
		for (int i = 0; i < 5; i++)
		{
			decimal? quadrantWidth = 2666.6672m;
			offset += quadrantWidth;
			offsets.Add((int)offset.Value);
		}
		return offsets;
	}
}
