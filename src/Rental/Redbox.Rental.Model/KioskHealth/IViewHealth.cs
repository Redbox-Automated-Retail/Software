namespace Redbox.Rental.Model.KioskHealth;

public interface IViewHealth
{
	void PostActivity(string viewEventType = "", string viewEventValue = "", string sessionId = "");
}
