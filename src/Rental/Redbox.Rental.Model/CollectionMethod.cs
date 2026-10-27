using System;

namespace Redbox.Rental.Model;

[Flags]
public enum CollectionMethod
{
	AuthorizeAndSettle = 1,
	CollectPartialAmounts = 2,
	AuthorizeOnly = 4
}
