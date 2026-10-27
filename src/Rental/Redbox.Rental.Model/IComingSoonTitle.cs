using System.Windows.Media.Imaging;

namespace Redbox.Rental.Model;

public interface IComingSoonTitle
{
	string TitleName { get; set; }

	string ReleaseDate { get; set; }

	BitmapImage Image { get; set; }
}
