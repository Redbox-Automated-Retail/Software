using System.Windows;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class PerksOfferActiveListItem : BaseUserControl, IComponentConnector
{
	private static DependencyProperty DetailsButtonCommandProperty = DependencyProperty.Register("DetailsButtonCommand", typeof(DynamicRoutedCommand), typeof(PerksOfferActiveListItem), new PropertyMetadata(null));

	public DynamicRoutedCommand DetailsButtonCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(DetailsButtonCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DetailsButtonCommandProperty, value);
			});
		}
	}

	public PerksOfferActiveListItem()
	{
		InitializeComponent();
	}
}
