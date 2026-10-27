using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Models;

public class MaintenanceViewModel
{
	public BitmapImage BackgroundImage { get; set; }

	public string Title { get; set; }

	public string TextLine1 { get; set; }

	public string TextLine2 { get; set; }

	public string TextLine3 { get; set; }

	public string Footnote { get; set; }

	public string StoreNumberText { get; set; }

	public string ZipCodeText { get; set; }

	public string ErrorCodeText { get; set; }

	public int MinutesInMaintenance { get; set; }

	public ITimer MaintenanceTimer { get; set; }
}
