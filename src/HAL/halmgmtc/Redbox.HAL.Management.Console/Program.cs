using System;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using log4net;

namespace Redbox.HAL.Management.Console;

public class Program : IDisposable, ILogger
{
	private static readonly ILog m_log = LogManager.GetLogger(typeof(Program));

	private NamedLock m_instanceLock;

	private static Guid m_applicationGuid = new Guid("{E3720F11-5B72-4d86-A1A8-445518455D4C}");

	public static string Version => typeof(Program).Assembly.GetName().Version.ToString();

	public void Configure()
	{
	}

	public void Configure(string path)
	{
	}

	public void ConfigureAndWatch(string path)
	{
	}

	public void Dispose()
	{
		if (m_instanceLock != null)
		{
			m_instanceLock.Dispose();
		}
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return entryLogLevel switch
		{
			LogEntryType.Info => m_log.IsInfoEnabled, 
			LogEntryType.Debug => m_log.IsDebugEnabled, 
			LogEntryType.Error => m_log.IsErrorEnabled, 
			LogEntryType.Fatal => m_log.IsFatalEnabled, 
			_ => false, 
		};
	}

	public void Log(string message, Exception e)
	{
		m_log.Error(message, e);
	}

	public void Log(string message, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message);
			break;
		case LogEntryType.Error:
			m_log.Error(message);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message);
			break;
		}
	}

	public void Log(string message, params object[] parms)
	{
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message, e);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message, e);
			break;
		case LogEntryType.Error:
			m_log.Error(message, e);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message, e);
			break;
		}
	}

	public void Run()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new Form1());
	}

	[STAThread]
	public static void Main()
	{
		using Program program = new Program();
		program.m_instanceLock = new NamedLock(m_applicationGuid.ToString(), LockScope.Local);
		if (!program.m_instanceLock.IsOwned)
		{
			MessageBox.Show("Only one instance of the Console is allowed to run.");
			return;
		}
		ServiceLocator.Instance.AddService(typeof(ILogger), program);
		program.Run();
	}
}
