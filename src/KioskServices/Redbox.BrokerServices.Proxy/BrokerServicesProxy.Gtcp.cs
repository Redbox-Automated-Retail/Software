#if !NETFRAMEWORK
using System;
using Redbox.BrokerServices.Proxy.Gtcp;
using Redbox.Core;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy;

// .NET 10 transport. .NET Remoting and GenuineChannels do not exist on .NET (Core); GtcpBrokerClient (Gtcp\) speaks
// GenuineChannels' GTCP protocol and the BinaryFormatter remoting messages for the broker's small interface itself
// (docs/modernization.md, section 7; protocol notes in Gtcp\GtcpConnection.cs and the files next to it).
public partial class BrokerServicesProxy
{
	private GtcpBrokerClient? m_gtcpClient;

	private GtcpConnectionSettings m_gtcpSettings = new GtcpConnectionSettings();

	private void InitializeTransport()
	{
	}

	private IKioskRegister OpenBroker()
	{
		GtcpBrokerClient client = new GtcpBrokerClient(Url, m_gtcpSettings, LogGtcp);
		client.ConnectionClosed += OnGtcpConnectionClosed;
		m_gtcpClient = client;
		return client;
	}

	// The counterpart of GenuineChannels' GeneralConnectionClosed / GeneralServerRestartDetected events: the same-identity
	// reconnection failed or the broker restarted, so the registration is gone and has to be made again.
	private void OnGtcpConnectionClosed(GtcpBrokerClient client, string reason)
	{
		if (!ReferenceEquals(client, m_gtcpClient))
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("GTCP connection to Broker Services closed: {0}", reason);
			StartReconnectTimer();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.OnGtcpConnectionClosed.", e);
		}
	}

	private void DisconnectFromBroker()
	{
		try
		{
			GtcpBrokerClient? client = m_gtcpClient;
			m_gtcpClient = null;
			m_broker = null;
			client?.Dispose();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.DisconnectFromBroker.", e);
		}
	}

	// The GenuineChannels channel properties of the .NET Framework build, as connection settings.
	private void RegisterChannel()
	{
		try
		{
			m_gtcpSettings = new GtcpConnectionSettings
			{
				ConnectTimeout = TimeSpan.FromMilliseconds(5000),
				InvocationTimeout = TimeSpan.FromMilliseconds(Timeout),
				MaxTimeSpanToReconnect = TimeSpan.FromMilliseconds(30000),
				SleepBetweenReconnections = TimeSpan.FromMilliseconds(5000),
				ReconnectionAttempts = 2,
				PingAfterInactivity = TimeSpan.FromMilliseconds(20000)
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.RegisterChannel.", e);
		}
	}

	private void UnregisterChannel()
	{
	}

	private static void LogGtcp(string message, Exception? exception)
	{
		// Through "{0}" so braces in the text are not taken as format items.
		if (exception == null)
		{
			LogHelper.Instance.Log("{0}", message);
		}
		else
		{
			LogHelper.Instance.Log("{0}", exception, message);
		}
	}
}
#endif
