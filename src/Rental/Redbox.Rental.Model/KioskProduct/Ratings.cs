using System.ComponentModel;

namespace Redbox.Rental.Model.KioskProduct;

public enum Ratings : long
{
	None = 0L,
	[Description("General Audiences. All Ages Admitted.")]
	G = 5L,
	[Description("Parental Guidance Suggested. Some Material May Not Be Suitable For Children.")]
	PG = 6L,
	[Description("Parents Strongly Cautioned. Some Material May Be Inappropriate For Children Under 13.")]
	PG13 = 7L,
	[Description("Restricted. Children Under 17 Require Accompanying Parent or Adult Guardian.")]
	R = 8L,
	[Description("Not Rated")]
	NR = 10L,
	[Description("ALL AGES")]
	ALLAGES = 13L,
	[Description("Early Childhood, 6 and older")]
	EC = 14L,
	[Description("Everyone")]
	E = 15L,
	[Description("Everyone 10 and older")]
	E10PLUS = 16L,
	[Description("Teen, ages 13 and older")]
	T = 17L,
	[Description("Mature, 17 and older")]
	M17PLUS = 18L,
	[Description("Rating Pending")]
	RP = 19L,
	[Description("Adult Only")]
	AO = 20L,
	[Description("Mature Audience Only")]
	TVMA = 21L,
	[Description("Parental Guidance Suggested")]
	TVPG = 22L,
	[Description("Parents Strongly Cautioned")]
	TV14 = 23L,
	[Description("General Audience")]
	TVG = 24L,
	[Description("All Children")]
	TVY = 25L,
	[Description("Directed to Older Children")]
	TVY7 = 26L,
	Max = 999L
}
