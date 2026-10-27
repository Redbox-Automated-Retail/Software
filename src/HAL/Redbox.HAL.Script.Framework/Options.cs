using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class Options
{
	private string m_file;

	private IDictionary<string, object> m_options = new Dictionary<string, object>();

	private bool FileValid
	{
		get
		{
			if (string.IsNullOrEmpty(m_file) || !File.Exists(m_file))
			{
				LogHelper.Instance.Log($"Options file {m_file} doesn't exist.", LogEntryType.Info);
				return false;
			}
			return true;
		}
	}

	internal Options(string file, bool read)
	{
		if (File.Exists(file))
		{
			m_file = file;
			if (read)
			{
				ReadInner();
			}
		}
	}

	internal T GetValue<T>(string name, T def)
	{
		if (!m_options.ContainsKey(name))
		{
			return def;
		}
		try
		{
			return (T)Convert.ChangeType(m_options[name], typeof(T));
		}
		catch (Exception)
		{
			LogHelper.Instance.Log($"Unable to convert option {name}", LogEntryType.Info);
			return def;
		}
	}

	internal void Read()
	{
		ReadInner();
	}

	private void ReadInner()
	{
		if (!FileValid)
		{
			return;
		}
		using TextReader textReader = new StreamReader(m_file);
		while (true)
		{
			string text = textReader.ReadLine();
			if (text == null)
			{
				break;
			}
			string[] array = text.Split(',');
			if (array.Length == 2)
			{
				m_options[array[0]] = array[1];
			}
		}
	}
}
