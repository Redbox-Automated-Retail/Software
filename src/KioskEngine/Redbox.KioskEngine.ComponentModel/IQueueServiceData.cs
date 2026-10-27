using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IQueueServiceData : IDisposable
{
	ErrorList Initialize(string path, int retry = 2);

	ErrorList Clear();

	ErrorList GetDepth(out int count);

	ErrorList DeleteMessage(int id);

	ErrorList DeleteMessageByType(string type);

	ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage);

	ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage, out int newMessageId);

	ErrorList Update(int id, byte[] message);

	ErrorList ForEach(Action<IMessage> action);

	ErrorList ForEach(Action<IMessage> action, string type);

	ErrorList SelectNextMessage(IQueueServicePriority queueServicePriority, out IMessage message);
}
