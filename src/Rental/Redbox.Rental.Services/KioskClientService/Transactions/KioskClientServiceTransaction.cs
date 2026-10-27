using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.TestData;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Model.Transaction;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class KioskClientServiceTransaction : BaseKioskClientService, IKioskClientServiceTransaction
{
	private const string _apiPath = "api/transaction";

	private int m_working;

	private bool IsAuthorizeLiveCall
	{
		get
		{
			bool result = true;
			IKioskClientServiceTestData kioskClientServiceTestData = ServiceLocator.Instance.GetService<IKioskClientService>()?.TestData;
			if (kioskClientServiceTestData != null)
			{
				IReadTestDataValueResult readTestDataValueResult = kioskClientServiceTestData.ReadPropertyValue("TransactionService", "Authorize.LiveCall");
				if (readTestDataValueResult != null && readTestDataValueResult.Success && bool.TryParse(readTestDataValueResult.PropertyValue, out var result2))
				{
					result = result2;
				}
			}
			return result;
		}
	}

	public void Authorize(Guid sessionId, AuthorizeType authorizeType, string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IAuthorizeResult result = AuthorizeAsync(sessionId, authorizeType, customerProfileNumber).Result;
			remoteServiceResult.Properties["AuthorizeResult"] = result;
			remoteServiceResult.Success = result.Success;
			completeCallback(remoteServiceResult);
		});
	}

	public async Task<IAuthorizeResult> AuthorizeAsync(Guid sessionId, AuthorizeType authorizeType, string customerProfileNumber)
	{
		AuthorizeResult authorizeResult = new AuthorizeResult
		{
			SessionId = sessionId,
			AuthorizeType = authorizeType
		};
		authorizeResult.IsOnline = true;
		ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionId.ToString());
		string errorString = AuthorizeResponse.Error.ToString();
		if (session == null)
		{
			authorizeResult.Response = errorString;
			return authorizeResult;
		}
		string approvedString = AuthorizeResponse.Approved.ToString();
		using ExecutionTimer executionTimer = new ExecutionTimer();
		_ = 1;
		try
		{
			Interlocked.Increment(ref m_working);
			if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
			{
				string text = (IsAuthorizeLiveCall ? "Live TransactionService" : "Test data");
				LogHelper.Instance.Log(text + " Authorize call via KCS");
				AuthorizeRequest authorizeRequest = new AuthorizeRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(session.StoreNumber),
					CreditCard = GetCreditCardData(session, includeTrack2: true),
					TransactionDate = DateTime.Now,
					AuthAcceptFlag = session.ShoppingCart.AuthAcceptFlag,
					AuthRuleId = session.ShoppingCart.AuthRuleId,
					AuthAmount = session.ShoppingCart.AuthAmount,
					SkipAuthRule = session.ShoppingCart.SkipAuthRule,
					ShoppingCart = ShoppingCartMessageHelper.CreateShoppingCart(sessionId),
					CustomerProfileNumber = customerProfileNumber
				};
				LogHelper.Instance.Log($"AuthorizeRequest: {authorizeRequest?.Scrub()}");
				authorizeResult = ((authorizeType != AuthorizeType.Authorize) ? (await CallGiftCardAuthorizeAsync(authorizeRequest)) : (await CallAuthorizeAsync(authorizeRequest)));
				if (authorizeResult == null)
				{
					authorizeResult = new AuthorizeResult
					{
						Success = false
					};
					authorizeResult.AddError(AuthorizeResponse.CommunicationError.GetResponseCodeAttributeName(), AuthorizeResponse.CommunicationError.ToString());
				}
				authorizeResult.Response = approvedString;
				authorizeResult.SessionId = sessionId;
				if (authorizeResult.Success && !string.IsNullOrEmpty(authorizeResult.ReferenceNumber))
				{
					session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(authorizeResult.ReferenceNumber));
				}
				string response = approvedString;
				if (!authorizeResult.Success && (authorizeResult.Errors?.Count ?? 0) > 0)
				{
					Redbox.Rental.Model.Error error = authorizeResult.Errors[0];
					response = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), error?.Code ?? errorString)?.ToString() ?? errorString;
				}
				authorizeResult.IsOnline = AuthErrorCodeIndicatesOnline(response);
				if (authorizeResult.IsOnline)
				{
					authorizeResult.Response = response;
				}
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
			}
			else
			{
				LogHelper.Instance.Log("In stand alone mode, skipping realtime call to Authorize.", LogEntryType.Info);
				authorizeResult.Success = true;
				authorizeResult.Response = approvedString;
				authorizeResult.IsOnline = false;
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
				authorizeResult.ConfirmationStatus = ConfirmationStatus.DontShow.ToString();
				new AutoResetEvent(initialState: false).WaitOne(1000);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceTransactions.Authorize.", e);
			authorizeResult.Response = approvedString;
			authorizeResult.IsOnline = false;
			PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: true);
			return authorizeResult;
		}
		finally
		{
			try
			{
				LogHelper.Instance.Log($"Authorize Result: {authorizeResult?.Scrub()}");
				LogHelper.Instance.Log($"Authorize completed in {executionTimer.Elapsed} ms");
				authorizeResult.StandAlone = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
		return authorizeResult;
	}

	public void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IAuthorizeResult result = PickupAuthorizeAsync(sessionId, customerProfileNumber, referenceNumber, cartWasModified).Result;
			AddPropertiesToResult(result, remoteServiceResult);
			remoteServiceResult.Success = result.Success;
			completeCallback(remoteServiceResult);
		});
	}

	public async Task<IAuthorizeResult> PickupAuthorizeAsync(string sessionIdString, string customerProfileNumber, long referenceNumber, bool cartWasModified)
	{
		Guid sessionId = new Guid(sessionIdString);
		AuthorizeResult authorizeResult = new AuthorizeResult
		{
			SessionId = sessionId
		};
		authorizeResult.IsOnline = true;
		ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionIdString);
		string errorString = AuthorizeResponse.Error.ToString();
		if (session == null)
		{
			authorizeResult.Response = errorString;
			return authorizeResult;
		}
		string approvedString = AuthorizeResponse.Approved.ToString();
		using ExecutionTimer executionTimer = new ExecutionTimer();
		try
		{
			Interlocked.Increment(ref m_working);
			if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
			{
				string text = (IsAuthorizeLiveCall ? "Live TransactionService" : "Test data");
				LogHelper.Instance.Log(text + " PickupAuthorize call via KCS");
				PickupAuthRequest pickupAuthRequest = new PickupAuthRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(session.StoreNumber),
					CreditCard = GetCreditCardData(session, includeTrack2: true),
					TransactionDate = DateTime.Now,
					AuthAcceptFlag = session.ShoppingCart.AuthAcceptFlag,
					AuthRuleId = session.ShoppingCart.AuthRuleId,
					AuthAmount = session.ShoppingCart.AuthAmount,
					SkipAuthRule = session.ShoppingCart.SkipAuthRule,
					ShoppingCart = ShoppingCartMessageHelper.CreateShoppingCart(sessionId),
					CustomerProfileNumber = customerProfileNumber,
					ReferenceNumber = referenceNumber,
					CartWasModified = cartWasModified
				};
				LogHelper.Instance.Log($"PickupAuthorizeRequest: {pickupAuthRequest?.Scrub()}");
				authorizeResult = await CallPickupAuthorizeAsync(pickupAuthRequest);
				if (authorizeResult == null)
				{
					authorizeResult = new AuthorizeResult
					{
						Success = false
					};
					authorizeResult.AddError(AuthorizeResponse.CommunicationError.GetResponseCodeAttributeName(), AuthorizeResponse.CommunicationError.ToString());
				}
				authorizeResult.Response = approvedString;
				authorizeResult.SessionId = sessionId;
				if (authorizeResult.Success && !string.IsNullOrEmpty(authorizeResult.ReferenceNumber))
				{
					session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(authorizeResult.ReferenceNumber));
				}
				string response = approvedString;
				if ((authorizeResult.Errors?.Count ?? 0) > 0)
				{
					Redbox.Rental.Model.Error error = authorizeResult.Errors[0];
					response = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), error?.Code ?? errorString)?.ToString() ?? errorString;
				}
				authorizeResult.IsOnline = AuthErrorCodeIndicatesOnline(response);
				if (authorizeResult.IsOnline)
				{
					authorizeResult.Response = response;
				}
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
			}
			else
			{
				LogHelper.Instance.Log("In stand alone mode, skipping realtime call to PickupAuthorize.", LogEntryType.Info);
				authorizeResult.Success = true;
				authorizeResult.Response = approvedString;
				authorizeResult.IsOnline = false;
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
				authorizeResult.ConfirmationStatus = ConfirmationStatus.DontShow.ToString();
				new AutoResetEvent(initialState: false).WaitOne(1000);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceTransactions.PickupAuthorize.", e);
			authorizeResult.Response = approvedString;
			authorizeResult.IsOnline = false;
			PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: true);
			return authorizeResult;
		}
		finally
		{
			try
			{
				LogHelper.Instance.Log($"PickupAuthorize Result: {authorizeResult?.Scrub()}");
				LogHelper.Instance.Log($"PickupAuthorize completed in {executionTimer.Elapsed} ms");
				authorizeResult.StandAlone = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
		return authorizeResult;
	}

	public void ReservationAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IAuthorizeResult result = ReservationAuthorizeAsync(sessionId, customerProfileNumber, referenceNumber).Result;
			AddPropertiesToResult(result, remoteServiceResult);
			remoteServiceResult.Success = result.Success;
			completeCallback(remoteServiceResult);
		});
	}

	public async Task<IAuthorizeResult> ReservationAuthorizeAsync(string sessionIdString, string customerProfileNumber, long referenceNumber)
	{
		Guid sessionId = new Guid(sessionIdString);
		AuthorizeResult authorizeResult = new AuthorizeResult
		{
			SessionId = sessionId
		};
		authorizeResult.IsOnline = true;
		ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionIdString);
		string errorString = AuthorizeResponse.Error.ToString();
		if (session == null)
		{
			authorizeResult.Response = errorString;
			return authorizeResult;
		}
		string approvedString = AuthorizeResponse.Approved.ToString();
		using ExecutionTimer executionTimer = new ExecutionTimer();
		try
		{
			Interlocked.Increment(ref m_working);
			if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
			{
				string text = (IsAuthorizeLiveCall ? "Live TransactionService" : "Test data");
				LogHelper.Instance.Log(text + " ReservationAuthorize call via KCS");
				ResvAuthRequest resvAuthRequest = new ResvAuthRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(session.StoreNumber),
					CreditCard = GetCreditCardData(session, includeTrack2: true),
					TransactionDate = DateTime.Now,
					ShoppingCart = ShoppingCartMessageHelper.CreateShoppingCart(sessionId),
					CustomerProfileNumber = customerProfileNumber,
					ReferenceNumber = referenceNumber.ToString()
				};
				LogHelper.Instance.Log($"ReservationAuthorizeRequest: {resvAuthRequest?.Scrub()}");
				authorizeResult = await CallReservationAuthorizeAsync(resvAuthRequest);
				if (authorizeResult == null)
				{
					authorizeResult = new AuthorizeResult
					{
						Success = false
					};
					authorizeResult.AddError(AuthorizeResponse.CommunicationError.GetResponseCodeAttributeName(), AuthorizeResponse.CommunicationError.ToString());
				}
				authorizeResult.Response = approvedString;
				authorizeResult.SessionId = sessionId;
				if (authorizeResult.Success && !string.IsNullOrEmpty(authorizeResult.ReferenceNumber))
				{
					session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(authorizeResult.ReferenceNumber));
				}
				string response = approvedString;
				if ((authorizeResult.Errors?.Count ?? 0) > 0)
				{
					Redbox.Rental.Model.Error error = authorizeResult.Errors[0];
					response = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), error?.Code ?? errorString)?.ToString() ?? errorString;
				}
				authorizeResult.IsOnline = AuthErrorCodeIndicatesOnline(response);
				if (authorizeResult.IsOnline)
				{
					authorizeResult.Response = response;
				}
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
			}
			else
			{
				LogHelper.Instance.Log("In stand alone mode, skipping realtime call to ReservationAuthorize.", LogEntryType.Info);
				authorizeResult.Success = true;
				authorizeResult.Response = approvedString;
				authorizeResult.IsOnline = false;
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
				authorizeResult.ConfirmationStatus = ConfirmationStatus.DontShow.ToString();
				new AutoResetEvent(initialState: false).WaitOne(1000);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceTransactions.ReservationAuthorize.", e);
			authorizeResult.Response = approvedString;
			authorizeResult.IsOnline = false;
			PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: true);
			return authorizeResult;
		}
		finally
		{
			try
			{
				LogHelper.Instance.Log($"ReservationAuthorize Result: {authorizeResult?.Scrub()}");
				LogHelper.Instance.Log($"ReservationAuthorize completed in {executionTimer.Elapsed} ms");
				authorizeResult.StandAlone = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
		return authorizeResult;
	}

	public void Return(string storeNumber, ReturnType returnType, string barcode, string returnDate, int deck, int slot, bool failedSecurityRead, string fraudFileName, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IReturnResult result = ReturnAsync(storeNumber, returnType, barcode, returnDate, deck, slot, failedSecurityRead, fraudFileName).Result;
			remoteServiceResult.Success = result.Success;
			completeCallback(remoteServiceResult);
		});
	}

	public async Task<IReturnResult> ReturnAsync(string storeNumber, ReturnType returnType, string barcode, string returnDate, int deck, int slot, bool failedSecurityRead, string fraudFileName)
	{
		IReturnResult result = null;
		try
		{
			ReturnRequest returnRequest = new ReturnRequest
			{
				MessageId = Guid.NewGuid(),
				KioskId = int.Parse(storeNumber),
				ReturnType = (byte)returnType,
				Barcode = barcode,
				ReturnDate = returnDate,
				Deck = deck,
				Slot = slot,
				FailedSecurityRead = failedSecurityRead
			};
			LogHelper.Instance.Log($"KioskClientServiceTransaction.ReturnAsync - sending request: {returnRequest}");
			WebResult<ReturnResult> webResult = await ExecuteAsyncWebRequest<ReturnResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/return"), returnRequest, "POST");
			LogResponse("KioskClientServiceTransactions.Return", webResult);
			return webResult.Response;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceTransactions.ReturnAsync.", e);
		}
		return result;
	}

	public void CancelReservation(string storeNumber, long referenceNumber, string reasonCode, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult result = new RemoteServiceResult
			{
				Success = (CancelReservationAsync(storeNumber, referenceNumber, reasonCode).Result?.Success ?? false)
			};
			if (callback != null)
			{
				callback(result);
			}
		});
	}

	public async Task<ICancelReservationResult> CancelReservationAsync(string storeNumber, long referenceNumber, string reasonCode)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		CancelReservationRequest cancelReservationRequest = new CancelReservationRequest
		{
			KioskId = long.Parse(storeNumber),
			ReferenceNumber = referenceNumber,
			MessageId = Guid.NewGuid(),
			ReasonCode = reasonCode
		};
		LogHelper.Instance.Log("Calling KCS api/transaction/CancelReservation with " + cancelReservationRequest.ToJson());
		WebResult<CancelReservationResponse> webResult = await ExecuteAsyncWebRequest<CancelReservationResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/CancelReservation"), cancelReservationRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.CancelReservation", webResult);
		return webResult?.Response;
	}

	public void ExpireReservation(string storeNumber, long referenceNumber, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult result = new RemoteServiceResult
			{
				Success = (ExpireReservationAsync(storeNumber, referenceNumber).Result?.Success ?? false)
			};
			if (callback != null)
			{
				callback(result);
			}
		});
	}

	public async Task<IExpireReservationResult> ExpireReservationAsync(string storeNumber, long referenceNumber)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		ExpireReservationRequest expireReservationRequest = new ExpireReservationRequest
		{
			KioskId = long.Parse(storeNumber),
			ReferenceNumber = referenceNumber,
			MessageId = Guid.NewGuid()
		};
		LogHelper.Instance.Log("Calling KCS api/transaction/ExpireReservation with " + expireReservationRequest.ToJson());
		WebResult<ExpireReservationResponse> webResult = await ExecuteAsyncWebRequest<ExpireReservationResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/ExpireReservation"), expireReservationRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.ExpireReservation", webResult);
		return webResult?.Response;
	}

	public void ValidatePickup(string sessionId, List<long> referenceNumbers, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IPickupValidationResult result = ValidatePickupAsync(sessionId, referenceNumbers).Result;
			AddPickupValidationPropertiesToResult(result, remoteServiceResult);
			remoteServiceResult.Success = result.Success;
			callback(remoteServiceResult);
		});
	}

	public async Task<IPickupValidationResult> ValidatePickupAsync(string sessionId, List<long> referenceNumbers)
	{
		PickupValidationResult result = new PickupValidationResult
		{
			IsOnline = true
		};
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionId);
		try
		{
			Interlocked.Increment(ref m_working);
			if (session == null)
			{
				result.Response = AuthorizeResponse.Error.ToString();
			}
			else if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
			{
				result.IsStandalone = false;
				LogHelper.Instance.Log("Live call to KCS ValidatePickup");
				ServiceLocator.Instance.GetService<IMacroService>();
				PickupValidationRequest validationRequest = new PickupValidationRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberInteger,
					CreditCard = GetCreditCardData(session, includeTrack2: true),
					ReferenceNumbers = referenceNumbers
				};
				DeserializablePickupValidationResult deserializablePickupValidationResult = await CallPickupValidationAsync(validationRequest);
				result.Success = deserializablePickupValidationResult?.Success ?? false;
				result.Response = AuthorizeResponse.Approved.ToString();
				result.IsOnline = deserializablePickupValidationResult != null && !deserializablePickupValidationResult.Errors.Any((Redbox.Rental.Model.Error error2) => error2.Code == "PV999");
				if (result.Success && (deserializablePickupValidationResult?.ReferenceNumbers?.Count).GetValueOrDefault() > 0)
				{
					session.ShoppingCart.SetReferenceNumber(deserializablePickupValidationResult.ReferenceNumbers.First());
					result.ReferenceNumbers = deserializablePickupValidationResult.ReferenceNumbers;
				}
				if (deserializablePickupValidationResult != null)
				{
					result.ValidReservations = deserializablePickupValidationResult.ValidReservations;
					result.AccountNumber = deserializablePickupValidationResult.AccountNumber;
					result.CustomerProfileNumber = deserializablePickupValidationResult.CustomerProfileNumber;
					if ((deserializablePickupValidationResult.TransactionEntities?.Count ?? 0) > 0)
					{
						AddTransactionEntitiesIntoTransactionDataSession(deserializablePickupValidationResult.TransactionEntities);
					}
					result.CancelledReservations = deserializablePickupValidationResult.CancelledReservations;
					result.AllowRop = deserializablePickupValidationResult.AllowRop;
					result.CancelReason = deserializablePickupValidationResult.Errors?.FirstOrDefault((Redbox.Rental.Model.Error error2) => error2.Code == "(A002)" || error2.Code == "(A038)")?.Message;
				}
				if (!result.Success && (result.Errors?.Count ?? 0) > 0)
				{
					List<Redbox.Rental.Model.Error> list = deserializablePickupValidationResult?.Errors;
					if (list != null && list.Count() > 0)
					{
						Redbox.Rental.Model.Error error = list[0];
						if (error != null)
						{
							object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), error.Code);
							result.Response = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : AuthorizeResponse.Error.ToString());
						}
					}
					else
					{
						result.Response = AuthorizeResponse.Error.ToString();
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("In stand alone mode, skipping realtime call to ValidatePickup.", LogEntryType.Info);
				result.Success = true;
				result.IsStandalone = true;
				result.Response = AuthorizeResponse.Approved.ToString();
				result.IsOnline = false;
				new AutoResetEvent(initialState: false).WaitOne(1000);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.ValidatePickup.", e);
			result.Success = true;
			result.Response = AuthorizeResponse.Approved.ToString();
			result.IsOnline = false;
		}
		finally
		{
			result.IsStandalone = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
			Interlocked.Decrement(ref m_working);
		}
		return result;
	}

	private void AddTransactionEntitiesIntoTransactionDataSession(List<TransactionEntity> transactionEntities)
	{
		try
		{
			ISession session = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
			if (transactionEntities != null)
			{
				foreach (TransactionEntity transactionEntity in transactionEntities)
				{
					Redbox.Rental.Model.Transaction.TransactionEntity item = new Redbox.Rental.Model.Transaction.TransactionEntity
					{
						DiscId = transactionEntity.DiscId.GetValueOrDefault(),
						Name = transactionEntity.Name,
						Value = transactionEntity.Value,
						TitleId = transactionEntity.TitleId.GetValueOrDefault()
					};
					session.TransactionData.TransactionEntities.Add(item);
				}
				return;
			}
			LogHelper.Instance.Log("No transaction entities found to add to the transaction data session");
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Exception occured in KioskClientTransaction.AddTransactionEntitiesIntoTransactionDataSession method. TransactionEntitiesObject: {transactionEntities}", e);
		}
	}

	public void Reconcile(string sessionId, string customerProfileNumber, RemoteServiceCallback callback)
	{
		ReconcileRequest request = BuildReconcileRequest(sessionId, customerProfileNumber);
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult result = new RemoteServiceResult
			{
				Success = (CallReconcileAsync(request).Result?.Success ?? false)
			};
			if (callback != null)
			{
				callback(result);
			}
		});
	}

	public async Task<IReconcileResult> ReconcileAsync(string sessionId, string customerProfileNumber)
	{
		ReconcileRequest request = BuildReconcileRequest(sessionId, customerProfileNumber);
		return await CallReconcileAsync(request);
	}

	private async Task<ReconcileResult> CallReconcileAsync(ReconcileRequest request)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<ReconcileResult> webResult = await ExecuteAsyncWebRequest<ReconcileResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/Reconcile"), request, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.Reconcile", webResult);
		LogHelper.Instance.Log($"Reconcile Response: {webResult?.Response?.Scrub()} ");
		return webResult?.Response;
	}

	private ReconcileRequest BuildReconcileRequest(string sessionId, string customerProfileNumber)
	{
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
		if (session == null)
		{
			LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
			return null;
		}
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
		ISession session2 = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		CustomerOffer acceptedOffer = session2?.PromoOfferState?.SelectedOffer;
		bool valueOrDefault = session2?.PromoOfferState?.WasOfferAccepted == true;
		IPromotion promo = rentalShoppingCart?.Promotion;
		bool flag = session.ShoppingCart.Type == ShoppingCartType.Offline;
		ReconcileRequest reconcileRequest = new ReconcileRequest
		{
			MessageId = Guid.NewGuid(),
			IsOffline = flag,
			Email = session.ShoppingCart.Email,
			KioskId = int.Parse(session.StoreNumber),
			ReferenceNumber = session.ShoppingCart.ReferenceNumber,
			CreditCard = GetCreditCardData(session, flag),
			TransactionDate = DateTime.Now,
			PlayPassPromptAccepted = session.ShoppingCart.PlayPassPromptAccepted,
			PlayPassPointsEarned = session.ShoppingCart.PlayPassPointsEarned,
			ShoppingCart = ShoppingCartMessageHelper.CreateShoppingCart(rentalShoppingCart, promo),
			CustomerProfileNumber = customerProfileNumber,
			UtcOffset = CalcUtcOffset(DateTime.Now),
			RedeemedCustomerOffer = (valueOrDefault ? BuildRedeemedOffer(acceptedOffer) : null),
			DiscOfferCodes = ToRequestOfferCodes(session2.PromoOfferState?.DiscOfferCodes),
			IsEarlyId = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdIndicator") == "true"),
			IsEarlyIdApple = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdApple") == "true"),
			IsEarlyIdGoogle = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdGoogle") == "true")
		};
		LogHelper.Instance.Log($"Reconcile Request: {reconcileRequest?.Scrub()}");
		return reconcileRequest;
	}

	private List<DiscOfferCode> ToRequestOfferCodes(List<IDiscOfferCode> promoStateOfferCodes)
	{
		List<DiscOfferCode> result = new List<DiscOfferCode>();
		if (promoStateOfferCodes != null)
		{
			result = promoStateOfferCodes.Select((IDiscOfferCode o) => new DiscOfferCode
			{
				Barcode = o.Barcode,
				OfferCode = o.OfferCode
			}).ToList();
		}
		return result;
	}

	private RedeemedOffer BuildRedeemedOffer(CustomerOffer acceptedOffer)
	{
		return new RedeemedOffer
		{
			OfferId = acceptedOffer.OfferId,
			StoredOfferId = acceptedOffer.StoredOfferId,
			CampaignId = acceptedOffer.CampaignId
		};
	}

	public void ReservationPickup(string sessionId, string customerProfileNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, RemoteServiceCallback callback)
	{
		ReservationPickupRequest request = BuildReservationPickupRequest(sessionId, customerProfileNumber, emailAddress, referenceNumber, pickupResults);
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult result = new RemoteServiceResult
			{
				Success = (CallReservationPickupAsync(request).Result?.Success ?? false)
			};
			if (callback != null)
			{
				callback(result);
			}
		});
	}

	public async Task<IReconcileResult> ReservationPickupAsync(string sessionId, string customerProfileNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults)
	{
		ReservationPickupRequest request = BuildReservationPickupRequest(sessionId, customerProfileNumber, emailAddress, referenceNumber, pickupResults);
		return await CallReservationPickupAsync(request);
	}

	private async Task<ReconcileResult> CallReservationPickupAsync(ReservationPickupRequest request)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<ReconcileResult> webResult = await ExecuteAsyncWebRequest<ReconcileResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/ReservationPickup"), request, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.Reconcile", webResult);
		LogHelper.Instance.Log($"Reconcile Response: {webResult?.Response?.Scrub()} ");
		return webResult?.Response;
	}

	private ReservationPickupRequest BuildReservationPickupRequest(string sessionId, string customerProfileNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults)
	{
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
		if (session == null)
		{
			LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
			return null;
		}
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		foreach (string key in pickupResults.Keys)
		{
			foreach (IProductItem result in pickupResults[key])
			{
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart.TitleItems.SingleOrDefault((IRentalShoppingCartTitleItem i) => i.Barcode == result.Barcode);
				if (rentalShoppingCartTitleItem != null)
				{
					rentalShoppingCartTitleItem.Status = result.Status;
				}
				else
				{
					LogHelper.Instance.LogError("Vend Reservation Mismatch", "The item with barcode " + result.Barcode + " was not found in the shopping cart.");
				}
			}
		}
		IPromotion promo = rentalShoppingCart?.Promotion;
		_ = session.ShoppingCart.Type;
		ReservationPickupRequest reservationPickupRequest = new ReservationPickupRequest
		{
			MessageId = Guid.NewGuid(),
			Email = emailAddress,
			KioskId = int.Parse(session.StoreNumber),
			ReferenceNumber = referenceNumber,
			CartWasModified = rentalShoppingCart.PickupRecommendationsSelected,
			PickupDateTime = DateTime.Now,
			CreditCard = GetCreditCardData(session, includeTrack2: true),
			ShoppingCart = ShoppingCartMessageHelper.CreateShoppingCart(rentalShoppingCart, promo),
			CustomerProfileNumber = customerProfileNumber,
			IsEarlyId = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdIndicator") == "true"),
			IsEarlyIdApple = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdApple") == "true"),
			IsEarlyIdGoogle = (session.ShoppingCart.GetPropertyBagEntry("EarlyIdGoogle") == "true")
		};
		LogHelper.Instance.Log($"Reservation Pickup Request: {reservationPickupRequest?.Scrub()}");
		return reservationPickupRequest;
	}

	public async Task<IGetCustomerOffersResult> GetCustomerOffersAsync(string kioskId, Dictionary<string, object> card, Dictionary<string, object> cart, string utcOffset, string lang, string storefrontEventType)
	{
		try
		{
			var anon = new
			{
				KioskId = kioskId,
				MessageId = Guid.NewGuid(),
				CreditCard = card,
				ShoppingCart = cart,
				UtcOffset = utcOffset,
				Lang = lang,
				StorefrontEventType = storefrontEventType
			};
			LogHelper.Instance.Log($"KioskClientServiceTransaction.GetCustomerOffersAsync - sending request with message id: {anon.MessageId}");
			return (await ExecuteAsyncWebRequest<GetCustomerOffersResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/getcustomeroffers"), anon, "POST")).Response;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceTransactions.GetCustomerOffersAsync.", e);
		}
		return null;
	}

	public void ReturnPromoCheck(string barcode, Action<ReturnVisitPromotionInfo> completeCallback)
	{
		Task.Run(delegate
		{
			ReturnVisitPromotionInfo result = ReturnPromoCheckAsync(barcode).Result;
			completeCallback(result);
		});
	}

	public async Task<ReturnVisitPromotionInfo> ReturnPromoCheckAsync(string barcode)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		ReturnVisitPromotionInfo result = new ReturnVisitPromotionInfo
		{
			Success = false
		};
		try
		{
			LogHelper.Instance.Log("KioskClientServiceTransaction.ReturnPromoCheckAsync - sending request for barcode: " + barcode);
			WebResult<ReturnPromoCheckResponse> webResult = await ExecuteAsyncWebRequest<ReturnPromoCheckResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/return/" + barcode + "/promocheck"), null, "GET", service.KioskSession.MarketingReturn.ReturnVisitPromoCheckTimeout);
			if (webResult.IsSuccessStatusCode)
			{
				result.Promo = webResult.Response.Promotion;
				result.Success = true;
				result.IsLastDisc = webResult.Response.IsLastDisc;
				return result;
			}
			LogHelper.Instance.Log(string.Format("KioskClientServiceTransaction.ReturnPromoCheckAsync - received result {0} message: {1}", webResult.StatusCode, webResult.Response?.Error ?? "null"));
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KioskClientServiceTransactions.ReturnPromoCheckAsync.", e);
		}
		return result;
	}

	private bool AuthErrorCodeIndicatesOnline(string response)
	{
		if (Enum.TryParse<AuthorizeResponse>(response, out var result))
		{
			if ((uint)(result - 26) <= 4u || result == AuthorizeResponse.CommunicationError)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	private async Task<AuthorizeResult> CallAuthorizeAsync(AuthorizeRequest authorizeRequest)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<AuthorizeResult> webResult = await ExecuteAsyncWebRequest<AuthorizeResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/Authorize"), authorizeRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.Authorize", webResult);
		return webResult?.Response;
	}

	private async Task<AuthorizeResult> CallGiftCardAuthorizeAsync(AuthorizeRequest authorizeRequest)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<GCAuthResult> webResult = await ExecuteAsyncWebRequest<GCAuthResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/GiftCardAuthorize"), authorizeRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.GiftCArdAuthorize", webResult);
		GCAuthResult gCAuthResult = webResult?.Response;
		if (gCAuthResult == null)
		{
			return null;
		}
		return new AuthorizeResult
		{
			MessageId = gCAuthResult.MessageId,
			SessionId = gCAuthResult.SessionId,
			AuthorizeType = gCAuthResult.AuthorizeType,
			IsOnline = gCAuthResult.IsOnline,
			StandAlone = gCAuthResult.StandAlone,
			ServerName = gCAuthResult.ServerName,
			Response = gCAuthResult.Response,
			Email = gCAuthResult.Email,
			ReferenceNumber = gCAuthResult.ReferenceNumber,
			HasProfile = gCAuthResult.HasProfile,
			IsRBI = gCAuthResult.IsRBI,
			FirstName = gCAuthResult.FirstName,
			LastName = gCAuthResult.LastName,
			ConfirmationStatus = gCAuthResult.ConfirmationStatus,
			AccountNumber = gCAuthResult.AccountNumber,
			CustomerProfileNumber = gCAuthResult.CustomerProfileNumber,
			StoreNumber = gCAuthResult.StoreNumber,
			GiftCardExists = (gCAuthResult.GiftCardSummary?.Exists ?? false),
			GiftCardStatus = (gCAuthResult.GiftCardSummary?.Status ?? GiftCardStatus.None),
			GiftCardBalance = (gCAuthResult.GiftCardSummary?.Balance ?? 0m),
			GiftCardMaxDiscs = (gCAuthResult.GiftCardSummary?.MaxDiscs ?? 0),
			KioskTransactionSuccess = gCAuthResult.KioskTransactionSuccess,
			SubscriptionTransactionSuccess = gCAuthResult.SubscriptionTransactionSuccess,
			Errors = gCAuthResult.Errors,
			HostName = gCAuthResult.HostName,
			StatusCode = gCAuthResult.StatusCode,
			Success = gCAuthResult.Success
		};
	}

	private async Task<AuthorizeResult> CallPickupAuthorizeAsync(PickupAuthRequest authorizeRequest)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<AuthorizeResult> webResult = await ExecuteAsyncWebRequest<AuthorizeResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/PickupAuthorize"), authorizeRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.PickupAuthorize", webResult);
		return webResult?.Response;
	}

	private async Task<AuthorizeResult> CallReservationAuthorizeAsync(ResvAuthRequest authorizeRequest)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<AuthorizeResult> webResult = await ExecuteAsyncWebRequest<AuthorizeResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/ReservationAuthorize"), authorizeRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.ReservationAuthorize", webResult);
		return webResult?.Response;
	}

	private async Task<DeserializablePickupValidationResult> CallPickupValidationAsync(PickupValidationRequest validationRequest)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		WebResult<DeserializablePickupValidationResult> webResult = await ExecuteAsyncWebRequest<DeserializablePickupValidationResult>(new Uri(base.KioskClientServiceBaseUrl + "/api/transaction/ValidatePickup"), validationRequest, "POST", service.AuthorizeTimeout);
		LogResponse("KioskClientServiceTransactions.CallPickupValidationAsync", webResult);
		return webResult?.Response;
	}

	private void PopulateFirstNameAndLastNameFromTrackData(IAuthorizeResult authorizeResult, ITrackData trackData)
	{
		PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: false);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(IAuthorizeResult authorizeResult, ITrackData trackData, bool defaultToNA)
	{
		authorizeResult.FirstName = (((trackData == null) & defaultToNA) ? "NA" : trackData?.FirstName);
		authorizeResult.LastName = (((trackData == null) & defaultToNA) ? "NA" : trackData?.LastName);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(RemoteServiceResult result, ITrackData trackData)
	{
		PopulateFirstNameAndLastNameFromTrackData(result, trackData, defaultToNA: false);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(RemoteServiceResult result, ITrackData trackData, bool defaultToNA)
	{
		result.Properties["first_name"] = (((trackData == null) & defaultToNA) ? "NA" : trackData?.FirstName);
		result.Properties["last_name"] = (((trackData == null) & defaultToNA) ? "NA" : trackData?.LastName);
	}

	private CreditCard GetCreditCardData(IShoppingSession session, bool includeTrack2)
	{
		ISession session2 = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ITrackData trackData = session2?.TrackData;
		IUnencryptedTrackData unencryptedTrackData = trackData as IUnencryptedTrackData;
		IEncryptedTrackData encryptedTrackData = trackData as IEncryptedTrackData;
		IEMVTrackData iEMVTrackData = trackData as IEMVTrackData;
		CreditCard creditCard = new CreditCard
		{
			KeyId = unencryptedTrackData?.EncryptionCerificateKeyId,
			Number = unencryptedTrackData?.EncryptedAccountNumber,
			CardType = (byte)(trackData?.CardType).GetValueOrDefault(),
			CardId = trackData?.CardHashId,
			LastFour = (encryptedTrackData?.LastFour ?? trackData?.LastFour),
			LastName = trackData?.LastName,
			FirstName = trackData?.FirstName,
			PostalCode = session?.ShoppingCart?.PostalCode,
			ExpirationYear = trackData?.ExpiryYear,
			ExpirationMonth = trackData?.ExpiryMonth,
			IsEncrypted = (encryptedTrackData != null),
			BIN = trackData?.FirstSix,
			KSN = encryptedTrackData?.KSN,
			ICEncryptedData = encryptedTrackData?.ICEncData,
			ReaderSerialNumber = encryptedTrackData?.MfgSerialNumber,
			EmvEnabledTerminal = (trackData?.EmvEnabled ?? false),
			EmvTags = iEMVTrackData?.Tags,
			FallbackType = (byte?)((trackData == null || !trackData.IsInTechnicalFallback || trackData.CardSourceType != CardSourceType.Swipe || !trackData.CardHasChip) ? ((FallbackType?)null) : session2?.LastFallbackType),
			CardSourceType = (CardSourceTypeEnum)(trackData?.CardSourceType ?? CardSourceType.Unknown),
			ContactlessEnabled = (trackData?.ContactlessEnabledAndSupportsEmv ?? false)
		};
		if (includeTrack2)
		{
			creditCard.Track2 = ExtractFromSecure(unencryptedTrackData?.EncryptedTrack2);
		}
		return creditCard;
	}

	private static string ExtractFromSecure(SecureString value)
	{
		try
		{
			return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
		}
		catch
		{
			return string.Empty;
		}
	}

	private static string CalcUtcOffset(DateTime now)
	{
		TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(now);
		return utcOffset.TotalHours.ToString("00") + ":" + utcOffset.Minutes.ToString("00");
	}

	private void AddPropertiesToResult(IAuthorizeResult authorizeResult, RemoteServiceResult result)
	{
		result.Properties["store_number"] = authorizeResult.StoreNumber;
		result.Properties["server_name"] = authorizeResult.ServerName;
		result.Properties["response"] = authorizeResult.Response;
		result.Properties["is_online"] = authorizeResult.IsOnline;
		result.Properties["email"] = authorizeResult.Email;
		result.Properties["reference_number"] = authorizeResult.ReferenceNumber;
		result.Properties["first_name"] = authorizeResult.FirstName;
		result.Properties["last_name"] = authorizeResult.LastName;
		result.Properties["has_profile"] = authorizeResult.HasProfile;
		result.Properties["is_rbi"] = authorizeResult.IsRBI;
		result.Properties["confirmation_status"] = authorizeResult.ConfirmationStatus;
		result.Properties["account_number"] = authorizeResult.AccountNumber;
		result.Properties["customer_profile_number"] = authorizeResult.CustomerProfileNumber;
	}

	private void AddPickupValidationPropertiesToResult(IPickupValidationResult validateResult, RemoteServiceResult result)
	{
		result.Properties["customer_profile_number"] = validateResult.CustomerProfileNumber;
		result.Properties["response"] = validateResult.Response;
		result.Properties["is_online"] = validateResult.IsOnline;
		result.Properties["stand_alone"] = validateResult.IsStandalone;
		result.Properties["validated_reservations"] = validateResult.ValidReservations?.ToLuaTable();
		result.Properties["cancelled_reservations"] = validateResult.CancelledReservations?.ToLuaTable();
		result.Properties["allow_rop"] = validateResult.AllowRop;
		result.Properties["cancel_reason"] = validateResult.CancelReason;
		result.Properties["reference_number"] = validateResult.ReferenceNumbers?.FirstOrDefault();
	}
}
