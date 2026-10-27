using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.Loyalty;

public class LoyaltyService : ILoyaltyService
{
	private Action<IEstimatePointsAccruedResult> _estimatePointsAccruedResultCallback;

	private Action<IEstimateRedemptionPointsResult> _estimateRedemptionPointsResultCallback;

	private int m_working;

	public void EstimatePointsAccrued(IRentalShoppingCart rentalShoppingCart, Action<IEstimatePointsAccruedResult> estimatPointsAccuedResultCallback)
	{
		_estimatePointsAccruedResultCallback = estimatPointsAccuedResultCallback;
		RemoteServiceCallback completeCallback = ProcessEstimatePointsAccruedResult;
		EstimatePointsAccrued(Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.CustomerProfileNumber, completeCallback);
	}

	private void EstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					string kioskClientServiceBaseUrl = ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl;
					LogHelper.Instance.Log("Live call to remote ILoyaltyServices.EstimatePointsAccrued, URL: " + kioskClientServiceBaseUrl);
					CartMessage request = ShoppingCartMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateAccrual");
					IDictionary<string, object> dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.EstimateAccrual(request);
					result.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("Items"))
					{
						result.Properties["Items"] = dictionary["Items"] as ArrayList;
					}
					if (dictionary.ContainsKey("ServerName"))
					{
						result.Properties["server_name"] = dictionary["ServerName"];
					}
					if (!result.Success)
					{
						ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
						if (arrayList != null && arrayList.Count > 0)
						{
							if (arrayList[0] is Dictionary<string, object> dictionary2)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(EstimateAccrualErrors?), (string)dictionary2["Code"]);
								result.Properties["response"] = fieldsForResponseCode ?? ((object)EstimateAccrualErrors.Error);
							}
						}
						else
						{
							result.Properties["response"] = EstimateAccrualErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to login user.", LogEntryType.Info);
					result.Properties["response"] = EstimateAccrualErrors.LoyaltyError;
					result.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformLiveEstimatePointsAccrued.", e);
				result.Properties["response"] = EstimateAccrualErrors.Error;
			}
			finally
			{
				try
				{
					result.ExecutionTime = executionTimer.Elapsed;
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", result.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = result,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformLiveEstimatePointsAccrued."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void ProcessEstimatePointsAccruedResult(IRemoteServiceResult result)
	{
		IEstimatePointsAccruedResult estimatePointsAccruedResult = new EstimatedPointsAccruedResult
		{
			Success = (result?.Success ?? false),
			ProductEstimatedPointsAccruedList = new List<IProductEstimatedPointsAccrued>()
		};
		if (result != null && result.Success && result.Properties.ContainsKey("Items") && result.Properties["Items"] is ArrayList arrayList)
		{
			foreach (object item in arrayList)
			{
				IDictionary<string, object> dictionary = item as IDictionary<string, object>;
				long result2 = 0L;
				long productId = ((dictionary != null && dictionary.ContainsKey("ProductId")) ? (long.TryParse(dictionary["ProductId"].ToString(), out result2) ? result2 : 0) : 0);
				int result3 = 0;
				int value = ((dictionary != null && dictionary.ContainsKey("Points")) ? (int.TryParse(dictionary["Points"].ToString(), out result3) ? result3 : 0) : 0);
				estimatePointsAccruedResult.ProductEstimatedPointsAccruedList.Add(new ProductEstimatePointsAccrued
				{
					ProductId = productId,
					LoyaltyPointsRecord = new LoyaltyPointsRecord(null)
					{
						PointsToBeEarned = value
					}
				});
			}
		}
		if (_estimatePointsAccruedResultCallback != null)
		{
			_estimatePointsAccruedResultCallback(estimatePointsAccruedResult);
		}
	}

	public void EstimateRedemptionPoints(IRentalShoppingCart rentalShoppingCart, Action<IEstimateRedemptionPointsResult> estimatRedemptionPointsResultCallback)
	{
		_estimateRedemptionPointsResultCallback = estimatRedemptionPointsResultCallback;
		RemoteServiceCallback completeCallback = ProcessEstimateRedemptionPointsResult;
		EstimateRedemptionPoints(Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.CustomerProfileNumber, completeCallback);
	}

	private void EstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					string kioskClientServiceBaseUrl = ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl;
					LogHelper.Instance.Log("Live call to remote ILoyaltyServices.EstimateRedemptionPoints, URL: " + kioskClientServiceBaseUrl);
					CartMessage request = ShoppingCartMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateRedemption");
					IDictionary<string, object> dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.EstimateRedemption(request);
					result.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("Items"))
					{
						result.Properties["Items"] = dictionary["Items"] as ArrayList;
					}
					if (dictionary.ContainsKey("ServerName"))
					{
						result.Properties["server_name"] = dictionary["ServerName"];
					}
					if (!result.Success)
					{
						ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
						if (arrayList != null && arrayList.Count > 0)
						{
							if (arrayList[0] is Dictionary<string, object> dictionary2)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(EstimateRedemptionErrors?), (string)dictionary2["Code"]);
								result.Properties["response"] = fieldsForResponseCode ?? ((object)EstimateRedemptionErrors.Error);
							}
						}
						else
						{
							result.Properties["response"] = EstimateRedemptionErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to login user.", LogEntryType.Info);
					result.Properties["response"] = EstimateRedemptionErrors.LoyaltyError;
					result.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformLiveEstimateRedemptionPoints.", e);
				result.Properties["response"] = EstimateRedemptionErrors.Error;
			}
			finally
			{
				try
				{
					result.ExecutionTime = executionTimer.Elapsed;
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", result?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = result,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformLiveEstimateRedemptionPoints."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void ProcessEstimateRedemptionPointsResult(IRemoteServiceResult result)
	{
		IEstimateRedemptionPointsResult estimateRedemptionPointsResult = new EstimateRedemptionPointsResult
		{
			Success = (result?.Success ?? false),
			ProductEstimatedRedemptionPointsList = new List<IProductEstimatedRedemptionPoints>()
		};
		if (result != null && result.Success && result.Properties.ContainsKey("Items") && result.Properties["Items"] is ArrayList arrayList)
		{
			foreach (object item in arrayList)
			{
				IDictionary<string, object> dictionary = item as IDictionary<string, object>;
				long result2 = 0L;
				long productId = (dictionary.ContainsKey("ProductId") ? (long.TryParse(dictionary["ProductId"].ToString(), out result2) ? result2 : 0) : 0);
				int result3 = 0;
				int value = (dictionary.ContainsKey("Points") ? (int.TryParse(dictionary["Points"].ToString(), out result3) ? result3 : 0) : 0);
				estimateRedemptionPointsResult.ProductEstimatedRedemptionPointsList.Add(new ProductEstimatedRedemptionPoints
				{
					ProductId = productId,
					LoyaltyPointsRecord = new LoyaltyPointsRecord(null)
					{
						PointsToRedeem = value
					}
				});
			}
		}
		if (_estimatePointsAccruedResultCallback != null)
		{
			_estimateRedemptionPointsResultCallback(estimateRedemptionPointsResult);
		}
	}
}
