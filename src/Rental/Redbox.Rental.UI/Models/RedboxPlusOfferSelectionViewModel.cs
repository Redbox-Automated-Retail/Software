using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class RedboxPlusOfferSelectionViewModel
{
	public DynamicRoutedCommand BackButtonCommand { get; set; }

	public DynamicRoutedCommand TermsButtonCommand { get; set; }

	public DynamicRoutedCommand Tier1AcceptButtonCommand { get; set; }

	public DynamicRoutedCommand Tier2AcceptButtonCommand { get; set; }

	public DynamicRoutedCommand ScanQRCodeButtonCommand { get; set; }

	public string Tier1Header { get; set; }

	public string LegalText { get; set; }

	public string TermsButtonText { get; set; }

	public string BackButtonText { get; set; }

	public string Tier1TitleText { get; set; }

	public string Tier1SubtitleText { get; set; }

	public string Tier1Bullet1Text { get; set; }

	public string Tier1Bullet2Text { get; set; }

	public string Tier2TitleText { get; set; }

	public string Tier2SubtitleText { get; set; }

	public string Tier2Bullet1Text { get; set; }

	public string Tier2Bullet2Text { get; set; }

	public string Tier2Bullet3Text { get; set; }

	public string Tier1AcceptButtonText { get; set; }

	public string Tier2AcceptButtonText { get; set; }

	public string NotReadyYetText { get; set; }

	public string ScanQRCodeButtonText { get; set; }

	public BitmapImage BoxArt1 { get; set; }

	public BitmapImage BoxArt2 { get; set; }

	public BitmapImage BoxArt3 { get; set; }

	public BitmapImage BoxArt4 { get; set; }

	public BitmapImage BoxArt5 { get; set; }

	public BitmapImage BoxArt6 { get; set; }

	public BitmapImage BoxArt7 { get; set; }

	public BitmapImage BoxArt8 { get; set; }

	public BitmapImage BoxArt9 { get; set; }
}
