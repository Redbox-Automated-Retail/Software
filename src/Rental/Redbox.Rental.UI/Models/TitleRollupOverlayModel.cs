using System;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class TitleRollupOverlayModel : DependencyObject
{
	public static readonly DependencyProperty _4kUhdButtonTextProperty = DependencyProperty.Register("_4kUhdButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty _4kUhdButtonLine2TextProperty = DependencyProperty.Register("_4kUhdButtonLine2Text", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty _4kUhdButtonVisibilityProperty = DependencyProperty.Register("_4kUhdButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty _4kUhdButtonEnabledProperty = DependencyProperty.Register("_4kUhdButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BlurayButtonTextProperty = DependencyProperty.Register("BlurayButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonLine2TextProperty = DependencyProperty.Register("BlurayButtonLine2Text", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BlurayButtonVisibilityProperty = DependencyProperty.Register("BlurayButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BlurayButtonEnabledProperty = DependencyProperty.Register("BlurayButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty DVDButtonTextProperty = DependencyProperty.Register("DVDButtonText", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonLine2TextProperty = DependencyProperty.Register("DVDButtonLine2Text", typeof(string), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DVDButtonVisibilityProperty = DependencyProperty.Register("DVDButtonVisibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty DVDButtonEnabledProperty = DependencyProperty.Register("DVDButtonEnabled", typeof(bool), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty Grid4kUhdButtonGridRowHeightProperty = DependencyProperty.Register("Grid4kUhdButtonGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty Grid4kUhdButtonMarginGridRowHeightProperty = DependencyProperty.Register("Grid4kUhdButtonMarginGridRowHeight", typeof(GridLength), typeof(TitleRollupOverlayModel), new FrameworkPropertyMetadata(null));

	private TimeSpan _timeout = TimeSpan.FromSeconds(10.0);

	public string _4kUhdButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(_4kUhdButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(_4kUhdButtonTextProperty, value);
			});
		}
	}

	public string _4kUhdButtonLine2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(_4kUhdButtonLine2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(_4kUhdButtonLine2TextProperty, value);
			});
		}
	}

	public Visibility _4kUhdButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(_4kUhdButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(_4kUhdButtonVisibilityProperty, value);
			});
		}
	}

	public string BlurayButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BlurayButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonTextProperty, value);
			});
		}
	}

	public string BlurayButtonLine2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BlurayButtonLine2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonLine2TextProperty, value);
			});
		}
	}

	public Visibility BlurayButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BlurayButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonVisibilityProperty, value);
			});
		}
	}

	public string DVDButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DVDButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonTextProperty, value);
			});
		}
	}

	public string DVDButtonLine2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DVDButtonLine2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonLine2TextProperty, value);
			});
		}
	}

	public Visibility DVDButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DVDButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(VisibilityProperty, value);
			});
		}
	}

	public bool DVDButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(DVDButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DVDButtonEnabledProperty, value);
			});
		}
	}

	public bool BlurayButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(BlurayButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BlurayButtonEnabledProperty, value);
			});
		}
	}

	public bool _4kUhdButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(_4kUhdButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(_4kUhdButtonEnabledProperty, value);
			});
		}
	}

	public GridLength Grid4kUhdButtonGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(Grid4kUhdButtonGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Grid4kUhdButtonGridRowHeightProperty, value);
			});
		}
	}

	public GridLength Grid4kUhdButtonMarginGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (GridLength)GetValue(Grid4kUhdButtonMarginGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Grid4kUhdButtonMarginGridRowHeightProperty, value);
			});
		}
	}

	public IBrowseItemModel BrowseItemModel { get; set; }

	public TimeSpan Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
		}
	}

	public event BrowseItemModelEvent OnTitleRollupOverlayCancel;

	public event BrowseItemModelEvent OnTitleRollupOverlayTimeout;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddDVD;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddBluray;

	public event BrowseItemModelEvent OnTitleRollupOverlayAdd4kUhd;

	public event VisibilityChanged OnTitleRollupOverlayVisibilityChange;

	public void ProcessTitleRollupVisibilityChange(Visibility newVisibility)
	{
		if (OnTitleRollupOverlayVisibilityChange != null)
		{
			OnTitleRollupOverlayVisibilityChange(newVisibility);
		}
	}

	public void ProcessTitleRollupOverlayAdd4kUhd(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAdd4kUhd(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddBluray(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAddBluray(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddDVD != null)
		{
			OnTitleRollupOverlayAddDVD(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayCancel != null)
		{
			OnTitleRollupOverlayCancel(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayTimeout != null)
		{
			OnTitleRollupOverlayTimeout(browseItemModel, parameter);
		}
	}
}
