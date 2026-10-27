using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel.Loyalty;
using Redbox.KioskServices.Proxy.Request;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Proxy;

public class LoyaltyServicesProxy : ILoyaltyServices
{
	private BindingList<ItemAccrualRedemptions> _items;

	private int m_working;

	public static LoyaltyServicesProxy Instance => Singleton<LoyaltyServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "LiveCall", value);
		}
	}

	public bool SimulateAccrualSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "SimulateAccrualSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "SimulateAccrualSuccess", value);
		}
	}

	public bool SimulateRedemptionSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "SimulateRedemptionSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "SimulateRedemptionSuccess", value);
		}
	}

	public int DefaultAccrualPoints
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "DefaultAccrualPoints", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "DefaultAccrualPoints", value);
		}
	}

	public int DefaultRedemptionPoints
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "DefaultRedemptionPoints", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "DefaultRedemptionPoints", value);
		}
	}

	public EstimateAccrualErrors AccrualError
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "AccrualError", "Error");
			return (EstimateAccrualErrors)Enum.Parse(typeof(EstimateAccrualErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "AccrualError", value);
		}
	}

	public EstimateRedemptionErrors RedemptionError
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "RedemptionError", "Error");
			return (EstimateRedemptionErrors)Enum.Parse(typeof(EstimateRedemptionErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "RedemptionError", value);
		}
	}

	public BindingList<ItemAccrualRedemptions> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new BindingList<ItemAccrualRedemptions>();
			}
			IRentalShoppingCartService session = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (session?.CurrentRentalShoppingCart?.TitleItems != null && session.CurrentRentalShoppingCart.TitleItems.Any())
			{
				_items.ToList().RemoveAll((ItemAccrualRedemptions x) => !session.CurrentRentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem y) => x.ProductId == y.ProductId));
			}
			else
			{
				_items.Clear();
			}
			session?.CurrentRentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => !_items.Any((ItemAccrualRedemptions y) => y.ProductId == x.ProductId)).ForEach(delegate(IRentalShoppingCartTitleItem x)
			{
				_items.Add(new ItemAccrualRedemptions
				{
					ProductId = x.ProductId,
					Name = x.SortName,
					AccrualPoints = DefaultAccrualPoints,
					RedemptionPoints = DefaultRedemptionPoints
				});
			});
			return _items;
		}
	}

	private LoyaltyServicesProxy()
	{
	}

	public void EstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveEstimatePointsAccrued(customerProfileNumber, completeCallback);
		}
		else
		{
			PerformSimulatedEstimatePointsAccrued(customerProfileNumber, completeCallback);
		}
	}

	private void PerformLiveEstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
					string arg = (service.SendEstimateAccrualToKioskClientService ? service.KioskClientServiceBaseUrl : KioskServicesProxy.Instance.Url);
					LogHelper.Instance.Log($"Live call to remote ILoyaltyServices.EstimatePointsAccrued, URL: {arg}");
					CartMessage request = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateAccrual");
					IDictionary<string, object> dictionary;
					if (service.SendEstimateAccrualToKioskClientService)
					{
						dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.EstimateAccrual(request);
					}
					else
					{
						dictionary = KioskServicesProxy.Instance.ExecuteMessage(request);
						ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(dictionary);
					}
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
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
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

	private void PerformSimulatedEstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Thread.Sleep(1500);
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote ILoyaltyServices.EstimatePointsAccrued");
				CartMessage instance = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateAccrual");
				LogHelper.Instance.Log(instance.ToJson(), LogEntryType.Info);
				if (SimulateAccrualSuccess)
				{
					IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
					ArrayList list = new ArrayList();
					Items.ForEach(delegate(ItemAccrualRedemptions x)
					{
						bool flag = rentalShoppingCart?.Promotion?.PromotionItems?.FirstOrDefault((IPromotionItem y) => y.RentalShoppingCartItem.ProductId == x.ProductId) != null;
						bool flag2 = rentalShoppingCart?.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem y) => y.ProductId == x.ProductId && (y?.IsRedboxPlusFreeOneNightRental ?? false)) != null;
						Dictionary<string, object> value = new Dictionary<string, object>
						{
							{ "ProductId", x.ProductId },
							{
								"Points",
								(!(flag | flag2)) ? x.AccrualPoints : 0
							}
						};
						list.Add(value);
					});
					remoteServiceResult.Properties["Items"] = list;
				}
				else
				{
					remoteServiceResult.Properties["response"] = AccrualError;
				}
				remoteServiceResult.Success = SimulateAccrualSuccess;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformSimulatedEstimatePointsAccrued.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformSimulatedEstimatePointsAccrued."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void EstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveEstimateRedemptionPoints(customerProfileNumber, completeCallback);
		}
		else
		{
			PerformSimulatedEstimateRedemptionPoints(customerProfileNumber, completeCallback);
		}
	}

	private void PerformLiveEstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
					string arg = (service.SendEstimateRedemptionToKioskClientService ? service.KioskClientServiceBaseUrl : KioskServicesProxy.Instance.Url);
					LogHelper.Instance.Log($"Live call to remote ILoyaltyServices.EstimateRedemptionPoints, URL: {arg}");
					CartMessage request = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateRedemption");
					IDictionary<string, object> dictionary;
					if (service.SendEstimateRedemptionToKioskClientService)
					{
						dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.EstimateRedemption(request);
					}
					else
					{
						dictionary = KioskServicesProxy.Instance.ExecuteMessage(request);
						ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(dictionary);
					}
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
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
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

	private void PerformSimulatedEstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				Thread.Sleep(1500);
				LogHelper.Instance.Log("Simulate call to remote ILoyaltyServices.EstimateRedemptionPoints, URL: {0}", KioskServicesProxy.Instance.Url);
				CartMessage instance = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateRedemption");
				LogHelper.Instance.Log(instance.ToJson(), LogEntryType.Info);
				if (SimulateRedemptionSuccess)
				{
					IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
					ArrayList list = new ArrayList();
					Items.ForEach(delegate(ItemAccrualRedemptions x)
					{
						bool flag = rentalShoppingCart?.Promotion?.PromotionItems?.FirstOrDefault((IPromotionItem y) => y.RentalShoppingCartItem.ProductId == x.ProductId) != null;
						bool flag2 = rentalShoppingCart?.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem y) => y.ProductId == x.ProductId && (y?.IsRedboxPlusFreeOneNightRental ?? false)) != null;
						Dictionary<string, object> value = new Dictionary<string, object>
						{
							{ "ProductId", x.ProductId },
							{
								"Points",
								(!(flag | flag2)) ? x.RedemptionPoints : 0
							}
						};
						list.Add(value);
					});
					remoteServiceResult.Properties["Items"] = list;
				}
				else
				{
					remoteServiceResult.Properties["response"] = RedemptionError;
				}
				remoteServiceResult.Success = SimulateRedemptionSuccess;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformSimulatedEstimateRedemptionPoints.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformSimulatedEstimateRedemptionPoints."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}
}
