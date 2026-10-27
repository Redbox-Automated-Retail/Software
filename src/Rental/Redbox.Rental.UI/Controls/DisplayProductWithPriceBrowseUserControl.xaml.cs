using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayProductWithPriceBrowseUserControl : UserControl, IComponentConnector
{
	private DisplayProductModel _displayProductModel;

	public DisplayProductWithPriceBrowseUserControl()
	{
		InitializeComponent();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayProductModel = base.DataContext as DisplayProductModel;
		MainGrid.Visibility = ((_displayProductModel == null) ? Visibility.Collapsed : Visibility.Visible);
	}
}
