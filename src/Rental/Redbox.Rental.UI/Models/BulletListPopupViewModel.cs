using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class BulletListPopupViewModel : BaseModel<BulletListPopupViewModel>
{
	public static readonly DependencyProperty TitleProperty = BaseModel<BulletListPopupViewModel>.CreateDependencyProperty("Title", TYPES.STRING);

	public static readonly DependencyProperty BulletPointsProperty = BaseModel<BulletListPopupViewModel>.CreateDependencyProperty("BulletPoints", typeof(ObservableCollection<string>));

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public string CancelButtonText { get; set; }

	public string Title
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleProperty, value);
			});
		}
	}

	public ObservableCollection<string> BulletPoints
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<string>)GetValue(BulletPointsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BulletPointsProperty, value);
			});
		}
	}
}
