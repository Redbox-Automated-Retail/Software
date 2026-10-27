using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseGroupSelectionUserControl : UserControl, IComponentConnector
{
	private DisplayGroupSelectionModel _displayGroupSelectionModel;

	public BrowseGroupSelectionUserControl()
	{
		InitializeComponent();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayGroupSelectionModel = base.DataContext as DisplayGroupSelectionModel;
		if (_displayGroupSelectionModel == null || _displayGroupSelectionModel.GroupSelectionItems == null)
		{
			return;
		}
		GroupSelectionContainer.Children.Clear();
		GroupSelectionContainer.ColumnDefinitions.Clear();
		GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(3.0, GridUnitType.Star)
		});
		int num = 0;
		foreach (DisplayGroupSelectionItemModel groupSelectionItem in _displayGroupSelectionModel.GroupSelectionItems)
		{
			GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(10.0, GridUnitType.Star)
			});
			num++;
			UserControl userControl = new UserControl
			{
				DataContext = groupSelectionItem
			};
			GroupSelectionContainer.Children.Add(userControl);
			userControl.Style = GetStyle(this, "group_selection_item");
			userControl.InputBindings.Add(new InputBinding(Commands.BrowseGroupSelectedCommand, new MouseGesture(MouseAction.LeftClick)));
			Grid.SetColumn(userControl, num);
		}
		GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(3.0, GridUnitType.Star)
		});
	}

	private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		if (!string.IsNullOrEmpty(styleName))
		{
			object obj = frameworkElement?.FindResource(styleName);
			if (obj != null)
			{
				result = obj as Style;
			}
		}
		return result;
	}

	private void BrowseGroupSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if ((e.OriginalSource as FrameworkElement).DataContext is DisplayGroupSelectionItemModel displayGroupSelectionItemModel && base.DataContext is DisplayGroupSelectionModel displayGroupSelectionModel)
		{
			displayGroupSelectionModel.ProcessOnGroupSelectionItemClicked(displayGroupSelectionItemModel);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}
}
