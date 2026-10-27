using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.Proxy;

public class EmailConfirmResult : IEmailConfirmResult
{
	public bool Success { get; set; }

	public ErrorList Errors { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool PromptForPerks { get; set; }

	public bool PromptForEarlyId { get; set; }

	public bool TextClubEnabled { get; set; }

	public bool? EmailVerified { get; set; }

	public ConfirmationStatus ConfirmationStatus { get; set; }
}
