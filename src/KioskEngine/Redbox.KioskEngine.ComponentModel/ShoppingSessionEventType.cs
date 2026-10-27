namespace Redbox.KioskEngine.ComponentModel;

public enum ShoppingSessionEventType : byte
{
	Start,
	End,
	Abandon,
	Commit,
	ShowView,
	ViewDuration,
	ActivateActor,
	ActivateTimer,
	Audit,
	Comment,
	AddItemToCart,
	RemoveItemFromCart,
	AddPromo,
	RemovePromo,
	Checkout,
	ChangeCulture,
	Hardware,
	IdleTimerExpired
}
