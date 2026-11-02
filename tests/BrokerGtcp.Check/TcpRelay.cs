using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BrokerGtcp.Check;

/// <summary>
/// A local TCP relay in front of the broker, so the checks can break the kiosk's connection from outside (as a network
/// failure would) and take the "network" down and up again on the same port.
/// </summary>
internal sealed class TcpRelay : IDisposable
{
	private readonly string _targetHost;
	private readonly int _targetPort;
	private readonly List<Socket> _connections = new List<Socket>();
	private TcpListener? _listener;

	public TcpRelay(string targetHost, int targetPort)
	{
		_targetHost = targetHost;
		_targetPort = targetPort;
	}

	public int Port { get; private set; }

	public int AcceptedConnections { get; private set; }

	public void Start()
	{
		_listener = new TcpListener(IPAddress.Loopback, Port);
		_listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		_listener.Start();
		Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
		TcpListener listener = _listener;
		Task.Run(() => AcceptLoop(listener));
	}

	/// <summary>Stops accepting and drops every relayed connection (an outage).</summary>
	public void Stop()
	{
		_listener?.Stop();
		_listener = null;
		KillConnections();
	}

	/// <summary>Resets every relayed connection (both sides see the socket die); new connections are still accepted.</summary>
	public void KillConnections()
	{
		lock (_connections)
		{
			foreach (Socket socket in _connections)
			{
				try
				{
					socket.LingerState = new LingerOption(true, 0); // RST, like a dropped link
					socket.Close();
				}
				catch (Exception)
				{
				}
			}
			_connections.Clear();
		}
	}

	public void Dispose() => Stop();

	private async Task AcceptLoop(TcpListener listener)
	{
		while (true)
		{
			Socket client;
			try
			{
				client = await listener.AcceptSocketAsync();
			}
			catch (Exception)
			{
				return; // stopped
			}
			Socket server = new Socket(SocketType.Stream, ProtocolType.Tcp);
			try
			{
				// IPv4 first: the broker's port is forwarded on 127.0.0.1 only.
				IPAddress[] addresses = (await Dns.GetHostAddressesAsync(_targetHost))
					.OrderBy(a => a.AddressFamily == AddressFamily.InterNetwork ? 0 : 1).ToArray();
				await server.ConnectAsync(addresses, _targetPort);
			}
			catch (Exception)
			{
				client.Close();
				continue;
			}
			lock (_connections)
			{
				_connections.Add(client);
				_connections.Add(server);
				AcceptedConnections++;
			}
			_ = Pump(client, server);
			_ = Pump(server, client);
		}
	}

	private static async Task Pump(Socket from, Socket to)
	{
		byte[] buffer = new byte[65536];
		try
		{
			while (true)
			{
				int read = await from.ReceiveAsync(buffer, SocketFlags.None);
				if (read == 0)
				{
					break;
				}
				await to.SendAsync(new ArraySegment<byte>(buffer, 0, read), SocketFlags.None);
			}
			to.Shutdown(SocketShutdown.Send);
		}
		catch (Exception)
		{
			try
			{
				to.LingerState = new LingerOption(true, 0);
				to.Close();
			}
			catch (Exception)
			{
			}
		}
	}
}
