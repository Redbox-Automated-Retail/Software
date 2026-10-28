using System;
using System.ServiceProcess;

namespace Redbox.HAL.Core;

public sealed class ServiceHelper
{
	public const string HALService = "halsvc$default";

	private const int ServiceTimeout = 30000;

	public static ServiceHelper Instance => Singleton<ServiceHelper>.Instance;

	public bool StopHALService(bool check)
	{
		return StopServiceInner("halsvc$default", null, check);
	}

	public bool StopService(string name, int? to, bool checkRunning)
	{
		return StopServiceInner(name, to, checkRunning);
	}

	public bool StartHALService()
	{
		return StartServiceInner("halsvc$default", 30000);
	}

	public bool StartService(string name, int? to)
	{
		return StartServiceInner(name, to);
	}

	public bool ServiceIsRunning(string name)
	{
		try
		{
			ServiceControllerStatus status = new ServiceController(name).Status;
			return status == ServiceControllerStatus.Running;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	public string ServiceStatus(string name)
	{
		try
		{
			return new ServiceController(name).Status.ToString();
		}
		catch (InvalidOperationException)
		{
			return "NOT INSTALLED";
		}
	}

	private TimeSpan GetTimeout(int? to)
	{
		return TimeSpan.FromMilliseconds((!to.HasValue) ? 30000 : to.Value);
	}

	private bool StopServiceInner(string name, int? to, bool checkRunning)
	{
		ServiceController serviceController = new ServiceController(name);
		try
		{
			TimeSpan timeout = GetTimeout(to);
			serviceController.Stop();
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			if (!checkRunning)
			{
				return true;
			}
			using ExecutionTimer executionTimer = new ExecutionTimer();
			do
			{
				PerfFunctions.Wait(1500);
				if (!ProcessHelper.ProcessIsRunning(name))
				{
					return true;
				}
			}
			while (executionTimer.ElapsedMilliseconds <= 30000);
			executionTimer.Stop();
			return false;
		}
		catch
		{
			return false;
		}
	}

	private bool StartServiceInner(string name, int? to)
	{
		ServiceController serviceController = new ServiceController(name);
		try
		{
			TimeSpan timeout = GetTimeout(to);
			serviceController.Start();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private ServiceHelper()
	{
	}
}
