namespace Redbox.UpdateManager.ComponentModel;

public interface IQueueService
{
	void Enqueue(string label, object entry);

	void EnqueueRaw(string label, string entry);

	void Dequeue(string label);

	T Peek<T>(string label);

	string PeekRaw(string label);
}
