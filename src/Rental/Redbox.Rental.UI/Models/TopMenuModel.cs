using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class TopMenuModel : DependencyObject
{
	public static readonly DependencyProperty MenuBackgroundStyleProperty = DependencyProperty.Register("MenuBackgroundStyle", typeof(Style), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MenuTextStyleProperty = DependencyProperty.Register("MenuTextStyle", typeof(Style), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MenuBackgroundTextProperty = DependencyProperty.Register("MenuBackgroundText", typeof(string), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AZMenuBackgroundVisibilityProperty = DependencyProperty.Register("AZMenuBackgroundVisibility", typeof(Visibility), typeof(TopMenuModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty AZMenuBorderMarginProperty = DependencyProperty.Register("AZMenuBorderMargin", typeof(Thickness), typeof(TopMenuModel), new FrameworkPropertyMetadata(new Thickness(40.0, 24.0, 161.0, 0.0)));

	public static readonly DependencyProperty RedboxPlusLogoVisiblityProperty = DependencyProperty.Register("RedboxPlusLogoVisiblity", typeof(Visibility), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AZMenuTextProperty = DependencyProperty.Register("AZMenuText", typeof(string), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AZFiltersMessageTextProperty = DependencyProperty.Register("AZFiltersMessageText", typeof(string), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AZButtonVisibilityProperty = DependencyProperty.Register("AZButtonVisibility", typeof(Visibility), typeof(TopMenuModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty AZButtonIconVisibilityProperty = DependencyProperty.Register("AZButtonIconVisibility", typeof(Visibility), typeof(TopMenuModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AZButtonExitIconVisibilityProperty = DependencyProperty.Register("AZButtonExitIconVisibility", typeof(Visibility), typeof(TopMenuModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty AZButtonTextProperty = DependencyProperty.Register("AZButtonText", typeof(string), typeof(TopMenuModel), new FrameworkPropertyMetadata(null));

	private List<BrowseMenuButton> _menuButtons = new List<BrowseMenuButton>();

	public List<BrowseMenuButton> MenuButtons
	{
		get
		{
			return _menuButtons;
		}
		set
		{
			_menuButtons = value;
		}
	}

	public List<CheckMarkButtonModel> CheckMarkButtonModels { get; set; }

	public Visibility MenuBackgroundVisibility { get; set; }

	public string MenuBackgroundText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MenuBackgroundTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MenuBackgroundTextProperty, value);
			});
		}
	}

	public Style MenuTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MenuTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MenuTextStyleProperty, value);
			});
		}
	}

	public Style MenuBackgroundStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MenuBackgroundStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MenuBackgroundStyleProperty, value);
			});
		}
	}

	public Visibility AZMenuBackgroundVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AZMenuBackgroundVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZMenuBackgroundVisibilityProperty, value);
			});
		}
	}

	public Thickness AZMenuBorderMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(AZMenuBorderMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZMenuBorderMarginProperty, value);
			});
		}
	}

	public Visibility RedboxPlusLogoVisiblity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusLogoVisiblityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RedboxPlusLogoVisiblityProperty, value);
			});
		}
	}

	public string AZMenuText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AZMenuTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZMenuTextProperty, value);
			});
		}
	}

	public string AZFiltersMessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AZFiltersMessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZFiltersMessageTextProperty, value);
			});
		}
	}

	public Visibility AZButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AZButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility AZButtonIconVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AZButtonIconVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZButtonIconVisibilityProperty, value);
			});
		}
	}

	public Visibility AZButtonExitIconVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AZButtonExitIconVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZButtonExitIconVisibilityProperty, value);
			});
		}
	}

	public string AZButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AZButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AZButtonTextProperty, value);
			});
		}
	}

	public event Action<BrowseMenuButton> OnBrowseMenuButtonClicked;

	public event Action OnAZButtonClicked;

	public void ProcessOnBrowseMenuButtonClicked(BrowseMenuButton browseMenuButton)
	{
		if (OnBrowseMenuButtonClicked != null)
		{
			OnBrowseMenuButtonClicked(browseMenuButton);
		}
	}

	public void ProcessOnAZButtonClicked()
	{
		if (OnAZButtonClicked != null)
		{
			OnAZButtonClicked();
		}
	}
}
