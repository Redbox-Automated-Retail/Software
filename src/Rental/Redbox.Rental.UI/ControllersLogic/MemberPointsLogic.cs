using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic;

public class MemberPointsLogic : BaseLogic
{
	public static readonly RoutedEvent CartEvent = EventManager.RegisterRoutedEvent("Cart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MemberPointsLogic));

	public bool Instructions { get; set; }

	public Action<CartRoutedEventArgs> CartAction { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action PointsAction { get; set; }

	public static MemberPointsModel Model { get; private set; }

	public static PointsMemberInfo Info { get; private set; }

	public MemberPointsModel GetModel()
	{
		return Model;
	}

	public PointsMemberInfo GetInfo()
	{
		return Info;
	}

	public void CancelButtonCommand_Execute()
	{
		CancelAction?.Invoke();
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPointsLogicCancel");
	}

	public void ContinueButtonCommand_Execute()
	{
		ContinueAction?.Invoke();
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPointsLogicOkay");
	}

	public List<DisplayProductModel> GetSelectedCartItems()
	{
		List<DisplayProductModel> list = new List<DisplayProductModel>();
		List<DisplayProductModel> displayProductModels = Model.DisplayProductModels;
		_ = Model.SelectedProductIds;
		foreach (DisplayProductModel item in displayProductModels)
		{
			IRentalShoppingCartTitleItem shoppingCartItem = GetShoppingCartItem(item);
			if (Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<DisplayProductModel> GetUnselectedCartItems()
	{
		List<DisplayProductModel> list = new List<DisplayProductModel>();
		List<DisplayProductModel> displayProductModels = Model.DisplayProductModels;
		_ = Model.SelectedProductIds;
		foreach (DisplayProductModel item in displayProductModels)
		{
			IRentalShoppingCartTitleItem shoppingCartItem = GetShoppingCartItem(item);
			if (!Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void DisplayErrorView()
	{
		PointsAction?.Invoke();
	}

	public bool IsOverPointLimit(DisplayProductModel displayModel)
	{
		int pointsToRedeem = GetPointsToRedeem(displayModel);
		return Info.RemainingPoints - pointsToRedeem < 0;
	}

	private IRentalShoppingCartTitleItem GetShoppingCartItem(DisplayProductModel displayModel)
	{
		if (displayModel != null && displayModel.Data != null && displayModel.Data is IRentalShoppingCartTitleItem)
		{
			return displayModel.Data as IRentalShoppingCartTitleItem;
		}
		return null;
	}

	public int GetPointsToRedeem(DisplayProductModel displayModel)
	{
		return GetPointsToRedeem(GetShoppingCartItem(displayModel));
	}

	public int GetPointsToRedeem(IRentalShoppingCartTitleItem shoppingCartItem)
	{
		int result = 0;
		if (shoppingCartItem?.LoyaltyPointsRecord != null)
		{
			result = shoppingCartItem.LoyaltyPointsRecord.PointsToRedeem.Value;
		}
		return result;
	}

	public void UpdatePointsInfo(int index, bool bAdd)
	{
		CartAction(new CartRoutedEventArgs(CartEvent)
		{
			ButtonName = index.ToString(),
			Parameter = bAdd
		});
	}

	private static void UpdatePointsInfo(IRentalShoppingCartTitleItem shoppingCartItem, int scalar)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = shoppingCartItem.LoyaltyPointsRecord;
		if (loyaltyPointsRecord != null && loyaltyPointsRecord.PointsToRedeem.HasValue)
		{
			Info.RemainingPoints += loyaltyPointsRecord.PointsToRedeem.Value * scalar;
			Model.PointsMessage = string.Format(Resources.points_message, Info.RemainingPoints);
		}
	}

	public void PointsIncludeCommand_Execute(IRentalShoppingCartTitleItem shoppingCartItem)
	{
		if (shoppingCartItem != null)
		{
			UpdatePointsInfo(shoppingCartItem, -1);
			if (!Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
			{
				Model.SelectedProductIds.Add(shoppingCartItem.ProductId);
				BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPointsLogicRedeem")?.AddData(ProductData.ToProductData(shoppingCartItem));
			}
		}
	}

	public void PointsExcludeCommand_Execute(IRentalShoppingCartTitleItem shoppingCartItem)
	{
		if (shoppingCartItem != null)
		{
			UpdatePointsInfo(shoppingCartItem, 1);
			if (Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
			{
				Model.SelectedProductIds.Remove(shoppingCartItem.ProductId);
				BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPointsLogicCancelRedeem")?.AddData(ProductData.ToProductData(shoppingCartItem));
			}
		}
	}

	public MemberPointsLogic(MemberPointsModel pointsModel, PointsMemberInfo memberInfo)
	{
		Instructions = true;
		CancelAction = memberInfo.CancelAction;
		ContinueAction = memberInfo.ContinueAction;
		Info = memberInfo;
		Model = pointsModel;
		Model.PointsIncludeCommand = DynamicRouting.RegisterRoutedCommand<IRentalShoppingCartTitleItem>(Commands.PointsIncludeCommand, PointsIncludeCommand_Execute);
		Model.PointsExcludeCommand = DynamicRouting.RegisterRoutedCommand<IRentalShoppingCartTitleItem>(Commands.PointsExcludeCommand, PointsExcludeCommand_Execute);
		Model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		Func<bool> canExecute = () => Model.ContinueButtonEnabled;
		Model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.ContinueButtonCommand, ContinueButtonCommand_Execute, canExecute);
	}
}
