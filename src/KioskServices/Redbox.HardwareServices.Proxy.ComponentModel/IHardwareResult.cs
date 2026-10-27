using System.Collections.Generic;

namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface IHardwareResult
{
	string Code { get; }

	int Deck { get; }

	int Slot { get; }

	string ItemID { get; }

	string ID { get; }

	string MetaData { get; }

	string Message { get; }

	string Timestamp { get; }

	string PreviousItem { get; }

	string ReturnTime { get; }

	IDictionary<string, object> MessageData { get; }
}
