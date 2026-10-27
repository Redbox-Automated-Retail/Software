using System;
using System.IO;
using System.IO.Pipes;
using Redbox.Core;

namespace Redbox.IPC.Framework.NamedPipes;

public class NamedPipeClientSession : ClientSession
{
	private NamedPipeClientStream m_stream;

	public string PipeName { get; private set; }

	protected override Stream Stream
	{
		get
		{
			if (m_stream == null)
			{
				m_stream = new NamedPipeClientStream(base.HostName, PipeName, PipeDirection.InOut)
				{
					ReadTimeout = base.Timeout
				};
				base.ReadBuffer = new byte[m_stream.InBufferSize];
			}
			return m_stream;
		}
	}

	public override bool Connect()
	{
		try
		{
			ConnectThrowOnError();
			return true;
		}
		catch (IOException)
		{
			return false;
		}
	}

	public override void ConnectThrowOnError()
	{
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (flag)
			{
				break;
			}
			try
			{
				if (!m_stream.IsConnected)
				{
					m_stream.Connect(5000);
					if (m_stream.IsConnected)
					{
						flag = true;
					}
				}
			}
			catch (TimeoutException)
			{
				LogHelper.Instance.Log($"NamedPipeclient.Connect() timed out; still have {3 - i} re-connect attempts.", LogEntryType.Info);
			}
		}
		if (!flag)
		{
			throw new TimeoutException("Failed to connect to pipe server after 3 tries; maybe it's down?");
		}
		ConsumeMessages();
		base.IsConnected = true;
	}

	protected internal NamedPipeClientSession(IPCProtocol protocol)
		: base(protocol)
	{
		string text = protocol.Host;
		if (text.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || text.Equals("127.0.0.1"))
		{
			text = ".";
		}
		m_stream = new NamedPipeClientStream(text, protocol.Port, PipeDirection.InOut, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
		base.ReadBuffer = new byte[1024];
	}

	protected internal override bool IsConnectionAvailable()
	{
		return ((NamedPipeClientStream)Stream).IsConnected;
	}

	protected internal override void CustomClose()
	{
	}

	protected override bool CanRead()
	{
		return Stream.CanRead;
	}

	protected override int GetAvailableData()
	{
		if (!((NamedPipeClientStream)Stream).IsMessageComplete)
		{
			return 0;
		}
		return 1;
	}
}
