using System.Security.Cryptography.X509Certificates;

namespace Redbox.Core;

public static class CertificateHelper
{
	public static X509Certificate2 GetCertificateFromFile(string fileName)
	{
		return new X509Certificate2(fileName);
	}

	public static X509Certificate2 GetCertificateFromFile(string fileName, string password)
	{
		return new X509Certificate2(fileName, password);
	}

	public static X509Certificate2 GetCertificateBySubjectName(StoreName storeName, StoreLocation storeLocation, string subjectName)
	{
		if (string.IsNullOrEmpty(subjectName))
		{
			return null;
		}
		X509Store x509Store = new X509Store(storeName, storeLocation);
		x509Store.Open(OpenFlags.ReadOnly);
		X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, validOnly: false);
		return (x509Certificate2Collection.Count > 0) ? x509Certificate2Collection[0] : null;
	}
}
