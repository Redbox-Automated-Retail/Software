using System;
using HALUtilities.KioskTest;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class KioskTestRunner : IDisposable
{
	private bool Disposed;

	private readonly double? RunTime;

	private readonly KioskTestLogger Logger;

	private readonly NamedLock m_instanceLock;

	private readonly Guid ProgramLock = new Guid("{7B780DBE-F37B-4f4a-83FA-C59F5B2C87D2}");

	private readonly AbstractKioskTest Test;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Test.Dispose();
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
			Logger.Dispose();
		}
	}

	internal static int RunKioskTest(string[] args, HardwareService s)
	{
		int result = -1;
		using (GetMachineInfoExecutor getMachineInfoExecutor = new GetMachineInfoExecutor(s))
		{
			getMachineInfoExecutor.Run();
			if (HardwareJobStatus.Completed != getMachineInfoExecutor.EndStatus || getMachineInfoExecutor.Info.Configuration == KioskConfiguration.None)
			{
				Console.WriteLine("Unable to determine machine configuration.");
			}
			else
			{
				using KioskTestRunner kioskTestRunner = new KioskTestRunner(getMachineInfoExecutor.Info, args, s);
				result = kioskTestRunner.Run(getMachineInfoExecutor.Info);
			}
		}
		return result;
	}

	private KioskTestRunner(MachineInformation config, string[] args, HardwareService service)
	{
		Logger = new KioskTestLogger();
		ServiceLocator.Instance.AddService<ILogger>(Logger);
		if (config.Configuration == KioskConfiguration.None)
		{
			LogHelper.Instance.Log("Unsupported test configuration {0}", config);
			throw new ArgumentException();
		}
		switch (config.Configuration)
		{
		case KioskConfiguration.R717:
			Test = new KioskTest_R717(service);
			break;
		case KioskConfiguration.R504:
		case KioskConfiguration.R630:
			Test = new QlmKioskTest(config.Configuration, service);
			break;
		}
		if (args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("--runtime", StringComparison.CurrentCultureIgnoreCase))
				{
					RunTime = CommandLineOption.GetOptionVal(args[i], RunTime);
				}
				else if (args[i] == "-help")
				{
					Console.WriteLine("\nList of Commands:\n--------------------\n");
					Console.WriteLine("\t--runTime:{number}\tSet the number of hours for test to run");
					Console.WriteLine("\t--vend:{number}\tSet the number of moves between each vend attempt, 0 prevents vends from being attempted");
					Console.WriteLine("\t-excludeAroundDump\tIgnores slots 81 & 85 on deck 8 VMZ machines.");
					Console.WriteLine("\t--quickTest:{true|false}\tTests disk at slots 1, 30 and 60 on each deck. Assumes a disk at 1,1 on each deck.");
					Console.WriteLine("\t--rewriteDataOnSuccess:{true|false}\tOn successful test, updates GAMP data with new values.");
					Console.WriteLine("\t-oneDiskQuickTest\tPerforms a quick test at slots 1, 30 and 60 on each deck. Assumes single disk at 1,1.");
					Console.WriteLine("\t--testDisksAroundBin:{true|false}\tPerforms a test on slots adjacent to the dumpbin.");
					Console.WriteLine("\t-help\t\t\tPrints this screen");
					Environment.Exit(0);
				}
				else
				{
					Test.AcceptArgument(args[i]);
				}
			}
		}
		m_instanceLock = new NamedLock(ProgramLock.ToString());
	}

	private int Run(MachineInformation config)
	{
		if (!m_instanceLock.IsOwned)
		{
			Console.WriteLine("Only one instance of the Kiosk Test is allowed to run - bailing test.");
			return -2;
		}
		TestLocation testLocation = new TestLocation(1, 1);
		if (Test.SingleTestOnly())
		{
			return Test.RunSingleTests(testLocation, config);
		}
		TimeSpan? runTime = null;
		if (RunTime.HasValue)
		{
			runTime = TimeSpan.FromHours(RunTime.Value);
		}
		return Test.Run(runTime, testLocation, config);
	}
}
