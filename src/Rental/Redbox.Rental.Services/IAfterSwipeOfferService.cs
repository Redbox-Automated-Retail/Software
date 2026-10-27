using System;

namespace Redbox.Rental.Services;

public interface IAfterSwipeOfferService
{
	void StartOfferFlow(Action<bool> callbackAction);
}
