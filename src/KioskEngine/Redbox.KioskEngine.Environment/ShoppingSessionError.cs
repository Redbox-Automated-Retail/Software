using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ShoppingSessionError : IShoppingSessionError
{
	public string Code { get; internal set; }

	public string Details { get; internal set; }

	public bool IsWarning { get; internal set; }

	public string Description { get; internal set; }

	public DateTime TimeStamp { get; internal set; }
}
