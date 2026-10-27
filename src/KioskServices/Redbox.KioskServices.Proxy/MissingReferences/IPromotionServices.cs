// From Redbox.KioskEngine.ComponentModel 1.71.0.21 (update-store history): removed from the shipped 1.88,
// but Redbox.KioskServices.Proxy 1.81 still implements it.
namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IPromotionServices
{
	bool LiveCall { get; set; }

	string[] PromoCodes { get; set; }

	bool SimulateServerBusyError { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	void Reset();

	void ValidatePromotionCode(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
