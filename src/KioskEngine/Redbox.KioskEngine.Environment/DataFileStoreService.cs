using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.KioskEngine.Environment;

public class DataFileStoreService : IDataFileStoreService
{
	private class JsonWrapper
	{
		public string SerializedType { get; set; }

		public string AssemblyName { get; set; }

		public string JsonData { get; set; }
	}

	private string _dataStoreName;

	private string _extension = "dat";

	private bool _UseEncryption;

	private string _corruptFileExtension = "corrupt_dat";

	private readonly object m_writeLock = new object();

	private Dictionary<string, Dictionary<string, Type>> _cachedSerializationTypes = new Dictionary<string, Dictionary<string, Type>>();

	public string Root => _root;

	public bool UseEncryption
	{
		get
		{
			return _UseEncryption;
		}
		set
		{
			_UseEncryption = value;
		}
	}

	private string _root { get; set; }

	public void Initialize(string dataStoreName, string dataFilePath, string extension, string corruptFileExtension, bool useEncryption)
	{
		_UseEncryption = useEncryption;
		_dataStoreName = dataStoreName;
		_extension = ((extension != null) ? extension : _extension);
		_corruptFileExtension = ((corruptFileExtension != null) ? corruptFileExtension : _corruptFileExtension);
		if (!Path.IsPathRooted(dataFilePath))
		{
			dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dataFilePath);
		}
		_root = dataFilePath;
		if (!Directory.Exists(_root))
		{
			Directory.CreateDirectory(_root);
		}
	}

	public void Reset()
	{
		if (!string.IsNullOrEmpty(_root))
		{
			if (Directory.Exists(_root))
			{
				Directory.Delete(_root, recursive: true);
			}
			Initialize(_dataStoreName, _root, _extension, _corruptFileExtension, _UseEncryption);
		}
	}

	private Type GetType(string assemblyName, string typeName)
	{
		Type type = null;
		type = GetCachedSerializationType(assemblyName, typeName);
		if (type == null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(assemblyName))
		{
			string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
			if (File.Exists(text))
			{
				Assembly assembly = Assembly.LoadFile(text);
				if (assembly != null)
				{
					type = assembly.GetType(typeName);
					if (type != null)
					{
						AddCachedSerializationType(assemblyName, typeName, type);
					}
					else
					{
						LogHelper.Instance.Log("Unable to get type {1} in assembly {0}", text, typeName);
					}
				}
				else
				{
					LogHelper.Instance.Log("Unable to load assembly {0}", text);
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to find assembly {0}", text);
			}
		}
		return type;
	}

	public bool Set(string fileName, object o)
	{
		bool result = false;
		if (o != null)
		{
			string jsonData = JsonConvert.SerializeObject(o);
			Type type = o?.GetType();
			JsonWrapper jsonWrapper = new JsonWrapper
			{
				SerializedType = type?.ToString(),
				AssemblyName = Path.GetFileName(type?.Assembly?.CodeBase),
				JsonData = jsonData
			};
			if (o != null && !string.IsNullOrEmpty(jsonWrapper.SerializedType) && !string.IsNullOrEmpty(jsonWrapper.AssemblyName))
			{
				AddCachedSerializationType(jsonWrapper.AssemblyName, jsonWrapper.SerializedType, type);
			}
			string o2 = JsonConvert.SerializeObject(jsonWrapper);
			result = SetRaw(fileName, o2);
		}
		return result;
	}

	public bool SetRaw(Guid guidFileName, string o)
	{
		return SetRaw(guidFileName.ToString(), o);
	}

	public bool SetRaw(string fileName, string o)
	{
		bool result = false;
		try
		{
			string text = FormatFilePath(fileName);
			if (!string.IsNullOrEmpty(text))
			{
				lock (m_writeLock)
				{
					if (_UseEncryption)
					{
						o = o.EncryptToBase64();
					}
					File.WriteAllText(text, o);
					result = true;
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in DataFileStoreService.SetRaw: target is null.");
			}
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Exception in DataFileStoreService.SetRaw. {arg}");
		}
		return result;
	}

	public List<T> GetAll<T>()
	{
		List<T> list = new List<T>();
		if (Directory.Exists(_root))
		{
			string[] files = Directory.GetFiles(_root, "*." + _extension);
			foreach (string fileName in files)
			{
				if (Get<T>(fileName, out var deserializedObject))
				{
					list.Add(deserializedObject);
				}
			}
		}
		return list;
	}

	public bool Get<T>(string fileName, out T deserializedObject)
	{
		bool flag = false;
		deserializedObject = default(T);
		string raw = GetRaw(fileName);
		if (raw != null)
		{
			try
			{
				JsonWrapper jsonWrapper = JsonConvert.DeserializeObject<JsonWrapper>(raw);
				if (jsonWrapper != null)
				{
					Type type = GetType(jsonWrapper?.AssemblyName, jsonWrapper?.SerializedType);
					if (type != null)
					{
						object obj = JsonConvert.DeserializeObject(jsonWrapper.JsonData, type);
						if (obj != null && obj is T)
						{
							deserializedObject = (T)obj;
							flag = true;
						}
						else
						{
							LogHelper.Instance.Log($"DataFileStoreService.Get<T> - error - deserialized type {type} is not the correct type from file {fileName}");
						}
					}
					else
					{
						LogHelper.Instance.Log("DataFileStoreService.Get<T> - error - unable to get type " + jsonWrapper?.SerializedType + " from file " + fileName);
					}
				}
				else
				{
					LogHelper.Instance.Log("DataFileStoreService.Get<T> - error - deserialized object was not of type JsonWrapper from file " + fileName);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("DataFileStoreService.Get<T> - An exception occurred deserializing an object from file " + fileName, e);
				ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId.ToString() ?? null, "ApplicationCrash", _dataStoreName + ", Read-Error-01", "There was an error in " + _dataStoreName + " deserializing file " + fileName, DateTime.Now, null);
			}
		}
		if (!flag)
		{
			RenameCorruptFileExtension(fileName);
		}
		return flag;
	}

	private void RenameCorruptFileExtension(string fileName)
	{
		fileName = FormatFilePath(fileName);
		if (!File.Exists(fileName))
		{
			return;
		}
		try
		{
			string text = Path.ChangeExtension(fileName, _corruptFileExtension);
			LogHelper.Instance.Log("Renaming corrupted json file " + fileName + " to " + text);
			lock (m_writeLock)
			{
				File.Move(fileName, text);
			}
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Error in DataFileStoreService.RenameCorruptFileExtension. {arg}");
		}
	}

	public string GetRaw(Guid fileName)
	{
		return GetRaw(fileName.ToString());
	}

	public string GetRaw(string fileName)
	{
		string path = FormatFilePath(fileName);
		if (!File.Exists(path))
		{
			return null;
		}
		using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream);
		string text = streamReader.ReadToEnd();
		if (_UseEncryption)
		{
			try
			{
				text = text.DecryptFromBase64();
			}
			catch (Exception arg)
			{
				LogHelper.Instance.Log($"Error in DataFileStoreService.GetRaw.  Unable to Decrypt data. {arg}");
				text = null;
			}
		}
		return text;
	}

	public void Delete(string fileName)
	{
		string path = FormatFilePath(fileName);
		if (File.Exists(path))
		{
			lock (m_writeLock)
			{
				File.Delete(path);
			}
		}
	}

	private string FormatFilePath(string fileName)
	{
		string result = null;
		string text = (Path.HasExtension(fileName) ? fileName : (fileName + "." + _extension));
		if (!string.IsNullOrEmpty(_root) && !string.IsNullOrEmpty(text))
		{
			result = Path.Combine(_root, text);
		}
		return result;
	}

	private Type GetCachedSerializationType(string assemblyName, string typeName)
	{
		Type value = null;
		Dictionary<string, Type> value2 = null;
		_cachedSerializationTypes.TryGetValue(assemblyName, out value2);
		value2?.TryGetValue(typeName, out value);
		return value;
	}

	private void AddCachedSerializationType(string assemblyName, string typeName, Type type)
	{
		if (GetCachedSerializationType(assemblyName, typeName) == null)
		{
			Dictionary<string, Type> value = null;
			_cachedSerializationTypes.TryGetValue(assemblyName, out value);
			if (value == null)
			{
				value = new Dictionary<string, Type>();
			}
			value[typeName] = type;
			_cachedSerializationTypes[assemblyName] = value;
		}
	}
}
