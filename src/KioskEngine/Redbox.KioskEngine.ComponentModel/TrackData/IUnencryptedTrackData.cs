using System.Security;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface IUnencryptedTrackData : ITrackData
{
	SecureString Track1 { get; set; }

	SecureString Track2 { get; set; }

	SecureString EncryptedTrack2 { get; set; }

	SecureString AccountNumber { get; set; }

	string EncryptedAccountNumber { get; set; }

	int EncryptionCerificateKeyId { get; set; }
}
