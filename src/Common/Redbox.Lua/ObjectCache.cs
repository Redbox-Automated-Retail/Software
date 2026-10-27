using System;
using System.Collections.Generic;

namespace Redbox.Lua;

public class ObjectCache
{
	private int m_id;

	private readonly IDictionary<int, object> m_idToObject = new Dictionary<int, object>();

	private readonly IDictionary<object, int> m_objectToId = new Dictionary<object, int>();

	public ObjectCache()
	{
		m_id = 1;
	}

	public object GetObject(int id)
	{
		if (m_idToObject.ContainsKey(id))
		{
			return m_idToObject[id];
		}
		return null;
	}

	public void RemoveId(int id)
	{
		if (m_idToObject.ContainsKey(id))
		{
			object obj = m_idToObject[id];
			m_objectToId.Remove(obj);
			m_idToObject.Remove(id);
			if (obj is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public int CacheObject(object instance)
	{
		if (m_objectToId.ContainsKey(instance))
		{
			return m_objectToId[instance];
		}
		m_idToObject[m_id] = instance;
		m_objectToId[instance] = m_id;
		int id = m_id;
		m_id++;
		return id;
	}

	public void RemoveObject(object instance)
	{
		if (m_objectToId.ContainsKey(instance))
		{
			int key = m_objectToId[instance];
			m_objectToId.Remove(instance);
			m_idToObject.Remove(key);
			if (instance is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}
