using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Redbox.Core;

namespace Redbox.Rental.Services;

public static class ValidationServices
{
	private static readonly Regex _hasNumber = new Regex("[0-9]+");

	private static readonly Regex _hasUpperChar = new Regex("[A-Z]+");

	private static readonly Regex _hasMiniMaxChars = new Regex(".{8,15}");

	private static readonly Regex _hasLowerChar = new Regex("[a-z]+");

	private static readonly Regex _hasSymbols = new Regex("[!@#$%^&*()_+=\\[{\\]};:<>|./?,-]");

	public static bool IsValidPromoCode(string promoCode)
	{
		if (!string.IsNullOrWhiteSpace(promoCode))
		{
			return promoCode.Length > 3;
		}
		return false;
	}

	public static bool IsValidEmailAddress(string emailAddress)
	{
		string errorMessage = null;
		bool result = false;
		try
		{
			result = ValidateEmail(emailAddress, out errorMessage);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				LogHelper.Instance.Log("Email Address validation error: {0}", errorMessage);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in ValidationServices.IsValidEmailAddress.", e);
		}
		return result;
	}

	private static bool ValidateEmail(string emailAddress, out string errorMessage)
	{
		errorMessage = null;
		if (string.IsNullOrEmpty(emailAddress))
		{
			errorMessage = "email address is empty";
			return false;
		}
		int num = emailAddress.IndexOf("@");
		if (num < 0)
		{
			errorMessage = "Missing @ symbol";
			return false;
		}
		if (emailAddress.Count((char x) => x == '@') > 1)
		{
			errorMessage = "Multiple @ symbols found";
			return false;
		}
		if (emailAddress.Contains(".."))
		{
			errorMessage = "Contiguous periods found";
			return false;
		}
		if (num < 1)
		{
			errorMessage = "Handle nil value";
			return false;
		}
		string text = emailAddress.Substring(0, num);
		if (emailAddress.Contains(" "))
		{
			errorMessage = "Spaces found";
			return false;
		}
		if (!emailAddress.Contains("."))
		{
			errorMessage = "Missing . symbol";
			return false;
		}
		if (text.StartsWith("."))
		{
			errorMessage = "Starting period found";
			return false;
		}
		if (text.EndsWith("."))
		{
			errorMessage = "Ending period found";
			return false;
		}
		int num2 = emailAddress.LastIndexOf(".");
		if (!ValidateTopLevelDomain(emailAddress.Substring(Math.Min(num2 + 1, emailAddress.Length - 1))))
		{
			errorMessage = "Invalid Top-Level Domain";
			return false;
		}
		string text2 = emailAddress.Substring(num + 1);
		string[] array = text2.Substring(0, text2.LastIndexOf(".")).Split('.');
		if (array != null)
		{
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				if (!ValidateDomain(text3))
				{
					errorMessage = $"Invalid domain part {text3}";
					return false;
				}
			}
		}
		if (!ValidateHandle(text, text2))
		{
			errorMessage = $"Invalid handle {text},  domain {text2}";
			return false;
		}
		return true;
	}

	private static bool ValidateTopLevelDomain(string topLevelDomain)
	{
		return new List<string>
		{
			"COM", "EDU", "GOV", "INFO", "INT", "JOBS", "MIL", "NET", "ORG", "US",
			"BIZ", "MX", "CC", "COOP", "MOBI", "CA", "PH", "AERO", "INT", "MUSEUM",
			"TRAVEL", "TEL", "PRO", "WS", "FM", "AC", "AD", "AE", "AF", "AG",
			"AI", "AL", "AM", "AN", "AO", "AQ", "AR", "ARPA", "AS", "ASIA",
			"AT", "AU", "AW", "AX", "AZ", "BA", "BB", "BD", "BE", "BF",
			"BG", "BH", "BI", "BJ", "BM", "BN", "BO", "BR", "BS", "BT",
			"BV", "BW", "BY", "BZ", "CAT", "CD", "CF", "CG", "CH", "CI",
			"CK", "CL", "CM", "CN", "CO", "CR", "CU", "CV", "CW", "CX",
			"CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE",
			"EG", "ER", "ES", "ET", "EU", "FI", "FJ", "FK", "FO", "FR",
			"GA", "GB", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM",
			"GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK",
			"HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN",
			"IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE",
			"KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ",
			"LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV",
			"LY", "MA", "MC", "MD", "ME", "MG", "MH", "MK", "ML", "MM",
			"MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW",
			"MY", "MZ", "NA", "NAME", "NC", "NE", "NF", "NG", "NI", "NL",
			"NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG",
			"PK", "PL", "PM", "PN", "POST", "PR", "PS", "PT", "PW", "PY",
			"QA", "RE", "RO", "RS", "RU", "RW", "SA", "SB", "SC", "SD",
			"SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO",
			"SR", "ST", "SU", "SV", "SX", "SY", "SZ", "TC", "TD", "TF",
			"TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO", "TP", "TR",
			"TT", "TV", "TW", "TZ", "UA", "UG", "UK", "UY", "UZ", "VA",
			"VC", "VE", "VG", "VI", "VN", "VU", "WF", "XXX", "YE", "YT",
			"ZA", "ZM", "ZW"
		}.Contains(topLevelDomain?.ToUpper() ?? null);
	}

	private static bool ValidateDomain(string domainPart)
	{
		if (string.IsNullOrWhiteSpace(domainPart))
		{
			return false;
		}
		domainPart = domainPart.Trim();
		if (!domainPart.StartsAndEndsAlphaNumeric())
		{
			return false;
		}
		if (!domainPart.GetMiddle().IsAphaNumericOr("-_"))
		{
			return false;
		}
		return true;
	}

	private static bool ValidateHandle(string handleText, string domain)
	{
		if (string.IsNullOrWhiteSpace(domain))
		{
			return false;
		}
		domain = domain.Trim();
		handleText = handleText.Trim();
		if (domain.Equals("YAHOO.COM", StringComparison.CurrentCultureIgnoreCase))
		{
			if (handleText.Length <= 32)
			{
				if (!handleText.StartsAlpha())
				{
					return false;
				}
				if (!handleText.IsAphaNumericOr("-_."))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		if (domain.Equals("HOTMAIL.COM", StringComparison.CurrentCultureIgnoreCase))
		{
			if (!handleText.StartsAlpha())
			{
				return false;
			}
			if (!handleText.IsAphaNumericOr("-_."))
			{
				return false;
			}
			return true;
		}
		if (domain.Equals("MSN.COM", StringComparison.CurrentCultureIgnoreCase) || domain.Equals("GMAIL.COM", StringComparison.CurrentCultureIgnoreCase))
		{
			if (!handleText.IsAphaNumericOr("-_."))
			{
				return false;
			}
			return true;
		}
		if (!handleText.GetFirst().IsAphaNumericOr("$-_."))
		{
			return false;
		}
		if (!handleText.IsAphaNumericOr("$-_.+!&'/=?^|~#*"))
		{
			return false;
		}
		return true;
	}

	public static bool IsValidCode(string codeValue)
	{
		if (!string.IsNullOrEmpty(codeValue))
		{
			return true;
		}
		return false;
	}

	public static bool IsValidEmailAndCode(string emailAddress, string codeValue)
	{
		if (IsValidCode(codeValue))
		{
			return IsValidEmailAddress(emailAddress);
		}
		return false;
	}

	public static bool IsValidPhoneAndPin(string phoneNumber, string pinValue)
	{
		return false;
	}
}
