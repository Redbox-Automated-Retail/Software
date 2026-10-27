using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseRedboxPlusTooltipUserControl : UserControl, IComponentConnector
{
	private BrowseRedboxPlusTooltipModel BrowseRedboxPlusTooltipModel => base.DataContext as BrowseRedboxPlusTooltipModel;

	public BrowseRedboxPlusTooltipUserControl()
	{
		InitializeComponent();
	}

	private void BrowseRedboxPlusTooltipCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseRedboxPlusTooltipModel?.ProcessOnBrowseRedboxPlusTooltipUserControlClicked(BrowseRedboxPlusTooltipModel);
	}
}
