using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Redbox.HAL.Core;

public sealed class RedboxSerialPort : IDisposable
{
	public delegate bool ResponseComplete(ChannelResponse response);

	public static readonly byte[] WriteCharsCRNull = new byte[2] { 13, 0 };

	public static readonly byte[] WriteCharsCR = new byte[1] { 13 };

	private readonly TimeSpan WaitSpan = new TimeSpan(0, 0, 0, 0, 10);

	public SerialPort Port { get; private set; }

	public bool IsOpen
	{
		get
		{
			if (Port != null)
			{
				return Port.IsOpen;
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

	private ResponseComplete Validate { get; set; }

	private bool Disposed { get; set; }

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	~RedboxSerialPort()
	{
		DisposeInner(fromDispose: false);
	}

	public RedboxSerialPort(RedboxSerialPortConfiguration conf)
	{
		WriteTerminator = conf.WriteTerminator;
		Validate = conf.Test;
		ConfigurePort(conf);
	}

	public bool ClosePort()
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
			if (EnableDebugging)
			{
				LogHelper.Instance.Log("PortManager.SafeClosePort {0}: success.", Port.PortName);
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("SafeClosePort caught an exception.", e);
			return false;
		}
	}

	public bool Open()
	{
		return OpenInner(-1, -1);
	}

	public bool Open(int readTimeout, int writeTimeout)
	{
		return OpenInner(readTimeout, writeTimeout);
	}

	public bool ResetBuffers()
	{
		return ResetPortBuffers(resetOut: false);
	}

	public ChannelResponse WriteCommand(string command, int readTimeout)
	{
		ChannelResponse channelResponse = ChannelResponse.NewChannel();
		try
		{
			if (!Port.IsOpen)
			{
				LogHelper.Instance.Log("Port.WriteCommand: the port was closed.", LogEntryType.Error);
				channelResponse.SetClosedError();
				return channelResponse;
			}
			if (EnableDebugging)
			{
				LogHelper.Instance.Log("Port.WriteCommand: {0}", command);
			}
			Port.Write(command);
			Port.Write(WriteTerminator, 0, WriteTerminator.Length);
			PerfFunctions.SpinWait(WaitSpan);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Port.WriteCommand() caught an exception.", e);
			channelResponse.SetCommunicationError();
			return channelResponse;
		}
		ReadInner(channelResponse);
		bool flag = false;
		if (!channelResponse.ReadEvent.WaitOne(readTimeout))
		{
			LogHelper.Instance.Log("Communication ERROR: port read timed out", LogEntryType.Error);
			channelResponse.SetCommunicationError();
			flag = true;
		}
		if (EnableDebugging)
		{
			string text = Encoding.ASCII.GetString(channelResponse.RawResponse);
			LogHelper.Instance.Log(" Read {0}, data: {1}", flag ? "timed out" : "ok", text);
			LogHelper.Instance.Log("  raw response: {0}", ByteHelper.AsString(channelResponse.RawResponse));
		}
		return channelResponse;
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			ClosePort();
			Port.Dispose();
			Port = null;
			GC.SuppressFinalize(this);
		}
	}

	private bool OpenInner(int readTimeout, int writeTimeout)
	{
		if (Port == null)
		{
			LogHelper.Instance.Log("RedboxSerialPort: no port is configured.");
			return false;
		}
		try
		{
			if (Port.IsOpen)
			{
				return true;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PortManager.Open: IsOpen generated an exception.", e);
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			try
			{
				Port.Open();
				Port.ReadTimeout = readTimeout;
				Port.WriteTimeout = writeTimeout;
				Port.RtsEnable = true;
				Port.DtrEnable = true;
				Thread.Sleep(3000);
				bool isOpen = Port.IsOpen;
				if (EnableDebugging)
				{
					LogHelper.Instance.Log("RedboxSerialPort.Open port {0}: {1}.", Port.PortName, isOpen ? "success" : "EPIC failure.");
				}
				if (isOpen)
				{
					break;
				}
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("RedboxSerialPort.Open() caught an exception.", e2);
				return false;
			}
		}
		if (ResetPortBuffers(resetOut: true))
		{
			return true;
		}
		ClosePort();
		return false;
	}

	private bool ResetPortBuffers(bool resetOut)
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

	private void PortDataReceived(object sender, SerialDataReceivedEventArgs args)
	{
	}

	private void ReadInner(ChannelResponse response)
	{
		try
		{
			Port.BaseStream.BeginRead(response.ReadBuffer, 0, response.ReadBuffer.Length, EndReadCallback, response);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CoreCommand: read caught an exception.", e);
		}
	}

	private void EndReadCallback(IAsyncResult result)
	{
		int num = 0;
		ChannelResponse channelResponse = null;
		try
		{
			channelResponse = (ChannelResponse)result.AsyncState;
			num = Port.BaseStream.EndRead(result);
			if (num == 0)
			{
				LogHelper.Instance.Log("SerialPort {0}: received 0 bytes in read.", Port.PortName);
				channelResponse.ReadEvent.Set();
				return;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Port.EndReadCallback caught an exception.", e);
			return;
		}
		channelResponse.Accumulate(num);
		if (!Validate(channelResponse))
		{
			ReadInner(channelResponse);
			return;
		}
		channelResponse.ReponseValid = true;
		channelResponse.ReadEvent.Set();
	}

	private void ConfigurePort(RedboxSerialPortConfiguration conf)
	{
		Port = new SerialPort(conf.PortName, conf.Baud, conf.Parity, conf.DataBits, conf.StopBits);
	}
}
