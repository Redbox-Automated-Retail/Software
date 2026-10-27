using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.Personalization;

public interface IEmailConfirmResult
{
	bool Success { get; }

	ErrorList Errors { get; }

	string CustomerProfileNumber { get; set; }

	string MobilePhoneNumber { get; set; }

	bool PromptForPerks { get; set; }

	bool PromptForEarlyId { get; set; }

	bool TextClubEnabled { get; set; }

	bool? EmailVerified { get; set; }

	ConfirmationStatus ConfirmationStatus { get; set; }
}
