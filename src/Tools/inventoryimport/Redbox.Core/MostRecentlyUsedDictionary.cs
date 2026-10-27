using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Redbox.Core;

public class MostRecentlyUsedDictionary<K, V>
{
	private readonly LinkedList<V> m_list = new LinkedList<V>();

	private readonly HybridDictionary m_linkToKey = new HybridDictionary();

	private readonly Dictionary<K, LinkedListNode<V>> m_dictionary = new Dictionary<K, LinkedListNode<V>>();

	public V this[K key]
	{
		get
		{
			if (!m_dictionary.TryGetValue(key, out var value))
			{
				return default(V);
			}
			m_list.Remove(value);
			m_list.AddFirst(value);
			return value.Value;
		}
		set
		{
			if (m_dictionary.ContainsKey(key))
			{
				LinkedListNode<V> linkedListNode = m_dictionary[key];
				linkedListNode.Value = value;
				m_list.Remove(linkedListNode);
				m_list.AddFirst(linkedListNode);
				m_dictionary[key] = linkedListNode;
				m_linkToKey[linkedListNode] = key;
			}
			else
			{
				Add(key, value);
			}
		}
	}

	public ICollection<LinkedListNode<V>> Values => m_dictionary.Values;

	public ICollection<K> Keys => m_dictionary.Keys;

	public int Capacity { get; set; }

	public event PurgedFromDictionaryDelegate<K, V> PurgedFromDictionary;

	public MostRecentlyUsedDictionary()
	{
		Capacity = 50;
	}

	public MostRecentlyUsedDictionary(int maxItems)
	{
		Capacity = maxItems;
		Capacity = 50;
	}

	public void Add(K key, V value)
	{
		LinkedListNode<V> linkedListNode = m_list.AddFirst(value);
		m_dictionary.Add(key, linkedListNode);
		m_linkToKey[linkedListNode] = key;
		if (m_dictionary.Keys.Count < Capacity)
		{
			return;
		}
		LinkedListNode<V> last = m_list.Last;
		if (last != null)
		{
			K key2 = (K)m_linkToKey[last];
			if (PurgedFromDictionary != null && PurgedFromDictionary.GetInvocationList().Length > 0)
			{
				PurgedFromDictionary(key2, last.Value);
			}
			Remove(key2);
		}
	}

	public bool Contains(K key)
	{
		bool flag = m_dictionary.ContainsKey(key);
		if (flag)
		{
			LinkedListNode<V> node = m_dictionary[key];
			m_list.Remove(node);
			m_list.AddFirst(node);
		}
		return flag;
	}

	public void Remove(K key)
	{
		if (m_dictionary.TryGetValue(key, out var value))
		{
			m_dictionary.Remove(key);
			if (value != null)
			{
				m_list.Remove(value);
				m_linkToKey.Remove(value);
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(Capacity);
		stringBuilder.Append("[");
		foreach (V item in m_list)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.ToString());
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
