using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public abstract class TrackData : ITrackData
{
	private CardSourceType _cardSourceType;

	private ErrorList _errors;

	private string _firstSix;

	private string _lastFour;

	private string _lastName;

	private Redbox.Core.CardType? _cardType;

	private string _firstName;

	private string _expiryYear;

	private string _expiryMonth;

	protected string _cardHashId;

	private string _reservationHashId;

	private bool _cardHasChip;

	private bool _isInTechnicalFallback;

	private bool _nextCardReadIsInTechnicalFallback;

	private FallbackType? _lastFallbackReason;

	private FallbackStatusAction _fallbackStatusAction;

	private readonly bool _chipEnabled;

	private readonly bool _contactlessEnabled;

	protected bool _isDataLocked;

	public ErrorList Errors
	{
		get
		{
			return _errors;
		}
		set
		{
			if (!_isDataLocked)
			{
				_errors = value;
			}
		}
	}

	public string FirstSix
	{
		get
		{
			return _firstSix;
		}
		set
		{
			if (!_isDataLocked)
			{
				_firstSix = value;
				if (this is EncryptedTrackData && !string.IsNullOrEmpty(_firstSix) && !string.IsNullOrEmpty(_lastFour))
				{
					_cardHashId = CardHelper.GenerateReservationCardId(_firstSix, _lastFour);
				}
			}
		}
	}

	public string LastFour
	{
		get
		{
			return _lastFour;
		}
		set
		{
			if (!_isDataLocked)
			{
				_lastFour = value;
				if (this is EncryptedTrackData && !string.IsNullOrEmpty(_firstSix) && !string.IsNullOrEmpty(_lastFour))
				{
					_cardHashId = CardHelper.GenerateReservationCardId(_firstSix, _lastFour);
				}
			}
		}
	}

	public string LastName
	{
		get
		{
			return _lastName;
		}
		set
		{
			if (!_isDataLocked)
			{
				_lastName = value;
			}
		}
	}

	public Redbox.Core.CardType? CardType
	{
		get
		{
			return _cardType;
		}
		set
		{
			if (!_isDataLocked)
			{
				_cardType = value;
			}
		}
	}

	public string FirstName
	{
		get
		{
			return _firstName;
		}
		set
		{
			if (!_isDataLocked)
			{
				_firstName = value;
			}
		}
	}

	public string ExpiryYear
	{
		get
		{
			return _expiryYear;
		}
		set
		{
			if (!_isDataLocked)
			{
				_expiryYear = value;
			}
		}
	}

	public string ExpiryMonth
	{
		get
		{
			return _expiryMonth;
		}
		set
		{
			if (!_isDataLocked)
			{
				_expiryMonth = value;
			}
		}
	}

	public string CardHashId => _cardHashId;

	public string ReservationHashId
	{
		get
		{
			if (_reservationHashId == null && !string.IsNullOrEmpty(_firstSix) && !string.IsNullOrEmpty(_lastFour))
			{
				_reservationHashId = CardHelper.GenerateReservationCardId(_firstSix, _lastFour);
			}
			return _reservationHashId;
		}
	}

	public CardSourceType CardSourceType
	{
		get
		{
			return _cardSourceType;
		}
		set
		{
			if (!_isDataLocked)
			{
				_cardSourceType = value;
			}
		}
	}

	public FallbackStatusAction FallbackStatusAction
	{
		get
		{
			return _fallbackStatusAction;
		}
		set
		{
			if (!_isDataLocked)
			{
				_fallbackStatusAction = value;
			}
		}
	}

	public FallbackType? FallbackReason { get; set; }

	public FallbackType? LastFallbackReason
	{
		get
		{
			return _lastFallbackReason;
		}
		set
		{
			if (!_isDataLocked)
			{
				_lastFallbackReason = value;
			}
		}
	}

	public bool IsInTechnicalFallback
	{
		get
		{
			return _isInTechnicalFallback;
		}
		set
		{
			if (!_isDataLocked)
			{
				_isInTechnicalFallback = value;
			}
		}
	}

	public bool NextCardReadIsInTechnicalFallback
	{
		get
		{
			return _nextCardReadIsInTechnicalFallback;
		}
		set
		{
			if (!_isDataLocked)
			{
				_nextCardReadIsInTechnicalFallback = value;
			}
		}
	}

	public bool CardHasChip
	{
		get
		{
			return _cardHasChip;
		}
		set
		{
			if (!_isDataLocked)
			{
				_cardHasChip = value;
			}
		}
	}

	public ResponseStatus? ReadStatus { get; set; }

	public bool ChipEnabledAndSupportsEmv => _chipEnabled;

	public bool ContactlessEnabledAndSupportsEmv => _contactlessEnabled;

	public bool EmvEnabled
	{
		get
		{
			if (!ChipEnabledAndSupportsEmv)
			{
				return ContactlessEnabledAndSupportsEmv;
			}
			return true;
		}
	}

	public string VasIdentifier { get; set; }

	public bool HasVas => !string.IsNullOrEmpty(VasIdentifier);

	public bool HasPay
	{
		get
		{
			if (!string.IsNullOrEmpty(FirstSix))
			{
				return !string.IsNullOrEmpty(LastFour);
			}
			return false;
		}
	}

	public WalletType WalletType { get; set; }

	public TrackData()
	{
		Errors = new ErrorList();
		IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (service != null && service.SupportsEMV)
		{
			IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
			_chipEnabled = service2.KioskSession.DeviceService.ChipEnabled;
			_contactlessEnabled = service2.KioskSession.DeviceService.ContactlessEnabled;
		}
	}

	public virtual bool HasValidData()
	{
		if ((!ReadStatus.HasValue || ReadStatus == ResponseStatus.Success) && Errors != null && !Errors.ContainsError())
		{
			if (!HasPay)
			{
				return HasVas;
			}
			return true;
		}
		return false;
	}

	public void LockData()
	{
		_isDataLocked = true;
	}
}
