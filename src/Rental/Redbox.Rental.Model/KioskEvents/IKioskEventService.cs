using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.KioskEvents;

public interface IKioskEventService
{
	void AddEvent(KioskEvent kioskEvent);

	void Start();

	void Stop();

	void Restart();
}
