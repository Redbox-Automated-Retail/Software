using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public class BatchCommandRunner : ISession, IMessageSink
{
	private Action<string, string> _paramAction;

	private Action<ISession> _beforeCommandAction;

	private List<string> _filters = new List<string>();

	private bool _enableFilters;

	private ParameterDictionary m_context;

	private readonly StreamReader m_reader;

	private IDictionary<string, object> m_properties;

	public List<string> Filters => _filters;

	public bool EnableFilters
	{
		get
		{
			return _enableFilters;
		}
		set
		{
			_enableFilters = value;
		}
	}

	public ParameterDictionary Context
	{
		get
		{
			if (m_context == null)
			{
				m_context = new ParameterDictionary();
			}
			return m_context;
		}
	}

	public IDictionary<string, object> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, object>();
			}
			return m_properties;
		}
	}

	public event EventHandler Disconnect;

	public BatchCommandRunner(StreamReader reader)
	{
		m_reader = reader;
	}

	public bool Send(string message)
	{
		return true;
	}

	public void SetParamAction(Action<string, string> paramAction)
	{
		_paramAction = paramAction;
	}

	public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
	{
		_beforeCommandAction = beforeCommandAction;
	}

	public void SetFilters(List<string> filters)
	{
		_filters.Clear();
		filters.ForEach(_filters.Add);
	}

	public bool IsConnected()
	{
		return true;
	}

	public static void ExecuteStartupFiles()
	{
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt");
		for (int i = 0; i < files.Length; i++)
		{
			ExecuteFile(files[i]);
		}
	}

	public static void ExecuteFile(string fileName)
	{
		using StreamReader reader = new StreamReader(fileName);
		new BatchCommandRunner(reader).Start();
	}

	public void Start()
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			stringBuilder.Length = 0;
			if (!Read(stringBuilder))
			{
				break;
			}
			string text = stringBuilder.ToString();
			if (text.Length > 0)
			{
				LogHelper.Instance.Log(text, LogEntryType.Info);
				if (text.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					break;
				}
				if (_beforeCommandAction != null)
				{
					_beforeCommandAction(this);
				}
				CommandResult commandResult = CommandService.Instance.Execute(this, string.Empty, text, Filters, EnableFilters, _paramAction);
				if (commandResult != null)
				{
					ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
					LogHelper.Instance.Log(commandResult.ToString(), LogEntryType.Info);
				}
			}
		}
	}

	private bool Read(StringBuilder builder)
	{
		string text = m_reader.ReadLine();
		if (text == null)
		{
			return false;
		}
		builder.Append(text);
		return true;
	}
}
