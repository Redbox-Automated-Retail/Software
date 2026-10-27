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
		string text = "[Pii Data]";
		TagDetails tagDetails = TagConstants.Details.FirstOrDefault((TagDetails x) => x.Tag == Tag);
		IUC285Proxy.Log(string.Format("Tag {0}({1}) Length = {2} TagData = {3}", Tag, tagDetails?.Description ?? "Undefined", TagLength, (tagDetails != null && tagDetails.IsPii) ? text : TagDataStr.ToUpper()));
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
				string text2 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text3 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text2);
				IUC285Proxy.Log("  " + text2[0] + " - Offline data authentication was performed");
				IUC285Proxy.Log("  " + text2[1] + " - Cardholder verification was performed");
				IUC285Proxy.Log("  " + text2[2] + " - Card risk management was performed");
				IUC285Proxy.Log("  " + text2[3] + " - Issuer authentication was performed");
				IUC285Proxy.Log("  " + text2[4] + " - Terminal risk management was performed");
				IUC285Proxy.Log("  " + text2[5] + " - Script processing was performed");
				IUC285Proxy.Log("  " + text2[6] + " - RFU");
				IUC285Proxy.Log("  " + text2[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + text3);
				IUC285Proxy.Log("  " + text3[0] + " - RFU");
				IUC285Proxy.Log("  " + text3[1] + " - RFU");
				IUC285Proxy.Log("  " + text3[2] + " - RFU");
				IUC285Proxy.Log("  " + text3[3] + " - RFU");
				IUC285Proxy.Log("  " + text3[4] + " - RFU");
				IUC285Proxy.Log("  " + text3[5] + " - RFU");
				IUC285Proxy.Log("  " + text3[6] + " - RFU");
				IUC285Proxy.Log("  " + text3[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F10")
			{
				if (CardBrand.Equals(CardBrandEnum.Amex))
				{
					IUC285Proxy.Log(string.Concat("--------Issuer Application Data(T9F10) ", CardBrand, " --------"));
					int num = 0;
					string text4 = TagDataStr.Substring(num, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + text4);
					num += 2;
					string text5 = TagDataStr.Substring(num, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + text5);
					num += 2;
					string text6 = TagDataStr.Substring(num, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + text6);
					num += 2;
					string text7 = TagDataStr.Substring(num, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length = " + text7);
					num += 2;
					int num2 = Convert.ToInt32(text7);
					int num3 = 0;
					string[] array = new string[num2];
					int[] array2 = new int[num2];
					string[] array3 = new string[num2];
					while (num3 < num2 && num + 2 <= TagDataStr.Length)
					{
						array[num3] = TagDataStr.Substring(num, 2);
						array2[num3] = Convert.ToInt32(array[num3], 16);
						array3[num3] = Convert.ToString(array2[num3], 2).PadLeft(8, '0');
						num3++;
						num += 2;
					}
					IUC285Proxy.Log("-----");
					if (num2 == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + array3[0]);
					string text8 = array3[0].Substring(0, 2);
					switch (text8)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text8 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text8 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text8 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text8 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text9 = array3[0].Substring(2, 2);
					if (text9 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text9 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text9 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text9 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text9 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text9 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text9 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text9 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text10 = array3[0].Substring(4, 1);
					if (text10 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text10 + "  - Issuer Authentication did not fail");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text10 + "  - Issuer Authentication failed");
					}
					string text11 = array3[0].Substring(5, 1);
					if (text11 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text11 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text11 + "  - Offline PIN verification performed");
					}
					string text12 = array3[0].Substring(6, 1);
					if (text12 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text12 + "  - Offline PIN verification did not fail");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text12 + "  - Offline PIN verification failed");
					}
					string text13 = array3[0].Substring(7, 1);
					if (text13 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text13 + "  - Not unable to go online");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text13 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array3[1]);
					string text14 = array3[1].Substring(0, 1);
					if (text14 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text14 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text14 + " - Last online transaction not completed");
					}
					string text15 = array3[1].Substring(1, 1);
					if (text15 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text15 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text15 + " - PIN try limit exceeded");
					}
					string text16 = array3[1].Substring(2, 1);
					if (text16 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text16 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text16 + " - Velocity checking counters exceeded");
					}
					string text17 = array3[1].Substring(3, 1);
					if (text17 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text17 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text17 + " - New card");
					}
					string text18 = array3[1].Substring(4, 1);
					if (text18 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text18 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text18 + " - Issuer Authentication failed on last online transaction");
					}
					string text19 = array3[1].Substring(5, 1);
					if (text19 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text19 + " - Issuer Authentication performed after online authorization");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text19 + " - Issuer Authentication not performed after online authorization");
					}
					string text20 = array3[1].Substring(6, 1);
					if (text20 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text20 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text20 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string text21 = array3[1].Substring(7, 1);
					if (text21 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text21 + " - Offline static data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text21 + " - Offline static data authentication failed on last transaction or transaction declined offline");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array3[2]);
					string text22 = array3[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text22 + " - Number of Issuer Script Commands received after the second GENERATE AC command");
					string text23 = array3[2].Substring(4, 1);
					if (text23 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text23 + "    - Issuer Script processing did not fail on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text23 + "    - Issuer Script processing failed on last transaction");
					}
					string text24 = array3[2].Substring(5, 1);
					if (text24 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text24 + "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text24 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string text25 = array3[2].Substring(6, 1);
					if (text25 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text25 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text25 + "    - Offline dynamic data authentication performed");
					}
					string text26 = array3[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + text26 + " -    RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Amex) && generalSourceType == GeneralSourceType.Tap)
				{
					IUC285Proxy.Log(string.Concat("-----Issuer Application Data(T9F10) ", CardBrand, " -----"));
					int num4 = 0;
					string text27 = TagDataStr.Substring(num4, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + text27);
					num4 += 2;
					string text28 = TagDataStr.Substring(num4, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + text28);
					num4 += 2;
					string text29 = TagDataStr.Substring(num4, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + text29);
					num4 += 2;
					string text30 = TagDataStr.Substring(num4, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num4, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + text30);
					num4 += 2;
					int num5 = Convert.ToInt32(text30);
					int num6 = 0;
					string[] array4 = new string[num5];
					int[] array5 = new int[num5];
					string[] array6 = new string[num5];
					while (num6 < num5 && num4 + 2 <= TagDataStr.Length)
					{
						array4[num6] = TagDataStr.Substring(num4, 2);
						array5[num6] = Convert.ToInt32(array4[num6], 16);
						array6[num6] = Convert.ToString(array5[num6], 2).PadLeft(8, '0');
						num6++;
						num4 += 2;
					}
					IUC285Proxy.Log("-----");
					if (num5 == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + array6[0]);
					string text31 = array6[0].Substring(0, 2);
					switch (text31)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text31 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text31 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text31 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text31 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text32 = array6[0].Substring(2, 2);
					if (text32 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text32 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text32 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text32 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text32 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text32 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text32 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text32 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text33 = array6[0].Substring(4, 1);
					if (text33 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text33 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text33 + "  - RFU");
					}
					string text34 = array6[0].Substring(5, 1);
					if (text34 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text34 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text34 + "  - RFU");
					}
					string text35 = array6[0].Substring(6, 1);
					if (text35 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text35 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text35 + "  - RFU");
					}
					string text36 = array6[0].Substring(7, 1);
					if (text36 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text36 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text36 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array6[1]);
					string text37 = array6[1].Substring(0, 1);
					if (text37 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text37 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text37 + " - Last online transaction not completed");
					}
					string text38 = array6[1].Substring(1, 1);
					if (text38 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text38 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text38 + " - PIN try limit exceeded");
					}
					string text39 = array6[1].Substring(2, 1);
					if (text39 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text39 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text39 + " - Velocity checking counters exceeded");
					}
					string text40 = array6[1].Substring(3, 1);
					if (text40 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text40 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text40 + " - New card");
					}
					string text41 = array6[1].Substring(4, 1);
					if (text41 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text41 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text41 + " - Issuer Authentication failed on last online transaction");
					}
					string text42 = array6[1].Substring(5, 1);
					if (text42 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text42 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text42 + " - RFU");
					}
					string text43 = array6[1].Substring(6, 1);
					if (text43 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text43 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text43 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string text44 = array6[1].Substring(7, 1);
					if (text44 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text44 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text44 + " - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array6[2]);
					string text45 = array6[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text45 + " - RFU");
					string text46 = array6[2].Substring(4, 1);
					if (text46 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text46 + "    - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text46 + "    - RFU");
					}
					string text47 = array6[2].Substring(5, 1);
					if (text47 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text47 + "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text47 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string text48 = array6[2].Substring(6, 1);
					if (text48 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text48 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text48 + "    - Offline dynamic data authentication performed");
					}
					string text49 = array6[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + text49 + " -    RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.VISA))
				{
					IUC285Proxy.Log(string.Concat("---------Issuer Application Data (T9F10) ", CardBrand, " ---------"));
					int num7 = 0;
					string text50 = TagDataStr.Substring(num7, 2);
					IUC285Proxy.Log("Byte 1 - Length Byte = " + text50);
					num7 += 2;
					string text51 = TagDataStr.Substring(num7, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + text51);
					num7 += 2;
					string text52 = TagDataStr.Substring(num7, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + text52.ToUpper());
					num7 += 2;
					string text53 = TagDataStr.Substring(num7, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num7, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length = " + text53);
					num7 += 2;
					int num8 = Convert.ToInt32(text53);
					int num9 = 0;
					string[] array7 = new string[num8];
					int[] array8 = new int[num8];
					string[] array9 = new string[num8];
					while (num9 < num8 && num7 + 2 <= TagDataStr.Length)
					{
						array7[num9] = TagDataStr.Substring(num7, 2);
						array8[num9] = Convert.ToInt32(array7[num9], 16);
						array9[num9] = Convert.ToString(array8[num9], 2).PadLeft(8, '0');
						num9++;
						num7 += 2;
					}
					IUC285Proxy.Log("-----");
					if (num8 == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + array9[0]);
					string text54 = array9[0].Substring(0, 2);
					switch (text54)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text54 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text54 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text54 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text54 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text55 = array9[0].Substring(2, 2);
					if (text55 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text55 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text55 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text55 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text55 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text55 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text55 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text55 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text56 = array9[0].Substring(4, 1);
					if (text56 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text56 + "  - Issuer Authentication successful or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text56 + "  - Issuer Authentication failed");
					}
					string text57 = array9[0].Substring(5, 1);
					if (text57 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text57 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text57 + "  - Offline PIN verification performed");
					}
					string text58 = array9[0].Substring(6, 1);
					if (text58 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text58 + "  - Offline PIN verification passed or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text58 + "  - Offline PIN verification failed");
					}
					string text59 = array9[0].Substring(7, 1);
					if (text59 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text59 + "  - Able to go online or offline transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text59 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array9[1]);
					string text60 = array9[1].Substring(0, 1);
					if (text60 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text60 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text60 + " - Last online transaction not completed");
					}
					string text61 = array9[1].Substring(1, 1);
					if (text61 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text61 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text61 + " - PIN try limit exceeded");
					}
					string text62 = array9[1].Substring(2, 1);
					if (text62 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text62 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text62 + " - Velocity checking counters exceeded");
					}
					string text63 = array9[1].Substring(3, 1);
					if (text63 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text63 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text63 + " - New card");
					}
					string text64 = array9[1].Substring(4, 1);
					if (text64 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text64 + " - Issuer Authentication successful on last online transaction or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text64 + " - Issuer Authentication failed on last online transaction");
					}
					string text65 = array9[1].Substring(5, 1);
					if (text65 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text65 + " - Issuer Authentication performed after online authorization or offline transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text65 + " - Issuer Authentication not performed after online authorization or offline transaction");
					}
					string text66 = array9[1].Substring(6, 1);
					if (text66 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text66 + " - Application not blocked by card");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text66 + " - Application blocked by card");
					}
					string text67 = array9[1].Substring(7, 1);
					if (text67 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text67 + " - Offline static data authentication passed or was not performed on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text67 + " - Offline static data authentication failed on last transaction or transaction declined offline");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array9[2]);
					string text68 = array9[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text68 + " - Number of Issuer Script Commands");
					string text69 = array9[2].Substring(4, 1);
					if (text69 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text69 + "    - Issuer Script processing passed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text69 + "    - Issuer Script processing failed");
					}
					string text70 = array9[2].Substring(5, 1);
					if (text70 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text70 + "    - Offline dynamic data authentication passed or was not performed on last transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text70 + "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
					}
					string text71 = array9[2].Substring(6, 1);
					if (text71 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text71 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text71 + "    - Offline dynamic data authentication performed");
					}
					string text72 = array9[2].Substring(7, 1);
					if (text72 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text72 + "    - PIN verification command received for a PIN-Expecting card or card does not expect PIN (i.e. Offline PIN verification not supported)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text72 + "    - PIN verification command NOT received for a PIN-Expecting card");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Discover))
				{
					IUC285Proxy.Log(string.Concat("------Issuer Application Data(T9F10) ", CardBrand, " ------"));
					int num10 = 0;
					string text73 = TagDataStr.Substring(num10, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + text73);
					num10 += 2;
					string text74 = TagDataStr.Substring(num10, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + text74);
					num10 += 2;
					int num11 = 0;
					string[] array10 = new string[6];
					int[] array11 = new int[6];
					string[] array12 = new string[6];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num10, 12).ToUpper());
					while (num11 < 6 && num10 + 2 <= TagDataStr.Length)
					{
						array10[num11] = TagDataStr.Substring(num10, 2);
						array11[num11] = Convert.ToInt32(array10[num11], 16);
						array12[num11] = Convert.ToString(array11[num11], 2).PadLeft(8, '0');
						num11++;
						num10 += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: " + array12[0]);
					string text75 = array12[0].Substring(0, 2);
					switch (text75)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text75 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text75 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text75 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text75 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text76 = array12[0].Substring(2, 2);
					if (text76 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text76 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text76 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text76 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text76 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text76 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text76 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text76 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text77 = array12[0].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + text77 + " - RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: " + array12[1]);
					string text78 = array12[1].Substring(0, 1);
					if (text78 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text78 + "  - DDA NOT Returned");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text78 + "  - DDA Returned");
					}
					string text79 = array12[1].Substring(1, 1);
					if (text79 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text79 + "  - CDA NOT returned in 1st GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text79 + "  - CDA returned in 1st GENERATE AC");
					}
					string text80 = array12[1].Substring(2, 1);
					if (text80 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text80 + "  - CDA NOT returned in 2nd GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text80 + "  - CDA returned in 2nd GENERATE AC");
					}
					string text81 = array12[1].Substring(3, 1);
					if (text81 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text81 + "  - Off-line PIN Verification NOT performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text81 + "  - Off-line PIN Verification performed");
					}
					string text82 = array12[1].Substring(4, 1);
					if (text82 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text82 + "  - Off-line Enciphered PIN Verification NOT performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text82 + "  - Off-line Enciphered PIN Verification performed");
					}
					string text83 = array12[1].Substring(5, 1);
					if (text83 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text83 + "  - Off-line PIN Verification NOT Successful");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text83 + "  - Off-line PIN Verification Successful");
					}
					string text84 = array12[1].Substring(6, 1);
					if (text84 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text84 + "  - CAC-default NOT ignored with CAT3 terminal");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text84 + "  - CAC-default ignored with CAT3 terminal");
					}
					string text85 = array12[1].Substring(7, 1);
					if (text85 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text85 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text85 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array12[2]);
					string text86 = array12[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text86 + " - Right nibble of Script Counter");
					string text87 = array12[2].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + text87 + " - Right nibble of PIN try Counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array12[3]);
					string text88 = array12[3].Substring(0, 1);
					if (text88 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text88 + "  - Go on-line during previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text88 + "  - Go on-line during previous transaction");
					}
					string text89 = array12[3].Substring(1, 1);
					if (text89 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text89 + "  - Issuer Authentication performed during previous online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text89 + "  - Issuer Authentication not performed during previous online transaction");
					}
					string text90 = array12[3].Substring(2, 1);
					if (text90 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text90 + "  - Issuer Authentication NOT failed during previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text90 + "  - Issuer Authentication failed during previous transaction");
					}
					string text91 = array12[3].Substring(3, 1);
					if (text91 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text91 + "  - Script NOT received on previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text91 + "  - Script received on previous transaction");
					}
					string text92 = array12[3].Substring(4, 1);
					if (text92 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text92 + "  - Script NOT failed on previous transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text92 + "  - Script failed on previous transaction");
					}
					string text93 = array12[3].Substring(5, 1);
					if (text93 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text93 + "  - PTH NOT forced on-line");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text93 + "  - PTH forced on-line");
					}
					string text94 = array12[3].Substring(6, 1);
					if (text94 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text94 + "  - PDOL NOT forced on-line (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text94 + "  - PDOL forced on-line (during GPO)");
					}
					string text95 = array12[3].Substring(7, 1);
					if (text95 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text95 + "  - PDOL NOT forced decline (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text95 + "  - PDOL forced decline (during GPO)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: " + array12[4]);
					string text96 = array12[4].Substring(0, 1);
					if (text96 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text96 + "  - Valid PDOL check");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text96 + "  - invalid PDOL check");
					}
					string text97 = array12[4].Substring(1, 1);
					if (text97 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text97 + "  - Off-line PIN verification NOT failed");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text97 + "  - Off-line PIN verification failed");
					}
					string text98 = array12[4].Substring(2, 1);
					if (text98 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text98 + "  - PIN Try Limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text98 + "  - PIN Try Limit exceeded");
					}
					string text99 = array12[4].Substring(3, 1);
					if (text99 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text99 + "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text99 + "  - Lower Consecutive Offline Transaction limit exceeded (LCOL). Note: May be profile-specific");
					}
					string text100 = array12[4].Substring(4, 1);
					if (text100 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text100 + "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text100 + "  - Upper Consecutive Offline Transaction limit exceeded (UCOL). Note: May be profile-specific");
					}
					string text101 = array12[4].Substring(5, 1);
					if (text101 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text101 + "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA).Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text101 + "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA).Note: May be profile-specific");
					}
					string text102 = array12[4].Substring(6, 1);
					if (text102 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text102 + "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text102 + "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA). Note: May be profile-specific");
					}
					string text103 = array12[4].Substring(7, 1);
					if (text103 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text103 + "  - Single Transaction Amount limit NOT exceeded (STA). Note: May be profile-specific");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text103 + "  - Single Transaction Amount limit exceeded (STA). Note: May be profile-specific");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: " + array12[5]);
					string text104 = array12[5].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text104 + " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
					string text105 = array12[5].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + text105 + " - Transaction profile Identifier (0000 by default)");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Discover) && generalSourceType == GeneralSourceType.Tap)
				{
					IUC285Proxy.Log(string.Concat("-------Issuer Application Data(T9F10) ", CardBrand, " -------"));
					int num12 = 0;
					string text106 = TagDataStr.Substring(num12, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + text106);
					num12 += 2;
					string text107 = TagDataStr.Substring(num12, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + text107);
					num12 += 2;
					int num13 = 0;
					string[] array13 = new string[8];
					int[] array14 = new int[8];
					string[] array15 = new string[8];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num12, 12).ToUpper());
					while (num13 < 8 && num12 + 2 <= TagDataStr.Length)
					{
						array13[num13] = TagDataStr.Substring(num12, 2);
						array14[num13] = Convert.ToInt32(array13[num13], 16);
						array15[num13] = Convert.ToString(array14[num13], 2).PadLeft(8, '0');
						num13++;
						num12 += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: Card decision " + array15[0]);
					string text108 = array15[0].Substring(0, 1);
					if (text108 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text108 + "  - Online PIN NOT Required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text108 + "  - Online PIN Required");
					}
					string text109 = array15[0].Substring(1, 1);
					if (text109 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text109 + "  - Signature NOT Required");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text109 + "  - Signature Required");
					}
					string text110 = array15[0].Substring(2, 2);
					switch (text110)
					{
					case "00":
						IUC285Proxy.Log("    bit 6-5 = " + text110 + " - AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 6-5 = " + text110 + " - TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 6-5 = " + text110 + " - ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 6-5 = " + text110 + " - RFU");
						break;
					}
					string text111 = array15[0].Substring(4, 1);
					if (text111 == "0")
					{
						IUC285Proxy.Log("    bit 4 = " + text111 + " - PID limit NOT reached");
					}
					else
					{
						IUC285Proxy.Log("    bit 4 = " + text111 + " - PID limit reached");
					}
					string text112 = array15[0].Substring(3, 3);
					IUC285Proxy.Log("    bit 3-1 = " + text112 + " - Script counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: Compared with CAC-CRM and CAC-CVM " + array15[1]);
					string text113 = array15[1].Substring(0, 1);
					if (text113 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text113 + "  - Online cryptogram NOT required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text113 + "  - Online cryptogram required");
					}
					string text114 = array15[1].Substring(1, 1);
					if (text114 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text114 + "  - Transaction type NOT required to be processed online with online PIN CVM(no cashback)");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text114 + "  - Transaction type required to be processed online with online PIN CVM(cashback)");
					}
					string text115 = array15[1].Substring(2, 1);
					if (text115 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text115 + "  - Transaction type NOT required to be processed offline without any CVM(no refund type transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text115 + "  - Transaction type required to be processed offline without any CVM(refund type transaction)");
					}
					string text116 = array15[1].Substring(3, 1);
					if (text116 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text116 + "  - NOT a domestic transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text116 + "  - Domestic transaction");
					}
					string text117 = array15[1].Substring(4, 1);
					if (text117 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text117 + "  - NOT an International transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text117 + "  - International transaction");
					}
					string text118 = array15[1].Substring(5, 1);
					if (text118 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text118 + "  - PIN Try Limit NOT exceeded (Dual-Interface implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text118 + "  - PIN Try Limit exceeded (Dual-Interface implementation only)");
					}
					string text119 = array15[1].Substring(6, 1);
					if (text119 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text119 + "  - Confirmation Code Verification performed (Mobile implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text119 + "  - Confirmation Code Verification NOT performed (Mobile implementation only)");
					}
					string text120 = array15[1].Substring(7, 1);
					if (text120 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text120 + "  - Confirmation Code Verification passed (Mobile implementation only)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text120 + "  - Confirmation Code Verification NOT performed & failed (Mobile implementation only");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: CVM related actions " + array15[2]);
					string text121 = array15[2].Substring(0, 1);
					if (text121 == "0")
					{
						IUC285Proxy.Log("    bit 8 = " + text121 + " - CVM NOT required");
					}
					else
					{
						IUC285Proxy.Log("    bit 8 = " + text121 + " - CVM required");
					}
					string text122 = array15[2].Substring(1, 1);
					if (text122 == "0")
					{
						IUC285Proxy.Log("    bit 7 = " + text122 + " - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 7 = " + text122 + " - RFU");
					}
					string text123 = array15[2].Substring(2, 1);
					if (text123 == "0")
					{
						IUC285Proxy.Log("    bit 6 = " + text123 + " - Consecutive CVM Transaction limit 1 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6 = " + text123 + " - Consecutive CVM Transaction limit 1 exceeded (CVM-Cons 1)");
					}
					string text124 = array15[2].Substring(3, 1);
					if (text124 == "0")
					{
						IUC285Proxy.Log("    bit 5 = " + text124 + " - Consecutive CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 5 = " + text124 + " - Consecutive CVM Transaction limit 2 exceeded (CVM-Cons 1)");
					}
					string text125 = array15[2].Substring(4, 1);
					if (text125 == "0")
					{
						IUC285Proxy.Log("    bit 4 = " + text125 + " - Cumulative CVM Transaction amount limit 1 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4 = " + text125 + " - Cumulative CVM Transaction amount limit 1 exceeded (CVM-Cons 1)");
					}
					string text126 = array15[2].Substring(5, 1);
					if (text126 == "0")
					{
						IUC285Proxy.Log("    bit 3 = " + text126 + " - Cumulative CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3 = " + text126 + " - Cumulative CVM Transaction limit 2 exceeded (CVM-Cons 1)");
					}
					string text127 = array15[2].Substring(6, 1);
					if (text127 == "0")
					{
						IUC285Proxy.Log("    bit 2 = " + text127 + " - CVM Single Transaction Amount limit 1 NOT exceeded (CVM-STA 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2 = " + text127 + " - CVM Single Transaction Amount limit 1 exceeded (CVM-STA 1)");
					}
					string text128 = array15[2].Substring(7, 1);
					if (text128 == "0")
					{
						IUC285Proxy.Log("    bit 1 = " + text128 + " - CVM Single Transaction Amount limit 2 NOT exceeded (CVM-STA 1)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1 = " + text128 + " - CVM Single Transaction Amount limit 2 exceeded (CVM-STA 1)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: CRM related actions " + array15[3]);
					string text129 = array15[3].Substring(0, 1);
					if (text129 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text129 + "  - CDA did NOT fail during previous contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text129 + "  - CDA failed during previous contactless transaction");
					}
					string text130 = array15[3].Substring(1, 1);
					if (text130 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text130 + "  - Last contactless transaction was completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text130 + "  - Last contactless transaction was not completed");
					}
					string text131 = array15[3].Substring(2, 1);
					if (text131 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text131 + "  - ‘Go on-line next transaction’ was NOT set by contact or contactless application");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text131 + "  - ‘Go on-line next transaction’ was set by contact or contactless application");
					}
					string text132 = array15[3].Substring(3, 1);
					if (text132 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text132 + "  - Issuer Authentication did NOT fail during previous contact or contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text132 + "  - Issuer Authentication did fail during previous contact or contactless transaction");
					}
					string text133 = array15[3].Substring(4, 1);
					if (text133 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text133 + "  - Script did NOT fail on previous contact or contactless transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text133 + "  - Script did fail on previous contact or contactless transaction");
					}
					string text134 = array15[3].Substring(5, 1);
					if (text134 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text134 + "  - NO Invalid PDOL check");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text134 + "  - Invalid PDOL check");
					}
					string text135 = array15[3].Substring(6, 1);
					if (text135 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text135 + "  - PDOL did NOT force online (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text135 + "  - PDOL did force online (during GPO)");
					}
					string text136 = array15[3].Substring(7, 1);
					if (text136 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text136 + "  - PDOL did NOT force decline (during GPO)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text136 + "  - PDOL did force decline (during GPO)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: CRM related actions " + array15[4]);
					string text137 = array15[4].Substring(0, 1);
					if (text137 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text137 + "  - Consecutive Contactless Transaction limit NOT exceeded (CL-Cons)");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text137 + "  - Consecutive Contactless Transaction limit exceeded (CL-Cons)");
					}
					string text138 = array15[4].Substring(1, 1);
					if (text138 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text138 + "  - Cumulative Contactless Transaction limit NOT exceeded (CL-Cumul)");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text138 + "  - Cumulative Contactless Transaction limit exceeded (CL-Cumul)");
					}
					string text139 = array15[4].Substring(2, 1);
					if (text139 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text139 + "  - Single Contactless Transaction Amount limit NOT exceeded (CL-STA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text139 + "  - Single Contactless Transaction Amount limit exceeded (CL-STA)");
					}
					string text140 = array15[4].Substring(3, 1);
					if (text140 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text140 + "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL)");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text140 + "  - Lower Consecutive Offline Transaction limit exceeded (LCOL)");
					}
					string text141 = array15[4].Substring(4, 1);
					if (text141 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text141 + "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text141 + "  - Upper Consecutive Offline Transaction limit exceeded (UCOL)");
					}
					string text142 = array15[4].Substring(5, 1);
					if (text142 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text142 + "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text142 + "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA)");
					}
					string text143 = array15[4].Substring(6, 1);
					if (text143 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text143 + "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text143 + "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA)");
					}
					string text144 = array15[4].Substring(7, 1);
					if (text144 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text144 + "  - Single Transaction Amount limit NOT exceeded (STA)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text144 + "  - Single Transaction Amount limit exceeded (STA)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: Info for Issuer " + array15[5]);
					string text145 = array15[5].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text145 + " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
					string text146 = array15[5].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + text146 + " - Transaction profile Identifier (0000 by default)");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 7: TTQ info for Issuer " + array15[6]);
					string text147 = array15[6].Substring(0, 1);
					if (text147 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text147 + "  - EMV contact chip not supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text147 + "  - EMV contact chip supported");
					}
					string text148 = array15[6].Substring(1, 1);
					if (text148 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text148 + "  - Online-capable reader");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text148 + "  - Not an Online-capable reader");
					}
					string text149 = array15[6].Substring(2, 1);
					if (text149 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text149 + "  - Online PIN NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text149 + "  - Online PIN supported");
					}
					string text150 = array15[6].Substring(3, 1);
					if (text150 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text150 + "  - Signature NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text150 + "  - Signature supported");
					}
					string text151 = array15[6].Substring(4, 1);
					if (text151 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text151 + "  - Issuer update processing NOT supported");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text151 + "  - Issuer update processing supported");
					}
					string text152 = array15[6].Substring(5, 1);
					if (text152 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text152 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text152 + "  - RFU");
					}
					string text153 = array15[6].Substring(6, 1);
					if (text153 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text153 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text153 + "  - RFU");
					}
					string text154 = array15[6].Substring(7, 1);
					if (text154 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text154 + "  - RFU");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text154 + "  - RFU");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 8: Info for Issuer related to PID " + array15[7]);
					string text155 = array15[7].Substring(0, 8);
					IUC285Proxy.Log("    bit 8-5 = " + text155 + " - Program ID");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.Mastercard))
				{
					IUC285Proxy.Log(string.Concat("-----Issuer Application Data(T9F10) ", CardBrand, " -----"));
					int num14 = 0;
					string text156 = TagDataStr.Substring(num14, 2);
					IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + text156);
					num14 += 2;
					string text157 = TagDataStr.Substring(num14, 2);
					IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " + text157);
					num14 += 2;
					int num15 = 0;
					string[] array16 = new string[6];
					int[] array17 = new int[6];
					string[] array18 = new string[6];
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num14, 12).ToUpper());
					while (num15 < 6 && num14 + 2 <= TagDataStr.Length)
					{
						array16[num15] = TagDataStr.Substring(num14, 2);
						array17[num15] = Convert.ToInt32(array16[num15], 16);
						array18[num15] = Convert.ToString(array17[num15], 2).PadLeft(8, '0');
						num15++;
						num14 += 2;
					}
					IUC285Proxy.Log("CVR Byte 1: " + array18[0]);
					string text158 = array18[0].Substring(0, 2);
					switch (text158)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text158 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text158 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text158 + " - AC returned in 2nd GENERATE AC: ARQC");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text158 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text159 = array18[0].Substring(2, 2);
					if (text159 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text159 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text159 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text159 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text159 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text159 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text159 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text159 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text160 = array18[0].Substring(4, 1);
					IUC285Proxy.Log("    bit 4   = " + text160 + " - RFU");
					string text161 = array18[0].Substring(5, 1);
					if (text161 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text161 + " - Offline PIN Verification NOT Performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text161 + " - Offline PIN Verification Performed");
					}
					string text162 = array18[0].Substring(6, 1);
					if (text162 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text162 + " - M/Chip Select 4: Offline Encrypted PIN Verification not performed, M/Chip Lite 4: Value not allowed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text162 + " - M/Chip Select 4: Offline Encrypted PIN Verification performed");
					}
					string text163 = array18[0].Substring(7, 1);
					if (text163 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text163 + " - Offline PIN Verification failed");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text163 + " - Offline PIN Verification Successful");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 2: " + array18[1]);
					string text164 = array18[1].Substring(0, 1);
					if (text164 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text164 + "  - DDA NOT Returned");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text164 + "  - DDA Returned");
					}
					string text165 = array18[1].Substring(1, 1);
					if (text165 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text165 + "  - CDA NOT returned in 1st GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text165 + "  - CDA returned in 1st GENERATE AC");
					}
					string text166 = array18[1].Substring(2, 1);
					if (text166 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text166 + "  - CDA NOT returned in 2nd GENERATE AC");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text166 + "  - CDA returned in 2nd GENERATE AC");
					}
					string text167 = array18[1].Substring(3, 1);
					if (text167 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text167 + "  - Issuer Authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text167 + "  - Issuer Authentication performed");
					}
					string text168 = array18[1].Substring(4, 1);
					if (text168 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text168 + "  - CIAC-Default not skipped on CAT3 or not required");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text168 + "  - CIAC-Default skipped on CAT3");
					}
					string text169 = array18[1].Substring(5, 1);
					IUC285Proxy.Log("    bit 3   = " + text169 + "  - RFU");
					string text170 = array18[1].Substring(6, 1);
					IUC285Proxy.Log("    bit 2   = " + text170 + "  - RFU");
					string text171 = array18[1].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + text171 + "  - RFU");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array18[2]);
					string text172 = array18[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text172 + " - Right nibble of Script Counter");
					string text173 = array18[2].Substring(4, 4);
					IUC285Proxy.Log("    bit 4-1 = " + text173 + " - Right nibble of PIN try Counter");
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array18[3]);
					string text174 = array18[3].Substring(0, 1);
					IUC285Proxy.Log("    bit 8   = " + text174 + "  - RFU");
					string text175 = array18[3].Substring(1, 1);
					if (text175 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text175 + "  - Unable To Go Online Not Indicated");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text175 + "  - Unable to go online indicated");
					}
					string text176 = array18[3].Substring(2, 1);
					if (text176 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text176 + "  - Offline PIN Verification Performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text176 + "  - Offline PIN Verification not Performed");
					}
					string text177 = array18[3].Substring(3, 1);
					if (text177 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text177 + "  - No Failure OF Offline PIN Verification");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text177 + "  - Offline PIN Verification failed");
					}
					string text178 = array18[3].Substring(4, 1);
					if (text178 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text178 + "  - PTL Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text178 + "  - PTL Exceeded");
					}
					string text179 = array18[3].Substring(5, 1);
					if (text179 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text179 + "  - Domestic Transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text179 + "  - International Transaction");
					}
					string text180 = array18[3].Substring(6, 1);
					if (text180 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text180 + "  - International Transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text180 + "  - Domestic Transaction");
					}
					string text181 = array18[3].Substring(7, 1);
					if (text181 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text181 + "  - Terminal Does Not Erroneously Consider Offline PIN OK");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text181 + "  - Terminal Erroneously Consider Offline PIN OK");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 5: " + array18[4]);
					string text182 = array18[4].Substring(0, 1);
					if (text182 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text182 + "  - Lower Consecutive Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text182 + "  - Lower Consecutive Offline Limit Exceeded");
					}
					string text183 = array18[4].Substring(1, 1);
					if (text183 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text183 + "  - Upper Consecutive Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text183 + "  - Upper Consecutive Offline Limit Exceeded");
					}
					string text184 = array18[4].Substring(2, 1);
					if (text184 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text184 + "  - Lower Cumulative Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text184 + "  - Lower Cumulative Offline Limit Exceeded");
					}
					string text185 = array18[4].Substring(3, 1);
					if (text185 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text185 + "  - Upper Cumulative Offline Limit Not Exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text185 + "  - Upper Cumulative Offline Limit Exceeded");
					}
					string text186 = array18[4].Substring(4, 1);
					if (text186 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text186 + "  - Go Online On Next Transaction Was Not Set (in this transaction or in a previous one)");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text186 + "  - Go Online On Next Transaction Was Set (in this transaction or in a previous one)");
					}
					string text187 = array18[4].Substring(5, 1);
					if (text187 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text187 + "  - No Issuer Authentication Failed (in this transaction or in a previous one)");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text187 + "  - Issuer Authentication Failed (in this transaction or in a previous one)");
					}
					string text188 = array18[4].Substring(6, 1);
					if (text188 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text188 + "  - No Script Received (in a previous transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text188 + "  - Script Received (in a previous transaction)");
					}
					string text189 = array18[4].Substring(7, 1);
					if (text189 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text189 + "  - No Script Failed (in a previous transaction)");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text189 + "  - Script Failed (in a previous transaction)");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 6: " + array18[5]);
					string text190 = array18[5].Substring(0, 1);
					IUC285Proxy.Log("    bit 8   = " + text190 + " - RFU");
					string text191 = array18[5].Substring(1, 1);
					IUC285Proxy.Log("    bit 7   = " + text191 + " - RFU");
					string text192 = array18[5].Substring(2, 1);
					IUC285Proxy.Log("    bit 6   = " + text192 + " - RFU");
					string text193 = array18[5].Substring(3, 1);
					IUC285Proxy.Log("    bit 5   = " + text193 + " - RFU");
					string text194 = array18[5].Substring(4, 1);
					IUC285Proxy.Log("    bit 4   = " + text194 + " - RFU");
					string text195 = array18[5].Substring(5, 1);
					IUC285Proxy.Log("    bit 3   = " + text195 + " - RFU");
					string text196 = array18[5].Substring(6, 1);
					if (text196 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text196 + " - No Match found in Additional Check Table");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text196 + " - Match found in Additional Check Table");
					}
					string text197 = array18[5].Substring(7, 1);
					if (text197 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text197 + " - Match found in Additional Check Table");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text197 + " - No Match found in Additional Check Table");
					}
					IUC285Proxy.Log("-----");
					string text198 = TagDataStr.Substring(num14, 4);
					IUC285Proxy.Log("DAC = " + text198);
					num14 += 4;
					string text199 = TagDataStr.Substring(num14, 16);
					IUC285Proxy.Log("Counters = " + text199);
					num14 += 16;
					IUC285Proxy.Log("----------------------------------------------------");
				}
				else if (CardBrand.Equals(CardBrandEnum.UnionPay))
				{
					IUC285Proxy.Log(string.Concat("------Issuer Application Data(T9F10) ", CardBrand, " ------"));
					int num16 = 0;
					string text200 = TagDataStr.Substring(num16, 2);
					IUC285Proxy.Log("Byte 1 - Length Indicator = " + text200);
					num16 += 2;
					string text201 = TagDataStr.Substring(num16, 2);
					IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + text201);
					num16 += 2;
					string text202 = TagDataStr.Substring(num16, 2);
					IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + text202);
					num16 += 2;
					string text203 = TagDataStr.Substring(num16, 2);
					IUC285Proxy.Log("CVR: " + TagDataStr.Substring(num16, 8).ToUpper());
					IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + text203);
					num16 += 2;
					int num17 = Convert.ToInt32(text203);
					int num18 = 0;
					string[] array19 = new string[num17];
					int[] array20 = new int[num17];
					string[] array21 = new string[num17];
					while (num18 < num17 && num16 + 2 <= TagDataStr.Length)
					{
						array19[num18] = TagDataStr.Substring(num16, 2);
						array20[num18] = Convert.ToInt32(array19[num18], 16);
						array21[num18] = Convert.ToString(array20[num18], 2).PadLeft(8, '0');
						num18++;
						num16 += 2;
					}
					IUC285Proxy.Log("-----");
					if (num17 == 0)
					{
						IUC285Proxy.Log("No Additional CVR bits");
						return;
					}
					IUC285Proxy.Log("CVR Byte 2: " + array21[0]);
					string text204 = array21[0].Substring(0, 2);
					switch (text204)
					{
					case "00":
						IUC285Proxy.Log("    bit 8-7 = " + text204 + " - AC returned in 2nd GENERATE AC: AAC");
						break;
					case "01":
						IUC285Proxy.Log("    bit 8-7 = " + text204 + " - AC returned in 2nd GENERATE AC: TC");
						break;
					case "10":
						IUC285Proxy.Log("    bit 8-7 = " + text204 + " - AC returned in 2nd GENERATE AC: Not requested");
						break;
					case "11":
						IUC285Proxy.Log("    bit 8-7 = " + text204 + " - AC returned in 2nd GENERATE AC: RFU");
						break;
					}
					string text205 = array21[0].Substring(2, 2);
					if (text205 == "00")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text205 + " - AC returned in 1st GENERATE AC: AAC");
					}
					if (text205 == "01")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text205 + " - AC returned in 1st GENERATE AC: TC");
					}
					if (text205 == "10")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text205 + " - AC returned in 1st GENERATE AC: ARQC");
					}
					if (text205 == "11")
					{
						IUC285Proxy.Log("    bit 6-5 = " + text205 + " - AC returned in 1st GENERATE AC: RFU");
					}
					string text206 = array21[0].Substring(4, 1);
					if (text206 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text206 + "  - Issuer Authentication performed and passed, or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text206 + "  - Issuer Authentication failed");
					}
					string text207 = array21[0].Substring(5, 1);
					if (text207 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text207 + "  - Offline PIN verification not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text207 + "  - Offline PIN verification performed");
					}
					string text208 = array21[0].Substring(6, 1);
					if (text208 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text208 + "  - Offline PIN verification passed, or not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text208 + "  - Offline PIN verification failed");
					}
					string text209 = array21[0].Substring(7, 1);
					if (text209 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text209 + "  - Able to go online, or transaction performed offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text209 + "  - Unable to go online");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 3: " + array21[1]);
					string text210 = array21[1].Substring(0, 1);
					if (text210 == "0")
					{
						IUC285Proxy.Log("    bit 8   = " + text210 + " - Last online transaction completed");
					}
					else
					{
						IUC285Proxy.Log("    bit 8   = " + text210 + " - Last online transaction not completed");
					}
					string text211 = array21[1].Substring(1, 1);
					if (text211 == "0")
					{
						IUC285Proxy.Log("    bit 7   = " + text211 + " - PIN try limit not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 7   = " + text211 + " - PIN try limit exceeded");
					}
					string text212 = array21[1].Substring(2, 1);
					if (text212 == "0")
					{
						IUC285Proxy.Log("    bit 6   = " + text212 + " - Velocity checking counters not exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 6   = " + text212 + " - Velocity checking counters exceeded");
					}
					string text213 = array21[1].Substring(3, 1);
					if (text213 == "0")
					{
						IUC285Proxy.Log("    bit 5   = " + text213 + " - Not a new card");
					}
					else
					{
						IUC285Proxy.Log("    bit 5   = " + text213 + " - New card");
					}
					string text214 = array21[1].Substring(4, 1);
					if (text214 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text214 + " - Issuer Authentication did not fail on last online transaction");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text214 + " - Issuer Authentication failed on last online transaction");
					}
					string text215 = array21[1].Substring(5, 1);
					if (text215 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text215 + " - Issuer Authentication performed after online authorization, or transaction was offline");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text215 + " - Issuer Authentication was not performed after online authorization");
					}
					string text216 = array21[1].Substring(6, 1);
					if (text216 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text216 + " - Application not blocked by card because PIN Try Limit exceeded");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text216 + " - Application blocked by card because PIN Try Limit exceeded");
					}
					string text217 = array21[1].Substring(7, 1);
					if (text217 == "0")
					{
						IUC285Proxy.Log("    bit 1   = " + text217 + " - Offline static data authentication passed on last transaction, or SDA not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 1   = " + text217 + " - Offline static data authentication failed on last transaction");
					}
					IUC285Proxy.Log("-----");
					IUC285Proxy.Log("CVR Byte 4: " + array21[2]);
					string text218 = array21[2].Substring(0, 4);
					IUC285Proxy.Log("    bit 8-5 = " + text218 + " - Number of Issuer Script Commands");
					string text219 = array21[2].Substring(4, 1);
					if (text219 == "0")
					{
						IUC285Proxy.Log("    bit 4   = " + text219 + "    - Issuer Script processing passed");
					}
					else
					{
						IUC285Proxy.Log("    bit 4   = " + text219 + "    - Issuer Script processing failed");
					}
					string text220 = array21[2].Substring(5, 1);
					if (text220 == "0")
					{
						IUC285Proxy.Log("    bit 3   = " + text220 + "    - Offline dynamic data authentication passed on last transaction, or DDA not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 3   = " + text220 + "    - Offline dynamic data authentication failed on last transaction");
					}
					string text221 = array21[2].Substring(6, 1);
					if (text221 == "0")
					{
						IUC285Proxy.Log("    bit 2   = " + text221 + "    - Offline dynamic data authentication not performed");
					}
					else
					{
						IUC285Proxy.Log("    bit 2   = " + text221 + "    - Offline dynamic data authentication performed");
					}
					string text222 = array21[2].Substring(7, 1);
					IUC285Proxy.Log("    bit 1   = " + text222 + " -    RFU");
					IUC285Proxy.Log("-----");
					TagDataStr.Substring(num16, 2);
					IUC285Proxy.Log("Byte 8 - Algorithm Indicator = " + text200);
					num16 += 2;
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
				string text223 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text224 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text223);
				IUC285Proxy.Log("  " + text223[0] + " - RFU");
				IUC285Proxy.Log("  " + text223[1] + " - SDA supported");
				IUC285Proxy.Log("  " + text223[2] + " - DDA supported");
				IUC285Proxy.Log("  " + text223[3] + " - Cardholder verification is supported");
				IUC285Proxy.Log("  " + text223[4] + " - Terminal risk management is to be performed");
				IUC285Proxy.Log("  " + text223[5] + " - Issuer authentication is supported");
				IUC285Proxy.Log("  " + text223[6] + " - RFU");
				IUC285Proxy.Log("  " + text223[7] + " - CDA supported");
				IUC285Proxy.Log("Byte 2: " + text224);
				IUC285Proxy.Log("  " + text224[0] + " - Reserved for EMV contactless spec");
				IUC285Proxy.Log("  " + text224[1] + " - RFU");
				IUC285Proxy.Log("  " + text224[2] + " - RFU");
				IUC285Proxy.Log("  " + text224[3] + " - RFU");
				IUC285Proxy.Log("  " + text224[4] + " - RFU");
				IUC285Proxy.Log("  " + text224[5] + " - RFU");
				IUC285Proxy.Log("  " + text224[6] + " - RFU");
				IUC285Proxy.Log("  " + text224[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "8E")
			{
				IUC285Proxy.Log("------------Cardholder Verification(T8E)------------");
				IUC285Proxy.Log("Amount X: " + TagDataByte[0].ToString("X2") + TagDataByte[1].ToString("X2") + TagDataByte[2].ToString("X2") + TagDataByte[3].ToString("X2"));
				IUC285Proxy.Log("Amount Y: " + TagDataByte[4].ToString("X2") + TagDataByte[5].ToString("X2") + TagDataByte[6].ToString("X2") + TagDataByte[7].ToString("X2"));
				string[] array22 = new string[(TagLength - 8) / 2];
				int num19 = 0;
				int num20 = 8;
				while (num20 < TagLength)
				{
					array22[num19] = Convert.ToString(TagDataByte[num20], 2).PadLeft(8, '0');
					IUC285Proxy.Log("-----CV Rule " + (num19 + 1) + ": " + TagDataByte[num20].ToString("X2") + TagDataByte[num20 + 1].ToString("X2"));
					IUC285Proxy.Log("CV Rule Byte 1: " + array22[num19].Substring(0, 8));
					IUC285Proxy.Log("  " + array22[num19].Substring(0, 1) + "      - RFU");
					string text225 = array22[num19].Substring(1, 1);
					if (text225 == "0")
					{
						IUC285Proxy.Log("  " + text225 + "      - Fail cardholder verification if this is unsuccessful");
					}
					else
					{
						IUC285Proxy.Log("  " + text225 + "      - Apply succeeding CV Rule if this is unsuccessful");
					}
					switch (array22[num19].Substring(2, 6))
					{
					case "000000":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Fail CVM Processing");
						break;
					case "000001":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Plaintext PIN verification performed by ICC");
						break;
					case "000010":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Enciphered PIN verified online");
						break;
					case "000011":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Plaintext PIN verification performed by ICC and signature");
						break;
					case "000100":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Enciphered PIN verification performed by ICC");
						break;
					case "000101":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Enciphered PIN verification performed by ICC and signature");
						break;
					case "011110":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Signature");
						break;
					case "011111":
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - No CVM");
						break;
					default:
						IUC285Proxy.Log("  " + array22[num19].Substring(2, 6) + " - Reserved");
						break;
					}
					string text226 = TagDataByte[num20 + 1].ToString("X2");
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
					num20 += 2;
					num19++;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "95")
			{
				IUC285Proxy.Log("---------Terminal Verification Results(T95)---------");
				string text227 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text228 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string text229 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string text230 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string text231 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text227);
				IUC285Proxy.Log("  " + text227[0] + " - Offline data authentication was not performed");
				IUC285Proxy.Log("  " + text227[1] + " - SDA failed");
				IUC285Proxy.Log("  " + text227[2] + " - ICC data missing");
				IUC285Proxy.Log("  " + text227[3] + " - Card appears on terminal exception file");
				IUC285Proxy.Log("  " + text227[4] + " - DDA failed");
				IUC285Proxy.Log("  " + text227[5] + " - CDA failed");
				IUC285Proxy.Log("  " + text227[6] + " - RFU");
				IUC285Proxy.Log("  " + text227[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + text228);
				IUC285Proxy.Log("  " + text228[0] + " - ICC and terminal have different application versions");
				IUC285Proxy.Log("  " + text228[1] + " - Expired application");
				IUC285Proxy.Log("  " + text228[2] + " - Application not yet effective");
				IUC285Proxy.Log("  " + text228[3] + " - Requested service not allowed for card product");
				IUC285Proxy.Log("  " + text228[4] + " - New card");
				IUC285Proxy.Log("  " + text228[5] + " - RFU");
				IUC285Proxy.Log("  " + text228[6] + " - RFU");
				IUC285Proxy.Log("  " + text228[7] + " - RFU");
				IUC285Proxy.Log("Byte 3: " + text229);
				IUC285Proxy.Log("  " + text229[0] + " - Cardholder verification was not successful");
				IUC285Proxy.Log("  " + text229[1] + " - Unrecognised CVM");
				IUC285Proxy.Log("  " + text229[2] + " - PIN Try Limit exceeded");
				IUC285Proxy.Log("  " + text229[3] + " - PIN entry required and PIN pad not present or not working");
				IUC285Proxy.Log("  " + text229[4] + " - PIN entry required, PIN pad present, but PIN was not entered");
				IUC285Proxy.Log("  " + text229[5] + " - Online PIN entered");
				IUC285Proxy.Log("  " + text229[6] + " - RFU");
				IUC285Proxy.Log("  " + text229[7] + " - RFU");
				IUC285Proxy.Log("Byte 4: " + text230);
				IUC285Proxy.Log("  " + text230[0] + " - Transaction exceeds floor limit");
				IUC285Proxy.Log("  " + text230[1] + " - Lower consecutive offline limit exceeded");
				IUC285Proxy.Log("  " + text230[2] + " - Upper consecutive offline limit exceeded");
				IUC285Proxy.Log("  " + text230[3] + " - Transaction selected randomly for online processing");
				IUC285Proxy.Log("  " + text230[4] + " - Merchant forced transaction online");
				IUC285Proxy.Log("  " + text230[5] + " - RFU");
				IUC285Proxy.Log("  " + text230[6] + " - RFU");
				IUC285Proxy.Log("  " + text230[7] + " - RFU");
				IUC285Proxy.Log("Byte 5: " + text231);
				IUC285Proxy.Log("  " + text231[0] + " - Default TDOL used");
				IUC285Proxy.Log("  " + text231[1] + " - Issuer authentication failed");
				IUC285Proxy.Log("  " + text231[2] + " - Script processing failed before final GENERATE AC");
				IUC285Proxy.Log("  " + text231[3] + " - Script processing failed after final GENERATE AC");
				IUC285Proxy.Log("  " + text231[4] + " - RFU");
				IUC285Proxy.Log("  " + text231[5] + " - RFU");
				IUC285Proxy.Log("  " + text231[6] + " - RFU");
				IUC285Proxy.Log("  " + text231[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F07")
			{
				IUC285Proxy.Log("----------Application Usage Control(T9F07)----------");
				string text232 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text233 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text232);
				IUC285Proxy.Log("  " + text232[0] + " - Valid for domestic cash transaction");
				IUC285Proxy.Log("  " + text232[1] + " - Valid for international cash transaction");
				IUC285Proxy.Log("  " + text232[2] + " - Valid for domestic goods");
				IUC285Proxy.Log("  " + text232[3] + " - Valid for international goods");
				IUC285Proxy.Log("  " + text232[4] + " - Valid for domestic services");
				IUC285Proxy.Log("  " + text232[5] + " - Valid for international services");
				IUC285Proxy.Log("  " + text232[6] + " - Valid at ATMs");
				IUC285Proxy.Log("  " + text232[7] + " - Valid at terminals other than ATMs");
				IUC285Proxy.Log("Byte 2: " + text233);
				IUC285Proxy.Log("  " + text233[0] + " - Domestic cashback allowed");
				IUC285Proxy.Log("  " + text233[1] + " - International cashback allowed");
				IUC285Proxy.Log("  " + text233[2] + " - RFU");
				IUC285Proxy.Log("  " + text233[3] + " - RFU");
				IUC285Proxy.Log("  " + text233[4] + " - RFU");
				IUC285Proxy.Log("  " + text233[5] + " - RFU");
				IUC285Proxy.Log("  " + text233[6] + " - RFU");
				IUC285Proxy.Log("  " + text233[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0D")
			{
				IUC285Proxy.Log("---------Issuer Action Code Default (T9F0D)---------");
				string text234 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text235 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string text236 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string text237 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string text238 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text234);
				if (text234[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[0] + " - Do not reject if unable to process online and if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[0] + " - Reject if unable to process online and if Offline data authentication was not performed");
				}
				if (text234[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[1] + " - Do not reject if unable to process online and if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[1] + " - Reject if unable to process online and if Offline static data authentication failed");
				}
				if (text234[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[2] + " - Do not reject if unable to process online and if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[2] + " - Reject if unable to process online and if ICC data missing");
				}
				if (text234[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[3] + " - Do not reject if unable to process online and if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[3] + " - Reject if unable to process online and if Card appears on terminal exception file");
				}
				if (text234[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[4] + " - Do not reject if unable to process online and if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[4] + " - Reject if unable to process online and if Offline dynamic data authentication failed");
				}
				if (text234[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[5] + " - Do not reject if unable to process online and if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[5] + " - Reject if unable to process online and if Combined DDA/AC Generation failed");
				}
				if (text234[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text234[6] + " - Do not reject if unable to process online and if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + text234[6] + " - Reject if unable to process online and if SDA selected");
				}
				IUC285Proxy.Log("  " + text234[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + text235);
				if (text235[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text235[0] + " - Do not reject if unable to process online and if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + text235[0] + " - Reject if unable to process online and if ICC and terminal have different application versions");
				}
				if (text235[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text235[1] + " - Do not reject if unable to process online and if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + text235[1] + " - Reject if unable to process online and if Expired application");
				}
				if (text235[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text235[2] + " - Do not reject if unable to process online and if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + text235[2] + " - Reject if unable to process online and if Application not yet effective");
				}
				if (text235[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text235[3] + " - Do not reject if unable to process online and if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + text235[3] + " - Reject if unable to process online and if Requested service not allowed for card product");
				}
				if (text235[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text235[4] + " - Do not reject if unable to process online and if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + text235[4] + " - Reject if unable to process online and if New card");
				}
				IUC285Proxy.Log("  " + text235[5] + " - RFU");
				IUC285Proxy.Log("  " + text235[6] + " - RFU");
				IUC285Proxy.Log("  " + text235[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + text236);
				if (text236[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[0] + " - Do not reject if unable to process online and if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[0] + " - Reject if unable to process online and if Cardholder verification was not successful");
				}
				if (text236[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[1] + " - Do not reject if unable to process online and if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[1] + " - Reject if unable to process online and if Unrecognised CVM");
				}
				if (text236[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[2] + " - Do not reject if unable to process online and if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[2] + " - Reject if unable to process online and if PIN Try Limit exceeded");
				}
				if (text236[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[3] + " - Do not reject if unable to process online and if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[3] + " - Reject if unable to process online and if PIN entry required and PIN pad not present or not working");
				}
				if (text236[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[4] + " - Do not reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[4] + " - Reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (text236[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text236[5] + " - Do not reject if unable to process online and if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text236[5] + " - Reject if unable to process online and if Online PIN entered");
				}
				IUC285Proxy.Log("  " + text236[6] + " - RFU");
				IUC285Proxy.Log("  " + text236[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + text237);
				if (text237[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text237[0] + " - Do not reject if unable to process online and if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + text237[0] + " - Reject if unable to process online and if Transaction exceeds floor limit");
				}
				if (text237[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text237[1] + " - Do not reject if unable to process online and if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text237[1] + " - Reject if unable to process online and if Lower consecutive offline limit exceeded");
				}
				if (text237[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text237[2] + " - Do not reject if unable to process online and if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text237[2] + " - Reject if unable to process online and if Upper consecutive offline limit exceeded");
				}
				if (text237[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text237[3] + " - Do not reject if unable to process online and if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + text237[3] + " - Reject if unable to process online and if Transaction selected randomly for online processing");
				}
				if (text237[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text237[4] + " - Do not reject if unable to process online and if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + text237[4] + " - Reject if unable to process online and if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + text237[5] + " - RFU");
				IUC285Proxy.Log("  " + text237[6] + " - RFU");
				IUC285Proxy.Log("  " + text237[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + text238);
				if (text238[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text238[0] + " - Do not reject if unable to process online and if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + text238[0] + " - Reject if unable to process online and if Default TDOL used");
				}
				if (text238[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text238[1] + " - Do not reject if unable to process online and if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + text238[1] + " - Reject if unable to process online and if Issuer authentication was unsuccessful");
				}
				if (text238[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text238[2] + " - Do not reject if unable to process online and if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text238[2] + " - Reject if unable to process online and if Script processing failed before final GENERATE AC");
				}
				if (text238[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text238[3] + " - Do not reject if unable to process online and if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text238[3] + " - Reject if unable to process online and if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + text238[4] + " - RFU");
				IUC285Proxy.Log("  " + text238[5] + " - RFU");
				IUC285Proxy.Log("  " + text238[6] + " - RFU");
				IUC285Proxy.Log("  " + text238[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0E")
			{
				IUC285Proxy.Log("----------Issuer Action Code Denial(T9F0E)----------");
				string text239 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text240 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string text241 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string text242 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string text243 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text239);
				if (text239[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[0] + " - Do not decline if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[0] + " - Decline if Offline data authentication was not performed");
				}
				if (text239[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[1] + " - Do not decline if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[1] + " - Decline if Offline static data authentication failed");
				}
				if (text239[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[2] + " - Do not decline if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[2] + " - Decline if ICC data missing");
				}
				if (text239[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[3] + " - Do not decline if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[3] + " - Decline if Card appears on terminal exception file");
				}
				if (text239[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[4] + " - Do not decline if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[4] + " - Decline if Offline dynamic data authentication failed");
				}
				if (text239[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[5] + " - Do not decline if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[5] + " - Decline if Combined DDA/AC Generation failed");
				}
				if (text239[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text239[6] + " - Do not decline if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + text239[6] + " - Decline if SDA selected");
				}
				IUC285Proxy.Log("  " + text239[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + text240);
				if (text240[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text240[0] + " - Do not decline if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + text240[0] + " - Decline if ICC and terminal have different application versions");
				}
				if (text240[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text240[1] + " - Do not decline if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + text240[1] + " - Decline if Expired application");
				}
				if (text240[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text240[2] + " - Do not decline if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + text240[2] + " - Decline if Application not yet effective");
				}
				if (text240[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text240[3] + " - Do not decline if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + text240[3] + " - Decline if Requested service not allowed for card product");
				}
				if (text240[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text240[4] + " - Do not decline if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + text240[4] + " - Decline if New card");
				}
				IUC285Proxy.Log("  " + text240[5] + " - RFU");
				IUC285Proxy.Log("  " + text240[6] + " - RFU");
				IUC285Proxy.Log("  " + text240[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + text241);
				if (text241[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[0] + " - Do not decline if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[0] + " - Decline if Cardholder verification was not successful");
				}
				if (text241[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[1] + " - Do not decline if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[1] + " - Decline if Unrecognised CVM");
				}
				if (text241[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[2] + " - Do not decline if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[2] + " - Decline if PIN Try Limit exceeded");
				}
				if (text241[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[3] + " - Do not decline if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[3] + " - Decline if PIN entry required and PIN pad not present or not working");
				}
				if (text241[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[4] + " - Do not decline if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[4] + " - Decline if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (text241[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text241[5] + " - Do not decline if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text241[5] + " - Decline if Online PIN entered");
				}
				IUC285Proxy.Log("  " + text241[6] + " - RFU");
				IUC285Proxy.Log("  " + text241[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + text242);
				if (text242[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text242[0] + " - Do not decline if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + text242[0] + " - Decline if Transaction exceeds floor limit");
				}
				if (text242[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text242[1] + " - Do not decline if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text242[1] + " - Decline if Lower consecutive offline limit exceeded");
				}
				if (text242[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text242[2] + " - Do not decline if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text242[2] + " - Decline if Upper consecutive offline limit exceeded");
				}
				if (text242[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text242[3] + " - Do not decline if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + text242[3] + " - Decline if Transaction selected randomly for online processing");
				}
				if (text242[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text242[4] + " - Do not decline if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + text242[4] + " - Decline if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + text242[5] + " - RFU");
				IUC285Proxy.Log("  " + text242[6] + " - RFU");
				IUC285Proxy.Log("  " + text242[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + text243);
				if (text243[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text243[0] + " - Do not decline if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + text243[0] + " - Decline if Default TDOL used");
				}
				if (text243[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text243[1] + " - Do not decline if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + text243[1] + " - Decline if Issuer authentication was unsuccessful");
				}
				if (text243[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text243[2] + " - Do not decline if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text243[2] + " - Decline if Script processing failed before final GENERATE AC");
				}
				if (text243[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text243[3] + " - Do not decline if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text243[3] + " - Decline if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + text243[4] + " - RFU");
				IUC285Proxy.Log("  " + text243[5] + " - RFU");
				IUC285Proxy.Log("  " + text243[6] + " - RFU");
				IUC285Proxy.Log("  " + text243[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F0F")
			{
				IUC285Proxy.Log("----------Issuer Action Code Online(T9F0F)----------");
				string text244 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text245 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string text246 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				string text247 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
				string text248 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text244);
				if (text244[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[0] + " - Do not go online if Offline data authentication was not performed");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[0] + " - Go online if Offline data authentication was not performed");
				}
				if (text244[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[1] + " - Do not go online if Offline static data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[1] + " - Go online if Offline static data authentication failed");
				}
				if (text244[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[2] + " - Do not go online if ICC data missing");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[2] + " - Go online if ICC data missing");
				}
				if (text244[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[3] + " - Do not go online if Card appears on terminal exception file");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[3] + " - Go online if Card appears on terminal exception file");
				}
				if (text244[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[4] + " - Do not go online if Offline dynamic data authentication failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[4] + " - Go online if Offline dynamic data authentication failed");
				}
				if (text244[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[5] + " - Do not go online if Combined DDA/AC Generation failed");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[5] + " - Go online if Combined DDA/AC Generation failed");
				}
				if (text244[6].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text244[6] + " - Do not go online if SDA selected");
				}
				else
				{
					IUC285Proxy.Log("  " + text244[6] + " - Go online if SDA selected");
				}
				IUC285Proxy.Log("  " + text244[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 2: " + text245);
				if (text245[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text245[0] + " - Do not go online if ICC and terminal have different application versions");
				}
				else
				{
					IUC285Proxy.Log("  " + text245[0] + " - Go online if ICC and terminal have different application versions");
				}
				if (text245[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text245[1] + " - Do not go online if Expired application");
				}
				else
				{
					IUC285Proxy.Log("  " + text245[1] + " - Go online if Expired application");
				}
				if (text245[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text245[2] + " - Do not go online if Application not yet effective");
				}
				else
				{
					IUC285Proxy.Log("  " + text245[2] + " - Go online if Application not yet effective");
				}
				if (text245[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text245[3] + " - Do not go online if Requested service not allowed for card product");
				}
				else
				{
					IUC285Proxy.Log("  " + text245[3] + " - Go online if Requested service not allowed for card product");
				}
				if (text245[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text245[4] + " - Do not go online if New card");
				}
				else
				{
					IUC285Proxy.Log("  " + text245[4] + " - Go online if New card");
				}
				IUC285Proxy.Log("  " + text245[5] + " - RFU");
				IUC285Proxy.Log("  " + text245[6] + " - RFU");
				IUC285Proxy.Log("  " + text245[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 3: " + text246);
				if (text246[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[0] + " - Do not go online if Cardholder verification was not successful");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[0] + " - Go online if Cardholder verification was not successful");
				}
				if (text246[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[1] + " - Do not go online if Unrecognised CVM");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[1] + " - Go online if Unrecognised CVM");
				}
				if (text246[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[2] + " - Do not go online if PIN Try Limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[2] + " - Go online if PIN Try Limit exceeded");
				}
				if (text246[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[3] + " - Do not go online if PIN entry required and PIN pad not present or not working");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[3] + " - Go online if PIN entry required and PIN pad not present or not working");
				}
				if (text246[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[4] + " - Do not go online if PIN entry required, PIN pad present, but PIN was not entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[4] + " - Go online if PIN entry required, PIN pad present, but PIN was not entered");
				}
				if (text246[5].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text246[5] + " - Do not go online if Online PIN entered");
				}
				else
				{
					IUC285Proxy.Log("  " + text246[5] + " - Go online if Online PIN entered");
				}
				IUC285Proxy.Log("  " + text246[6] + " - RFU");
				IUC285Proxy.Log("  " + text246[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 4: " + text247);
				if (text247[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text247[0] + " - Do not go online if Transaction exceeds floor limit");
				}
				else
				{
					IUC285Proxy.Log("  " + text247[0] + " - Go online if Transaction exceeds floor limit");
				}
				if (text247[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text247[1] + " - Do not go online if Lower consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text247[1] + " - Go online if Lower consecutive offline limit exceeded");
				}
				if (text247[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text247[2] + " - Do not go online if Upper consecutive offline limit exceeded");
				}
				else
				{
					IUC285Proxy.Log("  " + text247[2] + " - Go online if Upper consecutive offline limit exceeded");
				}
				if (text247[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text247[3] + " - Do not go online if Transaction selected randomly for online processing");
				}
				else
				{
					IUC285Proxy.Log("  " + text247[3] + " - Go online if Transaction selected randomly for online processing");
				}
				if (text247[4].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text247[4] + " - Do not go online if Merchant forced transaction online");
				}
				else
				{
					IUC285Proxy.Log("  " + text247[4] + " - Go online if Merchant forced transaction online");
				}
				IUC285Proxy.Log("  " + text247[5] + " - RFU");
				IUC285Proxy.Log("  " + text247[6] + " - RFU");
				IUC285Proxy.Log("  " + text247[7] + " - RFU");
				IUC285Proxy.Log("-----");
				IUC285Proxy.Log("Byte 5: " + text248);
				if (text248[0].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text248[0] + " - Do not go online if Default TDOL used");
				}
				else
				{
					IUC285Proxy.Log("  " + text248[0] + " - Go online if Default TDOL used");
				}
				if (text248[1].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text248[1] + " - Do not go online if Issuer authentication was unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + text248[1] + " - Go online if Issuer authentication was unsuccessful");
				}
				if (text248[2].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text248[2] + " - Do not go online if Script processing failed before final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text248[2] + " - Go online if Script processing failed before final GENERATE AC");
				}
				if (text248[3].ToString() == "0")
				{
					IUC285Proxy.Log("  " + text248[3] + " - Do not go online if Script processing failed after final GENERATE AC");
				}
				else
				{
					IUC285Proxy.Log("  " + text248[3] + " - Go online if Script processing failed after final GENERATE AC");
				}
				IUC285Proxy.Log("  " + text248[4] + " - RFU");
				IUC285Proxy.Log("  " + text248[5] + " - RFU");
				IUC285Proxy.Log("  " + text248[6] + " - RFU");
				IUC285Proxy.Log("  " + text248[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F27")
			{
				IUC285Proxy.Log("---------Cryptogram information data(T9F27)---------");
				string text249 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Binary data: " + text249);
				if (text249.Substring(0, 2) == "00")
				{
					IUC285Proxy.Log("  00  - AAC");
					Tag9F27CryptogramDeclined = true;
				}
				if (text249.Substring(0, 2) == "01")
				{
					IUC285Proxy.Log("  01  - TC");
				}
				if (text249.Substring(0, 2) == "10")
				{
					IUC285Proxy.Log("  10  - ARQC");
				}
				if (text249.Substring(0, 2) == "11")
				{
					IUC285Proxy.Log("  11  - RFU");
				}
				IUC285Proxy.Log("  " + text249.Substring(2, 2) + "  - Payment System-specific cryptogram");
				string text250 = text249.Substring(4, 1);
				if (text250 == "0")
				{
					IUC285Proxy.Log("  " + text250 + "   - No advice required");
				}
				else
				{
					IUC285Proxy.Log("  " + text250 + "   - Advice required");
				}
				string text251 = text249.Substring(5, 3);
				switch (text251)
				{
				case "000":
					IUC285Proxy.Log("  " + text251 + " - No information given");
					break;
				case "001":
					IUC285Proxy.Log("  " + text251 + " - Service not allowed");
					break;
				case "010":
					IUC285Proxy.Log("  " + text251 + " - PIN try limit exceeded");
					break;
				case "011":
					IUC285Proxy.Log("  " + text251 + " - Issuer authetication failed");
					break;
				default:
					if (text251.Substring(0, 1) == "1")
					{
						IUC285Proxy.Log("  " + text251 + " - Other values RFU");
					}
					else
					{
						IUC285Proxy.Log("  " + text251 + " - Reason / advice code");
					}
					break;
				}
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F33")
			{
				IUC285Proxy.Log("------------Terminal Capabilities(T9F33)------------");
				string text252 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text253 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
				string text254 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
				IUC285Proxy.Log("Byte 1: " + text252);
				IUC285Proxy.Log("  " + text252[0] + " - Manual key entry");
				IUC285Proxy.Log("  " + text252[1] + " - Magnetic stripe");
				IUC285Proxy.Log("  " + text252[2] + " - IC with contacts");
				IUC285Proxy.Log("  " + text252[3] + " - RFU");
				IUC285Proxy.Log("  " + text252[4] + " - RFU");
				IUC285Proxy.Log("  " + text252[5] + " - RFU");
				IUC285Proxy.Log("  " + text252[6] + " - RFU");
				IUC285Proxy.Log("  " + text252[7] + " - RFU");
				IUC285Proxy.Log("Byte 2: " + text253);
				IUC285Proxy.Log("  " + text253[0] + " - Plaintext PIN for ICC verification");
				IUC285Proxy.Log("  " + text253[1] + " - Enciphered PIN for online verification");
				IUC285Proxy.Log("  " + text253[2] + " - Signature (paper)");
				IUC285Proxy.Log("  " + text253[3] + " - Enciphered PIN for offline verification");
				IUC285Proxy.Log("  " + text253[4] + " - No CVM required");
				IUC285Proxy.Log("  " + text253[5] + " - RFU");
				IUC285Proxy.Log("  " + text253[6] + " - RFU");
				IUC285Proxy.Log("  " + text253[7] + " - RFU");
				IUC285Proxy.Log("Byte 3: " + text254);
				IUC285Proxy.Log("  " + text254[0] + " - SDA");
				IUC285Proxy.Log("  " + text254[1] + " - DDA");
				IUC285Proxy.Log("  " + text254[2] + " - Card capture");
				IUC285Proxy.Log("  " + text254[3] + " - RFU");
				IUC285Proxy.Log("  " + text254[4] + " - CDA");
				IUC285Proxy.Log("  " + text254[5] + " - RFU");
				IUC285Proxy.Log("  " + text254[6] + " - RFU");
				IUC285Proxy.Log("  " + text254[7] + " - RFU");
				IUC285Proxy.Log("----------------------------------------------------");
			}
			else if (Tag == "9F34")
			{
				IUC285Proxy.Log("-----------------CVM Results(T9F34)-----------------");
				string text255 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
				string text256 = TagDataByte[1].ToString("X2");
				string text257 = TagDataByte[2].ToString("X2");
				IUC285Proxy.Log("Byte 1 - CVM Performed: " + text255);
				IUC285Proxy.Log("  " + text255.Substring(0, 1) + "      - RFU");
				string text258 = text255.Substring(1, 1);
				if (text258 == "0")
				{
					IUC285Proxy.Log("  " + text258 + "      - Fail cardholder verification if this is unsuccessful");
				}
				else
				{
					IUC285Proxy.Log("  " + text258 + "      - Apply succeeding CV Rule if this is unsuccessful");
				}
				switch (text255.Substring(2, 6))
				{
				case "000000":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Fail CVM Processing");
					break;
				case "000001":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Plaintext PIN verification performed by ICC");
					break;
				case "000010":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Enciphered PIN verified online");
					break;
				case "000011":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Plaintext PIN verification performed by ICC and signature");
					break;
				case "000100":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Enciphered PIN verification performed by ICC");
					break;
				case "000101":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Enciphered PIN verification performed by ICC and signature");
					break;
				case "011110":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Signature");
					break;
				case "011111":
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - No CVM");
					break;
				default:
					IUC285Proxy.Log("  " + text255.Substring(2, 6) + " - Reserved");
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
				string[] array23 = new string[length / 2];
				int num21 = 0;
				for (int num22 = 0; num22 < TagDataStr.Length; num22 += 2)
				{
					array23[num21] = TagDataStr.Substring(num22, 2);
					num21++;
				}
				string[] array24 = array23;
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
