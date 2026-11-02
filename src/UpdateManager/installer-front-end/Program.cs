using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

public class Program : IDisposable
{
	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

	private static CommandLine? m_commandLine;

	private static CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Opts);
			}
			return m_commandLine;
		}
	}

	private static Options Opts => Singleton<Options>.Instance;

	[STAThread]
	public static int Main()
	{
#if !NETFRAMEWORK
		Redbox.Core.NetFrameworkCompatibility.ImportExeConfig();
#endif
		using Program program = new Program();
		return program.Run();
	}

	public int Run()
	{
		ErrorList errorList = new ErrorList();
		if (CommandLine.Errors.Count > 0 && !CommandLine.HelpRequested)
		{
			CommandLine.WriteUsage(includeHelp: false);
			Console.Error.WriteLine("Command Line errors forcing exit.");
			return 1;
		}
		if (CommandLine.HelpRequested)
		{
			CommandLine.WriteUsage(includeHelp: true);
			return 0;
		}
		try
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "installer-front-end-log4net.config"));
			LogHelper.Instance.Logger = m_logger;
			errorList.AddRange(StoreInstallerApplication.Instance.Initialize(Opts.Script, Opts.StoreNumber));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Program.Run.", e));
		}
		if (errorList.ContainsCode("INSTANCE"))
		{
			return -1;
		}
		if (errorList.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errorList;
			errorForm.ShowDialog();
			return -2;
		}
		if (!StoreInstallerApplication.Instance.Run())
		{
			return -3;
		}
		Application.Run(StoreInstallerApplication.Instance);
		if (!StoreInstallerApplication.Instance.ScriptCompleted)
		{
			return -1;
		}
		return 0;
	}

	public void Dispose()
	{
	}
}
