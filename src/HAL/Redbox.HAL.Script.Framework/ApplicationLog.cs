using System;
using System.IO;
using System.Runtime.Serialization;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ApplicationLog : IFormattedLog
{
	internal readonly ExecutionContext Context;

	private TextWriter Log = StreamWriter.Null;

	private bool m_open;

	private bool Configured;

	private string Directory;

	private string FileName;

	private const int MaxFileSize = 1024000;

	private const int BackDays = 60;

	private const int BackDaysForImages = 168;

	internal bool AppendToExisting { get; private set; }

	internal bool FlushOnWrite { get; private set; }

	public static implicit operator TextWriter(ApplicationLog log)
	{
		return log.Log;
	}

	public void WriteFormatted(string msg)
	{
		Write(msg);
	}

	public void WriteFormatted(string fmt, params object[] parm)
	{
		string msg = string.Format(fmt, parm);
		Write(msg);
	}

	internal void Serialize(SerializationInfo info)
	{
		info.AddValue("AppHasLog", Configured);
		if (Configured)
		{
			info.AddValue("AppLogDirectory", Directory);
			info.AddValue("AppLogName", FileName);
			info.AddValue("AppLogFlushOnWrite", FlushOnWrite);
			info.AddValue("AppLogAppendToExisting", AppendToExisting);
		}
	}

	internal static ApplicationLog Deserialize(SerializationInfo info, ExecutionContext context)
	{
		ApplicationLog applicationLog = new ApplicationLog(context);
		if (!info.GetBoolean("AppHasLog"))
		{
			return applicationLog;
		}
		applicationLog.Directory = info.GetString("AppLogDirectory");
		applicationLog.FileName = info.GetString("AppLogName");
		applicationLog.AppendToExisting = info.GetBoolean("AppLogAppendToExisting");
		applicationLog.FlushOnWrite = info.GetBoolean("AppLogFlushOnWrite");
		applicationLog.Configured = true;
		return applicationLog;
	}

	internal static ApplicationLog ConfigureLog(ExecutionContext ctx, bool logAppends, string subDirectory, bool flushOnWrite, string overrideName)
	{
		string fileName = ((!logAppends) ? $"{ctx.ProgramName}-{ctx.ID}.log" : $"{(string.IsNullOrEmpty(overrideName) ? ctx.ProgramName : overrideName)}.log");
		if (ControllerConfiguration.Instance.IsVMZMachine && ctx.ProgramName.Equals("qlm-unload"))
		{
			fileName = $"VMZ-Unload-{ctx.ID}.log";
		}
		ctx.AppLog.ConfigureAndOpen(subDirectory, fileName, flushOnWrite, logAppends);
		return ctx.AppLog;
	}

	internal void Write(string msg)
	{
		try
		{
			string arg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
			string value = (AppendToExisting ? $"{arg} {Context.ID} {msg}" : $"{DateTime.Now.ToShortTimeString()} {DateTime.Now.ToShortDateString()} {msg}");
			Log.WriteLine(value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("APPLOG caught an exception writing data.", e);
		}
	}

	internal void Close()
	{
		if (m_open)
		{
			Log.Flush();
			Log.Close();
			m_open = false;
			Log = StreamWriter.Null;
		}
	}

	internal ApplicationLog(ExecutionContext ctx)
	{
		Context = ctx;
		Log = StreamWriter.Null;
	}

	internal bool Open()
	{
		if (!Configured)
		{
			return false;
		}
		if (m_open)
		{
			return true;
		}
		string text = Path.Combine(Directory, FileName);
		try
		{
			Log = new StreamWriter(File.Open(text, FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = FlushOnWrite
			};
			return m_open = true;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log($"Unable to open file {text}", LogEntryType.Error);
			Log = StreamWriter.Null;
			return false;
		}
	}

	private void ConfigureAndOpen(string directory, string fileName, bool flushOnWrite, bool appends)
	{
		string logsBasePath = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().LogsBasePath;
		string text = logsBasePath;
		if (!string.IsNullOrEmpty(directory))
		{
			text = Path.Combine(logsBasePath, directory);
			if (!System.IO.Directory.Exists(text))
			{
				try
				{
					System.IO.Directory.CreateDirectory(text);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create subdirectory.", e);
					text = logsBasePath;
				}
			}
		}
		Configured = true;
		Directory = text;
		FileName = fileName;
		FlushOnWrite = flushOnWrite;
		AppendToExisting = appends;
		string text2 = Path.Combine(Directory, FileName);
		if (AppendToExisting && File.Exists(text2) && new FileInfo(text2).Length >= 1024000)
		{
			CreateBackup(text2);
		}
		Open();
	}

	private void CreateBackup(string originalPath)
	{
		DateTime now = DateTime.Now;
		string path = $"{FileName}-m{now.Month}d{now.Day}y{now.Year}h{now.Hour}m{now.Minute}s{now.Second}.log";
		string text = Path.Combine(Directory, path);
		try
		{
			File.Move(originalPath, text);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to move {originalPath} to {text}.", e);
		}
	}
}
