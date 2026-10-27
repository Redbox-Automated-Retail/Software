using System.Security;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITrackDataEncryptionService
{
	int KeyId { get; }

	string GetEncryptedAccountNumber(IUnencryptedTrackData unencryptedTrackData);

	SecureString GetEncryptedTrack2(IUnencryptedTrackData unencryptedTrackData);

	void RefreshCertificate();
}
