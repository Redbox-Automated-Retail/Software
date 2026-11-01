using System;
using System.IO;
using Newtonsoft.Json;

namespace DeviceService.Domain;

public static class DataFileHelper
{
	public static T GetFileData<T>(string dataFilePath, string fileName, Action<string, Exception> exceptionLogger)
	{
		string path = Path.Combine(dataFilePath, fileName);
		T result = default(T);
		if (File.Exists(path))
		{
			try
			{
				using StreamReader reader = File.OpenText(path);
				result = (T)new JsonSerializer().Deserialize(reader, typeof(T));
				return result;
			}
			catch (JsonSerializationException ex)
			{
				exceptionLogger("DataFileHelper.GetFileData - JSsonSerializationException", ex);
			}
			catch (Exception ex)
			{
				exceptionLogger("DataFileHelper.GetFileData - Exception", ex);
			}
		}
		return result;
	}

	public static bool TryUpdateFileData<T>(string dataFilePath, string fileName, T data)
	{
		try
		{
			if (!Directory.Exists(dataFilePath))
			{
				Directory.CreateDirectory(dataFilePath);
			}
			string path = Path.Combine(dataFilePath, fileName);
			string contents = JsonConvert.SerializeObject(data);
			File.WriteAllText(path, contents);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
