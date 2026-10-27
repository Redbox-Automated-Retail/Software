using System;
using System.IO;
using System.IO.Pipes;
using Redbox.Core;

namespace Redbox.IPC.Framework.NamedPipes;

public class NamedPipeServerSession : ServerSession
{
	public NamedPipeServerSession(Stream stream, IPCServiceHost serviceHost)
		: base(serviceHost)
	{
		NegotiateSecurityLayer(stream);
	}

	protected override bool CloseClientsInternal()
	{
		return true;
	}

	protected override string GetRemoteHostIP()
	{
		throw new NotSupportedException("Accessing remote endpoint ip address is not supported in Named Pipe Server Session.");
	}

	protected override bool IsConnectedInternal()
	{
		try
		{
			return base.Stream is NamedPipeServerStream namedPipeServerStream && namedPipeServerStream.IsConnected;
		}
		catch (IOException e)
		{
			LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal encountered an I/O Exception.", e);
			return false;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("NamedPipeServerSession.IsConnectedInternal() encountered an unhandled exception", e2);
			return false;
		}
	}

	protected override bool CloseStreamsInternal()
	{
		try
		{
			if (base.Stream is NamedPipeServerStream namedPipeServerStream)
			{
				if (namedPipeServerStream.IsConnected)
				{
					namedPipeServerStream.WaitForPipeDrain();
					namedPipeServerStream.Disconnect();
				}
				namedPipeServerStream.Close();
				return true;
			}
			return false;
		}
		catch (IOException e)
		{
			LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal encountered an I/O Exception.", e);
			return false;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal() encountered an unhandled exception", e2);
			return false;
		}
	}
}
