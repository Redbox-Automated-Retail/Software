namespace Redbox.Extensibility.ComponentModel;

public interface INotificationService
{
	void Enqueue(INotification notification);

	INotification Dequeue();

	void ClearAll();
}
