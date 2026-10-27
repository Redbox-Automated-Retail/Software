using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class CheckMarkButtonUserControl : UserControl, IComponentConnector
{
	public static readonly RoutedCommand Command = new RoutedCommand();

	private CheckMarkButtonModel CheckMarkButtonModel => base.DataContext as CheckMarkButtonModel;

	public CheckMarkButtonUserControl()
	{
		InitializeComponent();
	}

	private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CheckMarkButtonModel checkMarkButtonModel = (e.OriginalSource as RoundedButton)?.DataContext as CheckMarkButtonModel;
		CheckMarkButtonModel?.ProcessOnClicked(checkMarkButtonModel);
	}
}
