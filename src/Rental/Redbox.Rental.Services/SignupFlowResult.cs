using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services;

public class SignupFlowResult : ISignupFlowResult
{
	public SignupResult PerksSignupResult { get; set; }

	public bool AcceptedPerksSignup => PerksSignupResult == SignupResult.Accepted;

	public SignupResult MarketingEmailSignupResult { get; set; }

	public bool AcceptedMarketingEmailSignup => MarketingEmailSignupResult == SignupResult.Accepted;

	public SignupResult PhoneAndPinSignupResult { get; set; }

	public bool AcceptedPhoneAndPinSignup => PhoneAndPinSignupResult == SignupResult.Accepted;

	public SignupResult PerksThanksResult { get; set; }

	public SignupResult EarlyIdThanksResult { get; set; }

	public SignupResult AlreadyPerksMemberResult { get; set; }

	public SignupResult MarketingEmailThanksResult { get; set; }
}
