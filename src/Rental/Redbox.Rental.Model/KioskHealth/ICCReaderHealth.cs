namespace Redbox.Rental.Model.KioskHealth;

public interface ICCReaderHealth
{
	void EventOccurred(bool dataWasReceived, bool hasError);
}
