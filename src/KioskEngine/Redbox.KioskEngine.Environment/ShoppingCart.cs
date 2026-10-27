using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCart : IShoppingCart
{
	private readonly IDictionary<string, string> m_propertyBag = new Dictionary<string, string>();

	public string Email { get; internal set; }

	public string PostalCode { get; internal set; }

	public DateTime CreatedOn { get; internal set; }

	public long? ReferenceNumber { get; internal set; }

	public CreditElection UseCredits => CreditElection.OptedOut;

	public ShoppingCartType Type { get; internal set; }

	public Guid ShoppingSessionId { get; private set; }

	public bool AuthAcceptFlag { get; internal set; }

	public int AuthRuleId { get; internal set; }

	public decimal AuthAmount { get; internal set; }

	public bool SkipAuthRule { get; internal set; }

	public bool PlayPassPromptAccepted { get; internal set; }

	public int PlayPassPointsEarned { get; internal set; }

	public IDictionary<string, string> PropertyBag => m_propertyBag;

	public ShoppingCart(Guid shoppingSessionId)
	{
		CreatedOn = DateTime.Now;
		ShoppingSessionId = shoppingSessionId;
	}

	public void SetType(ShoppingCartType type)
	{
		LogHelper.Instance.Log("SetType on Shopping Cart for Shopping Session '{0}': type={1}", ShoppingSessionId, type);
		Type = type;
	}

	public void SetEmail(string email)
	{
		LogHelper.Instance.Log("SetEmail on Shopping Cart for Shopping Session '{0}'", ShoppingSessionId);
		Email = email;
	}

	public void SetPostalCode(string postalCode)
	{
		PostalCode = postalCode;
		LogHelper.Instance.Log("Set Postal Code on Shopping Cart for Shopping Session '{0}': postalCode={1}", ShoppingSessionId, postalCode);
	}

	public void SetReferenceNumber(long referenceNumber)
	{
		ReferenceNumber = referenceNumber;
		LogHelper.Instance.Log("Set Reference Number on Shopping Cart for Shopping Session '{0}': referenceNumber={1}", ShoppingSessionId, referenceNumber);
	}

	public void SetVariableAuthValues(bool authAcceptFlag, int authRuleId, decimal authAmount, bool skipAuthRule)
	{
		LogHelper.Instance.Log("SetVariableAuthValues on Shopping Cart for Shopping Session '{0}': authAcceptFlag={1}, authRuleId={2}, authAmount={3}, skipAuthRule={4}", ShoppingSessionId, authAcceptFlag, authRuleId, authAmount, skipAuthRule);
		AuthAcceptFlag = authAcceptFlag;
		AuthRuleId = authRuleId;
		AuthAmount = authAmount;
		SkipAuthRule = skipAuthRule;
	}

	public void SetPlayPassPromptAccepted(bool playPassPromptAccepted, int playPassPointsEarned)
	{
		LogHelper.Instance.Log("SetPlayPassPromptAccepted on Shopping Cart for Shopping Session '{0}': playPassPromptAccepted={1}, playPassPointsEarned={2}", ShoppingSessionId, playPassPromptAccepted, playPassPointsEarned);
		PlayPassPromptAccepted = playPassPromptAccepted;
		PlayPassPointsEarned = playPassPointsEarned;
	}

	public void SetPropertyBagEntry(string key, string value)
	{
		if (!string.IsNullOrWhiteSpace(key))
		{
			m_propertyBag[key] = value;
		}
	}

	public string GetPropertyBagEntry(string key)
	{
		if (!m_propertyBag.ContainsKey(key))
		{
			return null;
		}
		return m_propertyBag[key];
	}

	public void RemovePropertyBagEntry(string key)
	{
		if (m_propertyBag.ContainsKey(key))
		{
			m_propertyBag.Remove(key);
		}
	}
}
