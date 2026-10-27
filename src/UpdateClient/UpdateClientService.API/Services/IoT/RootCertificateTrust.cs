using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Client.Options;

namespace UpdateClientService.API.Services.IoT;

internal class RootCertificateTrust
{
	private X509Certificate2Collection certificates;

	internal RootCertificateTrust()
	{
		certificates = new X509Certificate2Collection();
	}

	internal void AddCert(X509Certificate2 x509Certificate2)
	{
		certificates.Add(x509Certificate2);
	}

	internal bool VerifyServerCertificate(MqttClientCertificateValidationCallbackContext arg)
	{
		return VerifyServerCertificate(new object(), arg.Certificate, arg.Chain, arg.SslPolicyErrors);
	}

	internal bool VerifyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		if (sslPolicyErrors == SslPolicyErrors.None)
		{
			return true;
		}
		X509Chain x509Chain = new X509Chain();
		chain.ChainPolicy.ExtraStore.AddRange(certificates);
		chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
		chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
		if (chain.Build(new X509Certificate2(certificate)))
		{
			return true;
		}
		X509ChainStatus[] chainStatus = chain.ChainStatus;
		foreach (X509ChainStatus x509ChainStatus in chainStatus)
		{
			if (x509ChainStatus.Status != X509ChainStatusFlags.UntrustedRoot)
			{
				return false;
			}
		}
		foreach (X509ChainElement chainElement in chain.ChainElements)
		{
			X509ChainStatus[] chainElementStatus = chainElement.ChainElementStatus;
			foreach (X509ChainStatus x509ChainStatus2 in chainElementStatus)
			{
				if (x509ChainStatus2.Status == X509ChainStatusFlags.UntrustedRoot)
				{
					X509Certificate2Collection x509Certificate2Collection = certificates.Find(X509FindType.FindByThumbprint, chainElement.Certificate.Thumbprint, validOnly: false);
					if (x509Certificate2Collection.Count == 0)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
