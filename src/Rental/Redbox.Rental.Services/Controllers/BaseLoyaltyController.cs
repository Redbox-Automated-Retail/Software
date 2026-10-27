using System;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.Loyalty;

namespace Redbox.Rental.Services.Controllers;

public class BaseLoyaltyController : BaseViewController
{
	protected static ILoyaltyService loyaltyService => ServiceLocator.Instance.GetService<ILoyaltyService>();

	protected static void GetEstimateAccruedPoints(Action<IEstimatePointsAccruedResult> processEstimateAccruedPoints)
	{
		Action<IEstimatePointsAccruedResult> estimatPointsAccuedResultCallback = delegate(IEstimatePointsAccruedResult estimatePointsAccruedResult)
		{
			Task.Factory.StartNew(delegate
			{
				processEstimateAccruedPoints(estimatePointsAccruedResult);
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		};
		loyaltyService?.EstimatePointsAccrued(BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart, estimatPointsAccuedResultCallback);
	}

	protected static void GetEstimateRedemptionPoints(Action<IEstimateRedemptionPointsResult> processEstimateRedemptionPoints)
	{
		Action<IEstimateRedemptionPointsResult> estimatRedemptionPointsResultCallback = delegate(IEstimateRedemptionPointsResult estimateRedemptionPointsResult)
		{
			Task.Factory.StartNew(delegate
			{
				processEstimateRedemptionPoints(estimateRedemptionPointsResult);
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		};
		loyaltyService?.EstimateRedemptionPoints(BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart, estimatRedemptionPointsResultCallback);
	}
}
