using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.ControlPanel.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "DisplayViewUserControl")]
public partial class DisplayViewUserControl : UserControl, IComponentConnector
{
	private DisplayHelper.DisplayDEVMODE? _initial;

	private DisplayHelper.DisplayDEVMODE? SelectedItem => resolution_combo.SelectedItem as DisplayHelper.DisplayDEVMODE?;

	public DisplayViewUserControl()
	{
		InitializeComponent();
		base.IsVisibleChanged += DisplayViewUserControl_IsVisibleChanged;
		resolution_combo.DropDownClosed += ResolutionDropDownClosed;
	}

	private void ButtonSaveClick(object sender, RoutedEventArgs e)
	{
		DisplayHelper.DisplayDEVMODE? selectedItem = SelectedItem;
		if (selectedItem.HasValue)
		{
			DisplayHelper.SetScreenResolution(selectedItem.Value);
		}
		CloseClick(sender, e);
	}

	private void CloseClick(object sender, RoutedEventArgs e)
	{
		base.Visibility = Visibility.Hidden;
	}

	private void PopulateResolutions()
	{
		resolution_combo.ItemsSource = DisplayHelper.GetSupportedScreenResolutions();
		resolution_combo.SelectedItem = (_initial = DisplayHelper.GetCurrentScreenResolution());
		button_save.IsEnabled = false;
	}

	private void DisplayViewUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			PopulateResolutions();
		}
	}

	private void ResolutionDropDownClosed(object sender, EventArgs e)
	{
		if (resolution_combo.SelectedItem != null)
		{
			button_save.IsEnabled = !SelectedItem.Equals(_initial);
		}
	}
}
