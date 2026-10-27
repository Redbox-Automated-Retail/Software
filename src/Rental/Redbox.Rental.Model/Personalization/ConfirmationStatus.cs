namespace Redbox.Rental.Model.Personalization;

public enum ConfirmationStatus
{
	DontShow = 0,
	DoNotShow = DontShow,
	SubscribedConfirmed = 1,
	SubscribedNotConfirmed = 2,
	NotSubscribedConfirmed = 3,
	NotSubscribedNotConfirmed = 4,
	NewCustomer = 5
}
