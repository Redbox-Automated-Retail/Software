using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Marketing;

internal class MarketingServices : IMarketingServices
{
	private class OptInItem : IOptInItem
	{
		public long ItemId { get; set; }

		public KioskOptIntItemType Item_Type { get; set; }

		public OptInMethodType OptInMethodType { get; set; }

		public string OptInMethodData { get; set; }

		public string TemplateName { get; set; }

		public string CampaignName { get; set; }
	}

	private int m_working;

	public void SendEmailConfirm(long kioskId, string emailAddress, string accountNumber, SendEmailConfirmCallback completeCallback)
	{
		SendEmailConfirm(kioskId, emailAddress, accountNumber, processConfirmEmailResult);
		void processConfirmEmailResult(IRemoteServiceResult remoteServiceResult)
		{
			completeCallback(GetEmailConfirmResult(remoteServiceResult));
		}
	}

	public void KioskOptIn(long kioskId, List<IOptInItem> optInItems, RemoteServiceCallback callback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ThreadPool.QueueUserWorkItem(delegate
		{
			Interlocked.Increment(ref m_working);
			ErrorList errorList = null;
			try
			{
				IEnumerable<IOptInItem> enumerable = (optInItems ?? new List<IOptInItem>()).Where((IOptInItem x) => !string.IsNullOrEmpty(x.OptInMethodData));
				errorList = ((!enumerable.Any()) ? new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("U001", "No Valid Optins To Send", (optInItems?.Count() ?? 0) + " items were missing OptInMethodData") } : KCSPerformKioskOptInApiCall(kioskId, enumerable));
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServices.KioskOptIn.", e);
			}
			finally
			{
				try
				{
					if (callback != null)
					{
						ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
						if (service != null)
						{
							RemoteServiceResult remoteServiceResult = new RemoteServiceResult
							{
								Success = true
							};
							if (errorList != null)
							{
								remoteServiceResult.Errors.AddRange(errorList);
								remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
							}
							service.EnqueueCallback(new RemoteServiceCallbackEntry
							{
								Callback = callback,
								Result = remoteServiceResult,
								Message = "Invoking callback from PromotionServices.KioskOptIn."
							});
						}
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public IOptInItem NewOptInItem()
	{
		return new OptInItem();
	}

	internal MarketingServices()
	{
	}

	private IEmailConfirmResult GetEmailConfirmResult(IRemoteServiceResult result)
	{
		try
		{
			EmailConfirmResult obj = new EmailConfirmResult
			{
				Success = result.Success,
				Errors = result.Errors,
				CustomerProfileNumber = result.GetProperty<string>("customer_profile_number"),
				MobilePhoneNumber = result.GetProperty<string>("mobile_phone_number"),
				PromptForPerks = result.GetProperty<bool>("prompt_for_perks"),
				PromptForEarlyId = result.GetProperty<bool>("prompt_for_early_id"),
				TextClubEnabled = result.GetProperty<bool>("text_club_enabled"),
				EmailVerified = result.GetProperty<bool?>("email_verified")
			};
			ConfirmationStatus confirmationStatus = ConfirmationStatus.DontShow;
			if (Enum.TryParse<ConfirmationStatus>(result.GetProperty<string>("confirmation_status"), out var result2))
			{
				confirmationStatus = result2;
			}
			if (confirmationStatus == ConfirmationStatus.DontShow)
			{
				confirmationStatus = ConfirmationStatus.DontShow;
			}
			((IEmailConfirmResult)obj).ConfirmationStatus = confirmationStatus;
			return obj;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Unhandled Exception processing Email result", e);
			return null;
		}
	}

	private void SendEmailConfirm(long kioskId, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				IRemoteServiceResult remoteServiceResult2 = KCSPerformEmailConfirmationApiCall(kioskId, emailAddress, accountNumber);
				remoteServiceResult.Success = remoteServiceResult2.Success;
				foreach (KeyValuePair<string, object> property in remoteServiceResult2.Properties)
				{
					remoteServiceResult.Properties.Add(property);
				}
				if (remoteServiceResult2.Errors != null)
				{
					remoteServiceResult.Errors.AddRange(remoteServiceResult2.Errors);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BaseMarketingServices.SendLiveCall.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from BaseMarketingServices.SendLiveCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private IRemoteServiceResult KCSPerformEmailConfirmationApiCall(long kioskId, string emailAddress, string accountNumber)
	{
		LogHelper.Instance.Log("Live call to KioskClientService EmailConfirmation, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
		return ServiceLocator.Instance.GetService<IKioskClientService>().Marketing.EmailConfirmation(kioskId, emailAddress, accountNumber);
	}

	private ErrorList KCSPerformKioskOptInApiCall(long kioskId, IEnumerable<IOptInItem> optInItems)
	{
		LogHelper.Instance.Log("Live call to KioskClientService KioskOptIn, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
		return ServiceLocator.Instance.GetService<IKioskClientService>().Marketing.KioskOptIn(kioskId, optInItems);
	}
}
