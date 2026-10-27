using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusOfferSelectionView")]
public partial class RedboxPlusOfferSelectionUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	public RedboxPlusOfferSelectionUserControl()
	{
		InitializeComponent();
	}
}
