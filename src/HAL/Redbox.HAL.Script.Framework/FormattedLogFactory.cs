using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

public sealed class FormattedLogFactory : IFormattedLogFactoryService
{
	private class NullLog : IFormattedLog
	{
		public void WriteFormatted(string msg)
		{
		}

		public void WriteFormatted(string msg, LogEntryType type)
		{
		}

		public void WriteFormatted(string format, params object[] stuff)
		{
		}

		public void WriteFormatted(LogEntryType type, string format, params object[] stuff)
		{
		}
	}

	private readonly IFormattedLog m_nilLog = new NullLog();

	public string LogsBasePath => "c:\\Program Files\\Redbox\\KioskLogs";

	public IFormattedLog NilLog => m_nilLog;

	public string CreateSubpath(string subfolder)
	{
		string text = Path.Combine(LogsBasePath, subfolder);
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("[FormattedLogFactory] Unable to create path {0}: {1}", text, ex.Message);
			}
		}
		return text;
	}
}
