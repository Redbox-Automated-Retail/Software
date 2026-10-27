using System.Windows.Media.Imaging;

namespace Redbox.Rental.Model;

public class ComingSoonTitle : IComingSoonTitle
{
	public string ReleaseDate { get; set; }

	public string TitleName { get; set; }

	public BitmapImage Image { get; set; }
}
