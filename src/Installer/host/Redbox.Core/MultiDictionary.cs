using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.Core;

public class MultiDictionary<K, V> : IDictionary<K, ICollection<V>>, ICollection<KeyValuePair<K, ICollection<V>>>, IEnumerable<KeyValuePair<K, ICollection<V>>>, IEnumerable
{
	private readonly Dictionary<K, ICollection<V>> m_dict;

	public int Count => m_dict.Count;

	bool ICollection<KeyValuePair<K, ICollection<V>>>.IsReadOnly => false;

	public ICollection<V> this[K key]
	{
		get
		{
			return new HashSet<V>(m_dict[key]);
		}
		set
		{
			m_dict[key] = new HashSet<V>(value);
		}
	}

	public ICollection<K> Keys => m_dict.Keys;

	public ICollection<ICollection<V>> Values => m_dict.Values;

	public MultiDictionary()
		: this((IEqualityComparer<K>)EqualityComparer<K>.Default, (IEqualityComparer<V>)EqualityComparer<V>.Default)
	{
	}

	public MultiDictionary(IEqualityComparer<K> keyCompare)
		: this(keyCompare, (IEqualityComparer<V>)EqualityComparer<V>.Default)
	{
	}

	public MultiDictionary(IEqualityComparer<K> keyCompare, IEqualityComparer<V> valueCompare)
	{
		m_dict = new Dictionary<K, ICollection<V>>(keyCompare);
	}

	public IEnumerator<KeyValuePair<K, ICollection<V>>> GetEnumerator()
	{
		return m_dict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(K key, V value)
	{
		if (!m_dict.ContainsKey(key))
		{
			m_dict[key] = new HashSet<V>();
		}
		m_dict[key].Add(value);
	}

	public void AddMany(K key, ICollection<V> values)
	{
		if (!m_dict.ContainsKey(key))
		{
			m_dict[key] = new HashSet<V>();
		}
		foreach (V value in values)
		{
			m_dict[key].Add(value);
		}
	}

	public void Add(K key, ICollection<V> values)
	{
		AddMany(key, values);
	}

	void ICollection<KeyValuePair<K, ICollection<V>>>.Add(KeyValuePair<K, ICollection<V>> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		m_dict.Clear();
	}

	bool ICollection<KeyValuePair<K, ICollection<V>>>.Contains(KeyValuePair<K, ICollection<V>> item)
	{
		return m_dict.ContainsKey(item.Key);
	}

	public void CopyTo(KeyValuePair<K, ICollection<V>>[] array, int arrayIndex)
	{
		if (array.Length - arrayIndex < m_dict.Count)
		{
			throw new ArgumentException("array.Length is too small.", "array");
		}
		int num = arrayIndex;
		foreach (KeyValuePair<K, ICollection<V>> item in m_dict)
		{
			array[num] = item;
			num++;
		}
	}

	bool ICollection<KeyValuePair<K, ICollection<V>>>.Remove(KeyValuePair<K, ICollection<V>> item)
	{
		return m_dict.Remove(item.Key);
	}

	public bool ContainsKey(K key)
	{
		return m_dict.ContainsKey(key);
	}

	public bool Remove(K key)
	{
		return m_dict.Remove(key);
	}

	public bool TryGetValue(K key, out ICollection<V> value)
	{
		return m_dict.TryGetValue(key, out value);
	}
}
