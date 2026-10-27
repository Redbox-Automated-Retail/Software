using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;

namespace DeviceService.Domain;

public class ReadCardContext
{
	private readonly ILogger<IUC285Proxy> _logger;

	private const int DEFAULT_TIMEOUT = 30000;

	private bool CompleteCallbackSent;

	public Guid Id { get; set; }

	public ICardReadRequest Request { get; set; }

	public int? UserInteractionTimeout { get; set; }

	public int TimeRemainingUntilTimeout
	{
		get
		{
			int? num = Request?.Timeout;
			if (_sentCardProcessing)
			{
				num = UserInteractionTimeout;
			}
			return Math.Max(0, (int)((num ?? 30000) - (Watch?.ElapsedMilliseconds ?? 0)));
		}
	}

	public CancellationToken CancelToken { get; set; }

	public Action<Base87CardReadModel> JobCompletedCallback { get; set; }

	public Action<string, string> EventsCallback { get; set; }

	public CardSourceType CardSource { get; set; }

	public Stopwatch Watch { get; set; }

	public IList<Error> Errors { get; set; } = new List<Error>();

	public ReadCardContextState State { get; set; } = ReadCardContextState.Continue;

	public Base87CardReadModel ResponseDataModel { get; set; }

	public bool CardRemoved { get; set; }

	public CardBrandEnum CardBrand { get; set; }

	public CardStats CardStats { get; set; }

	public UnitDataModel UnitData { get; set; }

	public CardReadExitType ExitType { get; set; } = CardReadExitType.NotPerformed;

	public bool IsVasEnabled
	{
		get
		{
			if (Request.VasMode != VasMode.VasOnly)
			{
				return Request.VasMode == VasMode.VasAndPay;
			}
			return true;
		}
	}

	public bool IsPayEnabled
	{
		get
		{
			if (Request.VasMode != VasMode.PayOnly)
			{
				return Request.VasMode == VasMode.VasAndPay;
			}
			return true;
		}
	}

	public string VasData { get; set; }

	public WalletType Wallet { get; set; }

	private bool _sentCardProcessing { get; set; }

	public ReadCardContext(ILogger<IUC285Proxy> logger)
	{
		_logger = logger;
		Watch = new Stopwatch();
		Watch.Start();
		Id = Guid.NewGuid();
	}

	public bool CanContinue(VasMode mode)
	{
		bool result = false;
		switch (mode)
		{
		case VasMode.PayOnly:
			result = IsPayEnabled && State.HasFlag(ReadCardContextState.PayContinue);
			break;
		case VasMode.VasOnly:
			result = IsVasEnabled && State.HasFlag(ReadCardContextState.VasContinue);
			break;
		case VasMode.VasAndPay:
			result = State == ReadCardContextState.Continue || (IsVasEnabled && State == ReadCardContextState.VasContinue) || (IsPayEnabled && State == ReadCardContextState.PayContinue);
			break;
		}
		return result;
	}

	public void UpdateStopStateFor(VasMode mode)
	{
		ReadCardContextState state = State;
		switch (mode)
		{
		case VasMode.VasOnly:
			if (State == ReadCardContextState.VasContinue)
			{
				State = ReadCardContextState.Stop;
			}
			else if (State == ReadCardContextState.Continue)
			{
				State = ReadCardContextState.PayContinue;
			}
			break;
		case VasMode.PayOnly:
			if (State == ReadCardContextState.PayContinue)
			{
				State = ReadCardContextState.Stop;
			}
			else if (State == ReadCardContextState.Continue)
			{
				State = ReadCardContextState.VasContinue;
			}
			break;
		case VasMode.VasAndPay:
			if (State.HasFlag(ReadCardContextState.Continue))
			{
				State = ReadCardContextState.Stop;
			}
			break;
		}
		_logger?.LogInformation($"Update Stop State for VasMode: {mode} - State Change From: {state} To: {State}");
	}

	public void SendCompletedCallback()
	{
		if (CompleteCallbackSent)
		{
			return;
		}
		CompleteCallbackSent = true;
		_logger?.LogInformation($"Send Card Read Model - Callback Exists: {JobCompletedCallback != null}, Model Exists: {ResponseDataModel != null}");
		Watch.Stop();
		if (ResponseDataModel == null || CardBrandAndSourceIsDeactivated())
		{
			switch (CardSource)
			{
			case CardSourceType.VASOnly:
				ResponseDataModel = new EMVCardReadModel();
				break;
			case CardSourceType.EMVContact:
			case CardSourceType.QuickChip:
				ResponseDataModel = new EMVCardReadModel();
				break;
			case CardSourceType.EMVContactless:
			case CardSourceType.Mobile:
				ResponseDataModel = new EMVCardReadModel();
				break;
			default:
				ResponseDataModel = new EncryptedCardReadModel();
				break;
			}
			switch (State)
			{
			case ReadCardContextState.Tampered:
				Errors.Add(new Error
				{
					Code = "TAMPERED",
					Message = "Device is reporting Tampered Alert."
				});
				ResponseDataModel.Status = ResponseStatus.Tampered;
				break;
			case ReadCardContextState.Canceled:
				Errors.Add(new Error
				{
					Code = "CANCELLED",
					Message = "Command Was Cancelled."
				});
				ResponseDataModel.Status = ResponseStatus.Cancelled;
				break;
			case ReadCardContextState.Timeout:
				Errors.Add(new Error
				{
					Code = "TIMEOUT",
					Message = $"Command Timed Out.  Exceeded {Request?.Timeout ?? 30000} Timeout"
				});
				ResponseDataModel.Status = ResponseStatus.Timeout;
				break;
			case ReadCardContextState.Blocked:
				Errors.Add(new Error
				{
					Code = "BLOCKED",
					Message = $"Card brand {CardBrand} and it's source {CardSource} is disabled in system.config"
				});
				ResponseDataModel.Status = ResponseStatus.Blocked;
				break;
			default:
				if (!IsVasEnabled || VasData == null)
				{
					ResponseDataModel.Status = ResponseStatus.Errored;
					Errors.Add(new Error
					{
						Code = "SCC001",
						Message = "Sending Default Error, ResponseDataModel Missing."
					});
				}
				break;
			}
		}
		if (ResponseDataModel is EMVCardReadModel)
		{
			((EMVCardReadModel)ResponseDataModel).CardRemoved = CardRemoved;
		}
		if (IsPayEnabled && (CardSource == CardSourceType.EMVContactless || CardSource == CardSourceType.MSDContactless) && Wallet != WalletType.None)
		{
			CardSource = CardSourceType.Mobile;
		}
		ResponseDataModel.TimeTaken = Watch.Elapsed;
		ResponseDataModel.AddErrors(Errors);
		ResponseDataModel.CardSource = CardSource;
		ResponseDataModel.VasData = VasData;
		ResponseDataModel.WalletFormat = Wallet;
		if (ResponseDataModel.Status == ResponseStatus.Success && ResponseDataModel.Errors.Any())
		{
			ResponseDataModel.Status = ResponseStatus.Errored;
		}
		CardStats = new CardStats
		{
			SessionId = Request?.SessionId,
			Id = Guid.NewGuid(),
			CardBrand = CardBrand,
			ManufacturerSerialNumber = UnitData?.ManufacturingSerialNumber,
			RBAVersion = UnitData?.GetApplicationVersionString(),
			ReadResult = ExitType,
			SourceType = CardSource,
			ErrorCode = ResponseDataModel.ErrorCode,
			WalletFormat = Wallet,
			HasPayData = ResponseDataModel.HasPayData,
			HasVasData = ResponseDataModel.HasVasData,
			VasErrorCode = ResponseDataModel.VasErrorCode
		};
		JobCompletedCallback?.Invoke(ResponseDataModel);
	}

	public void SendCardRemoved()
	{
		CardRemoved = true;
		EventsCallback?.Invoke("CardRemovedResponseEvent", null);
	}

	public void SendCardProcessing()
	{
		if (!_sentCardProcessing)
		{
			EventsCallback?.Invoke("CardProcessingStartedResponseEvent", CardSource.ToString());
			_sentCardProcessing = true;
			Watch.Restart();
			_logger.LogInformation($"Set New Timeout: {UserInteractionTimeout}");
		}
	}

	public bool CardBrandAndSourceIsDeactivated()
	{
		if (Request.ExcludeCardBrandBySource == null)
		{
			return false;
		}
		bool flag = false;
		CardBrandAndSource? cardBrandAndSource = null;
		foreach (CardBrandAndSource item in Request.ExcludeCardBrandBySource)
		{
			GeneralSourceType generalSourceType = item.GetGeneralSourceType();
			GeneralSourceType generalSourceType2 = CardSource.GetGeneralSourceType();
			if (generalSourceType == generalSourceType2 && generalSourceType != GeneralSourceType.None && item.GetBrand() == CardBrand)
			{
				flag = true;
				cardBrandAndSource = item;
				break;
			}
		}
		if (flag)
		{
			State = ReadCardContextState.Blocked;
			_logger?.LogInformation($"{cardBrandAndSource} is deactivated in system.config");
		}
		return flag;
	}
}
