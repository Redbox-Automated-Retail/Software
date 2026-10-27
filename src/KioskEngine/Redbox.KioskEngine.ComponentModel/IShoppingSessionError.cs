using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSessionError
{
	string Code { get; }

	string Details { get; }

	bool IsWarning { get; }

	string Description { get; }

	DateTime TimeStamp { get; }
}
