using System;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy;

// The transport is in BrokerServicesProxy.Remoting.cs (GenuineChannels / .NET Remoting, .NET Framework) and
// BrokerServicesProxy.Gtcp.cs (.NET 10).
public partial class BrokerServicesProxy : MarshalByRefObject, IKioskOperations
{
	private const int DefaultBrokerServicesTimeout = 30000;

	// Placeholder (RFC 2606 .invalid, never resolves): the real URL comes from the registry value, which the backend's
	// kiosk profile (rb-reprovision) sets. Redbox's default was gtcp://blt.accessredbox.net:7890/RegistrationBroker/Default.rem.
	private const string DefaultBrokerServicesUrl = "gtcp://broker.kiosk-backend.invalid:7890/RegistrationBroker/Default.rem";

	private int? m_kioskId;

	private string m_versionPricingDictionary;

	private bool m_allowReconnect;

	private Timer m_reconnectTimer;

	private IKioskRegister m_broker;

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
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "BrokerServicesURL", DefaultBrokerServicesUrl);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "BrokerServicesURL", value);
		}
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
		IConfiguration configuration = ServiceLocator.Instance.GetService<IConfiguration>();
		if (configuration != null && configuration.UseLegacyBrokerService)
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
			ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
			if (callbackService != null)
			{
				callbackService.EnqueueCallback(reservationCallbackEntry);
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
			ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
			if (callbackService != null)
			{
				callbackService.EnqueueCallback(cancelReservationCallbackEntry);
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
		InitializeTransport();
		RegisterChannel();
	}

	// Called by the transport when the connection to the broker is gone (closed, server restart, resources released).
	private void StartReconnectTimer()
	{
		if (m_allowReconnect && m_reconnectTimer == null)
		{
			LogHelper.Instance.Log("...Start reconnect timer, interval 60 seconds.");
			m_reconnectTimer = new Timer(delegate
			{
				ConnectToBroker();
			}, null, 1000, 60000);
		}
	}

	private bool ConnectToBroker()
	{
		if (m_isConnecting)
		{
			return false;
		}
		bool registered = false;
		try
		{
			m_isConnecting = true;
			DisconnectFromBroker();
			m_broker = OpenBroker();
			if (m_broker == null)
			{
				return false;
			}
			LogHelper.Instance.Log("Register with Broker Services, store number: {0}, version data: {1}", m_kioskId, m_versionPricingDictionary);
			m_broker.Register(m_kioskId.Value, this, m_versionPricingDictionary);
			LogHelper.Instance.Log("...Registration with Broker Services successful.");
			registered = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BrokerServicesProxy.ConnectToBroker.", e);
		}
		finally
		{
			m_isConnecting = false;
			if (registered && m_reconnectTimer != null)
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
		return registered;
	}
}
