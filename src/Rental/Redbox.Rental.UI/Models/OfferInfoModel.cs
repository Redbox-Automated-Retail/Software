using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.UI.Models;

public class OfferInfoModel
{
	public Visibility OneUpVisibility { get; set; }

	public Visibility TwoUpVisibility { get; set; }

	public Visibility ThreeUpVisibility { get; set; }

	public Visibility CancelButtonVisibility { get; set; }

	public Visibility BannerVisibility { get; set; }

	public Visibility ImageVisibility { get; set; }

	public Visibility MessageTextVisibility { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public CustomerOffer CustomerOffer { get; set; }

	public ObservableCollection<StyledTextModel> StyledTextLines { get; set; }

	public string ContinueButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public string BannerText { get; set; }

	public string MessageText { get; set; }

	public Style MessageTextStyle { get; set; }

	public int ControlWidth { get; set; }

	public int ControlHeight { get; set; }

	public int TopMargin { get; set; }

	public HorizontalAlignment TextHorizontalAlignment { get; set; }

	public BitmapImage Image { get; set; }

	public Visibility PerksIconsVisibility { get; set; }

	public string PerksFreeRentalText { get; set; }

	public string PerksDealsText { get; set; }

	public string PerksBdayGiftText { get; set; }
}
