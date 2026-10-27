using System;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.CameraTuner;

public class Program : IDisposable
{
	private bool Disposed;

	private readonly NamedLock InstanceLock;

	private readonly Guid AppGuid = new Guid("96C0A029-9835-41B5-8F45-53E9AD339B6D");

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	~Program()
	{
		DisposeInner(fromDispose: false);
	}

	public void Run(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), new RuntimeService());
		ServiceLocator.Instance.AddService(typeof(IEncryptionService), new TripleDesEncryptionService());
		TunerLog tunerLog = new TunerLog();
		ServiceLocator.Instance.AddService(typeof(ILogger), tunerLog);
		BarcodeConfiguration.MakeNewInstance2();
		BarcodeReaderFactory barcodeReaderFactory = new BarcodeReaderFactory();
		ServiceLocator.Instance.AddService(typeof(IBarcodeReaderFactory), barcodeReaderFactory);
		barcodeReaderFactory.Initialize(new ErrorList());
		Application.Run(new MainForm(tunerLog));
	}

	[STAThread]
	public static void Main(string[] args)
	{
		using Program program = new Program();
		if (!program.InstanceLock.IsOwned)
		{
			Environment.Exit(10);
		}
		program.Run(args);
	}

	public Program()
	{
		InstanceLock = new NamedLock(AppGuid.ToString());
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			if (fromDispose)
			{
				InstanceLock.Dispose();
			}
		}
	}
}
