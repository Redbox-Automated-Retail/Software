using System.Collections.Generic;
using System.Reflection;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Configuration;

namespace Redbox.BrokerServices.Proxy;

public class ReservationServicesProxy : IReservationServices
{
	private int m_working;

	public static ReservationServicesProxy Instance => Singleton<ReservationServicesProxy>.Instance;

	internal ReservationRequestCallback3 RequestCallback3 { get; private set; }

	internal CancelReservationCallback CancelRequestCallback { get; private set; }

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
		BrokerServicesProxy.Instance.Reset();
	}

	public void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback, CancelReservationCallback cancelRequestCallback)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["Runtime"] = "Kiosk Engine";
		dictionary["Version"] = AssemblyInfoHelper.GetVersion(Assembly.GetExecutingAssembly());
		dictionary["PricingPolicy"] = prices;
		dictionary["GiftCardsEnabled"] = false;
		dictionary["RecoOnPickup"] = false;
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null)
		{
			if (service.TryGetValue<bool>("system", "General", "GiftCardsEnabled", out var value))
			{
				dictionary["GiftCardsEnabled"] = value;
			}
			if (service.TryGetValue<bool>("system", "General", "RecommendationOnPickup", out var value2))
			{
				dictionary["RecoOnPickup"] = value2;
			}
		}
		dictionary["AuthorizeAtPickup"] = Configuration.Instance.AuthorizeAtPickup;
		dictionary["CancelReservation"] = true;
		dictionary["Features"] = Features.MultiDiscVend;
		if (int.TryParse(storeNumber, out var result))
		{
			RequestCallback3 = reservationRequestCallback;
			CancelRequestCallback = cancelRequestCallback;
			BrokerServicesProxy.Instance.Register(result, dictionary.ToJson());
		}
	}

	public IReservedItem NewReservedItem()
	{
		return new ReservedItem();
	}

	public IReservationResult NewReservationResult()
	{
		return new LocalReservationResult();
	}

	public ILocalCancelReservationResult NewCancelReservationResult()
	{
		return new LocalCancelReservationResult();
	}

	private ReservationServicesProxy()
	{
	}
}
