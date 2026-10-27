using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class RedboxPlusOfferDetailViewModel
{
	public IRedboxPlusSubscriptionProduct RedboxPlusSubscriptionProduct { get; set; }

	public DynamicRoutedCommand BackButtonCommand { get; set; }

	public DynamicRoutedCommand TermsButtonCommand { get; set; }

	public DynamicRoutedCommand AcceptButtonCommand { get; set; }

	public Thickness MainStackPanelMargin { get; set; }

	public string TitleText { get; set; }

	public string SubtitleText { get; set; }

	public string Benefit1HeaderText { get; set; }

	public string Benefit1DetailText { get; set; }

	public string Benefit2HeaderText { get; set; }

	public string Benefit2DetailText { get; set; }

	public Visibility Benefit3Visibility { get; set; }

	public string Benefit3HeaderText { get; set; }

	public string Benefit3DetailText { get; set; }

	public string AcceptButtonText { get; set; }

	public string AcceptButtonLegalText { get; set; }

	public string BackButtonText { get; set; }

	public Visibility BackButtonVisibility { get; set; }

	public string TermsButtonText { get; set; }

	public string LegalText { get; set; }

	public Visibility TopBoxArtDisplayVisibility { get; set; }

	public Visibility BottomBoxArtDisplayVisibility { get; set; }

	public string BoxArtHeaderText { get; set; }

	public BitmapImage BoxArt1 { get; set; }

	public BitmapImage BoxArt2 { get; set; }

	public BitmapImage BoxArt3 { get; set; }

	public BitmapImage BoxArt4 { get; set; }

	public BitmapImage BoxArt5 { get; set; }

	public BitmapImage BoxArt6 { get; set; }

	public BitmapImage BoxArt7 { get; set; }

	public Visibility BoxArt1Visibility { get; set; }

	public Visibility BoxArt2Visibility { get; set; }

	public Visibility BoxArt3Visibility { get; set; }

	public Visibility BoxArt4Visibility { get; set; }

	public Visibility BoxArt5Visibility { get; set; }

	public Visibility BoxArt6Visibility { get; set; }

	public Visibility BoxArt7Visibility { get; set; }
}
