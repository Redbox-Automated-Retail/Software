using System;
using System.Net;
using System.Net.Sockets;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets;

public class TcpServerSession : ServerSession
{
	internal Socket ClientSocket { get; set; }

	public TcpServerSession(Socket client, IPCServiceHost serviceHost)
		: base(serviceHost)
	{
		ClientSocket = client;
		NegotiateSecurityLayer(new NetworkStream(ClientSocket, ownsSocket: true));
	}

	protected override bool CloseClientsInternal()
	{
		try
		{
			ClientSocket.Close();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseClientsInternal.", e);
			return false;
		}
	}

	protected override bool IsConnectedInternal()
	{
		try
		{
			return !ClientSocket.Poll(10, SelectMode.SelectRead) || ClientSocket.Available != 0;
		}
		catch
		{
			return false;
		}
	}

	protected override bool CloseStreamsInternal()
	{
		try
		{
			base.Stream.Dispose();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseStreamsInternal.", e);
			return false;
		}
	}

	protected override string GetRemoteHostIP()
	{
		if (ClientSocket != null && ClientSocket.Connected)
		{
			IPEndPoint iPEndPoint = (IPEndPoint)ClientSocket.RemoteEndPoint;
			switch (iPEndPoint.AddressFamily)
			{
			case AddressFamily.InterNetworkV6:
				if (iPEndPoint.Address.ToString() == "::1")
				{
					return "127.0.0.1";
				}
				return "0.0.0.0";
			case AddressFamily.InterNetwork:
				return iPEndPoint.Address.ToString();
			default:
				return "0.0.0.0";
			}
		}
		return string.Empty;
	}
}
