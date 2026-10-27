using System;
using System.Collections.Generic;
using System.IO.Ports;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Core;

public sealed class PortManager : IPortManagerService
{
	private const string MsgHeader = "[Port Scanner]";

	private readonly List<string> InUsePorts = new List<string>();

	private readonly object ScanLock = new object();

	public bool Register(ICommPort port)
	{
		lock (ScanLock)
		{
			return RegisterUnderLock(port);
		}
	}

	public void Dispose(ICommPort port)
	{
		lock (ScanLock)
		{
			string item = port.PortName.ToUpper();
			InUsePorts.Remove(item);
			port.Dispose();
		}
	}

	public ICommChannelConfiguration CreateConfiguration()
	{
		return new RedboxSerialPortConfiguration();
	}

	public ICommPort Create(SerialPort port)
	{
		return Create(port, CommPortReadModes.Async);
	}

	public ICommPort Create(SerialPort port, CommPortReadModes mode)
	{
		if (port == null)
		{
			throw new ArgumentException("port");
		}
		RedboxSerialPort redboxSerialPort = OnCreate(port, mode);
		if (redboxSerialPort != null)
		{
			Register(redboxSerialPort);
		}
		return redboxSerialPort;
	}

	public ICommPort Scan(ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode)
	{
		return Scan(null, conf, probe, mode);
	}

	public ICommPort Scan(string tryFirst, ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode)
	{
		lock (ScanLock)
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			ICommPort commPort = null;
			if (tryFirst != null)
			{
				commPort = Probe(tryFirst, conf, probe, mode);
			}
			if (commPort == null)
			{
				commPort = ProbePorts(conf, probe, mode);
			}
			executionTimer.Stop();
			LogHelper.Instance.Log("[Port Scan] Time to scan ports: {0}ms", executionTimer.ElapsedMilliseconds);
			if (commPort != null)
			{
				RegisterUnderLock(commPort);
			}
			return commPort;
		}
	}

	private RedboxSerialPort Probe(string portName, ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode)
	{
		string text = InUsePorts.Find((string each) => each.Equals(portName, StringComparison.CurrentCultureIgnoreCase));
		if (text != null)
		{
			LogHelper.Instance.Log("{0} The port {1} is reported in-use by the port manager.", "[Port Scanner]", portName);
			return null;
		}
		SerialPort port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
		RedboxSerialPort redboxSerialPort = OnCreate(port, mode);
		redboxSerialPort.Configure(conf);
		LogHelper.Instance.Log("{0} try port {1}", "[Port Scanner]", portName);
		if (!redboxSerialPort.Open())
		{
			LogHelper.Instance.Log("{0} Could not open port {1}", "[Port Scanner]", portName);
			redboxSerialPort.Dispose();
			return null;
		}
		if (probe(redboxSerialPort))
		{
			LogHelper.Instance.Log("{0} Probe port {1} returned true", "[Port Scanner]", portName);
			return redboxSerialPort;
		}
		redboxSerialPort.Dispose();
		return null;
	}

	private bool RegisterUnderLock(ICommPort port)
	{
		string key = port.PortName.ToUpper();
		if (InUsePorts.Find((string each) => each.Equals(key, StringComparison.CurrentCultureIgnoreCase)) == null)
		{
			InUsePorts.Add(key);
			return true;
		}
		return false;
	}

	private RedboxSerialPort OnCreate(SerialPort port, CommPortReadModes mode)
	{
		RedboxSerialPort result = null;
		switch (mode)
		{
		case CommPortReadModes.Async:
			result = new AsyncReadPort(port)
			{
				Mode = CommPortReadModes.Async
			};
			break;
		case CommPortReadModes.Callback:
			result = new CallbackReadPort(port)
			{
				Mode = CommPortReadModes.Callback
			};
			break;
		}
		return result;
	}

	private RedboxSerialPort ProbePorts(ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode)
	{
		string[] portNames = SerialPort.GetPortNames();
		foreach (string portName in portNames)
		{
			RedboxSerialPort redboxSerialPort = Probe(portName, conf, probe, mode);
			if (redboxSerialPort != null)
			{
				return redboxSerialPort;
			}
		}
		return null;
	}
}
