using System;
using System.Collections.Generic;
using System.IO.Ports;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class PortManager
{
	public delegate bool PortTest(RedboxSerialPort port);

	private const string MsgHeader = "[Port Scanner]";

	private readonly List<string> InUsePorts = new List<string>();

	private readonly object ScanLock = new object();

	public static PortManager Instance => Singleton<PortManager>.Instance;

	public RedboxSerialPort Scan(RedboxSerialPortConfiguration conf, PortTest probe, PortModes mode)
	{
		lock (ScanLock)
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RedboxSerialPort result = TestPorts(conf, probe, mode);
			executionTimer.Stop();
			LogHelper.Instance.Log("[Port Scan] Time to scan ports: {0}ms", executionTimer.ElapsedMilliseconds);
			return result;
		}
	}

	public RedboxSerialPort Scan(string tryFirst, RedboxSerialPortConfiguration conf, PortTest probe, PortModes mode)
	{
		lock (ScanLock)
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RedboxSerialPort redboxSerialPort = ScanInner(tryFirst, conf, probe, mode);
			if (redboxSerialPort == null)
			{
				redboxSerialPort = TestPorts(conf, probe, mode);
			}
			executionTimer.Stop();
			LogHelper.Instance.Log("[Port Scan] Time to scan ports: {0}ms", executionTimer.ElapsedMilliseconds);
			return redboxSerialPort;
		}
	}

	internal void Register(string port)
	{
		lock (ScanLock)
		{
			if (InUsePorts.Find((string each) => each.Equals(port, StringComparison.CurrentCultureIgnoreCase)) == null)
			{
				InUsePorts.Add(port);
			}
		}
	}

	internal void Remove(string port)
	{
		lock (ScanLock)
		{
			if (!InUsePorts.Remove(port))
			{
				LogHelper.Instance.Log("[PortManager] Removing port {0} returned FALSE.", port);
			}
		}
	}

	private RedboxSerialPort ScanInner(string portName, RedboxSerialPortConfiguration conf, PortTest probe, PortModes mode)
	{
		string text = InUsePorts.Find((string each) => each.Equals(portName, StringComparison.CurrentCultureIgnoreCase));
		if (text != null)
		{
			LogHelper.Instance.Log("{0} The port {1} is reported in-use by the port manager.", "[Port Scanner]", portName);
			return null;
		}
		conf.PortName = portName;
		RedboxSerialPort redboxSerialPort = RedboxSerialPort.Create(conf, mode);
		LogHelper.Instance.Log("{0} try port {1}", "[Port Scanner]", portName);
		if (!redboxSerialPort.Open())
		{
			LogHelper.Instance.Log("{0} The port wouldn't open.", "[Port Scanner]");
			redboxSerialPort.Dispose();
			return null;
		}
		if (probe(redboxSerialPort))
		{
			return redboxSerialPort;
		}
		redboxSerialPort.Dispose();
		return null;
	}

	private RedboxSerialPort TestPorts(RedboxSerialPortConfiguration conf, PortTest probe, PortModes mode)
	{
		string[] portNames = SerialPort.GetPortNames();
		foreach (string portName in portNames)
		{
			RedboxSerialPort redboxSerialPort = ScanInner(portName, conf, probe, mode);
			if (redboxSerialPort != null)
			{
				return redboxSerialPort;
			}
		}
		return null;
	}

	private PortManager()
	{
	}
}
