using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public interface IRedboxRental
{
	ErrorList Initialize();

	void CloseOpenSessionAndStartNewRentalSession(string newSessionContext, bool clearAudioDeviceConnected = true);

	bool IsRentalSessionActive();

	void StartTheRentalApplication();

	void SetupTheRentalApplication();

	void EnterMaintenanceModeBecauseOfDisconnectedCardReader();

	void EnterMaintenanceModeBecauseOfTamperedCardReader();

	void HandleUnremovedChipInCardReader();
}
