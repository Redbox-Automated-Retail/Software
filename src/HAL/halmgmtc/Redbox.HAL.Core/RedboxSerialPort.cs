using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public abstract class RedboxSerialPort : IDisposable
{
	public delegate bool ResponseComplete(IChannelResponse response);

	public static readonly byte[] WriteCharsCRNull = new byte[2] { 13, 0 };

	public static readonly byte[] WriteCharsCR = new byte[1] { 13 };

	protected readonly SerialPort Port;

	protected readonly int WritePause;

	protected string FriendlyName;

	private readonly int ReadTimeout;

	private readonly int WriteTimeout;

	private readonly int OpenWait;

	private bool Disposed;

	public bool IsOpen
	{
		get
		{
			if (Port != null)
			{
				return PortIsOpen();
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (Port != null)
			{
				return Port.PortName;
			}
			return "UNKNOWN";
		}
	}

	public bool EnableDebugging { get; set; }

	public byte[] WriteTerminator { get; private set; }

	public ResponseComplete Validate { get; set; }

	protected abstract PortModes Mode { get; }

	public static RedboxSerialPort Create(RedboxSerialPortConfiguration conf, PortModes mode, string friendly)
	{
		RedboxSerialPort redboxSerialPort = Create(conf, mode);
		redboxSerialPort.FriendlyName = friendly;
		return redboxSerialPort;
	}

	public static RedboxSerialPort Create(RedboxSerialPortConfiguration conf, PortModes mode)
	{
		RedboxSerialPort result = null;
		switch (mode)
		{
		case PortModes.Async:
			result = new AsyncReadPort(conf);
			break;
		case PortModes.Callback:
			result = new CallbackReadPort(conf);
			break;
		}
		return result;
	}

	public void Dispose()
	{
		if (EnableDebugging)
		{
			LogHelper.Instance.Log("RDBX serial port: dispose port {0}", Name);
		}
		DisposeInner(fromDispose: true);
	}

	~RedboxSerialPort()
	{
		if (EnableDebugging)
		{
			LogHelper.Instance.Log("RDBX serial port: dispose port {0} (finalizer)", Name);
		}
		DisposeInner(fromDispose: false);
	}

	public bool Close()
	{
		if (Port == null || !Port.IsOpen)
		{
			return false;
		}
		try
		{
			Port.RtsEnable = true;
			Port.DtrEnable = false;
			Port.Close();
			if (Port.IsOpen)
			{
				LogHelper.Instance.Log("PortManager.SafeClosePort {0}: port is still open.", Port.PortName);
				return false;
			}
			if (string.IsNullOrEmpty(FriendlyName))
			{
				LogHelper.Instance.Log("The port {0} is closed.", Port.PortName);
			}
			else
			{
				LogHelper.Instance.Log("The {0} port ({1}) is closed.", FriendlyName, Port.PortName);
			}
			CloseInner();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("SafeClosePort caught an exception.", e);
			return false;
		}
	}

	public abstract bool Open();

	public abstract IChannelResponse SendRecv(byte[] bytes, int readTimeout);

	protected bool OpenInner()
	{
		if (Port == null)
		{
			LogHelper.Instance.Log("RedboxSerialPort: no port is configured.");
			return false;
		}
		if (PortIsOpen())
		{
			return true;
		}
		try
		{
			Port.Open();
			Port.ReadTimeout = ReadTimeout;
			Port.WriteTimeout = WriteTimeout;
			Port.RtsEnable = true;
			Port.DtrEnable = true;
			Thread.Sleep(OpenWait);
			if (string.IsNullOrEmpty(FriendlyName))
			{
				LogHelper.Instance.Log("The port {0} is open in mode {1}.", Port.PortName, Mode.ToString());
			}
			else
			{
				LogHelper.Instance.Log("The {0} port ({1}) is open in mode {2}.", FriendlyName, Port.PortName, Mode.ToString());
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("RedboxSerialPort.Open() caught an exception.", e);
			return false;
		}
		if (ResetPortBuffers(resetOut: true))
		{
			PortManager.Instance.Register(Name);
			return true;
		}
		Close();
		return false;
	}

	public bool ResetBuffers()
	{
		return ResetPortBuffers(resetOut: false);
	}

	public IChannelResponse SendRecv(string command, int readTimeout)
	{
		return SendRecv(Encoding.ASCII.GetBytes(command), readTimeout);
	}

	protected virtual void CloseInner()
	{
	}

	protected virtual void CoreDispose()
	{
	}

	protected bool ResetPortBuffers(bool resetOut)
	{
		try
		{
			if (!Port.IsOpen)
			{
				LogHelper.Instance.Log("ResetPortBuffers: port is closed.", LogEntryType.Info);
				return false;
			}
			Port.DiscardInBuffer();
			if (resetOut)
			{
				Port.DiscardOutBuffer();
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Reset buffer caught an exception.", e);
			return false;
		}
	}

	protected bool PortIsOpen()
	{
		string text = (string.IsNullOrEmpty(FriendlyName) ? Name : FriendlyName);
		try
		{
			return Port.IsOpen;
		}
		catch (ArgumentNullException e)
		{
			LogHelper.Instance.Log($"[RedboxSerialPort] {text} IsOpen generated an argument NULL exception.", e);
			return false;
		}
		catch (ArgumentException e2)
		{
			LogHelper.Instance.Log($"[RedboxSerialPort] {text} IsOpen generated an argument exception.", e2);
			return false;
		}
		catch
		{
			LogHelper.Instance.Log("[RedboxSerialPort] {0} IsOpen generated an unhandled exception.", text);
			return false;
		}
	}

	protected string AsString(byte[] array)
	{
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		foreach (byte b in array)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	protected RedboxSerialPort(RedboxSerialPortConfiguration conf)
	{
		WriteTerminator = conf.WriteTerminator;
		Validate = conf.Test;
		Port = new SerialPort(conf.PortName, conf.Baud, conf.Parity, conf.DataBits, conf.StopBits);
		if (conf.ReceiveBufferSize.HasValue)
		{
			Port.ReadBufferSize = conf.ReceiveBufferSize.Value;
		}
		WritePause = conf.WritePause;
		ReadTimeout = conf.ReadTimeout;
		WriteTimeout = conf.WriteTimeout;
		OpenWait = conf.OpenWait;
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			CoreDispose();
			Close();
			string name = Name;
			Port.Dispose();
			PortManager.Instance.Remove(name);
			GC.SuppressFinalize(this);
		}
	}
}
