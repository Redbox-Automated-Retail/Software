#if NETFRAMEWORK
using System;
using System.Collections.Specialized;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.GenuineTcp;
using Belikov.GenuineChannels.TransportContext;
using Redbox.Core;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy;

// .NET Framework transport: .NET Remoting over GenuineChannels (gtcp://), as the kiosk shipped it.
public partial class BrokerServicesProxy
{
	private GenuineTcpChannel? m_channel;

	public override object? InitializeLifetimeService()
	{
		return null;
	}

	private void InitializeTransport()
	{
		GenuineGlobalEventProvider.GenuineChannelsGlobalEvent += delegate(object sender, GenuineEventArgs args)
		{
			try
			{
				if (args.SourceException != null)
				{
					LogHelper.Instance.Log("An unhandled exception has been raised by GenuineChannels.", args.SourceException);
				}
				LogHelper.Instance.Log("GenuineChannels event raised, EventType: {0}", args.EventType);
				GenuineEventType eventType = args.EventType;
				if (eventType == GenuineEventType.GeneralServerRestartDetected || eventType == GenuineEventType.GeneralConnectionClosed || eventType == GenuineEventType.HostResourcesReleased)
				{
					StartReconnectTimer();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in GenuineGlobalEventProvider.GenuineChannelsGlobalEvent.", e);
			}
		};
	}

	private IKioskRegister OpenBroker()
	{
		return (IKioskRegister)Activator.GetObject(typeof(IKioskRegister), Url);
	}

	private void DisconnectFromBroker()
	{
		try
		{
			if (m_broker != null)
			{
				HostInformation hostInformation = GenuineUtility.FetchHostInformationFromMbr((MarshalByRefObject)m_broker);
				if (hostInformation != null)
				{
					GenuineUtility.FetchTransportContextFromMbr((MarshalByRefObject)m_broker).KnownHosts.ReleaseHostResources(hostInformation, new ApplicationException("Manual client disconnection."));
					m_broker = null;
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.DisconnectFromBroker.", e);
		}
	}

	private void RegisterChannel()
	{
		try
		{
			if (m_channel != null)
			{
				UnregisterChannel();
			}
			ListDictionary properties = new ListDictionary
			{
				{ "ConnectTimeout", 5000 },
				{ "MaxTotalSize", 500000 },
				{ "MaxContentSize", 500000 },
				{ "MaxQueuedItems", 500000 },
				{ "ReconnectionTries", 1 },
				{ "InvocationTimeout", Timeout },
				{ "MaxTimeSpanToReconnect", 30000 },
				{ "SleepBetweenReconnections", 5000 },
				{ "PersistentConnectionSendPingAfterInactivity", 20000 }
			};
			m_channel = new GenuineTcpChannel(properties, new BinaryClientFormatterSinkProvider(), new BinaryServerFormatterSinkProvider());
			ChannelServices.RegisterChannel(m_channel, ensureSecurity: false);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.RegisterChannel.", e);
		}
	}

	private void UnregisterChannel()
	{
		try
		{
			ChannelServices.UnregisterChannel(m_channel);
			m_channel = null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in BrokerServicesProxy.UnregisterChannel.", e);
		}
	}
}
#endif