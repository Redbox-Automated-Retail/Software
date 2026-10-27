using System;
using System.IO;
using System.Reflection;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class DataStoreService : IDataStoreService
{
	private readonly object m_writeLock = new object();

	public static DataStoreService Instance => Singleton<DataStoreService>.Instance;

	private string Root { get; set; }

	public void Initialize(string rootPath)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(rootPath);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		Root = text;
		if (!Directory.Exists(Root))
		{
			Directory.CreateDirectory(Root);
		}
		if (ServiceLocator.Instance.GetService<IDataStoreService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IDataStoreService), this);
		}
	}

	public void Reset()
	{
		if (Directory.Exists(Root))
		{
			Directory.Delete(Root, recursive: true);
		}
		Initialize(Root);
	}

	public void Set(string id, object o)
	{
		SetRaw(id, o.ToJson());
	}

	public void SetRaw(Guid id, string o)
	{
		SetRaw(id.ToString(), o);
	}

	public void SetRaw(string id, string o)
	{
		string path = FormatFilePath(id);
		lock (m_writeLock)
		{
			File.WriteAllText(path, o);
		}
	}

	public T Get<T>(string id)
	{
		string raw = GetRaw(id);
		if (raw == null)
		{
			return default(T);
		}
		T result;
		try
		{
			return raw.ToObject<T>();
		}
		catch (Exception e)
		{
			result = default(T);
			Delete(id);
			LogHelper.Instance.LogException("DataStoreService.Get<T> - An exception occurred deserializing an object. Deleting file.", e);
		}
		return result;
	}

	public string GetRaw(Guid id)
	{
		return GetRaw(id.ToString());
	}

	public string GetRaw(string id)
	{
		string path = FormatFilePath(id);
		if (!File.Exists(path))
		{
			return null;
		}
		using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	public void Delete(string id)
	{
		string path = FormatFilePath(id);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void Set(Guid id, object o)
	{
		Set(id.ToString(), o);
	}

	public T Get<T>(Guid id)
	{
		return Get<T>(id.ToString());
	}

	public void Delete(Guid id)
	{
		Delete(id.ToString());
	}

	public void CleanUp()
	{
		string[] files = Directory.GetFiles(Root, "*.dat");
		foreach (string path in files)
		{
			if (!Path.GetFileNameWithoutExtension(path).StartsWith("config", StringComparison.CurrentCultureIgnoreCase))
			{
				File.Delete(path);
			}
		}
	}

	public void CleanUp(string extension)
	{
		string[] files = Directory.GetFiles(Root, "*" + extension);
		foreach (string path in files)
		{
			if (!Path.GetFileNameWithoutExtension(path).StartsWith("config", StringComparison.CurrentCultureIgnoreCase))
			{
				File.Delete(path);
			}
		}
	}

	private DataStoreService()
	{
	}

	private string FormatFilePath(string id)
	{
		if (Path.HasExtension(id))
		{
			return Path.Combine(Root, id);
		}
		return Path.Combine(Root, $"{id}.dat");
	}
}
