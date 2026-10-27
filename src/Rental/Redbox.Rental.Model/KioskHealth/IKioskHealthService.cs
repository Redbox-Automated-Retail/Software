namespace Redbox.Rental.Model.KioskHealth;

public interface IKioskHealthService
{
	void Start();

	void Stop();

	void Restart();

	IKioskHealthItem AddHealthItem(IKioskHealthItem healthItem);
}
