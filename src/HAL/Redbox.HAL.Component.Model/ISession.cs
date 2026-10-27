using System;

namespace Redbox.HAL.Component.Model;

public interface ISession : IMessageSink
{
	bool LogDetailedMessages { get; }

	event EventHandler Disconnect;

	void Start();
}
