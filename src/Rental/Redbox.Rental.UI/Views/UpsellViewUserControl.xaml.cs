using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "UpsellView")]
public partial class UpsellViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private UpsellViewModel Model => base.DataContext as UpsellViewModel;

	public UpsellViewUserControl()
	{
		InitializeComponent();
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnBackButtonClicked();
	}

	private void NoThanksCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnNoThanksButtonClicked();
	}

	private void UpgradeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnUpgradeButtonClicked();
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		PopulateDisplayProductModels();
	}

	private void PopulateDisplayProductModels()
	{
		List<DisplayProductUpsellUserControl> list = new List<DisplayProductUpsellUserControl>();
		foreach (object child in UpsellStackPanel.Children)
		{
			if (child is DisplayProductUpsellUserControl)
			{
				list.Add(child as DisplayProductUpsellUserControl);
			}
		}
		foreach (DisplayProductUpsellUserControl item in list)
		{
			UpsellStackPanel.Children.Remove(item);
		}
		if (Model?.DisplayProductModels == null)
		{
			return;
		}
		foreach (DisplayUpsellProductModel displayProductModel in Model.DisplayProductModels)
		{
			DisplayProductUpsellUserControl element = new DisplayProductUpsellUserControl
			{
				DataContext = displayProductModel,
				Width = 167.0,
				Margin = new Thickness(-3.0, 0.0, -3.0, 0.0)
			};
			UpsellStackPanel.Children.Insert(Model.DisplayProductModels.IndexOf(displayProductModel), element);
		}
	}

	private void ToggleUpsellCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
		while (parent != null && !(parent is UserControl))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		if (parent is DisplayProductUpsellUserControl displayProductUpsellUserControl && displayProductUpsellUserControl.DataContext is DisplayUpsellProductModel displayUpsellProductModel)
		{
			Model.ProcessOnCheckBoxClick(displayUpsellProductModel);
		}
	}

	private void UpgradeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Model?.ProcessOnCanExecuteUpgradeButton() ?? false;
	}
}
