using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public static class CardHelper
{
	public static class ErrorCodes
	{
		public const string InvalidPAN = "T001";

		public const string FailedLuhnValidation = "T002";

		public const string InvalidBinRange = "T003";

		public const string InvalidExpiryYear = "T004";

		public const string InvalidExpiryMonth = "T005";

		public const string InvalidFirstDataAccountNumber = "T006";

		public const string InvalidRedboxCardName = "T007";

		public const string InvalidTracks = "T008";

		public const string SwipedCardIsChipEnabled = "T020";

		public const string AmexCannotFallback = "T021";

		public const string ReservationMobileDevice = "T022";

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidPANError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T001", "Invalid PAN provided in track data stream.", "A valid PAN is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidFailedLuhnValidationError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T002", "PAN failed Luhn validation.", "A valid PAN is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidBinRangeError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T003", "Invalid PAN provided in track data stream; unknown bin range.", "A valid PAN is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidExpiryYearError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T004", "Invalid expiry year in track data stream.", "A valid exiry year is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidExpiryMonthError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T005", "Invalid expiry month provided in track data stream.", "A valid expiry month is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidFirstDataAccountNumberError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T006", "Track data will fail First Data account number parsing.", "Valid track data is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidRedboxCardNameError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T007", "Invalid Redbox Card name provided in track data.", "Valid Redbox Card name is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewInvalidTracksError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T008", "No valid tracks in data stream.", "Valid track data is required.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewSwipedCardIsChipEnabledError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T020", "Swiped Card is chip enabled. Chip needs to be inserted", "Insert chip instead of swiping card.");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewAmexCannotFallbackError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T021", "Amex Cannot Fallback", "Amex Cannot Fallback");
		}

		public static Redbox.KioskEngine.ComponentModel.Error NewReservationMobileDeviceError()
		{
			return Redbox.KioskEngine.ComponentModel.Error.NewError("T022", "Cannot use a mobile wallet for a reservation", "Cannot use a mobile wallet for a reservation");
		}
	}

	public const string RedboxGiftCardBIN = "601056";

	public const string RedboxGiftCardName = "REDBOX";

	public static string GenerateCardId(SecureString cardNumber)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		string result = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(ExtractFromSecure(cardNumber)))));
		sHA256Managed.Clear();
		sHA256Managed.Dispose();
		return result;
	}

	public static string GenerateReservationCardId(string firstSix, string lastFour)
	{
		string text = GenerateHash(firstSix);
		string text2 = GenerateHash(lastFour);
		return text + text2;
	}

	public static string GenerateHash(string sourcString)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		string result = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(sourcString))));
		sHA256Managed.Clear();
		sHA256Managed.Dispose();
		return result;
	}

	public static SecureString CopyToSecure(string value)
	{
		SecureString secureString = new SecureString();
		char[] array = value.ToCharArray();
		foreach (char c in array)
		{
			secureString.AppendChar(c);
		}
		return secureString;
	}

	public static string ExtractFromSecure(SecureString value)
	{
		try
		{
			return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
		}
		catch
		{
			return string.Empty;
		}
	}

	public static string EncryptCreditCardFromXmlKey(SecureString cardNumber, string keyXml)
	{
		if (cardNumber.Length == 0)
		{
			return null;
		}
		RSACryptoServiceProvider rSACryptoServiceProvider = null;
		try
		{
			rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(keyXml);
			return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(ExtractFromSecure(cardNumber)), fOAEP: false));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromXmlKey.", e);
		}
		finally
		{
			if (rSACryptoServiceProvider != null)
			{
				rSACryptoServiceProvider.Clear();
				rSACryptoServiceProvider.Dispose();
			}
		}
		return null;
	}

	public static string EncryptCreditCardFromCertificate(SecureString cardNumber, X509Certificate2 certificate)
	{
		if (cardNumber.Length == 0)
		{
			return null;
		}
		RSACryptoServiceProvider rSACryptoServiceProvider = null;
		try
		{
			rSACryptoServiceProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
			return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(ExtractFromSecure(cardNumber)), fOAEP: false));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromCertificate.", e);
		}
		finally
		{
			rSACryptoServiceProvider?.Clear();
		}
		return null;
	}

	public static void CheckForTrackDataErrors(ITrackData trackData)
	{
		if (trackData == null || !trackData.HasPay)
		{
			return;
		}
		if (trackData.FirstSix == null || trackData.LastFour == null)
		{
			AddErrorMessageIfNotDuplicate(trackData, ErrorCodes.NewInvalidPANError());
		}
		if (!trackData.CardType.HasValue)
		{
			AddErrorMessageIfNotDuplicate(trackData, ErrorCodes.NewInvalidInvalidBinRangeError());
		}
		if (trackData.CardType != CardType.RedboxGiftCard)
		{
			if (!IsInNumericRange(trackData.ExpiryMonth, 1, 12))
			{
				AddErrorMessageIfNotDuplicate(trackData, ErrorCodes.NewInvalidInvalidExpiryMonthError());
			}
			if (!IsNumeric(trackData.ExpiryYear))
			{
				AddErrorMessageIfNotDuplicate(trackData, ErrorCodes.NewInvalidInvalidExpiryYearError());
			}
		}
		if (trackData is IUnencryptedTrackData unencryptedTrackData)
		{
			if (unencryptedTrackData.AccountNumber == null || unencryptedTrackData.AccountNumber.Length < 13 || unencryptedTrackData.AccountNumber.Length > 16)
			{
				unencryptedTrackData.AccountNumber.Clear();
				AddErrorMessageIfNotDuplicate(unencryptedTrackData, ErrorCodes.NewInvalidPANError());
			}
			if (unencryptedTrackData.CardType == CardType.RedboxGiftCard && ExtractFromSecure(unencryptedTrackData.Track2).Split('=')[1].Length < 20)
			{
				AddErrorMessageIfNotDuplicate(unencryptedTrackData, ErrorCodes.NewInvalidInvalidFirstDataAccountNumberError());
			}
		}
		if (trackData is IEncryptedTrackData { CardType: var cardType } encryptedTrackData)
		{
			if (cardType == CardType.RedboxGiftCard && encryptedTrackData.EncryptedFlag)
			{
				AddErrorMessageIfNotDuplicate(encryptedTrackData, ErrorCodes.NewInvalidInvalidRedboxCardNameError());
			}
			if (string.IsNullOrEmpty(encryptedTrackData.Expiry) || encryptedTrackData.Expiry.Length != 4 || !int.TryParse(encryptedTrackData.Expiry, out var result) || result <= 0)
			{
				AddErrorMessageIfNotDuplicate(encryptedTrackData, ErrorCodes.NewInvalidInvalidExpiryYearError());
			}
			if (encryptedTrackData.PANLength < 13 || encryptedTrackData.PANLength > 19)
			{
				AddErrorMessageIfNotDuplicate(encryptedTrackData, ErrorCodes.NewInvalidPANError());
			}
		}
	}

	private static void AddErrorMessageIfNotDuplicate(ITrackData trackData, Redbox.KioskEngine.ComponentModel.Error error)
	{
		if (error != null && trackData?.Errors != null && !trackData.Errors.ContainsCode(error.Code))
		{
			trackData.Errors.Add(error);
		}
	}

	private static bool IsInNumericRange(string data, int start, int? end)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}
		if (!int.TryParse(data, out var result))
		{
			return false;
		}
		if (start > result)
		{
			return false;
		}
		if (end.HasValue && end < result)
		{
			return false;
		}
		return true;
	}

	public static void AssignRedboxGiftCardAccountNumber(ITrackData trackData)
	{
		if (trackData.CardType == CardType.RedboxGiftCard && trackData is IUnencryptedTrackData unencryptedTrackData)
		{
			string[] array = ExtractFromSecure(unencryptedTrackData.Track2).Split('=');
			LogHelper.Instance.Log("Using First Data account number parsing.");
			if (array != null && array.Length >= 1 && array[0] != null && array[0].Length >= 15 && array[1] != null && array[1].Length >= 20)
			{
				unencryptedTrackData.AccountNumber = CopyToSecure(array[1].Substring(12, 1) + array[1].Substring(14, 2) + array[0].Substring(6, 9) + array[1].Substring(16, 4));
			}
			else
			{
				LogHelper.Instance.Log("Unable to parse First Data account number");
			}
		}
	}

	public static void AssignCardType(ITrackData trackData, string cardNumber)
	{
		if (!string.IsNullOrEmpty(cardNumber) && trackData != null)
		{
			if (cardNumber.Length >= 6 && cardNumber.Substring(0, 6) == "601056")
			{
				trackData.CardType = CardType.RedboxGiftCard;
			}
			else
			{
				trackData.CardType = CardTypeHelper.GetCardType(cardNumber);
			}
		}
	}

	public static IUnencryptedTrackData Parse(SecureString value)
	{
		IUnencryptedTrackData unencryptedTrackData = new UnencryptedTrackData();
		ErrorList errors = unencryptedTrackData.Errors;
		try
		{
			string[] array = ExtractFromSecure(value).Split(';');
			if (array.Length == 0)
			{
				errors.Add(ErrorCodes.NewInvalidTracksError());
				return unencryptedTrackData;
			}
			if (array.Length > 1)
			{
				unencryptedTrackData.Track1 = CopyToSecure(array[0]);
				unencryptedTrackData.Track2 = CopyToSecure(array[1]);
				ParseNameData(unencryptedTrackData);
			}
			else if (array.Length != 0)
			{
				unencryptedTrackData.Track1.Clear();
				unencryptedTrackData.Track2 = CopyToSecure(array[0]);
			}
			string[] array2 = ExtractFromSecure(unencryptedTrackData.Track2).Split('=');
			unencryptedTrackData.AccountNumber = CopyToSecure(array2[0]);
			AssignCardType(unencryptedTrackData, ExtractFromSecure(unencryptedTrackData.AccountNumber));
			if (unencryptedTrackData.CardType == CardType.RedboxGiftCard)
			{
				if (unencryptedTrackData.FirstName?.ToUpper() != "REDBOX" && unencryptedTrackData.LastName?.ToUpper() != "REDBOX")
				{
					errors.Add(ErrorCodes.NewInvalidInvalidRedboxCardNameError());
					return unencryptedTrackData;
				}
				AssignRedboxGiftCardAccountNumber(unencryptedTrackData);
			}
			if (unencryptedTrackData.CardType != CardType.RedboxGiftCard && !LuhnHelper.IsCreditCardValid(ExtractFromSecure(unencryptedTrackData.AccountNumber)))
			{
				errors.Add(ErrorCodes.NewInvalidFailedLuhnValidationError());
				return unencryptedTrackData;
			}
			unencryptedTrackData.FirstSix = ExtractFromSecure(unencryptedTrackData.AccountNumber).Substring(0, 6);
			unencryptedTrackData.LastFour = ExtractFromSecure(unencryptedTrackData.AccountNumber).Substring(unencryptedTrackData.AccountNumber.Length - 4);
			unencryptedTrackData.ExpiryYear = array2[1].Substring(0, 2);
			unencryptedTrackData.ExpiryMonth = array2[1].Substring(2, 2);
			if (unencryptedTrackData.Track2.Length > 1)
			{
				unencryptedTrackData.Track2 = CopyToSecure(ExtractFromSecure(unencryptedTrackData.Track2).Substring(0, ExtractFromSecure(unencryptedTrackData.Track2).IndexOf("?")));
			}
			CheckForTrackDataErrors(unencryptedTrackData);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardHelper.Parse.", e);
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T999", "An unhandled exception was raised in CardHelper.Parse.", e));
		}
		return unencryptedTrackData;
	}

	private static void ParseNameData(IUnencryptedTrackData unencryptedTrackData)
	{
		ParseNameData(ExtractFromSecure(unencryptedTrackData.Track1), unencryptedTrackData);
	}

	public static void ParseNameData(string nameData, ITrackData trackData)
	{
		Regex regex = new Regex("[a-z][^0-9][a-z]*", RegexOptions.IgnoreCase);
		if (!string.IsNullOrEmpty(nameData) && trackData != null)
		{
			MatchCollection matchCollection = regex.Matches(nameData);
			if (matchCollection.Count > 1)
			{
				trackData.FirstName = matchCollection[0].Groups[0].Value;
				trackData.LastName = matchCollection[1].Groups[0].Value;
			}
			else if (matchCollection.Count > 0)
			{
				trackData.FirstName = matchCollection[0].Groups[0].Value;
			}
		}
	}

	private static bool IsNumeric(string stringToCheck)
	{
		if (!string.IsNullOrEmpty(stringToCheck))
		{
			return new Regex("^[0-9]+$").IsMatch(stringToCheck);
		}
		return false;
	}
}
