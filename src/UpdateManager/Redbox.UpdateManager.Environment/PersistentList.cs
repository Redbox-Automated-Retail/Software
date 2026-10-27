using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Redbox.Core;

namespace Redbox.UpdateManager.Environment;

public class PersistentList<T> : IEnumerable<T>, IEnumerable
{
	private sealed class PersistentListEnumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private int m_index = -1;

		private readonly PersistentList<T> m_list;

		public T Current => m_list[m_index];

		object IEnumerator.Current => Current;

		public PersistentListEnumerator(PersistentList<T> list)
		{
			m_list = list;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			m_index++;
			return m_index < m_list.Count;
		}

		public void Reset()
		{
			m_index = 0;
		}
	}

	private readonly string m_path;

	public T this[int i]
	{
		get
		{
			string path = Path.Combine(m_path, i.ToString());
			try
			{
				return (!File.Exists(path)) ? default(T) : File.ReadAllText(path).ToObject<T>();
			}
			catch (Exception)
			{
				if (File.Exists(path))
				{
					if (i == 0)
					{
						Clear();
					}
					else
					{
						DecapitateAt(i);
					}
				}
				return default(T);
			}
		}
		set
		{
			EnsureDirectoryExists();
			File.WriteAllText(Path.Combine(m_path, i.ToString()), value.ToJson());
		}
	}

	public int Count
	{
		get
		{
			EnsureDirectoryExists();
			return Directory.GetFiles(m_path).Length;
		}
	}

	public PersistentList(string path)
	{
		m_path = path;
		if (!ConvertIfLegacyFile())
		{
			EnsureDirectoryExists();
			PackList();
		}
	}

	public int Add(T value)
	{
		int count = Count;
		File.WriteAllText(Path.Combine(m_path, count.ToString()), value.ToJson());
		return count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new PersistentListEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		Directory.Delete(m_path, recursive: true);
		EnsureDirectoryExists();
	}

	public void DecapitateAt(int i)
	{
		for (int j = i; j < Count; j++)
		{
			string path = Path.Combine(m_path, j.ToString());
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	public ReadOnlyCollection<T> AsReadOnly()
	{
		EnsureDirectoryExists();
		return new List<T>(this).AsReadOnly();
	}

	public List<T> AsList()
	{
		EnsureDirectoryExists();
		return new List<T>(this);
	}

	public void RemoveAt(int index)
	{
		File.Delete(Path.Combine(m_path, index.ToString()));
		PackList();
	}

	public void RemoveAllAt(IEnumerable<int> indexList)
	{
		foreach (int index in indexList)
		{
			File.Delete(Path.Combine(m_path, index.ToString()));
		}
		PackList();
	}

	private bool ConvertIfLegacyFile()
	{
		if (!File.Exists(m_path))
		{
			return false;
		}
		try
		{
			List<T> list = File.ReadAllText(m_path).ToObject<List<T>>();
			File.Delete(m_path);
			EnsureDirectoryExists();
			foreach (T item in list)
			{
				Add(item);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Conversion of legacy file '{m_path}' failed; removing the file.", e);
			File.Delete(m_path);
		}
		return true;
	}

	private void EnsureDirectoryExists()
	{
		if (!Directory.Exists(m_path))
		{
			Directory.CreateDirectory(m_path);
		}
	}

	private void PackList()
	{
		try
		{
			List<string> source = Directory.GetFiles(m_path).ToList();
			source = source.Select((string f, int i) => Path.GetFileName(f)).ToList();
			source = source.OrderBy((string s) => Convert.ToInt32(s)).ToList();
			for (int num = 0; num < source.Count(); num++)
			{
				if (source[num] != num.ToString())
				{
					File.Move(Path.Combine(m_path, source[num]), Path.Combine(m_path, num.ToString()));
				}
			}
		}
		catch (Exception)
		{
			Clear();
		}
	}
}
