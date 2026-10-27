using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ObjectCacheService : IObjectCacheService
{
	private readonly IDictionary<string, object> m_objects = new Dictionary<string, object>();

	public static ObjectCacheService Instance => Singleton<ObjectCacheService>.Instance;

	public T GetObject<T>(string name)
	{
		if (!m_objects.ContainsKey(name))
		{
			return default(T);
		}
		return (T)ConversionHelper.ChangeType(m_objects[name], typeof(T));
	}

	public void SetObject<T>(string name, T value)
	{
		m_objects[name] = value;
	}

	private ObjectCacheService()
	{
	}
}
