using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KioskAdmin;

public class LogService
{
	private static LogService? _logService;

	private List<Pair> _log = new List<Pair>();

	public static LogService Instance
	{
		get
		{
			if (_logService == null)
			{
				_logService = new LogService();
			}
			return _logService;
		}
	}

	private LogService()
	{
	}

	public static void Log(string name, string value)
	{
		Instance.LogItem(name, value);
	}

	public void LogItem(string name, string value)
	{
		_log.Add(new Pair
		{
			Name = name,
			Value = value
		});
		Console.WriteLine($"{name}, {value}");
	}

	public void DumpLog()
	{
		DumpLog(null);
	}

	public void DumpLog(string? fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = "output.txt";
		}
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		StringBuilder sb = new StringBuilder();
		_log.ToList().ForEach(delegate(Pair pair)
		{
			sb.AppendLine($"\"{pair.Name}\",\"{pair.Value.StripQuote()}\"");
		});
		File.WriteAllText(Path.Combine(directoryName, fileName), sb.ToString());
		_log.Clear();
	}
}
