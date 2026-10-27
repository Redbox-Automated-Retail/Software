using System.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface ILoyaltyServices
{
	bool LiveCall { get; set; }

	bool SimulateAccrualSuccess { get; set; }

	bool SimulateRedemptionSuccess { get; set; }

	EstimateAccrualErrors AccrualError { get; set; }

	EstimateRedemptionErrors RedemptionError { get; set; }

	int DefaultAccrualPoints { get; set; }

	int DefaultRedemptionPoints { get; set; }

	BindingList<ItemAccrualRedemptions> Items { get; }

	void EstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback);

	void EstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback);
}
