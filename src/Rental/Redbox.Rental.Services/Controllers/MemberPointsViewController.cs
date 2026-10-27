using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MemberPointsViewController : BaseLoyaltyController
{
	private static MemberPointsLogic GetPointsLogic()
	{
		return BaseViewController.GetViewModel<MemberPointsModel>()?.Logic;
	}

	public static void ConfigureView(PointsMemberInfo pointsInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null && PersonalizationFunctions.IsLoyaltySignedIn)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			MemberPointsModel memberPointsModel;
			MemberPointsLogic memberPointsLogic;
			if (pointsInfo.PointsModel != null && pointsInfo.PointsLogic != null)
			{
				memberPointsModel = pointsInfo.PointsModel;
				memberPointsLogic = pointsInfo.PointsLogic;
			}
			else
			{
				memberPointsModel = CreateMemberPointModel(pointsInfo);
				memberPointsLogic = (memberPointsModel.Logic = CreateMemberPointLogic(pointsInfo, memberPointsModel));
				pointsInfo.PointsModel = memberPointsModel;
				pointsInfo.PointsLogic = memberPointsLogic;
			}
			InitDisplayPoints(memberPointsModel, pointsInfo);
			memberPointsLogic.CartAction = ProcessCartAction;
			memberPointsLogic.PointsAction = ProcessPointsAction;
			InitDisplayProductModels(memberPointsModel);
			viewUserControl.DataContext = memberPointsModel;
			ProcessRedemptionPoints(pointsInfo);
			ConfigureTTS(memberPointsModel);
			BaseViewController.StartIdleTimer(reset: true);
		}
	}

	private static void InitDisplayPoints(MemberPointsModel model, PointsMemberInfo info)
	{
		info.RemainingPoints = (int)info.Points;
		model.PointsMessage = string.Format(Resources.points_message, info.Points);
	}

	private static MemberPointsModel CreateMemberPointModel(PointsMemberInfo pointsInfo)
	{
		return new MemberPointsModel
		{
			PointsTitle = Resources.points_title,
			PointsMarketing = Resources.points_marketing,
			CancelButtonText = Resources.points_cancel_btn,
			ContinueButtonText = Resources.okay
		};
	}

	private static MemberPointsLogic CreateMemberPointLogic(PointsMemberInfo pointsInfo, MemberPointsModel Model)
	{
		return new MemberPointsLogic(Model, pointsInfo)
		{
			CancelAction = ProcessCancelAction,
			ContinueAction = ProcessContinueAction
		};
	}

	private static void InitDisplayProductModels(MemberPointsModel model)
	{
		if (model.DisplayProductModels != null)
		{
			return;
		}
		model.DisplayProductModels = new List<DisplayProductModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null || BaseViewController.DataService == null)
		{
			return;
		}
		foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
		{
			IKioskProduct kioskProduct = null;
			BaseViewController.DataService.GetProduct(titleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
			if (kioskProduct != null)
			{
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = titleItem,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
				};
				displayProductModel.ConfigureForUpdateADACart();
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				BannerType productBannerType = BrowseFunctions.GetProductBannerType(titleProduct);
				BrowseFunctions.ConfigureBanner(displayProductModel, titleProduct, hideBanner: false, productBannerType, BaseViewController.GetViewUserControl());
				displayProductModel.CancelButtonVisibility = Visibility.Collapsed;
				ConfigureProductFlag(displayProductModel, ProductFlags.Points);
				model.DisplayProductModels.Add(displayProductModel);
			}
			if (titleItem.LoyaltyPointsRecord != null && titleItem.LoyaltyPointsRecord.ToBeRedeemed)
			{
				model.SelectedProductIds.Add(titleItem.ProductId);
				model.EntryShoppingProductIds.Add(titleItem.ProductId);
				titleItem.LoyaltyPointsRecord.ToBeRedeemed = false;
			}
		}
	}

	private static void ProcessRedemptionPoints(PointsMemberInfo pointsInfo)
	{
		if (pointsInfo.EstimateRedemptionPoints == null)
		{
			return;
		}
		IEstimateRedemptionPointsResult estimateRedemptionPoints = pointsInfo.EstimateRedemptionPoints;
		if (estimateRedemptionPoints != null && estimateRedemptionPoints.Success)
		{
			IList<IRentalShoppingCartTitleItem> source = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems;
			foreach (IProductEstimatedRedemptionPoints productEstimatedRedemptionPoints in estimateRedemptionPoints.ProductEstimatedRedemptionPointsList)
			{
				IRentalShoppingCartTitleItem rentalShoppingCartItem = source.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == productEstimatedRedemptionPoints.ProductId);
				if (rentalShoppingCartItem != null)
				{
					if (rentalShoppingCartItem.LoyaltyPointsRecord == null)
					{
						rentalShoppingCartItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartItem);
						rentalShoppingCartItem.LoyaltyPointsRecord.Clone(productEstimatedRedemptionPoints.LoyaltyPointsRecord);
					}
					else
					{
						rentalShoppingCartItem.LoyaltyPointsRecord.PointsToRedeem = productEstimatedRedemptionPoints.LoyaltyPointsRecord.PointsToRedeem;
					}
					ConfigureProductFlag(BaseViewController.GetViewModel<MemberPointsModel>()?.DisplayProductModels.FirstOrDefault((DisplayProductModel x) => x.Data == rentalShoppingCartItem), ProductFlags.Points, rentalShoppingCartItem.LoyaltyPointsRecord);
				}
			}
		}
		else
		{
			Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOutLoyaltySession();
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
		}
		pointsInfo.EstimateRedemptionPoints = null;
	}

	public static void ConfigureProductFlag(DisplayProductModel displayProductModel, ProductFlags productFlags, ILoyaltyPointsRecord pointsRecord = null)
	{
		if (displayProductModel == null || displayProductModel.Data == null)
		{
			return;
		}
		if (pointsRecord != null && !pointsRecord.CanRedeem)
		{
			productFlags = ProductFlags.Excluded;
		}
		ProductFlag productFlag = ProductFlagCollection.Instance.FirstOrDefault((ProductFlag x) => x.Flag == productFlags);
		displayProductModel.FlagVisibility = BaseViewController.BoolToVisibility(productFlag != null && productFlag.Flag != ProductFlags.None);
		displayProductModel.FlagStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), productFlag?.FlagStyleName);
		if (displayProductModel.FlagVisibility == Visibility.Visible)
		{
			displayProductModel.FlagMargin = new Thickness(0.0, 31.0, 0.0, 0.0);
		}
		double imageOpacity = 1.0;
		if (pointsRecord != null)
		{
			displayProductModel.CornerContainerVisibility = BaseViewController.BoolToVisibility(show: true);
			bool canRedeem = pointsRecord.CanRedeem;
			if (canRedeem)
			{
				displayProductModel.FlagText = string.Format(Resources.flag_points, pointsRecord.PointsToRedeem);
			}
			else
			{
				displayProductModel.FlagText = Resources.points_cannot_redeem;
				imageOpacity = 0.7;
			}
			displayProductModel.CornerIncludeButtonVisibility = BaseViewController.BoolToVisibility(canRedeem);
		}
		else
		{
			displayProductModel.FlagText = string.Empty;
			displayProductModel.CornerContainerVisibility = BaseViewController.BoolToVisibility(show: false);
		}
		displayProductModel.ImageOpacity = imageOpacity;
	}

	public static void GetRedemptionPoints(Action<IEstimateRedemptionPointsResult> processEstimateRedemptionPoints)
	{
		BaseLoyaltyController.GetEstimateRedemptionPoints(processEstimateRedemptionPoints);
	}

	private static void ProcessCancelAction()
	{
		BaseViewController.StopIdleTimer();
		MemberPointsModel viewModel = BaseViewController.GetViewModel<MemberPointsModel>();
		List<long> entryShoppingProductIds = viewModel.EntryShoppingProductIds;
		IList<IRentalShoppingCartTitleItem> list = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems;
		if (list != null && viewModel != null)
		{
			foreach (IRentalShoppingCartTitleItem item in list)
			{
				if (item.LoyaltyPointsRecord != null)
				{
					if (entryShoppingProductIds.Contains(item.ProductId))
					{
						item.LoyaltyPointsRecord.ToBeRedeemed = true;
					}
					else
					{
						item.LoyaltyPointsRecord.ToBeRedeemed = false;
					}
				}
			}
		}
		if (MemberPointsLogic.Info != null && MemberPointsLogic.Info.ContinueAction != null)
		{
			MemberPointsLogic.Info.CancelAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: MemberPointsLogic.Info.CancelAction was not set for: Member Points", popOnly: true);
		}
	}

	private static void ProcessContinueAction()
	{
		BaseViewController.StopIdleTimer();
		MemberPointsModel viewModel = BaseViewController.GetViewModel<MemberPointsModel>();
		IList<IRentalShoppingCartTitleItem> list = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems;
		if (list != null && viewModel != null)
		{
			foreach (long selectedProductId in viewModel.SelectedProductIds)
			{
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = list.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == selectedProductId);
				if (rentalShoppingCartTitleItem != null && rentalShoppingCartTitleItem.LoyaltyPointsRecord != null)
				{
					rentalShoppingCartTitleItem.LoyaltyPointsRecord.ToBeRedeemed = true;
				}
			}
		}
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpdateServiceFee();
		if (MemberPointsLogic.Info != null && MemberPointsLogic.Info.ContinueAction != null)
		{
			MemberPointsLogic.Info.ContinueAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: MemberPointsLogic.Info.ContinueAction was not set for: Member Points", popOnly: true);
		}
	}

	private static void ProcessCartAction(CartRoutedEventArgs RoutedEventArgs)
	{
		BaseViewController.GetViewUserControl()?.RaiseEvent(RoutedEventArgs);
	}

	private static void ProcessPointsAction()
	{
		PersonalizationFunctions.ShowMyPointsError();
	}

	public static void ClosingView(PointsMemberInfo pointsInfo)
	{
	}

	private static void ConfigureTTS(MemberPointsModel PointsModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			PointsModel.ProcessGetSpeechControl = () => BaseTTSController<MemberPointsTTSController, MemberPointsLogic>.GetSpeechControl(GetPointsLogic);
		}
	}
}
