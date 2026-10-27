using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class MarketingScreenSaverModel : BaseModel<MarketingScreenSaverModel>
{
	public Action OnLeaveLastImage;

	public static readonly DependencyProperty IsAnimationOnProperty = BaseModel<MarketingScreenSaverModel>.CreateDependencyProperty("IsAnimationOn", typeof(bool), false);

	public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register("GridHeight", typeof(int), typeof(MarketingScreenSaverModel), new PropertyMetadata(768));

	public ObservableCollection<ClickableImageModel> ImageModels { get; set; }

	public DynamicRoutedCommand BrowseCommand { get; set; }

	public string StartButtonText { get; set; }

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}

	public int GridHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(GridHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GridHeightProperty, value);
			});
		}
	}
}
