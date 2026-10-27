using System.ComponentModel;

namespace Redbox.Rental.Model.KioskProduct;

public enum Genres : long
{
	[Description("Action And Adventure")]
	ActionAndAdventure = 1000L,
	[Description("Animation")]
	Animation = 1001L,
	[Description("Award Winners")]
	AwardWinners = 1002L,
	[Description("Crime")]
	Crime = 1003L,
	[Description("Comedy")]
	Comedy = 1004L,
	[Description("Drama")]
	Drama = 1005L,
	[Description("Family")]
	FamilyMovies = 1006L,
	[Description("Foreign")]
	Foreign = 1007L,
	[Description("Holiday")]
	Holiday = 1008L,
	[Description("Horror")]
	Horror = 1009L,
	[Description("Kids")]
	Kids = 1010L,
	[Description("Musical")]
	Musical = 1011L,
	[Description("Romance")]
	Romance = 1012L,
	[Description("SciFi And Fantasy")]
	SciFiAndFantasy = 1013L,
	[Description("Special Interest")]
	SpecialInterest = 1014L,
	[Description("Suspense")]
	Suspense = 1016L,
	[Description("Television")]
	Television = 1017L,
	[Description("War")]
	War = 1018L,
	[Description("Western")]
	Western = 1019L,
	[Description("Hit Movies")]
	HitMovies = 1020L,
	[Description("GAME")]
	GAME = 1021L,
	[Description("Bluray")]
	Bluray = 1022L,
	[Description("redbox Replay")]
	RedboxReplayMovies = 1025L,
	[Description("Action Games")]
	ActionGames = 1026L,
	[Description("Fighting")]
	Fighting = 1028L,
	[Description("Music And Party")]
	MusicAndParty = 1030L,
	[Description("Shooter")]
	Shooter = 1034L,
	[Description("Sports")]
	Sports = 1036L,
	[Description("Documentary")]
	Documentary = 1092L,
	[Description("Top 20 Movies")]
	Top20Movies = 1093L,
	[Description("Family")]
	FamilyGames = 1094L,
	[Description("redbox Replay")]
	RedboxReplayGames = 1097L,
	[Description("War And Western")]
	WarAndWestern = 1098L,
	[Description("Martial Arts")]
	MartialArts = 1099L,
	[Description("Independent")]
	Independent = 1100L,
	[Description("Documentary And Special Interest")]
	DocumentaryAndSpecialInterest = 1101L,
	[Description("Adventure")]
	Adventure = 1102L,
	[Description("Action")]
	Action = 1103L,
	[Description("Award Nominees")]
	AwardNominees = 1104L
}
