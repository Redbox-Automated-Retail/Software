namespace Redbox.Core;

internal enum Redbox_002EMacros1309_002ECardType : byte
{
	Unknown,
	[Redbox_002EMacros1309_002ECardCode(Code = "AX")]
	AmericanExpress,
	[Redbox_002EMacros1309_002ECardCode(Code = "DI")]
	Discover,
	[Redbox_002EMacros1309_002ECardCode(Code = "JC")]
	JCB,
	[Redbox_002EMacros1309_002ECardCode(Code = "MC")]
	MasterCard,
	[Redbox_002EMacros1309_002ECardCode(Code = "VI")]
	Visa
}
