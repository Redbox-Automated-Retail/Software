using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayProductMoreLikeThisUserControl : TextToSpeechUserControl, IComponentConnector
{
	private DisplayProductMoreLikeThisModel DisplayProductMoreLikeThisModel => base.DataContext as DisplayProductMoreLikeThisModel;

	public DisplayProductMoreLikeThisUserControl()
	{
		InitializeComponent();
	}

	private void MoreLikeThis_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				IBrowseItemModel browseItemModel = (e.OriginalSource as DisplayProductMoreLikeThisUserControl)?.DataContext as IBrowseItemModel;
				DisplayProductMoreLikeThisModel?.ProcessMoreLikeThisSelected(browseItemModel);
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}
}
