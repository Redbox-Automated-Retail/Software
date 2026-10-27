using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models;

public class VendViewModel : BaseModel<VendViewModel>
{
	public VendViewType VendType { get; set; }

	public bool IsSignedIn { get; set; }

	public bool IsPickup { get; set; }

	public string TitleText1 { get; set; }

	public string MessageText1 { get; set; }

	public string MessageText2 { get; set; }

	public string MessageText3 { get; set; }

	public BitmapImage BannerImage { get; set; }

	public IAdImpression AdImpression { get; set; }
}
