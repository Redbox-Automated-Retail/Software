using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseMessagePopupUserControl : UserControl, IComponentConnector
{
	private BrowseMessagePopupModel BrowseMessagePopupModel => base.DataContext as BrowseMessagePopupModel;

	public BrowseMessagePopupUserControl()
	{
		InitializeComponent();
	}

	private void BrowseMessagePopupInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseMessagePopupModel?.ProcessOnBrowseMessagePopupInfoClicked();
	}

	private void BrowseMessagePopupCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseMessagePopupModel?.ProcessOnBrowseMessagePopupCloseClicked();
	}
}
