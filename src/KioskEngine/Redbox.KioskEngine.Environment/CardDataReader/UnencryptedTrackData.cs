using System.Security;
using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class UnencryptedTrackData : TrackData, IUnencryptedTrackData, ITrackData
{
	private SecureString _track1;

	private SecureString _track2;

	private SecureString _encryptedTrack2;

	private SecureString _accountNumber;

	private string _encryptedAccountNumber;

	private int _encryptionCerificateKeyId;

	public SecureString Track1
	{
		get
		{
			return _track1;
		}
		set
		{
			if (!_isDataLocked)
			{
				_track1 = value;
			}
		}
	}

	public SecureString Track2
	{
		get
		{
			return _track2;
		}
		set
		{
			if (!_isDataLocked)
			{
				_track2 = value;
			}
		}
	}

	public SecureString EncryptedTrack2
	{
		get
		{
			return _encryptedTrack2;
		}
		set
		{
			if (!_isDataLocked)
			{
				_encryptedTrack2 = value;
			}
		}
	}

	public SecureString AccountNumber
	{
		get
		{
			return _accountNumber;
		}
		set
		{
			if (!_isDataLocked)
			{
				_accountNumber = value;
				_cardHashId = ((_accountNumber != null) ? CardHelper.GenerateCardId(_accountNumber) : null);
			}
		}
	}

	public string EncryptedAccountNumber
	{
		get
		{
			return _encryptedAccountNumber;
		}
		set
		{
			if (!_isDataLocked)
			{
				_encryptedAccountNumber = value;
			}
		}
	}

	public int EncryptionCerificateKeyId
	{
		get
		{
			return _encryptionCerificateKeyId;
		}
		set
		{
			if (!_isDataLocked)
			{
				_encryptionCerificateKeyId = value;
			}
		}
	}
}
