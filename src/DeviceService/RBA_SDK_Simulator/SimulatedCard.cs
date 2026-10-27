using System.Collections.Generic;
using System.Linq;

namespace RBA_SDK_Simulator;

/// <summary>A card presented to the simulated terminal. Only test card numbers are used.</summary>
public class SimulatedCard
{
	public string Name { get; set; } = "Visa";

	public string Pan { get; set; } = "4761739001010010";

	/// <summary>YYMM, as on track 2.</summary>
	public string Expiry { get; set; } = "3012";

	public string ServiceCode { get; set; } = "201";

	public string CardholderName { get; set; } = "TEST/CARD";

	/// <summary>EMV application identifier (tag 4F). Determines the card brand the service reports.</summary>
	public string Aid { get; set; } = "A0000000031010";

	public string ApplicationLabel { get; set; } = "VISA CREDIT";

	/// <summary>When true, the chip declines the transaction (cryptogram AAC in 33.05).</summary>
	public bool Decline { get; set; }

	/// <summary>When true, a contactless tap is reported as a mobile wallet (tag 9F6E).</summary>
	public bool MobileWallet { get; set; }

	/// <summary>When true, swipe data is returned in the OnGuard encrypted layout (with dummy cryptograms).</summary>
	public bool Encrypted { get; set; }

	public string Track1 => $"%B{Pan}^{CardholderName}^{Expiry}{ServiceCode}000000000000?";

	public string Track2 => $";{Pan}={Expiry}{ServiceCode}0000000000?";

	/// <summary>Track 2 equivalent data (tag 57): PAN, 'D' separator, expiry, service code, padded to whole bytes.</summary>
	public string Track2EquivalentHex
	{
		get
		{
			string text = Pan + "D" + Expiry + ServiceCode + "00000";
			if (text.Length % 2 != 0)
			{
				text += "F";
			}
			return text;
		}
	}

	public SimulatedCard Clone()
	{
		return (SimulatedCard)MemberwiseClone();
	}

	public static IReadOnlyList<SimulatedCard> Presets { get; } = new List<SimulatedCard>
	{
		new SimulatedCard
		{
			Name = "Visa",
			Pan = "4761739001010010",
			Aid = "A0000000031010",
			ApplicationLabel = "VISA CREDIT"
		},
		new SimulatedCard
		{
			Name = "Mastercard",
			Pan = "5413330089010434",
			Aid = "A0000000041010",
			ApplicationLabel = "MASTERCARD"
		},
		new SimulatedCard
		{
			Name = "Amex",
			Pan = "374245001751006",
			Aid = "A00000002501",
			ApplicationLabel = "AMERICAN EXPRESS"
		},
		new SimulatedCard
		{
			Name = "Discover",
			Pan = "6510000000001240",
			Aid = "A0000001523010",
			ApplicationLabel = "DISCOVER"
		},
		new SimulatedCard
		{
			Name = "RedboxGiftCard",
			Pan = "6010561234567890",
			Expiry = "4912",
			ServiceCode = "120",
			Aid = "",
			ApplicationLabel = ""
		}
	};

	public static SimulatedCard Preset(string name)
	{
		return (Presets.FirstOrDefault((SimulatedCard p) => string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase)) ?? Presets[0]).Clone();
	}
}
