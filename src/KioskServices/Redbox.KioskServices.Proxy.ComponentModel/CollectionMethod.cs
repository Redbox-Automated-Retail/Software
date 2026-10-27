using System;

namespace Redbox.KioskServices.Proxy.ComponentModel;

[Flags]
public enum CollectionMethod
{
	AuthorizeAndSettle = 1,
	CollectPartialAmounts = 2,
	AuthorizeOnly = 4
}
