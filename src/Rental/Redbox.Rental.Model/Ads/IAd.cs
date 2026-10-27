using System.Windows.Media.Imaging;

namespace Redbox.Rental.Model.Ads;

public interface IAd
{
	string ImageName { get; set; }

	int? MinimumDisplay { get; set; }

	BitmapImage Image { get; }
}
