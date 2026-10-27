using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public static class TitleTypeProperties
{
	private static Dictionary<TitleType, TitleTypeProperty> _titleTypeProperties;

	public static Dictionary<TitleType, TitleTypeProperty> Properties
	{
		get
		{
			if (_titleTypeProperties == null)
			{
				_titleTypeProperties = new Dictionary<TitleType, TitleTypeProperty>
				{
					{
						TitleType.All,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_all"
						}
					},
					{
						TitleType.DVD,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_dvd"
						}
					},
					{
						TitleType.Bluray,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_bluray"
						}
					},
					{
						TitleType._4KUHD,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_4kuhd"
						}
					},
					{
						TitleType.Xbox360,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_xbox360",
							AlternateNamesResource = "title_type_alternate_names_xbox360"
						}
					},
					{
						TitleType.PS2,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_ps2"
						}
					},
					{
						TitleType.PS3,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_ps3"
						}
					},
					{
						TitleType.PSP,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_psp"
						}
					},
					{
						TitleType.Wii,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_wii"
						}
					},
					{
						TitleType.DS,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_ds"
						}
					},
					{
						TitleType.PC,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_pc"
						}
					},
					{
						TitleType.WiiU,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_wiiu"
						}
					},
					{
						TitleType.PS4,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_ps4"
						}
					},
					{
						TitleType.XboxOne,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_xboxone",
							AlternateNamesResource = "title_type_alternate_names_xboxone"
						}
					},
					{
						TitleType.DigitalCode,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_digital_code"
						}
					},
					{
						TitleType.NintendoSwitch,
						new TitleTypeProperty
						{
							NameResource = "title_type_name_nintendo_switch",
							AlternateNamesResource = "title_type_alternate_names_nintendo_switch"
						}
					}
				};
			}
			return _titleTypeProperties;
		}
	}
}
