using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class TitleRollupFormatButtonProperties
{
	public TitleType TitleType { get; set; }

	public string ButtonText { get; set; }

	public string ButtonTextLine2 { get; set; }

	public Visibility Visibility { get; set; }

	public bool IsEnabled { get; set; }

	public string StyleName { get; set; }

	public void HideAndDisableButton()
	{
		Visibility = Visibility.Collapsed;
		IsEnabled = false;
	}
}
