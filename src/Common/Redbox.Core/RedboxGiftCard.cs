using System.Collections.Generic;
using System.Linq;

namespace Redbox.Core;

public sealed class RedboxGiftCard
{
	private List<BinRange> m_redboxGiftCardRanges = new List<BinRange>();

	private List<BinRange> m_overlappingDiscoverRanges = new List<BinRange>
	{
		new BinRange(622126, 622925),
		new BinRange(624000, 626999),
		new BinRange(628200, 628899),
		new BinRange(644000, 659999)
	};

	public RedboxGiftCard()
	{
	}

	public RedboxGiftCard(List<BinRange> binRanges)
		: this()
	{
		AddBinRanges(binRanges);
	}

	public bool IsRedboxGiftCard(string number)
	{
		if (string.IsNullOrEmpty(number) || number.Length < 6)
		{
			return false;
		}
		int? bin = number.Substring(0, 6).ToInt32();
		if (number.Length == 16 && bin.HasValue && m_redboxGiftCardRanges.Any((BinRange binRange) => binRange.Between(bin.Value)) && !m_overlappingDiscoverRanges.Any((BinRange binRange) => binRange.Between(bin.Value)))
		{
			return true;
		}
		return false;
	}

	public void AddBinRanges(List<BinRange> binRanges)
	{
		m_redboxGiftCardRanges.AddRange(binRanges);
	}

	public void ClearRanges()
	{
		m_redboxGiftCardRanges.Clear();
	}
}
