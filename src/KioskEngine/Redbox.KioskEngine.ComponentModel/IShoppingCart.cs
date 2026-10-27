using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingCart
{
	string Email { get; }

	string PostalCode { get; }

	DateTime CreatedOn { get; }

	long? ReferenceNumber { get; }

	CreditElection UseCredits { get; }

	ShoppingCartType Type { get; }

	bool AuthAcceptFlag { get; }

	int AuthRuleId { get; }

	decimal AuthAmount { get; }

	bool SkipAuthRule { get; }

	bool PlayPassPromptAccepted { get; }

	int PlayPassPointsEarned { get; }

	IDictionary<string, string> PropertyBag { get; }

	void SetType(ShoppingCartType type);

	void SetEmail(string email);

	void SetPostalCode(string postalCode);

	void SetReferenceNumber(long referenceNumber);

	void SetVariableAuthValues(bool authAcceptFlag, int authRuleId, decimal authAmount, bool skipAuthRule);

	void SetPlayPassPromptAccepted(bool playPassPromptAccepted, int playPassPointsEarned);

	string GetPropertyBagEntry(string key);

	void SetPropertyBagEntry(string key, string value);

	void RemovePropertyBagEntry(string key);
}
