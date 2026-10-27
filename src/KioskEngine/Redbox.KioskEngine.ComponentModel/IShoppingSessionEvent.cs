using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSessionEvent
{
	DateTime TimeStamp { get; }

	string Description { get; }

	ShoppingSessionEventType Type { get; }
}
