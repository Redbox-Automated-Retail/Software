using System.Security;
using System.Security.Cryptography.X509Certificates;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class TrackDataEncryptionService : ITrackDataEncryptionService
{
	private int _keyId = 99999;

	private bool _useStoreCertificate;

	public int KeyId => _keyId;

	public void RefreshCertificate()
	{
		int num = 0;
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null && service.TryGetValue<int>("system", "General", "CertificateName", out var value))
		{
			num = value;
		}
		if (num == 0)
		{
			_keyId = GetBundleKeyId();
			_useStoreCertificate = false;
		}
		else
		{
			_keyId = num;
			_useStoreCertificate = true;
		}
		if (_useStoreCertificate && GetStoreCertificate() == null)
		{
			_keyId = GetBundleKeyId();
			_useStoreCertificate = false;
		}
	}

	public string GetEncryptedAccountNumber(IUnencryptedTrackData unencryptedTrackData)
	{
		if (unencryptedTrackData == null || unencryptedTrackData.AccountNumber == null)
		{
			return null;
		}
		return Encrypt(unencryptedTrackData.AccountNumber);
	}

	public SecureString GetEncryptedTrack2(IUnencryptedTrackData unencryptedTrackData)
	{
		if (unencryptedTrackData == null)
		{
			return null;
		}
		return CardHelper.CopyToSecure(Encrypt(unencryptedTrackData.Track2));
	}

	private string Encrypt(SecureString value)
	{
		if (_useStoreCertificate)
		{
			X509Certificate2 storeCertificate = GetStoreCertificate();
			if (value == null)
			{
				return null;
			}
			return CardHelper.EncryptCreditCardFromCertificate(value, storeCertificate);
		}
		string bundleCertificateXML = GetBundleCertificateXML();
		if (bundleCertificateXML == null)
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		return CardHelper.EncryptCreditCardFromXmlKey(value, bundleCertificateXML);
	}

	private string GetBundleCertificateXML()
	{
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return null;
		}
		return ResourceBundleService.Instance.GetXml((string)manifest["public_key"]).InnerXml;
	}

	private X509Certificate2 GetStoreCertificate()
	{
		return CertificateHelper.GetCertificateBySubjectName(StoreName.My, StoreLocation.LocalMachine, _keyId.ToString());
	}

	private int GetBundleKeyId()
	{
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return 99999;
		}
		string resourceName = (string)manifest["public_key"];
		if (!ResourceBundleService.Instance.HasActiveBundle())
		{
			return 99999;
		}
		IResource resource = ResourceBundleService.Instance.GetResource(resourceName);
		if (resource == null)
		{
			return 99999;
		}
		return (int)resource["key_id"];
	}
}
