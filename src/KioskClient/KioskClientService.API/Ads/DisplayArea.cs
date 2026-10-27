using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Ads;

public class DisplayArea
{
	public static readonly Size AdSize728x90 = new Size(728, 90);

	public static readonly Size AdSize1024x576 = new Size(1024, 576);

	public static readonly List<string> StandardSupportedMedia = new List<string> { "image/jpeg", "image/png" };

	public static List<AdLocation> AdLocations = (from AdLocation l in Enum.GetValues(typeof(AdLocation))
		where l != AdLocation.Unknown && l != AdLocation.Multiple
		select l).ToList();

	public AdLocation AdLocation { get; set; }

	public List<string> SupportedMedia { get; set; } = StandardSupportedMedia;

	public Size Size { get; set; }

	public static List<DisplayArea> GetAllDisplayAreas { get; } = new List<DisplayArea>
	{
		new DisplayArea(AdLocation.Welcome, AdSize728x90),
		new DisplayArea(AdLocation.TitleDetail, AdSize728x90),
		new DisplayArea(AdLocation.VSM, AdSize1024x576),
		new DisplayArea(AdLocation.ZipCode, AdSize728x90),
		new DisplayArea(AdLocation.Pickup, AdSize728x90),
		new DisplayArea(AdLocation.Return, AdSize728x90),
		new DisplayArea(AdLocation.EmailNotification, AdSize728x90),
		new DisplayArea(AdLocation.Vending, AdSize728x90),
		new DisplayArea(AdLocation.PleaseWait, AdSize728x90)
	};

	public DisplayArea(AdLocation adLocation, Size size)
	{
		AdLocation = adLocation;
		Size = size;
	}

	public VistarDisplayArea ToVistarDisplayArea()
	{
		return new VistarDisplayArea
		{
			id = AdLocation.ToString(),
			supported_media = SupportedMedia,
			allow_audio = false,
			width = Size.Width,
			height = Size.Height
		};
	}

	public static DisplayArea GetDisplayArea(AdLocation location)
	{
		return GetAllDisplayAreas.Where((DisplayArea da) => da.AdLocation == location).FirstOrDefault();
	}
}
