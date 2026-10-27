namespace Redbox.Core;

public enum CardType : byte
{
	Unknown,
	[CardCode(Code = "AX")]
	AmericanExpress,
	[CardCode(Code = "DI")]
	Discover,
	[CardCode(Code = "JC")]
	JCB,
	[CardCode(Code = "MC")]
	MasterCard,
	[CardCode(Code = "VI")]
	Visa,
	[CardCode(Code = "RGC")]
	RedboxGiftCard
}
