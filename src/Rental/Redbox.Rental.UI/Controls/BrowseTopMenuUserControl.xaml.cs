using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseTopMenuUserControl : RentalUserControl, IWPFActor, IComponentConnector
{
	private TopMenuModel BrowseTopMenuModel => base.DataContext as TopMenuModel;

	public BrowseTopMenuUserControl()
	{
		InitializeComponent();
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

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(e.NewValue is TopMenuModel topMenuModel))
		{
			return;
		}
		NonFixedButtonContainer.Children.Clear();
		NonFixedButtonContainer.ColumnDefinitions.Clear();
		FixedButtonContainer.Children.Clear();
		if (topMenuModel.MenuButtons == null)
		{
			return;
		}
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (BrowseMenuButton menuButton in topMenuModel.MenuButtons)
		{
			BrowseProductFamilyMenuButton browseProductFamilyMenuButton = menuButton as BrowseProductFamilyMenuButton;
			RoundedButton roundedButton = new RoundedButton
			{
				DataContext = menuButton,
				Style = GetStyle(this, "classic_menu_button_style"),
				CornerRadius = 28.0,
				Width = menuButton.Width,
				Height = menuButton.Height,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(5.0, 0.0, 5.0, 0.0)
			};
			roundedButton.Click += MenuButton_Click;
			roundedButton.SetBinding(UIElement.IsEnabledProperty, new Binding
			{
				Path = new PropertyPath("IsButtonEnabled"),
				Source = menuButton
			});
			TextBlock textBlock = new TextBlock
			{
				Text = menuButton.Text,
				Style = GetStyle(this, menuButton.TextStyleName),
				TextWrapping = TextWrapping.Wrap,
				TextAlignment = TextAlignment.Center
			};
			textBlock.SetBinding(TextBlock.TextProperty, new Binding
			{
				Path = new PropertyPath("Text"),
				Source = menuButton
			});
			StackPanel stackPanel = null;
			if (browseProductFamilyMenuButton != null)
			{
				bool flag = browseProductFamilyMenuButton.ArrowDirection == ArrowDirection.Right;
				stackPanel = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				UserControl userControl = new UserControl
				{
					Style = GetStyle(this, flag ? "right_arrow" : "left_arrow"),
					Margin = (flag ? new Thickness(10.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 0.0, 10.0, 0.0)),
					VerticalAlignment = VerticalAlignment.Center
				};
				UIElement element = (flag ? ((UIElement)textBlock) : ((UIElement)userControl));
				UIElement element2 = (flag ? ((UIElement)userControl) : ((UIElement)textBlock));
				stackPanel.Children.Add(element);
				stackPanel.Children.Add(element2);
			}
			if (menuButton is SignOutMenuButton)
			{
				roundedButton.Style = GetStyle(this, "transparent_button_style");
				textBlock.Foreground = new SolidColorBrush
				{
					Color = (Color)FindResource("RubineRed")
				};
				textBlock.TextDecorations = TextDecorations.Underline;
			}
			if (menuButton.FixedPosition)
			{
				roundedButton.Margin = new Thickness(menuButton.X, menuButton.Y, 0.0, 0.0);
			}
			roundedButton.Content = ((stackPanel != null) ? ((object)stackPanel) : ((object)textBlock));
			if (menuButton.FixedPosition)
			{
				FixedButtonContainer.Children.Add(roundedButton);
			}
			else
			{
				NonFixedButtonContainer.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = new GridLength(menuButton.Width + 15)
				});
				StackPanel stackPanel2 = new StackPanel
				{
					Orientation = Orientation.Horizontal
				};
				stackPanel2.Children.Add(roundedButton);
				if (menuButton.IsSelected)
				{
					roundedButton.Style = GetStyle(this, "classic_menu_button_style_disabled");
					stackPanel2.Children.Add(new Image
					{
						Source = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/purple-white-checkmark-lg.png")),
						Width = 22.0,
						Height = 22.0,
						Margin = new Thickness(-19.0, -35.0, 0.0, 0.0)
					});
				}
				int value = topMenuModel.MenuButtons.IndexOf(menuButton);
				Grid.SetColumn(stackPanel2, value);
				Grid.SetRow(stackPanel2, 0);
				NonFixedButtonContainer.Children.Add(stackPanel2);
			}
			theme?.SetStyle(roundedButton);
		}
	}

	private void MenuButton_Click(object sender, RoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				if (e.OriginalSource is RoundedButton { DataContext: BrowseMenuButton dataContext })
				{
					BrowseTopMenuModel?.ProcessOnBrowseMenuButtonClicked(dataContext);
					Commands.ResetIdleTimerCommand.Execute(null, this);
				}
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void AZButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				BrowseTopMenuModel?.ProcessOnAZButtonClicked();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}
}
