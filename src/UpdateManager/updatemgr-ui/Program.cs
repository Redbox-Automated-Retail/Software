using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Log.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class Program : IDisposable
{
	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

	[STAThread]
	public static void Main()
	{
		using Program program = new Program();
		program.Run();
	}

	public void Run()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "updatemgr-ui-log4net.config"));
			LogHelper.Instance.Logger = m_logger;
			errorList.AddRange(UpdateManagerApplication.Instance.Initialize());
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E989", "An unhandled exception was thrown in Program.Run.", e));
		}
		if (!errorList.ContainsCode("INSTANCE"))
		{
			if (errorList.ContainsError())
			{
				ErrorForm errorForm = new ErrorForm();
				errorForm.Errors = errorList;
				errorForm.ShowDialog();
			}
			else if (UpdateManagerApplication.Instance.Run())
			{
				Application.Run(UpdateManagerApplication.Instance);
			}
		}
	}

	public void Dispose()
	{
	}
}
