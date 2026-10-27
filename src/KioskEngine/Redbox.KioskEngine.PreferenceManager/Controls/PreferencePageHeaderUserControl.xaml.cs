using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.Controls;

public partial class PreferencePageHeaderUserControl : UserControl, IComponentConnector
{
	private IPreferencePageHost _preferencePageHost;

	public static readonly DependencyProperty HeaderBackgroundColorProperty = DependencyProperty.Register("HeaderBackgroundColor", typeof(Brush), typeof(PreferencePageHeaderUserControl), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty HeaderTitleProperty = DependencyProperty.Register("HeaderTitle", typeof(string), typeof(PreferencePageHeaderUserControl), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty RefreshButtonVisibiltyProperty = DependencyProperty.Register("RefreshButtonVisibilty", typeof(Visibility), typeof(PreferencePageHeaderUserControl), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SaveButtonVisibiltyProperty = DependencyProperty.Register("SaveButtonVisibilty", typeof(Visibility), typeof(PreferencePageHeaderUserControl), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PopOutButtonVisibiltyProperty = DependencyProperty.Register("PopOutButtonVisibilty", typeof(Visibility), typeof(PreferencePageHeaderUserControl), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public Brush HeaderBackgroundColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(HeaderBackgroundColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HeaderBackgroundColorProperty, value);
			});
		}
	}

	public string HeaderTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(HeaderTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HeaderTitleProperty, value);
			});
		}
	}

	public Visibility RefreshButtonVisibilty
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RefreshButtonVisibiltyProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RefreshButtonVisibiltyProperty, value);
			});
		}
	}

	public Visibility PopOutButtonVisibilty
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PopOutButtonVisibiltyProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PopOutButtonVisibiltyProperty, value);
			});
		}
	}

	public Visibility SaveButtonVisibilty
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SaveButtonVisibiltyProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SaveButtonVisibiltyProperty, value);
			});
		}
	}

	public PreferencePageHeaderUserControl()
	{
		InitializeComponent();
	}

	public void SetStatusText(string value)
	{
		StatusText.Text = value;
	}

	public void SetStatusText<T>(APIResponse<T> response)
	{
		if (response == null)
		{
			return;
		}
		string arg = null;
		if (response.Errors != null && response.Errors.Count > 0)
		{
			arg = "Errors: " + string.Join(", ", response.Errors?.Select((Error x) => "Code: " + x.Code + ", Message: " + x.Message).ToArray());
		}
		StatusText.Text = ((response != null && response.IsSuccessStatusCode) ? string.Empty : $"Status code: {(int)response.StatusCode} {response.StatusCode} {arg}");
	}

	private string GetHeaderTitleFromParent()
	{
		string result = null;
		for (FrameworkElement frameworkElement = base.Parent as FrameworkElement; frameworkElement != null; frameworkElement = frameworkElement.Parent as FrameworkElement)
		{
			PreferencePageAttribute preferencePageAttribute = (PreferencePageAttribute)Attribute.GetCustomAttribute(frameworkElement.GetType(), typeof(PreferencePageAttribute));
			if (preferencePageAttribute != null)
			{
				result = preferencePageAttribute.Name;
				break;
			}
		}
		return result;
	}

	private IPreferencePageHost GetPreferencePageHostFromParent()
	{
		IPreferencePageHost preferencePageHost = null;
		FrameworkElement frameworkElement = base.Parent as FrameworkElement;
		while (frameworkElement != null)
		{
			if ((PreferencePageAttribute)Attribute.GetCustomAttribute(frameworkElement.GetType(), typeof(PreferencePageAttribute)) != null)
			{
				preferencePageHost = frameworkElement as IPreferencePageHost;
				if (preferencePageHost != null)
				{
					break;
				}
			}
			else
			{
				frameworkElement = frameworkElement.Parent as FrameworkElement;
			}
		}
		return preferencePageHost;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		HeaderTitle = GetHeaderTitleFromParent();
		_preferencePageHost = GetPreferencePageHostFromParent();
	}

	private void RefreshButton_Click(object sender, RoutedEventArgs e)
	{
		_preferencePageHost?.LoadValues(null);
	}

	private void PopOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PopOutButtonVisibilty = Visibility.Collapsed;
		PreferenceManagerCommands.PopOutCommand.Execute(sender, base.Parent as IInputElement);
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e)
	{
		_preferencePageHost?.SaveValues(null);
	}
}
