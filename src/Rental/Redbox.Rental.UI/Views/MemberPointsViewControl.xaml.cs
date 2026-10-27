using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPointsView")]
public partial class MemberPointsViewControl : ViewUserControl, IComponentConnector
{
	private Brush RbPurpleBrush;

	private MemberPointsModel Model => base.DataContext as MemberPointsModel;

	public event RoutedEventHandler CartRoutedEventHandler
	{
		add
		{
			AddHandler(MemberPointsLogic.CartEvent, value);
		}
		remove
		{
			RemoveHandler(MemberPointsLogic.CartEvent, value);
		}
	}

	public MemberPointsViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		CartRoutedEventHandler += CartEventHandler;
		RbPurpleBrush = (Brush)FindResource("RbPurpleBrush");
	}

	private void CartEventHandler(object sender, RoutedEventArgs e)
	{
		CartRoutedEventArgs e2 = e as CartRoutedEventArgs;
		DisplayProductWithCommandsUserControl displayProductWithCommandsUserControl = (DisplayProductWithCommandsUserControl)CartStackPanelElem.Children[int.Parse(e2.ButtonName)];
		if (displayProductWithCommandsUserControl == null)
		{
			return;
		}
		if (e2.Parameter)
		{
			RoundedButton cornerIncludeButtonElem = displayProductWithCommandsUserControl.CornerIncludeButtonElem;
			if (cornerIncludeButtonElem.IsVisible)
			{
				CornerIncludeButton_Pressed(cornerIncludeButtonElem, e);
			}
		}
		else
		{
			RoundedButton cornerExcludeButtonElem = displayProductWithCommandsUserControl.CornerExcludeButtonElem;
			if (cornerExcludeButtonElem.IsVisible)
			{
				CornerExcludeButton_Pressed(cornerExcludeButtonElem, e);
			}
		}
	}

	private long GetProductId(DisplayProductWithCommandsUserControl displayProductUserControl)
	{
		if (displayProductUserControl?.DataContext is DisplayProductModel)
		{
			DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
			if (displayProductModel?.Data is IRentalShoppingCartTitleItem && displayProductModel.Data is IRentalShoppingCartTitleItem rentalShoppingCartTitleItem)
			{
				return rentalShoppingCartTitleItem.ProductId;
			}
		}
		return -1L;
	}

	private void PopulateDisplayProductModels()
	{
		List<DisplayProductWithCommandsUserControl> list = new List<DisplayProductWithCommandsUserControl>();
		foreach (object child in CartStackPanelElem.Children)
		{
			if (child is DisplayProductWithCommandsUserControl)
			{
				list.Add(child as DisplayProductWithCommandsUserControl);
			}
		}
		foreach (DisplayProductWithCommandsUserControl item in list)
		{
			CartStackPanelElem.Children.Remove(item);
		}
		if (Model?.DisplayProductModels == null)
		{
			return;
		}
		foreach (DisplayProductModel displayProductModel in Model.DisplayProductModels)
		{
			DisplayProductWithCommandsUserControl displayProductWithCommandsUserControl = new DisplayProductWithCommandsUserControl
			{
				DataContext = displayProductModel,
				Height = 180.0,
				Margin = new Thickness(15.0, 0.0, 15.0, 0.0)
			};
			int num = Model.DisplayProductModels.IndexOf(displayProductModel);
			displayProductWithCommandsUserControl.CornerIncludeButtonElem.Click += CornerIncludeButton_Pressed;
			displayProductWithCommandsUserControl.CornerExcludeButtonElem.Click += CornerExcludeButton_Pressed;
			displayProductWithCommandsUserControl.Name = $"Points_Item_{num}";
			CartStackPanelElem.Children.Insert(num, displayProductWithCommandsUserControl);
		}
		List<long> selectedProductIds = Model.SelectedProductIds;
		if (selectedProductIds == null || selectedProductIds.Count <= 0)
		{
			return;
		}
		foreach (long item2 in selectedProductIds)
		{
			foreach (object child2 in CartStackPanelElem.Children)
			{
				DisplayProductWithCommandsUserControl displayProductWithCommandsUserControl2 = child2 as DisplayProductWithCommandsUserControl;
				if (item2 == GetProductId(displayProductWithCommandsUserControl2))
				{
					DisplayProductModel displayModel = ChangeControlLook(displayProductWithCommandsUserControl2.CornerIncludeButtonElem, "FlagGrayColor", "FlagGrayShadow", includeOpacity: true);
					InvokeDisplayProductUserControl(Model.PointsIncludeCommand, displayModel);
				}
			}
		}
	}

	protected override void DataContextHasChanged()
	{
		PopulateDisplayProductModels();
	}

	private void InvokeDisplayProductUserControl(DynamicRoutedCommand routeCommand, DisplayProductModel displayModel)
	{
		if (displayModel != null && displayModel != null)
		{
			ViewUserControl.InvokeRoutedCommand(routeCommand, displayModel.Data as IRentalShoppingCartTitleItem);
		}
	}

	private DisplayProductModel ChangeControlLook(RoundedButton buttonToHide, string flagColor, string shadowColor, bool includeOpacity = false)
	{
		bool flag = buttonToHide.Name.Equals("CornerExcludeButtonElem");
		Grid grid = buttonToHide.Parent as Grid;
		DisplayProductUserControl obj = (grid.Parent as Grid).Children[0] as DisplayProductUserControl;
		DisplayProductModel displayProductModel = obj.DataContext as DisplayProductModel;
		displayProductModel.ImageOpacity = (includeOpacity ? 0.8 : 1.0);
		Flag flag2 = obj.Flag;
		if (grid.Children.Count > 1)
		{
			RoundedButton obj2 = grid.Children[(!flag) ? 1 : 0] as RoundedButton;
			Color color = (Color)FindResource(flagColor);
			Color flagShadowColor = (Color)FindResource(shadowColor);
			flag2.FlagColor = color;
			flag2.FlagGradientColor = color;
			flag2.FlagShadowColor = flagShadowColor;
			buttonToHide.Visibility = Visibility.Collapsed;
			obj2.Visibility = Visibility.Visible;
		}
		return displayProductModel;
	}

	private void CornerIncludeButton_Pressed(object sender, RoutedEventArgs e)
	{
		if (!base.IsMultiClickBlocked && Model != null)
		{
			base.IsMultiClickBlocked = true;
			RoundedButton roundedButton = sender as RoundedButton;
			MemberPointsLogic logic = Model.Logic;
			if (logic.IsOverPointLimit(roundedButton.DataContext as DisplayProductModel))
			{
				logic.DisplayErrorView();
			}
			else
			{
				DisplayProductModel displayModel = ChangeControlLook(roundedButton, "FlagGrayColor", "FlagGrayShadow", includeOpacity: true);
				InvokeDisplayProductUserControl(Model.PointsIncludeCommand, displayModel);
			}
			UpdateContinueButton();
			HandleWPFHit();
		}
	}

	private void CornerExcludeButton_Pressed(object sender, RoutedEventArgs e)
	{
		if (!base.IsMultiClickBlocked && Model != null)
		{
			base.IsMultiClickBlocked = true;
			DisplayProductModel displayModel = ChangeControlLook(sender as RoundedButton, "FlagRubineColor", "FlagRubineShadow");
			InvokeDisplayProductUserControl(Model.PointsExcludeCommand, displayModel);
			UpdateContinueButton();
			HandleWPFHit();
		}
	}

	private void UpdateContinueButton()
	{
		bool continueButtonEnabled = !Model.EntryShoppingProductIds.IsEqualTo(Model.SelectedProductIds);
		if (Model != null)
		{
			Model.ContinueButtonEnabled = continueButtonEnabled;
		}
	}
}
