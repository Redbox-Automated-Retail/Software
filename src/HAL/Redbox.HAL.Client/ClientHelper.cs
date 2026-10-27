using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public sealed class ClientHelper : IDisposable
{
	private bool Disposed;

	private readonly IClientOutputSink Sink;

	private readonly NullSink NullInstance = new NullSink();

	public HardwareService Service { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}

	public bool BootstrapInitRunning()
	{
		if (Service == null)
		{
			Sink.WriteMessage("The service is not configured.");
			return false;
		}
		return BootstrapInitRunningChecked();
	}

	public bool WaitforInit()
	{
		return WaitforInit(500);
	}

	public bool WaitforInit(int pause)
	{
		Sink.WriteMessage("Checking for bootstrap init.");
		DateTime dateTime = DateTime.Now;
		TimeSpan value = new TimeSpan(0, 0, 5);
		while (BootstrapInitRunningChecked())
		{
			DateTime now = DateTime.Now;
			if (now.Subtract(value) >= dateTime)
			{
				dateTime = now;
				Sink.WriteMessage("{0}: still waiting for bootstrap init.", now.ToLongTimeString());
			}
			Thread.Sleep(pause);
		}
		return true;
	}

	public HardwareService Connect()
	{
		return ConnectInner(Constants.HALIPCStrings.TcpServer);
	}

	public HardwareService Connect(string ipc)
	{
		if (string.IsNullOrEmpty(ipc))
		{
			throw new UriFormatException("The URI string is null or empty - please re-configure.");
		}
		return ConnectInner(ipc);
	}

	public bool TestCommunication()
	{
		if (Service == null)
		{
			ConnectInner(Constants.HALIPCStrings.TcpServer);
		}
		return TestServiceConnection(Service);
	}

	public bool WaitForJob(HardwareJob job, out HardwareJobStatus endStatus)
	{
		endStatus = HardwareJobStatus.Completed;
		try
		{
			HardwareJobStatus _s0 = HardwareJobStatus.Completed;
			bool waitForJob = true;
			job.StatusChanged += delegate(HardwareJob j, HardwareJobStatus status)
			{
				_s0 = status;
				waitForJob = status != HardwareJobStatus.Stopped && status != HardwareJobStatus.Errored && status != HardwareJobStatus.Garbage && status != HardwareJobStatus.Completed && status != HardwareJobStatus.Suspended;
			};
			if (!job.Connect().Success || !job.Pend().Success)
			{
				return false;
			}
			while (waitForJob)
			{
				Thread.Sleep(250);
			}
			endStatus = _s0;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			job.Disconnect();
		}
	}

	public bool ExecuteImmediate(string inst)
	{
		if (Service == null)
		{
			return false;
		}
		HardwareJob job;
		return Service.ExecuteImmediate(inst, out job).Success;
	}

	public T ExecuteImmediateAndGetResult<T>(HardwareService service, string inst)
	{
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(inst, out var job);
		if (hardwareCommandResult == null || !hardwareCommandResult.Success)
		{
			return default(T);
		}
		return ConversionHelper.ChangeType<T>(GetStackEntriesInner(job, 1)[0]);
	}

	public ClientHelper(IClientOutputSink sink, HardwareService service)
	{
		IClientOutputSink sink2;
		if (sink != null)
		{
			sink2 = sink;
		}
		else
		{
			IClientOutputSink nullInstance = NullInstance;
			sink2 = nullInstance;
		}
		Sink = sink2;
		Service = service;
	}

	public ClientHelper(HardwareService service)
		: this(null, service)
	{
	}

	private HardwareService ConnectInner(string ipcUrl)
	{
		try
		{
			HardwareService hardwareService = new HardwareService(IPCProtocol.Parse(ipcUrl));
			return TestServiceConnection(hardwareService) ? hardwareService : null;
		}
		catch (UriFormatException)
		{
			return Service = null;
		}
	}

	private bool TestServiceConnection(HardwareService service)
	{
		if (service == null)
		{
			return false;
		}
		HardwareCommandResult hardwareCommandResult = service.ExecuteServiceCommand("SERVICE test-comm", 5000);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		return hardwareCommandResult.CommandMessages[0] == "ACK";
	}

	private string FindLocalIP()
	{
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		return null;
	}

	private string[] GetStackEntriesInner(HardwareJob job, int depth)
	{
		string[] array = new string[depth];
		for (int i = 0; i < depth; i++)
		{
			array[i] = string.Empty;
		}
		if (job.GetStack(out var stack).Success && stack.Count >= depth)
		{
			for (int j = 0; j < depth; j++)
			{
				array[j] = stack.Pop();
			}
		}
		return array;
	}

	private bool BootstrapInitRunningChecked()
	{
		if (!Service.GetInitStatus(out var status).Success)
		{
			Sink.WriteMessage("Unable to determine init status.");
			return false;
		}
		if ("COMPLETED" == status || "ERRORED" == status)
		{
			Sink.WriteMessage("Boot init ended with status: {0}", status);
			return false;
		}
		return true;
	}
}
