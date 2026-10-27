using System.Collections.Generic;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

public class ObfuscationHelper : IObfuscationService
{
	public class Constants
	{
		public const string Username = "Username";

		public const string Password = "Password";

		public const string Email = "Email";

		public const string LowerCaseEmail = "email";

		public const string EmailAddress = "EmailAddress";

		public const string LoginEmail = "LoginEmail";

		public const string PhoneNumber = "PhoneNumber";

		public const string MobilePhoneNumber = "MobilePhoneNumber";

		public const string Mobile_Phone_Number = "mobile_phone_number";

		public const string Pin = "Pin";

		public const string CardHash = "CardHash";

		public const string CardId = "CardId";

		public const string Track2 = "Track2";

		public const string Number = "Number";

		public const string LastFour = "LastFour";

		public const string LastName = "LastName";

		public const string CustomerProfileLastName = "CustomerProfileLastName";

		public const string FirstName = "FirstName";

		public const string CustomerProfileFirstName = "CustomerProfileFirstName";

		public const string PostalCode = "PostalCode";

		public const string ExpirationYear = "ExpirationYear";

		public const string ExpirationMonth = "ExpirationMonth";

		public const string BirthDate = "BirthDate";

		public const string BIN = "BIN";

		public const string KSN = "KSN";

		public const string ICEncryptedData = "ICEncryptedData";

		public const string ReaderSerialNumber = "ReaderSerialNumber";

		public const string user_name = "user_name";

		public const string password = "password";

		public const char X = 'x';

		public const char PoundSign = '#';
	}

	private class ObfuscationInstance
	{
		public string Key { get; set; }

		public char ReplacementCharacter { get; set; }

		public int NumberOfNonObfuscatedLeadingCharacters { get; set; }

		public int NumberOfNonObfuscatedTrailingCharacters { get; set; }
	}

	private static List<string> _keyRemovalList = new List<string> { "Username", "Password", "Pin", "CardHash", "CardId", "Track2", "Number", "user_name", "password" };

	private static List<ObfuscationInstance> _obfuscationsList = new List<ObfuscationInstance>
	{
		new ObfuscationInstance
		{
			Key = "Email",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "email",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "EmailAddress",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "LoginEmail",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "PhoneNumber",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "MobilePhoneNumber",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "mobile_phone_number",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "LastFour",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 1
		},
		new ObfuscationInstance
		{
			Key = "LastName",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 2,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "CustomerProfileLastName",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 2,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "FirstName",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "CustomerProfileFirstName",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "PostalCode",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 1
		},
		new ObfuscationInstance
		{
			Key = "ExpirationYear",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "ExpirationMonth",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "BirthDate",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 1,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "BIN",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "KSN",
			ReplacementCharacter = '#',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "ICEncryptedData",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		},
		new ObfuscationInstance
		{
			Key = "ReaderSerialNumber",
			ReplacementCharacter = 'x',
			NumberOfNonObfuscatedLeadingCharacters = 0,
			NumberOfNonObfuscatedTrailingCharacters = 0
		}
	};

	public string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		string result = null;
		if (!string.IsNullOrEmpty(msgText))
		{
			if (leading + trailing > msgText.Length)
			{
				leading = 0;
				trailing = 0;
			}
			string obj = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
			string text = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
			string text2 = new string(newChar, msgText.Length - (leading + trailing));
			result = obj + text2 + text;
		}
		return result;
	}

	public string ObfuscateStringData(string value, int leading, int trailing)
	{
		return ObfuscateMessageData(value, 'x', leading, trailing);
	}

	public string ObfuscateNumericData(string value, int leading, int trailing)
	{
		return ObfuscateMessageData(value, '#', leading, trailing);
	}

	public void ObfuscateDictionary(Dictionary<string, object> workDictionary)
	{
		foreach (string keyRemoval in _keyRemovalList)
		{
			if (workDictionary.ContainsKey(keyRemoval) && workDictionary[keyRemoval] != null)
			{
				workDictionary.Remove(keyRemoval);
			}
		}
		foreach (ObfuscationInstance obfuscations in _obfuscationsList)
		{
			if (workDictionary.ContainsKey(obfuscations.Key) && workDictionary[obfuscations.Key] != null)
			{
				workDictionary[obfuscations.Key] = ObfuscateMessageData(workDictionary[obfuscations.Key].ToString(), obfuscations.ReplacementCharacter, obfuscations.NumberOfNonObfuscatedLeadingCharacters, obfuscations.NumberOfNonObfuscatedTrailingCharacters);
			}
		}
	}
}
