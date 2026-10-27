using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface ICustomerOfferService
{
	void GetOffers(string kioskId, string languageCode, string storefrontEventType, string sessionId, RemoteServiceCallback completeCallback);
}
