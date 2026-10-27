using System.ComponentModel.DataAnnotations;

namespace Redbox.Rental.Model.Session;

public enum BrowseSelectedSort
{
	[Display(Name = "Newest")]
	Newest,
	[Display(Name = "alpha")]
	Alpha,
	[Display(Name = "coming-soon")]
	Bluray,
	[Display(Name = "DVD")]
	Dvd,
	[Display(Name = "Digital Code")]
	DigitalCode,
	[Display(Name = "4K UHD")]
	_4KUHD,
	[Display(Name = "for-sale")]
	ForSale,
	[Display(Name = "coming-soon")]
	ComingSoon,
	[Display(Name = "Xbox 360")]
	Xbox360,
	[Display(Name = "Xbox One")]
	XboxOne,
	[Display(Name = "PS2")]
	Ps2,
	[Display(Name = "PS3")]
	Ps3,
	[Display(Name = "PS4")]
	Ps4,
	[Display(Name = "Nintendo DS")]
	NintendoDS,
	[Display(Name = "Wii")]
	Wii,
	[Display(Name = "Wii U")]
	WiiU,
	[Display(Name = "Nintendo Switch")]
	NintendoSwitch,
	[Display(Name = "Top 20")]
	Top20,
	[Display(Name = "Action")]
	Action,
	[Display(Name = "Comedy")]
	Comedy,
	[Display(Name = "Drama")]
	Drama,
	[Display(Name = "Family")]
	Family,
	[Display(Name = "Horror")]
	Horror,
	[Display(Name = "$.99")]
	_99Cents,
	[Display(Name = "$15K Summer Sweeps")]
	SonySweepstakes,
	[Display(Name = "Deals")]
	Deals
}
