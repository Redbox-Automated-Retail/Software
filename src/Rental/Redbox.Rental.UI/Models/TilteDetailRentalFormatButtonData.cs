using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class TilteDetailRentalFormatButtonData
{
	public string Line1Text { get; set; }

	public string Line2Text { get; set; }

	public TitleType Format { get; set; }

	public bool IsEnabled { get; set; }

	public TilteDetailRentalFormatButtonData(string text, TitleType format, string line2Text = null, bool isEnabled = true)
	{
		Line1Text = text;
		Line2Text = line2Text;
		Format = format;
		IsEnabled = isEnabled;
	}
}
