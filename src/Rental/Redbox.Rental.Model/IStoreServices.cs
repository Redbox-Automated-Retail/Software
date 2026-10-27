using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model;

public interface IStoreServices
{
	int StoreNumberInteger { get; }

	string StoreNumberString { get; set; }

	bool IsStandAlone { get; set; }

	bool RunInitialSync { get; set; }

	string InitialSyncType { get; set; }

	string GetEnvironment();

	string GetApplication();

	void GetPendingStates(RemoteServiceCallback completeCallback);

	void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback);

	void GetPendingBanners(int stateId, RemoteServiceCallback completeCallback);

	void GetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback);

	DateTime? GetRebootTime();

	bool ShouldRunInitialSync();

	string GetInitialSyncType();

	void SetRunInitialSync(bool flag);
}
