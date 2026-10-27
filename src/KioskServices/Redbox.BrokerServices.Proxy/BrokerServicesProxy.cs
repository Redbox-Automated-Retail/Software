using System;
using System.Collections.Specialized;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Belikov.GenuineChannels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.GenuineTcp;
using Belikov.GenuineChannels.TransportContext;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy;

public class BrokerServicesProxy : MarshalByRefObject, IKioskOperations
{
	private const int DefaultBrokerServicesTimeout = 30000;

	private const string DefaultBrokerServicesUrl = "gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem";

	private int? m_kioskId;

	private string m_versionPricingDictionary;

	private bool m_allowReconnect;

	private Timer m_reconnectTimer;

	private IKioskRegister m_broker;

	private GenuineTcpChannel m_channel;

	private volatile bool m_isConnecting;

	public static BrokerServicesProxy Instance => Singleton<BrokerServicesProxy>.Instance;

	public int Timeout
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "BrokerServicesTimeout", 30000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "BrokerServicesTimeout", value);
			RegisterChannel();
			if (m_kioskId.HasValue && !string.IsNullOrEmpty(m_versionPricingDictionary))
			{
				ConnectToBroker();
			}
		}
	}

	public string Url
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "BrokerServicesURL", "gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "BrokerServicesURL", value);
		}
	}

	public override object InitializeLifetimeService()
	{
		return null;
	}

	public void Ping()
	{
		LogHelper.Instance.Log("BrokerServicesProxy received a remote ping.");
	}

	public void Reset()
	{
		m_allowReconnect = false;
		if (m_reconnectTimer != null)
		{
			LogHelper.Instance.Log("Reset called...Stop and dispose reconnect timer.");
			try
			{
				m_reconnectTimer.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised while disposing the broker services reconnect timer during Reset().", e);
			}
			finally
			{
				m_reconnectTimer = null;
			}
		}
		DisconnectFromBroker();
	}

	public bool Register(int kioskId, string versionPricingDictionary)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		if (service != null && service.UseLegacyBrokerService)
		{
			m_kioskId = kioskId;
			m_versionPricingDictionary = versionPricingDictionary;
			m_allowReconnect = true;
			return ConnectToBroker();
		}
		LogHelper.Instance.Log("BrokerServiceProxy.Register aborted.  Config setting UseLegacyBrokerService not enabled.");
		return false;
	}

	public ReservationResult Reserve(ReservationCart cart)
	{
		return null;
	}

	public ReservationResult Reserve2(ReservationCart2 cart)
	{
		return null;
	}

	public ReservationResult Reserve3(ReservationCart3 cart)
	{
		ReservationCallbackEntry3 reservationCallbackEntry = new ReservationCallbackEntry3
		{
			Cart = cart
		};
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
			if (service != null)
			{
				service.EnqueueCallback(reservationCallbackEntry);
				reservationCallbackEntry.ResetEvent.WaitOne();
				LogHelper.Instance.Log("BrokerServicesProxy.Reserve3 execution time: {0}", executionTimer.Elapsed);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.Reserve3.", e);
			reservationCallbackEntry.BrokerResult.ErrorMessage = "An error ocurred during reserve3.";
			reservationCallbackEntry.ResetEvent.Set();
		}
		finally
		{
			reservationCallbackEntry.ResetEvent.Close();
		}
		return reservationCallbackEntry.BrokerResult;
	}

	public CancelReservationResult CancelReservation(long referenceNumber)
	{
		CancelReservationCallbackEntry cancelReservationCallbackEntry = new CancelReservationCallbackEntry(referenceNumber);
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
			if (service != null)
			{
				service.EnqueueCallback(cancelReservationCallbackEntry);
				cancelReservationCallbackEntry.ResetEvent.WaitOne();
				LogHelper.Instance.Log("BrokerServicesProxy.CancelReservation execution time: {0}", executionTimer.Elapsed);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.CancelReservation.", e);
			cancelReservationCallbackEntry.BrokerResult.ErrorMessage = "An error ocurred during CancelReservation.";
			cancelReservationCallbackEntry.ResetEvent.Set();
		}
		finally
		{
			cancelReservationCallbackEntry.ResetEvent.Close();
		}
		return cancelReservationCallbackEntry.BrokerResult;
	}

	private BrokerServicesProxy()
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
				if ((eventType == GenuineEventType.GeneralServerRestartDetected || eventType == GenuineEventType.GeneralConnectionClosed || eventType == GenuineEventType.HostResourcesReleased) && m_allowReconnect && m_reconnectTimer == null)
				{
					LogHelper.Instance.Log("...Start reconnect timer, interval 60 seconds.");
					m_reconnectTimer = new Timer(delegate
					{
						ConnectToBroker();
					}, null, 1000, 60000);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in GenuineGlobalEventProvider.GenuineChannelsGlobalEvent.", e);
			}
		};
		RegisterChannel();
	}

	private bool ConnectToBroker()
	{
		if (m_isConnecting)
		{
			return false;
		}
		bool flag = false;
		try
		{
			m_isConnecting = true;
			DisconnectFromBroker();
			m_broker = (IKioskRegister)Activator.GetObject(typeof(IKioskRegister), Url);
			if (m_broker == null)
			{
				return false;
			}
			LogHelper.Instance.Log("Register with Broker Services, store number: {0}, version data: {1}", m_kioskId, m_versionPricingDictionary);
			m_broker.Register(m_kioskId.Value, this, m_versionPricingDictionary);
			LogHelper.Instance.Log("...Registration with Broker Services successful.");
			flag = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.ConnectToBroker.", e);
		}
		finally
		{
			m_isConnecting = false;
			if (flag && m_reconnectTimer != null)
			{
				LogHelper.Instance.Log("...Stop and dispose reconnect timer.");
				try
				{
					m_reconnectTimer.Dispose();
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("An unhandled exception was raised while disposing the broker services reconnect timer during ConnectToBroker().", e2);
				}
				finally
				{
					m_reconnectTimer = null;
				}
			}
		}
		return flag;
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
