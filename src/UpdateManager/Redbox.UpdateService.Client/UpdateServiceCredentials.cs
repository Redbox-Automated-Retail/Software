using System;
using System.IO;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Client;

public class UpdateServiceCredentials
{
	public static UpdateServiceCredentials _instance;

	private Credentials _credentials;

	private static readonly string _key1 = "{EFFBA16E-4871-4495-9E6B-BB78452BBD97}";

	private static readonly string _key2 = "{929CD6EA-8D36-4ECB-B8B7-6B5053536968}";

	public static UpdateServiceCredentials Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new UpdateServiceCredentials();
			}
			return _instance;
		}
	}

	public Credentials GetCredentials()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(typeof(UpdateService).Assembly.Location);
			string text = Path.Combine(directoryName, "usdata.active");
			string text2 = Path.Combine(directoryName, "usdata.staged");
			if (File.Exists(text2))
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				File.Move(text2, text);
				_credentials = null;
			}
			if (_credentials != null)
			{
				return _credentials;
			}
			if (!File.Exists(text))
			{
				_credentials = new Credentials
				{
					Key1 = _key1,
					Key2 = _key2
				};
				return _credentials;
			}
			byte[] bytes = File.ReadAllBytes(text).Decrypt();
			_credentials = Encoding.UTF8.GetString(bytes).ToObject<Credentials>();
			return _credentials;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception occurred in getting credentials", e);
		}
		return null;
	}

	public string GetCredentialHash(string kioskId)
	{
		Credentials credentials = GetCredentials();
		return GetCredentialHash(kioskId, credentials.Key1, credentials.Key2);
	}

	private string GetCredentialHash(string kioskId, string key1, string key2)
	{
		string s = $"{kioskId}-{key1}-{key2}";
		return Encoding.ASCII.GetBytes(s).ToASCIISHA1Hash();
	}
}
