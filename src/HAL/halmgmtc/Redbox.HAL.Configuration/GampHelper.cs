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
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length >= 2 && int.TryParse(array2[1], out var result))
			{
				dictionary[array2[0]] = result;
			}
		}
		return dictionary;
	}

	public GampBackupResult WriteSystemData(IDictionary<string, int> data, bool createBackup)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		GampBackupResult gampBackupResult = new GampBackupResult();
		if (createBackup)
		{
			gampBackupResult.BackupFile = service.CreateBackup("C:\\gamp\\SystemData.dat", BackupAction.Move);
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		if (createBackup)
		{
			gampBackupResult.BackupFile = service.CreateBackup("C:\\gamp\\SlotData.dat", BackupAction.Move);
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
		List<List<int>> list = new List<List<int>>();
		if (!File.Exists(path))
		{
			return list;
		}
		int num = 1;
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = File.ReadAllLines(path);
		foreach (string text in array)
		{
			List<int> list2 = new List<int>();
			int result;
			for (int j = 0; j < text.Length; j++)
			{
				if (char.IsWhiteSpace(text[j]) || text[j] == ',')
				{
					if (stringBuilder.Length > 0)
					{
						if (!int.TryParse(stringBuilder.ToString(), out result))
						{
							result = 0;
							LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {num}) is invalid", LogEntryType.Error);
							flag = true;
						}
						else
						{
							list2.Add(result);
						}
						stringBuilder.Remove(0, stringBuilder.Length);
					}
				}
				else
				{
					stringBuilder.Append(text[j]);
				}
			}
			if (stringBuilder.Length > 0)
			{
				if (!int.TryParse(stringBuilder.ToString(), out result))
				{
					result = 0;
					LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {num}) is invalid", LogEntryType.Error);
					flag = true;
				}
				else
				{
					list2.Add(result);
				}
				stringBuilder.Remove(0, stringBuilder.Length);
			}
			num++;
			if (list2.Count > 0)
			{
				list.Add(list2);
			}
		}
		if (flag)
		{
			list.Clear();
		}
		else if (removeZeroes)
		{
			foreach (List<int> item in list)
			{
				item.RemoveAll((int each) => each == 0);
			}
		}
		return list;
	}

	public List<int> ComputeDenseQuadrants(decimal? startOffset)
	{
		List<int> list = new List<int>();
		decimal? num = startOffset;
		list.Add((int)num.Value);
		for (int i = 0; i < 5; i++)
		{
			decimal? num2 = 2666.6672m;
			num += num2;
			list.Add((int)num.Value);
		}
		return list;
	}
}
