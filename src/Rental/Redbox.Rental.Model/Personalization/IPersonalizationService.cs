using System;
using System.Security;

namespace Redbox.Rental.Model.Personalization;

public interface IPersonalizationService
{
	bool IsEarlyIdEnabled { get; }

	bool IsLoyaltyEnabled { get; }

	void UpdateAccount(string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, Action<IUpdateAccountResult> resultCallback, bool queued = false, AccountSourceType source = AccountSourceType.General);

	void SetSelectedPromo(string promoCode);

	void ClearSelectedPromo();
}
