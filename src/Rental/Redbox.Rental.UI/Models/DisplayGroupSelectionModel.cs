using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class DisplayGroupSelectionModel : DependencyObject
{
	public Action<DisplayGroupSelectionItemModel> OnGroupSelectionItemClicked;

	public static readonly DependencyProperty GroupSelectionItemsProperty = DependencyProperty.Register("GroupSelectionItems", typeof(List<DisplayGroupSelectionItemModel>), typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BrowseGroupSelectionVisibilityProperty = DependencyProperty.Register("BrowseGroupSelectionVisibility", typeof(Visibility), typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BrowseGroupSelectionMarginProperty = DependencyProperty.Register("BrowseGroupSelectionMargin", typeof(Thickness), typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(null));

	public List<DisplayGroupSelectionItemModel> GroupSelectionItems
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayGroupSelectionItemModel>)GetValue(GroupSelectionItemsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GroupSelectionItemsProperty, value);
			});
		}
	}

	public Visibility BrowseGroupSelectionVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BrowseGroupSelectionVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseGroupSelectionVisibilityProperty, value);
			});
		}
	}

	public Thickness BrowseGroupSelectionMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(BrowseGroupSelectionMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseGroupSelectionMarginProperty, value);
			});
		}
	}

	public DisplayGroupSelectionModel()
	{
		GroupSelectionItems = new List<DisplayGroupSelectionItemModel>();
	}

	public void ProcessOnGroupSelectionItemClicked(DisplayGroupSelectionItemModel displayGroupSelectionItemModel)
	{
		if (OnGroupSelectionItemClicked != null)
		{
			OnGroupSelectionItemClicked(displayGroupSelectionItemModel);
			Commands.ResetIdleTimerCommand.Execute(null, null);
		}
	}
}
