namespace Redbox.Rental.Model.KioskHealth;

public interface IEMVReaderHealth
{
	bool ContactlessOffDueToDisconnectThresholdReached { get; }

	void SessionWasInTechnicalFallback(bool inFallback);

	void TrackCardReaderDisconnects();
}
