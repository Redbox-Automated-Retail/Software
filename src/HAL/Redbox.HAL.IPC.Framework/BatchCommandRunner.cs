using System;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public sealed class BatchCommandRunner : ISession, IMessageSink
{
	private readonly ErrorList m_errorList = new ErrorList();

	private readonly StreamReader m_reader;

	public bool LogDetailedMessages { get; set; }

	internal ErrorList Errors => m_errorList;

	public event EventHandler Disconnect
	{
		add
		{
		}
		remove
		{
		}
	}

	public BatchCommandRunner(StreamReader reader)
	{
		m_reader = reader;
		LogDetailedMessages = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug);
	}

	public bool Send(string message)
	{
		return true;
	}

	public static ErrorList ExecuteStartupFiles()
	{
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt");
		ErrorList errorList = new ErrorList();
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			ExecuteFile(array[i], errorList);
		}
		return errorList;
	}

	public static void ExecuteFile(string fileName, ErrorList errorList)
	{
		using StreamReader reader = new StreamReader(fileName);
		BatchCommandRunner batchCommandRunner = new BatchCommandRunner(reader);
		batchCommandRunner.Start();
		if (batchCommandRunner.Errors.Count > 0)
		{
			errorList.AddRange(batchCommandRunner.Errors);
			batchCommandRunner.Errors.Clear();
		}
	}

	public ErrorList ExecuteFile()
	{
		Start();
		return Errors;
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
			if (text.Length <= 0)
			{
				continue;
			}
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log(text);
			}
			if (text.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
			{
				break;
			}
			CommandResult commandResult = CommandService.Instance.Execute(this, text);
			if (commandResult != null)
			{
				ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
				if (LogDetailedMessages)
				{
					LogEntryType type = ((commandResult.Errors.Count > 0) ? LogEntryType.Error : LogEntryType.Info);
					LogHelper.Instance.Log(commandResult.ToString(), type);
				}
				if (commandResult.Errors.ContainsError())
				{
					Errors.AddRange(commandResult.Errors);
					break;
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
