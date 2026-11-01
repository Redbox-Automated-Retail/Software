using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DeviceService.ComponentModel;

namespace DeviceService.Domain;

public class ParseTags
{
	public CardBrandEnum CardBrand;

	public FallbackType? FallbackReason;

	private const string CDIV = "CDIV";

	private const string CNSUP = "CNSUP";

	private readonly IDictionary<string, FallbackStatusAction> _fallbackErrorStatusActions = new Dictionary<string, FallbackStatusAction>
	{
		{
			"CDIV",
			FallbackStatusAction.IncrementFallbackCounter
		},
		{
			"CNSUP",
			FallbackStatusAction.NoAction
		},
		{
			"CRPRE",
			FallbackStatusAction.NoAction
		},
		{
			"APBLK",
			FallbackStatusAction.ResetFallbackCounter
		},
		{
			"CABLK",
			FallbackStatusAction.ResetFallbackCounter
		},
		{
			"CEXP",
			FallbackStatusAction.ResetFallbackCounter
		}
	};

	public string Last4 { get; set; }

	public string First6 { get; set; }

	public string AID { get; set; }

	public string PAN { get; set; }

	public string Track2 { get; set; }

	public string ApplicationLabel { get; set; }

	public string CardHolderName { get; set; }

	public bool IsMobileWallet { get; set; }

	public string ErrorCode { get; set; }

	public string FF1FOnGuardCardData { get; set; }

	public FallbackStatusAction Fallback { get; set; } = FallbackStatusAction.ResetFallbackCounter;

	public bool Tag9F27CryptogramDeclined { get; set; }

	public void ParseEMVTags(string Tag, int TagLength, string TagDataStr, byte[] TagDataByte, CardSourceType transactionType, Action<string> log, IList<Error> errors)
	{
		GeneralSourceType generalSourceType = transactionType.GetGeneralSourceType();
		string piiMask = "[Pii Data]";
		TagDetails tagDetails = TagConstants.Details.FirstOrDefault((TagDetails x) => x.Tag == Tag);
		IUC285Proxy.Log(string.Format("Tag {0}({1}) Length = {2} TagData = {3}", Tag, tagDetails?.Description ?? "Undefined", TagLength, (tagDetails != null && tagDetails.IsPii) ? piiMask : TagDataStr.ToUpper()));
		if (Tag == "4F" || Tag == "84")
		{
			AID = TagDataStr.ToUpper();
			if (AID.Contains("A000000003") || AID.Contains("A00000009"))
			{
				CardBrand = CardBrandEnum.VISA;
			}
			else if (AID.Contains("A00000002"))
			{
				CardBrand = CardBrandEnum.Amex;
			}
			else if (AID.Contains("A000000004"))
			{
				CardBrand = CardBrandEnum.Mastercard;
			}
			else if (AID.Contains("A0000001") || AID.Contains("A000000324"))
			{
				CardBrand = CardBrandEnum.Discover;
			}
			else if (AID.Contains("A000000333"))
			{
				CardBrand = CardBrandEnum.UnionPay;
			}
			else
			{
				CardBrand = CardBrandEnum.Unknown;
			}
			IUC285Proxy.Log($"CardBrand: {CardBrand}");
		}
		else if (Tag == "50")
		{
			ApplicationLabel = ConvertHexToString(TagDataStr, errors).ToUpper();
		}
		else if (Tag == "57")
		{
			Track2 = TagDataStr.ToUpper();
			First6 = TagDataStr.Substring(0, 6);
			Last4 = TagDataStr.Substring(12, 4);
		}
		else if (Tag == "5A")
		{
			PAN = TagDataStr;
		}
		else
		{
			if (Tag == "8A")
			{
				return;
			}
			if (Tag == "9B")
			{
				IUC285Proxy.Log("--------Transaction Status Information (T9B)--------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits[0] + " - Offline data authentication was performed");
				IUC285Proxy.Log("  " + byte1Bits[1] + " - Cardholder verification was performed");
				IUC285Proxy.Log("  " + byte1Bits[2] + " - Card risk management was performed");
				IUC285Proxy.Log("  " + byte1Bits[3] + " - Issuer authentication was performed");
				IUC285Proxy.Log("  " + byte1Bits[4] + " - Terminal risk management was performed");
				IUC285Proxy.Log("  " + byte1Bits[5] + " - Script processing was performed");
				IUC285Proxy.Log("  " + byte1Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				IUC285Proxy.Log("  " + byte2Bits[0] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[1] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[2] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[3] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F10")
			{
				if (CardBrand.Equals(CardBrandEnum.Amex))
				{
					IUC285Proxy.Log(string.Concat("--------Issuer Application Data(T9F10) ", CardBrand, " --------"));
					int offset = 0;
					string lengthByte = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + lengthByte);
					offset += 2;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					string cvrLength = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length = " + cvrLength);
					offset += 2;
					int cvrLengthValue = Convert.ToInt32(cvrLength);
					int byteIndex = 0;
					string[] hexBytes = new string[cvrLengthValue];
					int[] byteValues = new int[cvrLengthValue];
					string[] cvrBits = new string[cvrLengthValue];
					while (byteIndex < cvrLengthValue && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("-----");
					if (cvrLengthValue == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[0]);
					string cvrByte2Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte2Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte2Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte2Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte2Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte2Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte2Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte2Bit4 = cvrBits[0].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication did not fail");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication failed");
					}
					string cvrByte2Bit3 = cvrBits[0].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification performed");
					}
					string cvrByte2Bit2 = cvrBits[0].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification did not fail");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification failed");
					}
					string cvrByte2Bit1 = cvrBits[0].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Not unable to go online");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[1]);
					string cvrByte3Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte3Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction not completed");
					}
					string cvrByte3Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte3Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit exceeded");
					}
					string cvrByte3Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte3Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters exceeded");
					}
					string cvrByte3Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte3Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - New card");
					}
					string cvrByte3Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte3Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication failed on last online transaction");
					}
					string cvrByte3Bit3 = cvrBits[1].Substring(5, 1);
					if (cvrByte3Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication performed after online authorization");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication not performed after online authorization");
					}
					string cvrByte3Bit2 = cvrBits[1].Substring(6, 1);
					if (cvrByte3Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string cvrByte3Bit1 = cvrBits[1].Substring(7, 1);
					if (cvrByte3Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication failed on last transaction or transaction declined offline");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[2]);
					string cvrByte4Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte4Bits8To5 + " - Number of Issuer Script Commands received after the second GENERATE AC command");
					string cvrByte4Bit4 = cvrBits[2].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing did not fail on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing failed on last transaction");
					}
					string cvrByte4Bit3 = cvrBits[2].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string cvrByte4Bit2 = cvrBits[2].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication performed");
					}
					string cvrByte4Bit1 = cvrBits[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + " -    RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Amex) && generalSourceType == GeneralSourceType.Tap)
				{
					IUC285Proxy.Log(string.Concat("-----Issuer Application Data(T9F10) ", CardBrand, " -----"));
					int offset = 0;
					string lengthByte = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + lengthByte);
					offset += 2;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					string cvrLengthIndicator = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + cvrLengthIndicator);
					offset += 2;
					int cvrLengthIndicatorValue = Convert.ToInt32(cvrLengthIndicator);
					int byteIndex = 0;
					string[] hexBytes = new string[cvrLengthIndicatorValue];
					int[] byteValues = new int[cvrLengthIndicatorValue];
					string[] cvrBits = new string[cvrLengthIndicatorValue];
					while (byteIndex < cvrLengthIndicatorValue && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("-----");
					if (cvrLengthIndicatorValue == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[0]);
					string cvrByte2Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte2Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte2Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte2Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte2Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte2Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte2Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte2Bit4 = cvrBits[0].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - RFU");
					}
					string cvrByte2Bit3 = cvrBits[0].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - RFU");
					}
					string cvrByte2Bit2 = cvrBits[0].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - RFU");
					}
					string cvrByte2Bit1 = cvrBits[0].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[1]);
					string cvrByte3Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte3Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction not completed");
					}
					string cvrByte3Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte3Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit exceeded");
					}
					string cvrByte3Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte3Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters exceeded");
					}
					string cvrByte3Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte3Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - New card");
					}
					string cvrByte3Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte3Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication failed on last online transaction");
					}
					string cvrByte3Bit3 = cvrBits[1].Substring(5, 1);
					if (cvrByte3Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - RFU");
					}
					string cvrByte3Bit2 = cvrBits[1].Substring(6, 1);
					if (cvrByte3Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string cvrByte3Bit1 = cvrBits[1].Substring(7, 1);
					if (cvrByte3Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[2]);
					string cvrByte4Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte4Bits8To5 + " - RFU");
					string cvrByte4Bit4 = cvrBits[2].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - RFU");
					}
					string cvrByte4Bit3 = cvrBits[2].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string cvrByte4Bit2 = cvrBits[2].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication performed");
					}
					string cvrByte4Bit1 = cvrBits[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + " -    RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.VISA))
				{
					IUC285Proxy.Log(string.Concat("---------Issuer Application Data (T9F10) ", CardBrand, " ---------"));
					int offset = 0;
					string lengthByte = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + lengthByte);
					offset += 2;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string text52 = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + text52.ToUpper());
					offset += 2;
					string cvrLength = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length = " + cvrLength);
					offset += 2;
					int cvrLengthValue = Convert.ToInt32(cvrLength);
					int byteIndex = 0;
					string[] hexBytes = new string[cvrLengthValue];
					int[] byteValues = new int[cvrLengthValue];
					string[] cvrBits = new string[cvrLengthValue];
					while (byteIndex < cvrLengthValue && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("-----");
					if (cvrLengthValue == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[0]);
					string cvrByte2Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte2Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte2Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte2Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte2Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte2Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte2Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte2Bit4 = cvrBits[0].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication successful or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication failed");
					}
					string cvrByte2Bit3 = cvrBits[0].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification performed");
					}
					string cvrByte2Bit2 = cvrBits[0].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification passed or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification failed");
					}
					string cvrByte2Bit1 = cvrBits[0].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Able to go online or offline transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[1]);
					string cvrByte3Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte3Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction not completed");
					}
					string cvrByte3Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte3Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit exceeded");
					}
					string cvrByte3Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte3Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters exceeded");
					}
					string cvrByte3Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte3Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - New card");
					}
					string cvrByte3Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte3Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication successful on last online transaction or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication failed on last online transaction");
					}
					string cvrByte3Bit3 = cvrBits[1].Substring(5, 1);
					if (cvrByte3Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication performed after online authorization or offline transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication not performed after online authorization or offline transaction");
					}
					string cvrByte3Bit2 = cvrBits[1].Substring(6, 1);
					if (cvrByte3Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application not blocked by card");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application blocked by card");
					}
					string cvrByte3Bit1 = cvrBits[1].Substring(7, 1);
					if (cvrByte3Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication passed or was not performed on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication failed on last transaction or transaction declined offline");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[2]);
					string cvrByte4Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte4Bits8To5 + " - Number of Issuer Script Commands");
					string cvrByte4Bit4 = cvrBits[2].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing passed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing failed");
					}
					string cvrByte4Bit3 = cvrBits[2].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication passed or was not performed on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string cvrByte4Bit2 = cvrBits[2].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication performed");
					}
					string cvrByte4Bit1 = cvrBits[2].Substring(7, 1);
					if (cvrByte4Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit1 + "    - PIN verification command received for a PIN-Expecting card or card does not expect PIN (i.e. Offline PIN verification not supported)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit1 + "    - PIN verification command NOT received for a PIN-Expecting card");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Discover))
				{
					IUC285Proxy.Log(string.Concat("------Issuer Application Data(T9F10) ", CardBrand, " ------"));
					int offset = 0;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					int byteIndex = 0;
					string[] hexBytes = new string[6];
					int[] byteValues = new int[6];
					string[] cvrBits = new string[6];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 12).ToUpper());
					while (byteIndex < 6 && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: " + cvrBits[0]);
					string cvrByte1Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte1Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte1Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte1Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte1Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte1Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte1Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte1Bits4To1 = cvrBits[0].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + cvrByte1Bits4To1 + " - RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[1]);
					string cvrByte2Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte2Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - DDA NOT Returned");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - DDA Returned");
					}
					string cvrByte2Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte2Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - CDA NOT returned in 1st GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - CDA returned in 1st GENERATE AC");
					}
					string cvrByte2Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte2Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - CDA NOT returned in 2nd GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - CDA returned in 2nd GENERATE AC");
					}
					string cvrByte2Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte2Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - Off-line PIN Verification NOT performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - Off-line PIN Verification performed");
					}
					string cvrByte2Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Off-line Enciphered PIN Verification NOT performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Off-line Enciphered PIN Verification performed");
					}
					string cvrByte2Bit3 = cvrBits[1].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Off-line PIN Verification NOT Successful");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Off-line PIN Verification Successful");
					}
					string cvrByte2Bit2 = cvrBits[1].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - CAC-default NOT ignored with CAT3 terminal");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - CAC-default ignored with CAT3 terminal");
					}
					string cvrByte2Bit1 = cvrBits[1].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[2]);
					string cvrByte3Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte3Bits8To5 + " - Right nibble of Script Counter");
					string cvrByte3Bits4To1 = cvrBits[2].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + cvrByte3Bits4To1 + " - Right nibble of PIN try Counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[3]);
					string cvrByte4Bit8 = cvrBits[3].Substring(0, 1);
					if (cvrByte4Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte4Bit8 + "  - Go on-line during previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte4Bit8 + "  - Go on-line during previous transaction");
					}
					string cvrByte4Bit7 = cvrBits[3].Substring(1, 1);
					if (cvrByte4Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Issuer Authentication performed during previous online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Issuer Authentication not performed during previous online transaction");
					}
					string cvrByte4Bit6 = cvrBits[3].Substring(2, 1);
					if (cvrByte4Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - Issuer Authentication NOT failed during previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - Issuer Authentication failed during previous transaction");
					}
					string cvrByte4Bit5 = cvrBits[3].Substring(3, 1);
					if (cvrByte4Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - Script NOT received on previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - Script received on previous transaction");
					}
					string cvrByte4Bit4 = cvrBits[3].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - Script NOT failed on previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - Script failed on previous transaction");
					}
					string cvrByte4Bit3 = cvrBits[3].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - PTH NOT forced on-line");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - PTH forced on-line");
					}
					string cvrByte4Bit2 = cvrBits[3].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - PDOL NOT forced on-line (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - PDOL forced on-line (during GPO)");
					}
					string cvrByte4Bit1 = cvrBits[3].Substring(7, 1);
					if (cvrByte4Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - PDOL NOT forced decline (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - PDOL forced decline (during GPO)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: " + cvrBits[4]);
					string cvrByte5Bit8 = cvrBits[4].Substring(0, 1);
					if (cvrByte5Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - Valid PDOL check");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - invalid PDOL check");
					}
					string cvrByte5Bit7 = cvrBits[4].Substring(1, 1);
					if (cvrByte5Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Off-line PIN verification NOT failed");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Off-line PIN verification failed");
					}
					string cvrByte5Bit6 = cvrBits[4].Substring(2, 1);
					if (cvrByte5Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - PIN Try Limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - PIN Try Limit exceeded");
					}
					string cvrByte5Bit5 = cvrBits[4].Substring(3, 1);
					if (cvrByte5Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Lower Consecutive Offline Transaction limit exceeded (LCOL). Note: May be profile-specific");
					}
					string cvrByte5Bit4 = cvrBits[4].Substring(4, 1);
					if (cvrByte5Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Upper Consecutive Offline Transaction limit exceeded (UCOL). Note: May be profile-specific");
					}
					string cvrByte5Bit3 = cvrBits[4].Substring(5, 1);
					if (cvrByte5Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA).Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA).Note: May be profile-specific");
					}
					string cvrByte5Bit2 = cvrBits[4].Substring(6, 1);
					if (cvrByte5Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA). Note: May be profile-specific");
					}
					string cvrByte5Bit1 = cvrBits[4].Substring(7, 1);
					if (cvrByte5Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - Single Transaction Amount limit NOT exceeded (STA). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - Single Transaction Amount limit exceeded (STA). Note: May be profile-specific");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: " + cvrBits[5]);
					string cvrByte6Bits8To5 = cvrBits[5].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte6Bits8To5 + " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
					string cvrByte6Bits4To1 = cvrBits[5].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + cvrByte6Bits4To1 + " - Transaction profile Identifier (0000 by default)");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Discover) && generalSourceType == GeneralSourceType.Tap)
				{
					IUC285Proxy.Log(string.Concat("-------Issuer Application Data(T9F10) ", CardBrand, " -------"));
					int offset = 0;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					int byteIndex = 0;
					string[] hexBytes = new string[8];
					int[] byteValues = new int[8];
					string[] byteBits = new string[8];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 12).ToUpper());
					while (byteIndex < 8 && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						byteBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: Card decision " + byteBits[0]);
					string cvrByte1Bit8 = byteBits[0].Substring(0, 1);
					if (cvrByte1Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte1Bit8 + "  - Online PIN NOT Required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte1Bit8 + "  - Online PIN Required");
					}
					string cvrByte1Bit7 = byteBits[0].Substring(1, 1);
					if (cvrByte1Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte1Bit7 + "  - Signature NOT Required");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte1Bit7 + "  - Signature Required");
					}
					string cvrByte1Bits6To5 = byteBits[0].Substring(2, 2);
					switch (cvrByte1Bits6To5)
					{
					case "00":
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - RFU");
						break;
					}
					string cvrByte1Bit4 = byteBits[0].Substring(4, 1);
					if (cvrByte1Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4 = " + cvrByte1Bit4 + " - PID limit NOT reached");
					}
					else
					{
						IUC285Proxy.Log("    bit 4 = " + cvrByte1Bit4 + " - PID limit reached");
					}
					string cvrByte1Bits5To3 = byteBits[0].Substring(3, 3);
					IUC285Proxy.Log("    bit 3-1 = " + cvrByte1Bits5To3 + " - Script counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: Compared with CAC-CRM and CAC-CVM " + byteBits[1]);
					string cvrByte2Bit8 = byteBits[1].Substring(0, 1);
					if (cvrByte2Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - Online cryptogram NOT required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - Online cryptogram required");
					}
					string cvrByte2Bit7 = byteBits[1].Substring(1, 1);
					if (cvrByte2Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - Transaction type NOT required to be processed online with online PIN CVM(no cashback)");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - Transaction type required to be processed online with online PIN CVM(cashback)");
					}
					string cvrByte2Bit6 = byteBits[1].Substring(2, 1);
					if (cvrByte2Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - Transaction type NOT required to be processed offline without any CVM(no refund type transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - Transaction type required to be processed offline without any CVM(refund type transaction)");
					}
					string cvrByte2Bit5 = byteBits[1].Substring(3, 1);
					if (cvrByte2Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - NOT a domestic transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - Domestic transaction");
					}
					string cvrByte2Bit4 = byteBits[1].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - NOT an International transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - International transaction");
					}
					string cvrByte2Bit3 = byteBits[1].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - PIN Try Limit NOT exceeded (Dual-Interface implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - PIN Try Limit exceeded (Dual-Interface implementation only)");
					}
					string cvrByte2Bit2 = byteBits[1].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Confirmation Code Verification performed (Mobile implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Confirmation Code Verification NOT performed (Mobile implementation only)");
					}
					string cvrByte2Bit1 = byteBits[1].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Confirmation Code Verification passed (Mobile implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Confirmation Code Verification NOT performed & failed (Mobile implementation only");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: CVM related actions " + byteBits[2]);
					string cvrByte3Bit8 = byteBits[2].Substring(0, 1);
					if (cvrByte3Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8 = " + cvrByte3Bit8 + " - CVM NOT required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8 = " + cvrByte3Bit8 + " - CVM required");
					}
					string cvrByte3Bit7 = byteBits[2].Substring(1, 1);
					if (cvrByte3Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7 = " + cvrByte3Bit7 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 7 = " + cvrByte3Bit7 + " - RFU");
					}
					string cvrByte3Bit6 = byteBits[2].Substring(2, 1);
					if (cvrByte3Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6 = " + cvrByte3Bit6 + " - Consecutive CVM Transaction limit 1 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6 = " + cvrByte3Bit6 + " - Consecutive CVM Transaction limit 1 exceeded (CVM-Cons 1)");
					}
					string cvrByte3Bit5 = byteBits[2].Substring(3, 1);
					if (cvrByte3Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5 = " + cvrByte3Bit5 + " - Consecutive CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 5 = " + cvrByte3Bit5 + " - Consecutive CVM Transaction limit 2 exceeded (CVM-Cons 1)");
					}
					string cvrByte3Bit4 = byteBits[2].Substring(4, 1);
					if (cvrByte3Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4 = " + cvrByte3Bit4 + " - Cumulative CVM Transaction amount limit 1 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4 = " + cvrByte3Bit4 + " - Cumulative CVM Transaction amount limit 1 exceeded (CVM-Cons 1)");
					}
					string cvrByte3Bit3 = byteBits[2].Substring(5, 1);
					if (cvrByte3Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3 = " + cvrByte3Bit3 + " - Cumulative CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3 = " + cvrByte3Bit3 + " - Cumulative CVM Transaction limit 2 exceeded (CVM-Cons 1)");
					}
					string cvrByte3Bit2 = byteBits[2].Substring(6, 1);
					if (cvrByte3Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2 = " + cvrByte3Bit2 + " - CVM Single Transaction Amount limit 1 NOT exceeded (CVM-STA 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2 = " + cvrByte3Bit2 + " - CVM Single Transaction Amount limit 1 exceeded (CVM-STA 1)");
					}
					string cvrByte3Bit1 = byteBits[2].Substring(7, 1);
					if (cvrByte3Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1 = " + cvrByte3Bit1 + " - CVM Single Transaction Amount limit 2 NOT exceeded (CVM-STA 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1 = " + cvrByte3Bit1 + " - CVM Single Transaction Amount limit 2 exceeded (CVM-STA 1)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: CRM related actions " + byteBits[3]);
					string cvrByte4Bit8 = byteBits[3].Substring(0, 1);
					if (cvrByte4Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte4Bit8 + "  - CDA did NOT fail during previous contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte4Bit8 + "  - CDA failed during previous contactless transaction");
					}
					string cvrByte4Bit7 = byteBits[3].Substring(1, 1);
					if (cvrByte4Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Last contactless transaction was completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Last contactless transaction was not completed");
					}
					string cvrByte4Bit6 = byteBits[3].Substring(2, 1);
					if (cvrByte4Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - ‘Go on-line next transaction’ was NOT set by contact or contactless application");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - ‘Go on-line next transaction’ was set by contact or contactless application");
					}
					string cvrByte4Bit5 = byteBits[3].Substring(3, 1);
					if (cvrByte4Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - Issuer Authentication did NOT fail during previous contact or contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - Issuer Authentication did fail during previous contact or contactless transaction");
					}
					string cvrByte4Bit4 = byteBits[3].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - Script did NOT fail on previous contact or contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - Script did fail on previous contact or contactless transaction");
					}
					string cvrByte4Bit3 = byteBits[3].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - NO Invalid PDOL check");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - Invalid PDOL check");
					}
					string cvrByte4Bit2 = byteBits[3].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - PDOL did NOT force online (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - PDOL did force online (during GPO)");
					}
					string cvrByte4Bit1 = byteBits[3].Substring(7, 1);
					if (cvrByte4Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - PDOL did NOT force decline (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - PDOL did force decline (during GPO)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: CRM related actions " + byteBits[4]);
					string cvrByte5Bit8 = byteBits[4].Substring(0, 1);
					if (cvrByte5Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - Consecutive Contactless Transaction limit NOT exceeded (CL-Cons)");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - Consecutive Contactless Transaction limit exceeded (CL-Cons)");
					}
					string cvrByte5Bit7 = byteBits[4].Substring(1, 1);
					if (cvrByte5Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Cumulative Contactless Transaction limit NOT exceeded (CL-Cumul)");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Cumulative Contactless Transaction limit exceeded (CL-Cumul)");
					}
					string cvrByte5Bit6 = byteBits[4].Substring(2, 1);
					if (cvrByte5Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - Single Contactless Transaction Amount limit NOT exceeded (CL-STA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - Single Contactless Transaction Amount limit exceeded (CL-STA)");
					}
					string cvrByte5Bit5 = byteBits[4].Substring(3, 1);
					if (cvrByte5Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL)");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Lower Consecutive Offline Transaction limit exceeded (LCOL)");
					}
					string cvrByte5Bit4 = byteBits[4].Substring(4, 1);
					if (cvrByte5Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Upper Consecutive Offline Transaction limit exceeded (UCOL)");
					}
					string cvrByte5Bit3 = byteBits[4].Substring(5, 1);
					if (cvrByte5Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA)");
					}
					string cvrByte5Bit2 = byteBits[4].Substring(6, 1);
					if (cvrByte5Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA)");
					}
					string cvrByte5Bit1 = byteBits[4].Substring(7, 1);
					if (cvrByte5Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - Single Transaction Amount limit NOT exceeded (STA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - Single Transaction Amount limit exceeded (STA)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: Info for Issuer " + byteBits[5]);
					string cvrByte6Bits8To5 = byteBits[5].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte6Bits8To5 + " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
					string cvrByte6Bits4To1 = byteBits[5].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + cvrByte6Bits4To1 + " - Transaction profile Identifier (0000 by default)");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 7: TTQ info for Issuer " + byteBits[6]);
					string cvrByte7Bit8 = byteBits[6].Substring(0, 1);
					if (cvrByte7Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte7Bit8 + "  - EMV contact chip not supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte7Bit8 + "  - EMV contact chip supported");
					}
					string cvrByte7Bit7 = byteBits[6].Substring(1, 1);
					if (cvrByte7Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte7Bit7 + "  - Online-capable reader");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte7Bit7 + "  - Not an Online-capable reader");
					}
					string cvrByte7Bit6 = byteBits[6].Substring(2, 1);
					if (cvrByte7Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte7Bit6 + "  - Online PIN NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte7Bit6 + "  - Online PIN supported");
					}
					string cvrByte7Bit5 = byteBits[6].Substring(3, 1);
					if (cvrByte7Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte7Bit5 + "  - Signature NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte7Bit5 + "  - Signature supported");
					}
					string cvrByte7Bit4 = byteBits[6].Substring(4, 1);
					if (cvrByte7Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte7Bit4 + "  - Issuer update processing NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte7Bit4 + "  - Issuer update processing supported");
					}
					string cvrByte7Bit3 = byteBits[6].Substring(5, 1);
					if (cvrByte7Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte7Bit3 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte7Bit3 + "  - RFU");
					}
					string cvrByte7Bit2 = byteBits[6].Substring(6, 1);
					if (cvrByte7Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte7Bit2 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte7Bit2 + "  - RFU");
					}
					string cvrByte7Bit1 = byteBits[6].Substring(7, 1);
					if (cvrByte7Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte7Bit1 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte7Bit1 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 8: Info for Issuer related to PID " + byteBits[7]);
					string cvrByte8Bits8To1 = byteBits[7].Substring(0, 8);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte8Bits8To1 + " - Program ID");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Mastercard))
				{
					IUC285Proxy.Log(string.Concat("-----Issuer Application Data(T9F10) ", CardBrand, " -----"));
					int offset = 0;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					int byteIndex = 0;
					string[] hexBytes = new string[6];
					int[] byteValues = new int[6];
					string[] cvrBits = new string[6];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 12).ToUpper());
					while (byteIndex < 6 && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: " + cvrBits[0]);
					string cvrByte1Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte1Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte1Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte1Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte1Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte1Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte1Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte1Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte1Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte1Bit4 = cvrBits[0].Substring(4, 1);
					IUC285Proxy.Log("    bit 4   = " + cvrByte1Bit4 + " - RFU");
					string cvrByte1Bit3 = cvrBits[0].Substring(5, 1);
					if (cvrByte1Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte1Bit3 + " - Offline PIN Verification NOT Performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte1Bit3 + " - Offline PIN Verification Performed");
					}
					string cvrByte1Bit2 = cvrBits[0].Substring(6, 1);
					if (cvrByte1Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte1Bit2 + " - M/Chip Select 4: Offline Encrypted PIN Verification not performed, M/Chip Lite 4: Value not allowed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte1Bit2 + " - M/Chip Select 4: Offline Encrypted PIN Verification performed");
					}
					string cvrByte1Bit1 = cvrBits[0].Substring(7, 1);
					if (cvrByte1Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte1Bit1 + " - Offline PIN Verification failed");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte1Bit1 + " - Offline PIN Verification Successful");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[1]);
					string cvrByte2Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte2Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - DDA NOT Returned");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte2Bit8 + "  - DDA Returned");
					}
					string cvrByte2Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte2Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - CDA NOT returned in 1st GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte2Bit7 + "  - CDA returned in 1st GENERATE AC");
					}
					string cvrByte2Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte2Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - CDA NOT returned in 2nd GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte2Bit6 + "  - CDA returned in 2nd GENERATE AC");
					}
					string cvrByte2Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte2Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - Issuer Authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte2Bit5 + "  - Issuer Authentication performed");
					}
					string cvrByte2Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - CIAC-Default not skipped on CAT3 or not required");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - CIAC-Default skipped on CAT3");
					}
					string cvrByte2Bit3 = cvrBits[1].Substring(5, 1);
					IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - RFU");
					string cvrByte2Bit2 = cvrBits[1].Substring(6, 1);
					IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - RFU");
					string cvrByte2Bit1 = cvrBits[1].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[2]);
					string cvrByte3Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte3Bits8To5 + " - Right nibble of Script Counter");
					string cvrByte3Bits4To1 = cvrBits[2].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + cvrByte3Bits4To1 + " - Right nibble of PIN try Counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[3]);
					string cvrByte4Bit8 = cvrBits[3].Substring(0, 1);
					IUC285Proxy.Log("    bit 8   = " + cvrByte4Bit8 + "  - RFU");
					string cvrByte4Bit7 = cvrBits[3].Substring(1, 1);
					if (cvrByte4Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Unable To Go Online Not Indicated");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte4Bit7 + "  - Unable to go online indicated");
					}
					string cvrByte4Bit6 = cvrBits[3].Substring(2, 1);
					if (cvrByte4Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - Offline PIN Verification Performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte4Bit6 + "  - Offline PIN Verification not Performed");
					}
					string cvrByte4Bit5 = cvrBits[3].Substring(3, 1);
					if (cvrByte4Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - No Failure OF Offline PIN Verification");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte4Bit5 + "  - Offline PIN Verification failed");
					}
					string cvrByte4Bit4 = cvrBits[3].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - PTL Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "  - PTL Exceeded");
					}
					string cvrByte4Bit3 = cvrBits[3].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - Domestic Transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "  - International Transaction");
					}
					string cvrByte4Bit2 = cvrBits[3].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - International Transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "  - Domestic Transaction");
					}
					string cvrByte4Bit1 = cvrBits[3].Substring(7, 1);
					if (cvrByte4Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - Terminal Does Not Erroneously Consider Offline PIN OK");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + "  - Terminal Erroneously Consider Offline PIN OK");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: " + cvrBits[4]);
					string cvrByte5Bit8 = cvrBits[4].Substring(0, 1);
					if (cvrByte5Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - Lower Consecutive Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte5Bit8 + "  - Lower Consecutive Offline Limit Exceeded");
					}
					string cvrByte5Bit7 = cvrBits[4].Substring(1, 1);
					if (cvrByte5Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Upper Consecutive Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte5Bit7 + "  - Upper Consecutive Offline Limit Exceeded");
					}
					string cvrByte5Bit6 = cvrBits[4].Substring(2, 1);
					if (cvrByte5Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - Lower Cumulative Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte5Bit6 + "  - Lower Cumulative Offline Limit Exceeded");
					}
					string cvrByte5Bit5 = cvrBits[4].Substring(3, 1);
					if (cvrByte5Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Upper Cumulative Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte5Bit5 + "  - Upper Cumulative Offline Limit Exceeded");
					}
					string cvrByte5Bit4 = cvrBits[4].Substring(4, 1);
					if (cvrByte5Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Go Online On Next Transaction Was Not Set (in this transaction or in a previous one)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte5Bit4 + "  - Go Online On Next Transaction Was Set (in this transaction or in a previous one)");
					}
					string cvrByte5Bit3 = cvrBits[4].Substring(5, 1);
					if (cvrByte5Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - No Issuer Authentication Failed (in this transaction or in a previous one)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte5Bit3 + "  - Issuer Authentication Failed (in this transaction or in a previous one)");
					}
					string cvrByte5Bit2 = cvrBits[4].Substring(6, 1);
					if (cvrByte5Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - No Script Received (in a previous transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte5Bit2 + "  - Script Received (in a previous transaction)");
					}
					string cvrByte5Bit1 = cvrBits[4].Substring(7, 1);
					if (cvrByte5Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - No Script Failed (in a previous transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte5Bit1 + "  - Script Failed (in a previous transaction)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: " + cvrBits[5]);
					string cvrByte6Bit8 = cvrBits[5].Substring(0, 1);
					IUC285Proxy.Log("    bit 8   = " + cvrByte6Bit8 + " - RFU");
					string cvrByte6Bit7 = cvrBits[5].Substring(1, 1);
					IUC285Proxy.Log("    bit 7   = " + cvrByte6Bit7 + " - RFU");
					string cvrByte6Bit6 = cvrBits[5].Substring(2, 1);
					IUC285Proxy.Log("    bit 6   = " + cvrByte6Bit6 + " - RFU");
					string cvrByte6Bit5 = cvrBits[5].Substring(3, 1);
					IUC285Proxy.Log("    bit 5   = " + cvrByte6Bit5 + " - RFU");
					string cvrByte6Bit4 = cvrBits[5].Substring(4, 1);
					IUC285Proxy.Log("    bit 4   = " + cvrByte6Bit4 + " - RFU");
					string cvrByte6Bit3 = cvrBits[5].Substring(5, 1);
					IUC285Proxy.Log("    bit 3   = " + cvrByte6Bit3 + " - RFU");
					string cvrByte6Bit2 = cvrBits[5].Substring(6, 1);
					if (cvrByte6Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte6Bit2 + " - No Match found in Additional Check Table");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte6Bit2 + " - Match found in Additional Check Table");
					}
					string cvrByte6Bit1 = cvrBits[5].Substring(7, 1);
					if (cvrByte6Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte6Bit1 + " - Match found in Additional Check Table");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte6Bit1 + " - No Match found in Additional Check Table");
					}
					IUC285Proxy.Log("-----");
					string dac = TagDataStr.Substring(offset, 4);
					IUC285Proxy.Log("DAC = " + dac);
					offset += 4;
					string counters = TagDataStr.Substring(offset, 16);
					IUC285Proxy.Log("Counters = " + counters);
					offset += 16;
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.UnionPay))
				{
					IUC285Proxy.Log(string.Concat("------Issuer Application Data(T9F10) ", CardBrand, " ------"));
					int offset = 0;
					string lengthIndicator = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 1 - Length Indicator = " + lengthIndicator);
					offset += 2;
					string derivationKeyIndex = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + derivationKeyIndex);
					offset += 2;
					string cryptogramVersionNumber = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + cryptogramVersionNumber);
					offset += 2;
					string cvrLengthIndicator = TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(offset, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + cvrLengthIndicator);
					offset += 2;
					int cvrLengthIndicatorValue = Convert.ToInt32(cvrLengthIndicator);
					int byteIndex = 0;
					string[] hexBytes = new string[cvrLengthIndicatorValue];
					int[] byteValues = new int[cvrLengthIndicatorValue];
					string[] cvrBits = new string[cvrLengthIndicatorValue];
					while (byteIndex < cvrLengthIndicatorValue && offset + 2 <= TagDataStr.Length)
					{
						hexBytes[byteIndex] = TagDataStr.Substring(offset, 2);
						byteValues[byteIndex] = Convert.ToInt32(hexBytes[byteIndex], 16);
						cvrBits[byteIndex] = Convert.ToString(byteValues[byteIndex], 2).PadLeft(8, '0');
						byteIndex++;
						offset += 2;
					}
					IUC285Proxy.Log("-----");
					if (cvrLengthIndicatorValue == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + cvrBits[0]);
					string cvrByte2Bits8To7 = cvrBits[0].Substring(0, 2);
					switch (cvrByte2Bits8To7)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + cvrByte2Bits8To7 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string cvrByte2Bits6To5 = cvrBits[0].Substring(2, 2);
					if (cvrByte2Bits6To5 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (cvrByte2Bits6To5 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (cvrByte2Bits6To5 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (cvrByte2Bits6To5 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + cvrByte2Bits6To5 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string cvrByte2Bit4 = cvrBits[0].Substring(4, 1);
					if (cvrByte2Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication performed and passed, or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte2Bit4 + "  - Issuer Authentication failed");
					}
					string cvrByte2Bit3 = cvrBits[0].Substring(5, 1);
					if (cvrByte2Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte2Bit3 + "  - Offline PIN verification performed");
					}
					string cvrByte2Bit2 = cvrBits[0].Substring(6, 1);
					if (cvrByte2Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification passed, or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte2Bit2 + "  - Offline PIN verification failed");
					}
					string cvrByte2Bit1 = cvrBits[0].Substring(7, 1);
					if (cvrByte2Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Able to go online, or transaction performed offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte2Bit1 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + cvrBits[1]);
					string cvrByte3Bit8 = cvrBits[1].Substring(0, 1);
					if (cvrByte3Bit8 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + cvrByte3Bit8 + " - Last online transaction not completed");
					}
					string cvrByte3Bit7 = cvrBits[1].Substring(1, 1);
					if (cvrByte3Bit7 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + cvrByte3Bit7 + " - PIN try limit exceeded");
					}
					string cvrByte3Bit6 = cvrBits[1].Substring(2, 1);
					if (cvrByte3Bit6 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + cvrByte3Bit6 + " - Velocity checking counters exceeded");
					}
					string cvrByte3Bit5 = cvrBits[1].Substring(3, 1);
					if (cvrByte3Bit5 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + cvrByte3Bit5 + " - New card");
					}
					string cvrByte3Bit4 = cvrBits[1].Substring(4, 1);
					if (cvrByte3Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte3Bit4 + " - Issuer Authentication failed on last online transaction");
					}
					string cvrByte3Bit3 = cvrBits[1].Substring(5, 1);
					if (cvrByte3Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication performed after online authorization, or transaction was offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte3Bit3 + " - Issuer Authentication was not performed after online authorization");
					}
					string cvrByte3Bit2 = cvrBits[1].Substring(6, 1);
					if (cvrByte3Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte3Bit2 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string cvrByte3Bit1 = cvrBits[1].Substring(7, 1);
					if (cvrByte3Bit1 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication passed on last transaction, or SDA not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + cvrByte3Bit1 + " - Offline static data authentication failed on last transaction");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + cvrBits[2]);
					string cvrByte4Bits8To5 = cvrBits[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + cvrByte4Bits8To5 + " - Number of Issuer Script Commands");
					string cvrByte4Bit4 = cvrBits[2].Substring(4, 1);
					if (cvrByte4Bit4 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing passed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + cvrByte4Bit4 + "    - Issuer Script processing failed");
					}
					string cvrByte4Bit3 = cvrBits[2].Substring(5, 1);
					if (cvrByte4Bit3 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication passed on last transaction, or DDA not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + cvrByte4Bit3 + "    - Offline dynamic data authentication failed on last transaction");
					}
					string cvrByte4Bit2 = cvrBits[2].Substring(6, 1);
					if (cvrByte4Bit2 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + cvrByte4Bit2 + "    - Offline dynamic data authentication performed");
					}
					string cvrByte4Bit1 = cvrBits[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + cvrByte4Bit1 + " -    RFU");
					IUC285Proxy.Log("-----");
					TagDataStr.Substring(offset, 2);
					IUC285Proxy.Log("Byte 8 - Algorithm Indicator = " + cvrLengthIndicator);
					offset += 2;
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else
				{
					IUC285Proxy.Log(string.Concat("-----Issuer Application Data(T9F10) ", CardBrand, " AID-----"));
					IUC285Proxy.Log("Unknown card brand, please include the tag for AID");
					IUC285Proxy.Log("----------------------------------------------------");
				}
			}
			else if (Tag == "82")
			{
				IUC285Proxy.Log("--------Application Interchange Profile(T82)--------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits[0] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[1] + " - SDA supported");
				IUC285Proxy.Log("  " + byte1Bits[2] + " - DDA supported");
				IUC285Proxy.Log("  " + byte1Bits[3] + " - Cardholder verification is supported");
				IUC285Proxy.Log("  " + byte1Bits[4] + " - Terminal risk management is to be performed");
				IUC285Proxy.Log("  " + byte1Bits[5] + " - Issuer authentication is supported");
				IUC285Proxy.Log("  " + byte1Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[7] + " - CDA supported");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				IUC285Proxy.Log("  " + byte2Bits[0] + " - Reserved for EMV contactless spec");
				IUC285Proxy.Log("  " + byte2Bits[1] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[2] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[3] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "8E")
			{
				IUC285Proxy.Log("------------Cardholder Verification(T8E)------------");
				IUC285Proxy.Log("Amount X: " + TagDataByte[0].ToString("X2") + TagDataByte[1].ToString("X2") + TagDataByte[2].ToString("X2") + TagDataByte[3].ToString("X2"));
				IUC285Proxy.Log("Amount Y: " + TagDataByte[4].ToString("X2") + TagDataByte[5].ToString("X2") + TagDataByte[6].ToString("X2") + TagDataByte[7].ToString("X2"));
				string[] cvRuleBits = new string[(TagLength - 8) / 2];
				int ruleIndex = 0;
				int byteIndex = 8;
				while (byteIndex < TagLength)
				{
					cvRuleBits[ruleIndex] = Convert.ToString(TagDataByte[byteIndex], 2).PadLeft(8, '0');
					IUC285Proxy.Log("-----CV Rule " + (ruleIndex + 1) + ": " + TagDataByte[byteIndex].ToString("X2") + TagDataByte[byteIndex + 1].ToString("X2"));
					IUC285Proxy.Log("CV Rule Byte 1: " + cvRuleBits[ruleIndex].Substring(0, 8));
					IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(0, 1) + "      - RFU");
					string cvRuleBit7 = cvRuleBits[ruleIndex].Substring(1, 1);
					if (cvRuleBit7 == "0")
					{
						IUC285Proxy.Log("  " + cvRuleBit7 + "      - Fail cardholder verification if this is unsuccessful");
					}
					else
					{
						IUC285Proxy.Log("  " + cvRuleBit7 + "      - Apply succeeding CV Rule if this is unsuccessful");
					}
					switch (cvRuleBits[ruleIndex].Substring(2, 6))
					{
					case "000000":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Fail CVM Processing");
						break;
					case "000001":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Plaintext PIN verification performed by ICC");
						break;
					case "000010":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Enciphered PIN verified online");
						break;
					case "000011":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Plaintext PIN verification performed by ICC and signature");
						break;
					case "000100":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Enciphered PIN verification performed by ICC");
						break;
					case "000101":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Enciphered PIN verification performed by ICC and signature");
						break;
					case "011110":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Signature");
						break;
					case "011111":
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - No CVM");
						break;
					default:
						IUC285Proxy.Log("  " + cvRuleBits[ruleIndex].Substring(2, 6) + " - Reserved");
						break;
					}
					string text226 = TagDataByte[byteIndex + 1].ToString("X2");
					IUC285Proxy.Log("CV Rule Byte 2: " + text226);
					switch (text226)
					{
					case "00":
						IUC285Proxy.Log("  00     - Always");
						break;
					case "01":
						IUC285Proxy.Log("  01     - If unattended cash");
						break;
					case "02":
						IUC285Proxy.Log("  02     - If not unattended cash and not manual cash and not purchase with cashback");
						break;
					case "03":
						IUC285Proxy.Log("  03     - If terminal supports the CVM");
						break;
					case "04":
						IUC285Proxy.Log("  04     - If manual cash");
						break;
					case "05":
						IUC285Proxy.Log("  05     - If purchase with cashback");
						break;
					case "06":
						IUC285Proxy.Log("  06     - If transaction is in the application currency and is under X value");
						break;
					case "07":
						IUC285Proxy.Log("  07     - If transaction is in the application currency and is over X value");
						break;
					case "08":
						IUC285Proxy.Log("  08     - If transaction is in the application currency and is under Y value");
						break;
					case "09":
						IUC285Proxy.Log("  09     - If transaction is in the application currency and is over Y value");
						break;
					default:
						IUC285Proxy.Log("  " + text226 + " - RFU");
						break;
					}
					byteIndex += 2;
					ruleIndex++;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "95")
			{
				IUC285Proxy.Log("---------Terminal Verification Results(T95)---------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string byte3Bits = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string byte4Bits = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string byte5Bits = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits[0] + " - Offline data authentication was not performed");
				IUC285Proxy.Log("  " + byte1Bits[1] + " - SDA failed");
				IUC285Proxy.Log("  " + byte1Bits[2] + " - ICC data missing");
				IUC285Proxy.Log("  " + byte1Bits[3] + " - Card appears on terminal exception file");
				IUC285Proxy.Log("  " + byte1Bits[4] + " - DDA failed");
				IUC285Proxy.Log("  " + byte1Bits[5] + " - CDA failed");
				IUC285Proxy.Log("  " + byte1Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				IUC285Proxy.Log("  " + byte2Bits[0] + " - ICC and terminal have different application versions");
				IUC285Proxy.Log("  " + byte2Bits[1] + " - Expired application");
				IUC285Proxy.Log("  " + byte2Bits[2] + " - Application not yet effective");
				IUC285Proxy.Log("  " + byte2Bits[3] + " - Requested service not allowed for card product");
				IUC285Proxy.Log("  " + byte2Bits[4] + " - New card");
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 3: " + byte3Bits);
				IUC285Proxy.Log("  " + byte3Bits[0] + " - Cardholder verification was not successful");
				IUC285Proxy.Log("  " + byte3Bits[1] + " - Unrecognised CVM");
				IUC285Proxy.Log("  " + byte3Bits[2] + " - PIN Try Limit exceeded");
				IUC285Proxy.Log("  " + byte3Bits[3] + " - PIN entry required and PIN pad not present or not working");
				IUC285Proxy.Log("  " + byte3Bits[4] + " - PIN entry required, PIN pad present, but PIN was not entered");
				IUC285Proxy.Log("  " + byte3Bits[5] + " - Online PIN entered");
				IUC285Proxy.Log("  " + byte3Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 4: " + byte4Bits);
				IUC285Proxy.Log("  " + byte4Bits[0] + " - Transaction exceeds floor limit");
				IUC285Proxy.Log("  " + byte4Bits[1] + " - Lower consecutive offline limit exceeded");
				IUC285Proxy.Log("  " + byte4Bits[2] + " - Upper consecutive offline limit exceeded");
				IUC285Proxy.Log("  " + byte4Bits[3] + " - Transaction selected randomly for online processing");
				IUC285Proxy.Log("  " + byte4Bits[4] + " - Merchant forced transaction online");
				IUC285Proxy.Log("  " + byte4Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 5: " + byte5Bits);
				IUC285Proxy.Log("  " + byte5Bits[0] + " - Default TDOL used");
				IUC285Proxy.Log("  " + byte5Bits[1] + " - Issuer authentication failed");
				IUC285Proxy.Log("  " + byte5Bits[2] + " - Script processing failed before final GENERATE AC");
				IUC285Proxy.Log("  " + byte5Bits[3] + " - Script processing failed after final GENERATE AC");
				IUC285Proxy.Log("  " + byte5Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F07")
			{
				IUC285Proxy.Log("----------Application Usage Control(T9F07)----------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits[0] + " - Valid for domestic cash transaction");
				IUC285Proxy.Log("  " + byte1Bits[1] + " - Valid for international cash transaction");
				IUC285Proxy.Log("  " + byte1Bits[2] + " - Valid for domestic goods");
				IUC285Proxy.Log("  " + byte1Bits[3] + " - Valid for international goods");
				IUC285Proxy.Log("  " + byte1Bits[4] + " - Valid for domestic services");
				IUC285Proxy.Log("  " + byte1Bits[5] + " - Valid for international services");
				IUC285Proxy.Log("  " + byte1Bits[6] + " - Valid at ATMs");
				IUC285Proxy.Log("  " + byte1Bits[7] + " - Valid at terminals other than ATMs");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				IUC285Proxy.Log("  " + byte2Bits[0] + " - Domestic cashback allowed");
				IUC285Proxy.Log("  " + byte2Bits[1] + " - International cashback allowed");
				IUC285Proxy.Log("  " + byte2Bits[2] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[3] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0D")
			{
				IUC285Proxy.Log("---------Issuer Action Code Default (T9F0D)---------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string byte3Bits = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string byte4Bits = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string byte5Bits = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				if (byte1Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Do not reject if unable to process online and if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Reject if unable to process online and if Offline data authentication was not performed");
				}
				if (byte1Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Do not reject if unable to process online and if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Reject if unable to process online and if Offline static data authentication failed");
				}
				if (byte1Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Do not reject if unable to process online and if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Reject if unable to process online and if ICC data missing");
				}
				if (byte1Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Do not reject if unable to process online and if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Reject if unable to process online and if Card appears on terminal exception file");
				}
				if (byte1Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Do not reject if unable to process online and if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Reject if unable to process online and if Offline dynamic data authentication failed");
				}
				if (byte1Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Do not reject if unable to process online and if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Reject if unable to process online and if Combined DDA/AC Generation failed");
				}
				if (byte1Bits[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Do not reject if unable to process online and if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Reject if unable to process online and if SDA selected");
				}
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				if (byte2Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Do not reject if unable to process online and if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Reject if unable to process online and if ICC and terminal have different application versions");
				}
				if (byte2Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Do not reject if unable to process online and if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Reject if unable to process online and if Expired application");
				}
				if (byte2Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Do not reject if unable to process online and if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Reject if unable to process online and if Application not yet effective");
				}
				if (byte2Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Do not reject if unable to process online and if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Reject if unable to process online and if Requested service not allowed for card product");
				}
				if (byte2Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Do not reject if unable to process online and if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Reject if unable to process online and if New card");
				}
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + byte3Bits);
				if (byte3Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Do not reject if unable to process online and if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Reject if unable to process online and if Cardholder verification was not successful");
				}
				if (byte3Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Do not reject if unable to process online and if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Reject if unable to process online and if Unrecognised CVM");
				}
				if (byte3Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Do not reject if unable to process online and if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Reject if unable to process online and if PIN Try Limit exceeded");
				}
				if (byte3Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Do not reject if unable to process online and if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Reject if unable to process online and if PIN entry required and PIN pad not present or not working");
				}
				if (byte3Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Do not reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (byte3Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Do not reject if unable to process online and if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Reject if unable to process online and if Online PIN entered");
				}
				IUC285Proxy.Log("  " + byte3Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + byte4Bits);
				if (byte4Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Do not reject if unable to process online and if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Reject if unable to process online and if Transaction exceeds floor limit");
				}
				if (byte4Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Do not reject if unable to process online and if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Reject if unable to process online and if Lower consecutive offline limit exceeded");
				}
				if (byte4Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Do not reject if unable to process online and if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Reject if unable to process online and if Upper consecutive offline limit exceeded");
				}
				if (byte4Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Do not reject if unable to process online and if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Reject if unable to process online and if Transaction selected randomly for online processing");
				}
				if (byte4Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Do not reject if unable to process online and if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Reject if unable to process online and if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + byte4Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + byte5Bits);
				if (byte5Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Do not reject if unable to process online and if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Reject if unable to process online and if Default TDOL used");
				}
				if (byte5Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Do not reject if unable to process online and if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Reject if unable to process online and if Issuer authentication was unsuccessful");
				}
				if (byte5Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Do not reject if unable to process online and if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Reject if unable to process online and if Script processing failed before final GENERATE AC");
				}
				if (byte5Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Do not reject if unable to process online and if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Reject if unable to process online and if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + byte5Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0E")
			{
				IUC285Proxy.Log("----------Issuer Action Code Denial(T9F0E)----------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string byte3Bits = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string byte4Bits = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string byte5Bits = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				if (byte1Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Do not decline if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Decline if Offline data authentication was not performed");
				}
				if (byte1Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Do not decline if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Decline if Offline static data authentication failed");
				}
				if (byte1Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Do not decline if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Decline if ICC data missing");
				}
				if (byte1Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Do not decline if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Decline if Card appears on terminal exception file");
				}
				if (byte1Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Do not decline if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Decline if Offline dynamic data authentication failed");
				}
				if (byte1Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Do not decline if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Decline if Combined DDA/AC Generation failed");
				}
				if (byte1Bits[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Do not decline if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Decline if SDA selected");
				}
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				if (byte2Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Do not decline if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Decline if ICC and terminal have different application versions");
				}
				if (byte2Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Do not decline if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Decline if Expired application");
				}
				if (byte2Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Do not decline if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Decline if Application not yet effective");
				}
				if (byte2Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Do not decline if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Decline if Requested service not allowed for card product");
				}
				if (byte2Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Do not decline if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Decline if New card");
				}
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + byte3Bits);
				if (byte3Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Do not decline if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Decline if Cardholder verification was not successful");
				}
				if (byte3Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Do not decline if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Decline if Unrecognised CVM");
				}
				if (byte3Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Do not decline if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Decline if PIN Try Limit exceeded");
				}
				if (byte3Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Do not decline if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Decline if PIN entry required and PIN pad not present or not working");
				}
				if (byte3Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Do not decline if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Decline if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (byte3Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Do not decline if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Decline if Online PIN entered");
				}
				IUC285Proxy.Log("  " + byte3Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + byte4Bits);
				if (byte4Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Do not decline if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Decline if Transaction exceeds floor limit");
				}
				if (byte4Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Do not decline if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Decline if Lower consecutive offline limit exceeded");
				}
				if (byte4Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Do not decline if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Decline if Upper consecutive offline limit exceeded");
				}
				if (byte4Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Do not decline if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Decline if Transaction selected randomly for online processing");
				}
				if (byte4Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Do not decline if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Decline if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + byte4Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + byte5Bits);
				if (byte5Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Do not decline if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Decline if Default TDOL used");
				}
				if (byte5Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Do not decline if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Decline if Issuer authentication was unsuccessful");
				}
				if (byte5Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Do not decline if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Decline if Script processing failed before final GENERATE AC");
				}
				if (byte5Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Do not decline if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Decline if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + byte5Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0F")
			{
				IUC285Proxy.Log("----------Issuer Action Code Online(T9F0F)----------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string byte3Bits = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string byte4Bits = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string byte5Bits = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				if (byte1Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Do not go online if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[0] + " - Go online if Offline data authentication was not performed");
				}
				if (byte1Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Do not go online if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[1] + " - Go online if Offline static data authentication failed");
				}
				if (byte1Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Do not go online if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[2] + " - Go online if ICC data missing");
				}
				if (byte1Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Do not go online if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[3] + " - Go online if Card appears on terminal exception file");
				}
				if (byte1Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Do not go online if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[4] + " - Go online if Offline dynamic data authentication failed");
				}
				if (byte1Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Do not go online if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[5] + " - Go online if Combined DDA/AC Generation failed");
				}
				if (byte1Bits[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Do not go online if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bits[6] + " - Go online if SDA selected");
				}
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				if (byte2Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Do not go online if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[0] + " - Go online if ICC and terminal have different application versions");
				}
				if (byte2Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Do not go online if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[1] + " - Go online if Expired application");
				}
				if (byte2Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Do not go online if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[2] + " - Go online if Application not yet effective");
				}
				if (byte2Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Do not go online if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[3] + " - Go online if Requested service not allowed for card product");
				}
				if (byte2Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Do not go online if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + byte2Bits[4] + " - Go online if New card");
				}
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + byte3Bits);
				if (byte3Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Do not go online if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[0] + " - Go online if Cardholder verification was not successful");
				}
				if (byte3Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Do not go online if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[1] + " - Go online if Unrecognised CVM");
				}
				if (byte3Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Do not go online if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[2] + " - Go online if PIN Try Limit exceeded");
				}
				if (byte3Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Do not go online if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[3] + " - Go online if PIN entry required and PIN pad not present or not working");
				}
				if (byte3Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Do not go online if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[4] + " - Go online if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (byte3Bits[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Do not go online if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + byte3Bits[5] + " - Go online if Online PIN entered");
				}
				IUC285Proxy.Log("  " + byte3Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + byte4Bits);
				if (byte4Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Do not go online if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[0] + " - Go online if Transaction exceeds floor limit");
				}
				if (byte4Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Do not go online if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[1] + " - Go online if Lower consecutive offline limit exceeded");
				}
				if (byte4Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Do not go online if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[2] + " - Go online if Upper consecutive offline limit exceeded");
				}
				if (byte4Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Do not go online if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[3] + " - Go online if Transaction selected randomly for online processing");
				}
				if (byte4Bits[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Do not go online if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + byte4Bits[4] + " - Go online if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + byte4Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte4Bits[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + byte5Bits);
				if (byte5Bits[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Do not go online if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[0] + " - Go online if Default TDOL used");
				}
				if (byte5Bits[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Do not go online if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[1] + " - Go online if Issuer authentication was unsuccessful");
				}
				if (byte5Bits[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Do not go online if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[2] + " - Go online if Script processing failed before final GENERATE AC");
				}
				if (byte5Bits[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Do not go online if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + byte5Bits[3] + " - Go online if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + byte5Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte5Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F27")
			{
				IUC285Proxy.Log("---------Cryptogram information data(T9F27)---------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Binary data: " + byte1Bits);
				if (byte1Bits.Substring(0, 2) == "00")
				{
					IUC285Proxy.Log("  00  - AAC");
					Tag9F27CryptogramDeclined = true;
				}
				if (byte1Bits.Substring(0, 2) == "01")
				{
					IUC285Proxy.Log("  01  - TC");
				}
				if (byte1Bits.Substring(0, 2) == "10")
				{
					IUC285Proxy.Log("  10  - ARQC");
				}
				if (byte1Bits.Substring(0, 2) == "11")
				{
					IUC285Proxy.Log("  11  - RFU");
				}
				IUC285Proxy.Log("  " + byte1Bits.Substring(2, 2) + "  - Payment System-specific cryptogram");
				string byte1Bit4 = byte1Bits.Substring(4, 1);
				if (byte1Bit4 == "0")
				{
					IUC285Proxy.Log("  " + byte1Bit4 + "   - No advice required");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bit4 + "   - Advice required");
				}
				string byte1Bits3To1 = byte1Bits.Substring(5, 3);
				switch (byte1Bits3To1)
				{
				case "000":
					IUC285Proxy.Log("  " + byte1Bits3To1 + " - No information given");
					break;
				case "001":
					IUC285Proxy.Log("  " + byte1Bits3To1 + " - Service not allowed");
					break;
				case "010":
					IUC285Proxy.Log("  " + byte1Bits3To1 + " - PIN try limit exceeded");
					break;
				case "011":
					IUC285Proxy.Log("  " + byte1Bits3To1 + " - Issuer authetication failed");
					break;
				default:
					if (byte1Bits3To1.Substring(0, 1) == "1")
					{
						IUC285Proxy.Log("  " + byte1Bits3To1 + " - Other values RFU");
					}
					else
					{
						IUC285Proxy.Log("  " + byte1Bits3To1 + " - Reason / advice code");
					}
					break;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F33")
			{
				IUC285Proxy.Log("------------Terminal Capabilities(T9F33)------------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string byte2Bits = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string byte3Bits = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits[0] + " - Manual key entry");
				IUC285Proxy.Log("  " + byte1Bits[1] + " - Magnetic stripe");
				IUC285Proxy.Log("  " + byte1Bits[2] + " - IC with contacts");
				IUC285Proxy.Log("  " + byte1Bits[3] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[4] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte1Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + byte2Bits);
				IUC285Proxy.Log("  " + byte2Bits[0] + " - Plaintext PIN for ICC verification");
				IUC285Proxy.Log("  " + byte2Bits[1] + " - Enciphered PIN for online verification");
				IUC285Proxy.Log("  " + byte2Bits[2] + " - Signature (paper)");
				IUC285Proxy.Log("  " + byte2Bits[3] + " - Enciphered PIN for offline verification");
				IUC285Proxy.Log("  " + byte2Bits[4] + " - No CVM required");
				IUC285Proxy.Log("  " + byte2Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte2Bits[7] + " - RFU");
				IUC285Proxy.Log("Byte 3: " + byte3Bits);
				IUC285Proxy.Log("  " + byte3Bits[0] + " - SDA");
				IUC285Proxy.Log("  " + byte3Bits[1] + " - DDA");
				IUC285Proxy.Log("  " + byte3Bits[2] + " - Card capture");
				IUC285Proxy.Log("  " + byte3Bits[3] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[4] + " - CDA");
				IUC285Proxy.Log("  " + byte3Bits[5] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[6] + " - RFU");
				IUC285Proxy.Log("  " + byte3Bits[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F34")
			{
				IUC285Proxy.Log("-----------------CVM Results(T9F34)-----------------");
				string byte1Bits = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text256 = TagDataByte[1].ToString("X2");
				string text257 = TagDataByte[2].ToString("X2");
				IUC285Proxy.Log("Byte 1 - CVM Performed: " + byte1Bits);
				IUC285Proxy.Log("  " + byte1Bits.Substring(0, 1) + "      - RFU");
				string byte1Bit7 = byte1Bits.Substring(1, 1);
				if (byte1Bit7 == "0")
				{
					IUC285Proxy.Log("  " + byte1Bit7 + "      - Fail cardholder verification if this is unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + byte1Bit7 + "      - Apply succeeding CV Rule if this is unsuccessful");
				}
				switch (byte1Bits.Substring(2, 6))
				{
				case "000000":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Fail CVM Processing");
					break;
				case "000001":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Plaintext PIN verification performed by ICC");
					break;
				case "000010":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Enciphered PIN verified online");
					break;
				case "000011":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Plaintext PIN verification performed by ICC and signature");
					break;
				case "000100":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Enciphered PIN verification performed by ICC");
					break;
				case "000101":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Enciphered PIN verification performed by ICC and signature");
					break;
				case "011110":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Signature");
					break;
				case "011111":
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - No CVM");
					break;
				default:
					IUC285Proxy.Log("  " + byte1Bits.Substring(2, 6) + " - Reserved");
					break;
				}
				IUC285Proxy.Log("Byte 2 - CVM Condition: " + text256);
				switch (text256)
				{
				case "00":
					IUC285Proxy.Log("  00     - Always");
					break;
				case "01":
					IUC285Proxy.Log("  01     - If unattended cash");
					break;
				case "02":
					IUC285Proxy.Log("  02     - If not unattended cash and not manual cash and not purchase with cashback");
					break;
				case "03":
					IUC285Proxy.Log("  03     - If terminal supports the CVM");
					break;
				case "04":
					IUC285Proxy.Log("  04     - If manual cash");
					break;
				case "05":
					IUC285Proxy.Log("  05     - If purchase with cashback");
					break;
				case "06":
					IUC285Proxy.Log("  06     - If transaction is in the application currency and is under X value");
					break;
				case "07":
					IUC285Proxy.Log("  07     - If transaction is in the application currency and is over X value");
					break;
				case "08":
					IUC285Proxy.Log("  08     - If transaction is in the application currency and is under Y value");
					break;
				case "09":
					IUC285Proxy.Log("  09     - If transaction is in the application currency and is over Y value");
					break;
				default:
					IUC285Proxy.Log("  " + text256 + " - RFU");
					break;
				}
				IUC285Proxy.Log("Byte 3 - CVM Result: " + text257);
				switch (text257)
				{
				case "00":
					IUC285Proxy.Log("  00     - Unknown");
					break;
				case "01":
					IUC285Proxy.Log("  01     - Failed");
					break;
				case "02":
					IUC285Proxy.Log("  02     - Successful");
					break;
				default:
					IUC285Proxy.Log("  " + text257 + "     - Undefined");
					break;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F6E")
			{
				IUC285Proxy.Log("---------Third Party Data - FFI (Visa) - Device Type (MasterCard) (9F6E)----------");
				if (CardBrand == CardBrandEnum.Mastercard)
				{
					IUC285Proxy.Log("---------Device Type (MasterCard) (9F6E)----------");
					if (TagDataStr.Length > 12)
					{
						string text259 = ConvertHexToString(TagDataStr.Substring(8, 4), errors);
						IUC285Proxy.Log("DeviceType: " + text259);
						if (int.TryParse(text259, out var result) && result > 0 && result != 20)
						{
							IsMobileWallet = true;
							IUC285Proxy.Log("MobileWallet Transaction");
						}
					}
				}
				else
				{
					if (CardBrand != CardBrandEnum.VISA)
					{
						return;
					}
					IUC285Proxy.Log("---------Device Type (VISA) (9F6E)----------");
					if (TagDataStr.Length >= 4)
					{
						IUC285Proxy.Log("DeviceType: " + TagDataStr);
						if ((Convert.ToInt16(TagDataStr.Substring(1, 1), 16) & 2) > 0)
						{
							IsMobileWallet = true;
							IUC285Proxy.Log("MobileWallet Transaction");
						}
					}
				}
			}
			else if (Tag == "1003")
			{
				IUC285Proxy.Log("---------Confirmation response code (1003)----------");
				char c = Convert.ToChar(TagDataByte[0]);
				switch (c)
				{
				case 'A':
					IUC285Proxy.Log("  " + c + " - Approved");
					break;
				case 'D':
					IUC285Proxy.Log("  " + c + " - Declined");
					break;
				case 'C':
					IUC285Proxy.Log("  " + c + " - Completed(refund)");
					break;
				case 'E':
					IUC285Proxy.Log("  " + c + " - Error");
					break;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "1010")
			{
				IUC285Proxy.Log("-------------Error response code(1010)--------------");
				int length = TagDataStr.Length;
				StringBuilder stringBuilder = new StringBuilder();
				string[] hexBytes = new string[length / 2];
				int num21 = 0;
				for (int offset = 0; offset < TagDataStr.Length; offset += 2)
				{
					hexBytes[num21] = TagDataStr.Substring(offset, 2);
					num21++;
				}
				string[] array24 = hexBytes;
				for (int num23 = 0; num23 < array24.Length; num23++)
				{
					int utf = Convert.ToInt32(array24[num23], 16);
					stringBuilder.Append(char.ConvertFromUtf32(utf));
				}
				ErrorCode = stringBuilder.ToString();
				IUC285Proxy.Log("Error response code = " + ErrorCode);
				IUC285Proxy.Log("----------------------------------------------------");
				Fallback = (_fallbackErrorStatusActions.TryGetValue(ErrorCode, out var value) ? value : FallbackStatusAction.NoAction);
				switch (ErrorCode)
				{
				case "CDIV":
					FallbackReason = FallbackType.CardDataInvalid;
					break;
				case "CNSUP":
					FallbackReason = FallbackType.CardNotSupported;
					break;
				}
			}
			else if (Tag == "5F20")
			{
				string cardHolderName = ConvertHexToString(TagDataStr, errors);
				CardHolderName = cardHolderName;
			}
			else if (Tag == "FF1F")
			{
				FF1FOnGuardCardData = ConvertHexToString(TagDataStr, errors);
			}
		}
	}

	public static string ConvertHexToString(string hexString, IList<Error> errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < hexString.Length; i += 2)
		{
			if (int.TryParse(hexString.Substring(i, 2), NumberStyles.HexNumber, null, out var result))
			{
				stringBuilder.Append(Convert.ToChar(result));
				continue;
			}
			errors?.Add(new Error
			{
				Code = "HTS001",
				Message = "Failed to Convert Data to string."
			});
			return null;
		}
		return stringBuilder.ToString();
	}
}
