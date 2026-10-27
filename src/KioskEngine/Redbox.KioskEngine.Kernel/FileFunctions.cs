using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class FileFunctions
{
	[KernelFunction(Name = "kernel.readalltext")]
	internal static string ReadAllText(string filePath)
	{
		if (File.Exists(filePath))
		{
			return File.ReadAllText(filePath);
		}
		return string.Empty;
	}

	[KernelFunction(Name = "kernel.writealltext")]
	internal static void WriteAllText(string filePath, string text)
	{
		File.WriteAllText(filePath, text);
	}

	[KernelFunction(Name = "kernel.replaceinfile")]
	internal static void ReplaceInFile(string filePath, string pattern, string token, string newvalue)
	{
		if (File.Exists(filePath))
		{
			new RegExFileEditor(filePath, backup: true, "saved").Replace(pattern, token, newvalue);
		}
	}

	[KernelFunction(Name = "kernel.createfile")]
	internal static void CreateFile(string filePath)
	{
		File.Create(filePath);
	}

	[KernelFunction(Name = "kernel.deletefile")]
	internal static void DeleteFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	[KernelFunction(Name = "kernel.deletefilefromrelativepath")]
	internal static void DeleteFileFromRelativePath(string relativeFilePath)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			string path = Path.Combine(service.ExpandProperties("${RunningPath}\\..\\"), relativeFilePath);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[KernelFunction(Name = "kernel.tablefromfileserializeddictionary")]
	internal static LuaTable LoadLuaTableFromFileSerializedDictionary(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return null;
		}
		LuaTable table = new LuaTable(KernelService.Instance.LuaRuntime);
		try
		{
			string json;
			using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
			{
				json = streamReader.ReadToEnd();
			}
			json.ToObject<Dictionary<string, object>>().ForEach(delegate(KeyValuePair<string, object> x)
			{
				new LuaTable(KernelService.Instance.LuaRuntime);
				table[x.Key] = x.Value;
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Failed to load table from file {filePath}", e);
		}
		return table;
	}

	[KernelFunction(Name = "kernel.tablefromfileserializedlistdictionary")]
	internal static LuaTable LoadLuaTableFromFileSerializedListDictionary(string filePath, string kioskIdFilter)
	{
		if (!File.Exists(filePath))
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		try
		{
			string json;
			using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
			{
				json = streamReader.ReadToEnd();
			}
			List<Dictionary<string, object>> list = json.ToObject<List<Dictionary<string, object>>>();
			int num = 1;
			foreach (Dictionary<string, object> item in list)
			{
				if (kioskIdFilter != null && (!item.ContainsKey("KioskId") || !(item["KioskId"].ToString() == kioskIdFilter)))
				{
					continue;
				}
				LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
				luaTable[num++] = luaTable2;
				foreach (KeyValuePair<string, object> item2 in item)
				{
					if (kioskIdFilter == null || item2.Key.ToString() != "KioskId")
					{
						luaTable2[item2.Key] = item2.Value;
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Failed to load table from file {filePath}", e);
		}
		return luaTable;
	}
}
