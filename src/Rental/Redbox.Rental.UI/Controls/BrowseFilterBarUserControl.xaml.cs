using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseFilterBarUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private FilterBarModel FilterBarModel => base.DataContext as FilterBarModel;

	private static bool IsADAMode => ServiceLocator.Instance.GetService<IApplicationState>()?.IsADAAccessible ?? false;

	public event WPFHitHandler OnWPFHit;

	public BrowseFilterBarUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(startover_button);
		obj?.SetStyle(BackButton);
	}

	private void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
	}

	private void StartOverCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnStartOverButtonClicked();
		HandleWPFHit();
	}

	private void StartOverCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(e.NewValue is FilterBarModel filterBarModel))
		{
			return;
		}
		AZFilterButtonContainer.Children.Clear();
		FilterButtonContainer.Children.Clear();
		if (filterBarModel.Filters == null)
		{
			return;
		}
		ServiceLocator.Instance.GetService<IConfiguration>();
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		int num = filterBarModel.Filters.Count((BrowseFilter browseFilter) => browseFilter.IsVisible) + ((filterBarModel.FormatFilterButtonVisibility == Visibility.Visible) ? 1 : 0) + ((filterBarModel.PriceRangeFilterButtonVisibility == Visibility.Visible) ? 1 : 0);
		FilterButtonPanel.Margin = new Thickness(0.0, 0.0, (num <= 4) ? 178 : 0, 0.0);
		foreach (BrowseFilter filter in filterBarModel.Filters)
		{
			RoundedButton roundedButton = new RoundedButton
			{
				DataContext = filter,
				Style = GetStyle(this, "purple_with_purple_border_button_style"),
				CornerRadius = 16.0,
				MinWidth = filter.Width,
				Height = filter.Height,
				Margin = filterBarModel.FilterMargin,
				Command = Commands.BrowseFilterCommand
			};
			roundedButton.SetBinding(UIElement.IsEnabledProperty, new Binding
			{
				Path = new PropertyPath("IsButtonEnabled"),
				Source = filter
			});
			roundedButton.SetBinding(UIElement.VisibilityProperty, new Binding
			{
				Path = new PropertyPath("ButtonVisibility"),
				Source = filter
			});
			TextBlock textBlock = new TextBlock
			{
				Text = filter.Text.ToUpper(),
				Style = GetStyle(this, "font_montserrat_extrabold_14_correct"),
				Margin = new Thickness(10.0, 0.0, 10.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center
			};
			textBlock.SetBinding(TextBlock.TextProperty, new Binding
			{
				Path = new PropertyPath("Text"),
				Source = filter
			});
			if (filter.ButtonType == BrowseFilterType.AtoZ)
			{
				StackPanel stackPanel = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				if (!IsADAMode)
				{
					stackPanel.Children.Add(new Image
					{
						Source = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/magnifying-glass-icon.png")),
						Width = 17.0,
						Height = 17.0
					});
				}
				stackPanel.Children.Add(textBlock);
				roundedButton.Content = stackPanel;
				StackPanel stackPanel2 = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				stackPanel2.Children.Add(roundedButton);
				if (filter.IsSelected && filter.IsVisible)
				{
					roundedButton.Style = GetStyle(this, "purple_with_purple_border_button_style_disabled");
					stackPanel2.Children.Add(new Image
					{
						Source = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/purple-white-checkmark-sm.png")),
						Width = 19.0,
						Height = 18.0,
						Margin = new Thickness(-11.0, -15.0, -9.0, 0.0)
					});
				}
				AZFilterButtonContainer.Children.Add(stackPanel2);
			}
			else
			{
				roundedButton.Content = textBlock;
				StackPanel stackPanel3 = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				stackPanel3.Children.Add(roundedButton);
				if (filter.ButtonType == BrowseFilterType.Deals && filter.IsVisible && !filter.IsSelected)
				{
					stackPanel3.Children.Add(new Image
					{
						Source = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/purple-white-new-sm.png")),
						Width = 36.0,
						Height = 20.0,
						Margin = new Thickness(-11.0, -15.0, -9.0, 0.0)
					});
				}
				else if (filter.IsVisible && filter.IsSelected)
				{
					roundedButton.Style = GetStyle(this, "purple_with_purple_border_button_style_disabled");
					stackPanel3.Children.Add(new Image
					{
						Source = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/purple-white-checkmark-sm.png")),
						Width = 19.0,
						Height = 18.0,
						Margin = new Thickness(-11.0, -15.0, -9.0, 0.0)
					});
				}
				FilterButtonContainer.Children.Add(stackPanel3);
			}
			theme?.SetStyle(roundedButton);
		}
	}

	private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		object obj = frameworkElement.FindResource(styleName);
		if (obj != null)
		{
			result = obj as Style;
		}
		return result;
	}

	private void BrowseFilterCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.OriginalSource is RoundedButton { DataContext: BrowseFilter dataContext })
		{
			FilterBarModel?.ProcessOnBrowseFilterButtonClicked(dataContext);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnBackButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void HomeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnHomeButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void FormatFilterPopupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnFormatFilterButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void ForSaleFilterPopupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		FilterBarModel?.ProcessOnPriceRangeFilterButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}
}
