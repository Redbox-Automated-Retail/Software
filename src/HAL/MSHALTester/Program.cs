using System;
using System.Windows.Forms;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.MSHALTester;

public class Program : IDisposable
{
	private NamedLock m_instanceLock;

	private static Guid m_applicationGuid = new Guid("{8609B87B-126E-489a-98E9-6818639534DF}");

	[STAThread]
	public static void Main(string[] args)
	{
		using Program program = new Program();
		program.m_instanceLock = new NamedLock(m_applicationGuid.ToString());
		if (!program.m_instanceLock.IsOwned)
		{
			MessageBox.Show("Only one instance of the MS HAL tester is allowed to run.");
			return;
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		bool secure = false;
		string text = "UNKNOWN";
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].Equals("--secure", StringComparison.CurrentCultureIgnoreCase))
			{
				secure = true;
			}
			else if (args[i].StartsWith("--username"))
			{
				text = CommandLineOption.GetOptionVal(args[i], text);
			}
			else
			{
				Console.WriteLine("Unrecognized option {0}", args[i]);
			}
		}
		Application.Run(new Form1(secure, text));
	}

	public void Dispose()
	{
		if (m_instanceLock != null)
		{
			m_instanceLock.Dispose();
		}
	}
}
