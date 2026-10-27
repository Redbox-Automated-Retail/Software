using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSession
{
	Guid Id { get; }

	DateTime? EndedOn { get; }

	DateTime StartedOn { get; }

	string StoreNumber { get; }

	int ViewsShown { get; set; }

	IShoppingCart ShoppingCart { get; }

	ShoppingSessionStatus Status { get; }

	void End();

	void AddEvent(ShoppingSessionEventType type, string description);

	void Commit(string reason);

	void Abandon(string reason);

	bool StartUserInteraction();
}
