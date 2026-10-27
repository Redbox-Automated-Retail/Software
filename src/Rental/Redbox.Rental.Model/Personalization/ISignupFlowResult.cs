namespace Redbox.Rental.Model.Personalization;

public interface ISignupFlowResult
{
	SignupResult PerksSignupResult { get; set; }

	bool AcceptedPerksSignup { get; }

	SignupResult MarketingEmailSignupResult { get; set; }

	bool AcceptedMarketingEmailSignup { get; }

	SignupResult PhoneAndPinSignupResult { get; set; }

	bool AcceptedPhoneAndPinSignup { get; }

	SignupResult PerksThanksResult { get; set; }

	SignupResult EarlyIdThanksResult { get; set; }

	SignupResult AlreadyPerksMemberResult { get; set; }

	SignupResult MarketingEmailThanksResult { get; set; }
}
