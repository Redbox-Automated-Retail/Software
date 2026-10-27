using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ShoppingCartUpdateADAView")]
public partial class ShoppingCartUpdateADAViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private ShoppingCartUpdateADAViewModel Model => base.DataContext as ShoppingCartUpdateADAViewModel;

	public ShoppingCartUpdateADAViewUserControl()
	{
		InitializeComponent();
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOkButtonPress();
	}

	private void AddMovieCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessAddMovieButtonPress();
	}

	private void PopulateDisplayProductModels()
	{
		List<DisplayProductWithCommandsUserControl> list = new List<DisplayProductWithCommandsUserControl>();
		foreach (object child in ShoppingCartStackPanel.Children)
		{
			if (child is DisplayProductWithCommandsUserControl)
			{
				list.Add(child as DisplayProductWithCommandsUserControl);
			}
		}
		foreach (DisplayProductWithCommandsUserControl item in list)
		{
			ShoppingCartStackPanel.Children.Remove(item);
		}
		if (Model?.DisplayProductModels == null)
		{
			return;
		}
		foreach (DisplayProductModel displayProductModel in Model.DisplayProductModels)
		{
			DisplayProductWithCommandsUserControl element = new DisplayProductWithCommandsUserControl
			{
				DataContext = displayProductModel,
				Width = 135.0,
				Height = 166.3,
				Margin = new Thickness(15.0, 0.0, 5.0, 0.0)
			};
			ShoppingCartStackPanel.Children.Insert(Model.DisplayProductModels.IndexOf(displayProductModel), element);
		}
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		PopulateDisplayProductModels();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}

	private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		DisplayProductModel displayProductModel = (e.OriginalSource as FrameworkElement)?.DataContext as DisplayProductModel;
		if (Model != null)
		{
			Model.ProcessOnCancelBrowseItemModel(displayProductModel, e.Parameter);
		}
		HandleWPFHit();
	}
}
