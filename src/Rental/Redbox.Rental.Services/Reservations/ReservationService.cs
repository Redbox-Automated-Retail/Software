using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.UpdateClientService;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.UpdateClientService.Reservation;

namespace Redbox.Rental.Services.Reservations;

public class ReservationService : IReservationService
{
	private DateTime? _brokerServiceRegistrationDateTime;

	private int _retryRegisterInterval;

	private readonly object _retryRegisterLock = new object();

	private System.Timers.Timer _retryRegisterTimer;

	public bool ReservationCreationEnabled { get; set; }

	public bool IsRegisteredWithBrokerService => _brokerServiceRegistrationDateTime.HasValue;

	public DateTime? BrokerServiceRegistrationDateTime => _brokerServiceRegistrationDateTime;

	public async Task<GetReservationServiceStatusResponse> GetReservationServiceStatus()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		GetReservationServiceStatusResponse response = new GetReservationServiceStatusResponse();
		await Task.Factory.StartNew(delegate
		{
			try
			{
				IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
				IMacroService service3 = ServiceLocator.Instance.GetService<IMacroService>();
				response.Content = new ReservationServiceStatus
				{
					IsRegisteredWithBrokerService = IsRegisteredWithBrokerService,
					BrokerServiceRegistrationDateTime = BrokerServiceRegistrationDateTime,
					AuthorizeAtPickup = (service2?.AuthorizeAtPickup ?? false),
					GiftCardsEnabled = (service2?.GiftCardsEnabled ?? false),
					RecommendationOnPickupEnabled = (service2?.RecommendationOnPickup ?? false),
					KioskEngineVersion = service3["EngineVersion"]
				};
				response.Success = true;
				response.StatusCode = HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = ex?.ToString()
				});
			}
			LogHelper.Instance.Log("ReservationService.GetReservationServiceStatus result=" + JsonConvert.SerializeObject(response));
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		return response;
	}

	public async Task<GetReservationsResponse> GetReservations()
	{
		LogHelper.Instance.Log("Starting ReservationService.GetReservations");
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		GetReservationsResponse response = new GetReservationsResponse
		{
			Content = new List<IReservation>()
		};
		await Task.Factory.StartNew(delegate
		{
			response.Content = ReservationFunctions.GetAllReservations();
			response.StatusCode = ((response.Content != null && response.Content.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.NotFound);
			response.Success = true;
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		LogHelper.Instance.Log("ReservationService.GetReservations result=" + JsonConvert.SerializeObject(response));
		return response;
	}

	public async Task<GetReservableTitlesResponse> GetReservableTitles()
	{
		LogHelper.Instance.Log("Starting ReservationService.GetReservableTitles");
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		GetReservableTitlesResponse response = new GetReservableTitlesResponse
		{
			Content = new List<ReservableTitle>()
		};
		await Task.Factory.StartNew(delegate
		{
			List<IKioskProduct> products = new List<IKioskProduct>();
			ServiceLocator.Instance.GetService<IDataService>()?.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.Alpha, null, null, null, null, isADAView: true, null, null, out products);
			if (products != null)
			{
				foreach (IKioskProduct item in products)
				{
					ITitleProduct titleProduct = item as ITitleProduct;
					if (titleProduct.InStock)
					{
						response.Content.Add(new ReservableTitle
						{
							ProductId = titleProduct.ProductId,
							TitleType = titleProduct.TitleType,
							TitleFamily = titleProduct.TitleFamily,
							LongName = titleProduct.LongName,
							BarcodeCount = (titleProduct.BarCodeProducts?.Count ?? 0),
							PurchasePrice = titleProduct.PurchasePrice,
							OneNightRentalPrice = (titleProduct.RentalPriceRecords?.FirstOrDefault((IPricingRecord x) => !x.InitialDays.HasValue || x.InitialDays == 1)?.InitialNight).GetValueOrDefault()
						});
					}
				}
			}
			response.StatusCode = ((response.Content != null && response.Content.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.NotFound);
			response.Success = true;
			LogHelper.Instance.Log("ReservationService.GetReservableTitles result=" + JsonConvert.SerializeObject(response));
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		return response;
	}

	public async Task<CreateReservationResponse> CreateReservation(ReservationRequest reservationRequest)
	{
		LogHelper.Instance.Log("Starting ReservationService.CreateReservation");
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		CreateReservationResponse response = new CreateReservationResponse();
		await Task.Factory.StartNew(delegate
		{
			LogHelper.Instance.Log("ReservationService.CreateReservation request=" + JsonConvert.SerializeObject(reservationRequest));
			IReservationRequestResult reservationRequestResult = ReservationFunctions.CreateReservation(reservationRequest);
			response.Success = reservationRequestResult != null && reservationRequestResult.Status == ReservationRequestStatus.ReservationCreated;
			response.Content = reservationRequestResult;
			if (reservationRequestResult != null)
			{
				switch (reservationRequestResult.Status)
				{
				case ReservationRequestStatus.InvalidRequest:
					response.StatusCode = HttpStatusCode.BadRequest;
					break;
				case ReservationRequestStatus.SystemError:
					response.StatusCode = HttpStatusCode.InternalServerError;
					break;
				case ReservationRequestStatus.TitleNotAvailable:
					response.StatusCode = HttpStatusCode.NotFound;
					break;
				case ReservationRequestStatus.ReservationCreated:
					response.StatusCode = HttpStatusCode.OK;
					break;
				default:
					response.StatusCode = HttpStatusCode.InternalServerError;
					break;
				}
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
			if (!string.IsNullOrEmpty(reservationRequestResult?.ErrorMessage))
			{
				response.Errors.Add(new ResponseError
				{
					Code = response.StatusCode.ToString(),
					Message = reservationRequestResult?.ErrorMessage
				});
			}
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		LogHelper.Instance.Log("ReservationService.CreateReservation result=" + JsonConvert.SerializeObject(response));
		return response;
	}

	public async Task<CancelReservationResponse> CancelReservation(string referenceNumber)
	{
		CancelReservationResponse response = new CancelReservationResponse
		{
			Success = false
		};
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		await Task.Factory.StartNew(delegate
		{
			LogHelper.Instance.Log("ReservationService.CreateReservation referenceNumber=" + referenceNumber);
			if (string.IsNullOrEmpty(referenceNumber))
			{
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new ResponseError
				{
					Code = "400",
					Message = "Reference number parameter must not be nuull or empty."
				});
			}
			else
			{
				switch (ReservationFunctions.CancelLocalAndServerReservation(referenceNumber))
				{
				case ReservationFunctions.ReservationCancelResult.ReservationCancelled:
					response.StatusCode = HttpStatusCode.OK;
					response.Success = true;
					break;
				case ReservationFunctions.ReservationCancelResult.ReservationNotFound:
					response.StatusCode = HttpStatusCode.NotFound;
					response.Errors.Add(new ResponseError
					{
						Code = HttpStatusCode.NotFound.ToString(),
						Message = "Reservation not found for reference number " + referenceNumber + "."
					});
					break;
				case ReservationFunctions.ReservationCancelResult.Error:
					response.StatusCode = HttpStatusCode.InternalServerError;
					response.Errors.Add(new ResponseError
					{
						Code = "500",
						Message = "Error in ReservationFunctions.CancelReservation."
					});
					break;
				}
			}
			LogHelper.Instance.Log("ReservationService.CancelReservation result=" + JsonConvert.SerializeObject(response));
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		return response;
	}

	public void RegisterWithAWSBrokerService()
	{
		Task.Run(async delegate
		{
			await RegisterWithAWSBrokerServiceAsync();
		});
	}

	public async Task<IRegisterWithBrokerServiceResponse> RegisterWithAWSBrokerServiceAsync()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		IRegisterWithBrokerServiceResponse registerWithBrokerServiceResponse = new RegisterWithBrokerServiceResponse
		{
			StatusCode = 500,
			Success = false
		};
		LogHelper.Instance.Log("ReservationService.RegisterWithAWSBrokerServiceAsync");
		if (!(ServiceLocator.Instance.GetService<IConfiguration>()?.UseAWSBrokerService ?? false))
		{
			LogHelper.Instance.Log("RegisterWithAWSBrokerService aborted.  Config setting UseAWSBrokerService not enabled.");
			registerWithBrokerServiceResponse = new RegisterWithBrokerServiceResponse
			{
				StatusCode = 409,
				Success = false,
				Errors = new List<Redbox.Rental.Model.Error>
				{
					new Redbox.Rental.Model.Error
					{
						Code = HttpStatusCode.Conflict.ToString(),
						Message = "Config setting UseAWSBrokerService not enabled."
					}
				}
			};
		}
		else if (IsRegisteredWithBrokerService)
		{
			LogHelper.Instance.Log($"RegisterWithAWSBrokerService aborted.  Already registered at {_brokerServiceRegistrationDateTime}");
			registerWithBrokerServiceResponse = new RegisterWithBrokerServiceResponse
			{
				StatusCode = 409,
				Success = false,
				Errors = new List<Redbox.Rental.Model.Error>
				{
					new Redbox.Rental.Model.Error
					{
						Code = HttpStatusCode.Conflict.ToString(),
						Message = "Kiosk Engine is already registered with Broker Service."
					}
				}
			};
		}
		else
		{
			long num = service?.KioskId ?? 0;
			if (num > 0)
			{
				RegisterWithBrokerServiceRequest registerWithBrokerServiceRequest = new RegisterWithBrokerServiceRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = num
				};
				registerWithBrokerServiceResponse = await (ServiceLocator.Instance.GetService<IUpdateClientService>()?.Reservation?.RegisterWithBrokerService(registerWithBrokerServiceRequest));
				if (registerWithBrokerServiceResponse != null && registerWithBrokerServiceResponse.Success)
				{
					_brokerServiceRegistrationDateTime = DateTime.Now;
					ReservationCreationEnabled = true;
					LogHelper.Instance.Log("RegisterWithAWSBrokerService successful");
				}
			}
			else
			{
				LogHelper.Instance.Log("RegisterWithAWSBrokerService: unable to register because of invalid kiosk id.");
			}
			if (registerWithBrokerServiceResponse == null || !registerWithBrokerServiceResponse.Success)
			{
				Task.Run(delegate
				{
					StartRetryRegisterTimer();
				});
			}
		}
		return registerWithBrokerServiceResponse;
	}

	public void UnRegisterFromAWSBrokerService(UnRegisterFromBrokerServiceReason reason)
	{
		Task.Run(async delegate
		{
			await UnRegisterFromAWSBrokerServiceAsync(reason);
		});
	}

	public async Task<IUnregisterFromBrokerServiceResponse> UnRegisterFromAWSBrokerServiceAsync(UnRegisterFromBrokerServiceReason reason)
	{
		LogHelper.Instance.Log("ReservationService.UnRegisterFromAWSBrokerServiceAsync");
		IUnregisterFromBrokerServiceResponse result = null;
		if (!(ServiceLocator.Instance.GetService<IConfiguration>()?.UseAWSBrokerService ?? false))
		{
			LogHelper.Instance.Log("UnRegisterFromAWSBrokerServiceAsync aborted.  Config setting UseAWSBrokerService not enabled.");
			result = new UnregisterFromBrokerServiceResponse
			{
				StatusCode = 409,
				Success = false,
				Errors = new List<Redbox.Rental.Model.Error>
				{
					new Redbox.Rental.Model.Error
					{
						Code = HttpStatusCode.Conflict.ToString(),
						Message = "Config setting UseAWSBrokerService not enabled."
					}
				}
			};
		}
		else
		{
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
			CancelRetryRegisterTimer();
			if (IsRegisteredWithBrokerService)
			{
				UnregisterFromBrokerServiceRequest unregisterWithBrokerServiceRequest = new UnregisterFromBrokerServiceRequest
				{
					MessageId = Guid.NewGuid(),
					KioskId = (service?.KioskId ?? 0),
					AppName = service2["ProductName"],
					Reason = reason.ToString()
				};
				result = await (ServiceLocator.Instance.GetService<IUpdateClientService>()?.Reservation?.UnregisterFromBrokerService(unregisterWithBrokerServiceRequest));
			}
			else
			{
				LogHelper.Instance.Log("Skipped sending BrokerService Unregister message because kiosk is not currently registered.");
			}
			ReservationCreationEnabled = false;
			_brokerServiceRegistrationDateTime = null;
			LogHelper.Instance.Log("ReservationService disabled.");
		}
		return result;
	}

	public void StartRetryRegisterTimer()
	{
		lock (_retryRegisterLock)
		{
			if (_retryRegisterTimer == null)
			{
				_retryRegisterTimer = new System.Timers.Timer
				{
					AutoReset = false
				};
				_retryRegisterTimer.Elapsed += RetryRegisterTimerElapsed;
			}
			if (!_retryRegisterTimer.Enabled)
			{
				_retryRegisterTimer.Interval = GetRetryRegisterInterval();
				LogHelper.Instance.Log($"Starting ReservationService retry register timer.  interval = {_retryRegisterTimer.Interval}");
				_retryRegisterTimer.Enabled = true;
			}
		}
	}

	private void CancelRetryRegisterTimer()
	{
		lock (_retryRegisterLock)
		{
			System.Timers.Timer retryRegisterTimer = _retryRegisterTimer;
			if (retryRegisterTimer != null && retryRegisterTimer.Enabled)
			{
				_retryRegisterTimer.Enabled = false;
				LogHelper.Instance.Log("Canceled ReservationService retry register timer.");
			}
			ResetRetryRegisterInterval();
		}
	}

	private void RetryRegisterTimerElapsed(object sender, ElapsedEventArgs e)
	{
		RegisterWithAWSBrokerService();
	}

	private int GetRetryRegisterInterval()
	{
		if (_retryRegisterInterval == 0)
		{
			_retryRegisterInterval = 5000;
		}
		else
		{
			_retryRegisterInterval *= 2;
		}
		int num = 300000;
		if (_retryRegisterInterval > num)
		{
			_retryRegisterInterval = num;
		}
		return _retryRegisterInterval;
	}

	private void ResetRetryRegisterInterval()
	{
		_retryRegisterInterval = 0;
	}
}
