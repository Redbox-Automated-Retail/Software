using System.Collections.Generic;

namespace DeviceService.ComponentModel;

public class TagConstants
{
	public static List<TagDetails> Details { get; set; } = new List<TagDetails>
	{
		new TagDetails
		{
			Tag = "42",
			Description = "Issuer Identification Number"
		},
		new TagDetails
		{
			Tag = "4F",
			Description = "Application Identifier (AID)"
		},
		new TagDetails
		{
			Tag = "50",
			Description = "Application label"
		},
		new TagDetails
		{
			Tag = "57",
			Description = "Track 2 Equivalent Data",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "5A",
			Description = "Primary Account Number",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "82",
			Description = "Application Interchange Profile"
		},
		new TagDetails
		{
			Tag = "84",
			Description = "Dedicated File (DF) Name"
		},
		new TagDetails
		{
			Tag = "8A",
			Description = "Authorization Response Code"
		},
		new TagDetails
		{
			Tag = "8E",
			Description = "Cardholder Verification Method (CVM) List"
		},
		new TagDetails
		{
			Tag = "91",
			Description = "Issuer Authentication Data"
		},
		new TagDetails
		{
			Tag = "95",
			Description = "Terminal Verification Results"
		},
		new TagDetails
		{
			Tag = "9A",
			Description = "Transaction Date"
		},
		new TagDetails
		{
			Tag = "9B",
			Description = "Transaction Status Information"
		},
		new TagDetails
		{
			Tag = "9C",
			Description = "Transaction Type"
		},
		new TagDetails
		{
			Tag = "5F20",
			Description = "Cardholder Name",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "5F24",
			Description = "Expiry Date",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "5F28",
			Description = "Issuer Country Code, three-digit numeric"
		},
		new TagDetails
		{
			Tag = "5F2A",
			Description = "Transaction Currency Code"
		},
		new TagDetails
		{
			Tag = "5F2D",
			Description = "Preferred Languages"
		},
		new TagDetails
		{
			Tag = "5F30",
			Description = "Service Code"
		},
		new TagDetails
		{
			Tag = "5F34",
			Description = "Application PAN Sequence Number",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "5F54",
			Description = "Bank Identifier Code (BIC)"
		},
		new TagDetails
		{
			Tag = "5F55",
			Description = "Issuer Country Code, two-digit alpha"
		},
		new TagDetails
		{
			Tag = "5F56",
			Description = "Issuer Country Code, three-digit alpha"
		},
		new TagDetails
		{
			Tag = "9F02",
			Description = "Amount, Authorized (Numeric)"
		},
		new TagDetails
		{
			Tag = "9F03",
			Description = "Amount, Other (Numeric)"
		},
		new TagDetails
		{
			Tag = "9F06",
			Description = "Application ID Terminal"
		},
		new TagDetails
		{
			Tag = "9F07",
			Description = "Application Usage Control"
		},
		new TagDetails
		{
			Tag = "9F08",
			Description = "Application Version Number (ICC)"
		},
		new TagDetails
		{
			Tag = "9F09",
			Description = "Application Version Number (Terminal)"
		},
		new TagDetails
		{
			Tag = "9F0B",
			Description = "Cardholder Name Extended"
		},
		new TagDetails
		{
			Tag = "9F0D",
			Description = "Issuer Action Code Default"
		},
		new TagDetails
		{
			Tag = "9F0E",
			Description = "Issuer Action Code Denial"
		},
		new TagDetails
		{
			Tag = "9F0F",
			Description = "Issuer Action Code Online"
		},
		new TagDetails
		{
			Tag = "9F10",
			Description = "Issuer Application Data"
		},
		new TagDetails
		{
			Tag = "9F11",
			Description = "Issuer Code Table Index"
		},
		new TagDetails
		{
			Tag = "9F12",
			Description = "Application Preferred Name"
		},
		new TagDetails
		{
			Tag = "9F14",
			Description = "Lower Consecutive Offline Limit"
		},
		new TagDetails
		{
			Tag = "9F17",
			Description = "PIN Try Count"
		},
		new TagDetails
		{
			Tag = "9F1A",
			Description = "Terminal Country Code"
		},
		new TagDetails
		{
			Tag = "9F1B",
			Description = "Terminal Floor Limit"
		},
		new TagDetails
		{
			Tag = "9F1E",
			Description = "Interface Device (IFD) Serial Number"
		},
		new TagDetails
		{
			Tag = "9F1F",
			Description = "Track 1 Discretionary Data",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "9F20",
			Description = "Track 2 Discretionary Data",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "9F21",
			Description = "Transaction Time."
		},
		new TagDetails
		{
			Tag = "9F26",
			Description = "Application Cryptogram (AC).",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "9F27",
			Description = "Cryptogram Information Data (CID)."
		},
		new TagDetails
		{
			Tag = "9F33",
			Description = "Terminal Capabilities."
		},
		new TagDetails
		{
			Tag = "9F34",
			Description = "Cardholder Verification method (CVM) Results."
		},
		new TagDetails
		{
			Tag = "9F35",
			Description = "Terminal Type."
		},
		new TagDetails
		{
			Tag = "9F36",
			Description = "Application Transaction Counter (ATC)."
		},
		new TagDetails
		{
			Tag = "9F37",
			Description = "Unpredictable Number.",
			IsPii = true
		},
		new TagDetails
		{
			Tag = "9F39",
			Description = "POS Entry Mode."
		},
		new TagDetails
		{
			Tag = "9F40",
			Description = "Additional Terminal Capabilities."
		},
		new TagDetails
		{
			Tag = "9F41",
			Description = "Transaction Sequence Counter."
		},
		new TagDetails
		{
			Tag = "9F42",
			Description = "Application Currency Code"
		},
		new TagDetails
		{
			Tag = "9F51",
			Description = "Application Currency Code/DRDOL"
		},
		new TagDetails
		{
			Tag = "9F53",
			Description = "Transaction Category Code (VISA only)."
		},
		new TagDetails
		{
			Tag = "9F5B",
			Description = "Transaction Category Code (VISA only)."
		},
		new TagDetails
		{
			Tag = "9F5D",
			Description = "Available Offline Spending Amount (AOSA)."
		},
		new TagDetails
		{
			Tag = "9F66",
			Description = "Terminal Transaction Qualifiers (TTQ)"
		},
		new TagDetails
		{
			Tag = "9F67",
			Description = "(Amex spec) - NATC (Track2) / MSD Offset."
		},
		new TagDetails
		{
			Tag = "9F6C",
			Description = "Card Transaction Qualifiers (CTQ)."
		},
		new TagDetails
		{
			Tag = "9F6D",
			Description = "EMV Proprietary tag. See brand specifications for details per brand."
		},
		new TagDetails
		{
			Tag = "9F6E",
			Description = "Third Party Data."
		},
		new TagDetails
		{
			Tag = "9F71",
			Description = "Protected Data Envelope 2 & Mobile CVM Results"
		},
		new TagDetails
		{
			Tag = "9F7C",
			Description = "Customer Exclusive Data (CED) & Merchant Custom Data."
		},
		new TagDetails
		{
			Tag = "DF03",
			Description = "Terminal Action Code Default."
		},
		new TagDetails
		{
			Tag = "DF04",
			Description = "Terminal Action Code Denial."
		},
		new TagDetails
		{
			Tag = "DF05",
			Description = "Terminal Action Code Online."
		},
		new TagDetails
		{
			Tag = "DF11",
			Description = "Issuer Script Results."
		},
		new TagDetails
		{
			Tag = "1000",
			Description = "Account Type (Interac only).",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1001",
			Description = "PIN Entry Required Flag.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1002",
			Description = "Signature required Flag.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1003",
			Description = "Confirmation response Code.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1004",
			Description = "Host response available.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1005",
			Description = "Transaction Type.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "100E",
			Description = "Selected Transaction Language.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "100F",
			Description = "PIN Entry Success Flag. (Required for Offline PIN entry only)",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1010",
			Description = "Error Response Code.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1011",
			Description = "Special Case Authorization.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1012",
			Description = "Contactless Transaction Outcome.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1013",
			Description = "Contactless Profile Used.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1014",
			Description = "Card Payment Type.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1015",
			Description = "Suncor.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1016",
			Description = "U.S. Common AID Flag.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1017",
			Description = "MSR Track 1.",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1018",
			Description = "MSR Track 2.",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1019",
			Description = "MSR Track 3.",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "101A",
			Description = "Fallback to MSR Status.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "101B",
			Description = "Contactless Online PIN CVM Flag.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "101C",
			Description = "Contactless NoCVM Flag.",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "101D",
			Description = "Cless Mobile CVM performed",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "101E",
			Description = "Cless Mobile CVM results",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "1020",
			Description = "Merchant Coupon Data",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "FF1D",
			Description = "Masked PAN",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "FF1E",
			Description = "Encrypted Track 1 Data",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "FF1F",
			Description = "Encrypted Track 2 Data",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "FF20",
			Description = "ETB",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "FF21",
			Description = "Encrypted Track 3 Data",
			IsPii = true,
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "9000",
			Description = "Card payment Type",
			IsIngenicoTag = true
		},
		new TagDetails
		{
			Tag = "9001",
			Description = "Card Entry Mode",
			IsIngenicoTag = true
		}
	};
}
