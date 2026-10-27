using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets;

public class TcpClientSession : ClientSession
{
	private Stream m_stream;

	private TcpClient m_transportClient;

	private bool _validateCallback = true;

	protected override Stream Stream
	{
		get
		{
			if (m_stream == null)
			{
				if (!base.Protocol.IsSecure)
				{
					m_stream = TransportClient.GetStream();
					m_stream.ReadTimeout = base.Timeout;
					m_stream.WriteTimeout = base.Timeout;
				}
				else
				{
					SslStream sslStream = new SslStream(TransportClient.GetStream(), leaveInnerStreamOpen: false, delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
					{
						if (sslPolicyErrors == SslPolicyErrors.None)
						{
							return true;
						}
						LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback failed.");
						LogHelper.Instance.Log("Policy Errors: {0}", sslPolicyErrors);
						LogHelper.Instance.Log("Certificate: {0}", certificate);
						chain?.ChainStatus.ForEach(delegate(X509ChainStatus cs)
						{
							LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", cs.Status, cs.StatusInformation);
						});
						return !_validateCallback;
					});
					sslStream.ReadTimeout = base.Timeout;
					sslStream.WriteTimeout = base.Timeout;
					sslStream.AuthenticateAsClient(base.Protocol.Host);
					m_stream = sslStream;
				}
			}
			return m_stream;
		}
	}

	private TcpClient TransportClient
	{
		get
		{
			return m_transportClient;
		}
		set
		{
			m_transportClient = value;
			if (m_transportClient != null)
			{
				base.ReadBuffer = new byte[m_transportClient.ReceiveBufferSize];
			}
		}
	}

	public override bool Connect()
	{
		try
		{
			ConnectThrowOnError();
			return true;
		}
		catch (IOException)
		{
			return false;
		}
	}

	public override void Dispose()
	{
		if (base.OwningPool != null)
		{
			base.OwningPool.ReturnSession(this);
		}
		base.Dispose();
	}

	public override void ConnectThrowOnError()
	{
		TransportClient = new TcpClient(base.Protocol.Host, int.Parse(base.Protocol.Port))
		{
			SendTimeout = base.Timeout,
			ReceiveTimeout = base.Timeout
		};
		ConsumeMessages();
		base.IsConnected = true;
	}

	protected internal TcpClientSession(IPCProtocol protocol, int? timeout)
		: base(protocol)
	{
		base.Timeout = timeout ?? 30000;
		try
		{
			object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox", "ValidateCallback", "true");
			_validateCallback = true;
			if (value != null && value is string && bool.TryParse(value as string, out var result))
			{
				_validateCallback = result;
				if (!_validateCallback)
				{
					LogHelper.Instance.Log("Certificate callback validation is turned off");
				}
			}
		}
		catch
		{
		}
	}

	protected internal override bool IsConnectionAvailable()
	{
		if (TransportClient.Client.Poll(10, SelectMode.SelectRead))
		{
			return TransportClient.Client.Available != 0;
		}
		return true;
	}

	protected internal override void CustomClose()
	{
		try
		{
			Stream.Close();
			TransportClient.Close();
		}
		catch (Exception)
		{
		}
		finally
		{
			base.IsConnected = false;
		}
	}

	protected override bool CanRead()
	{
		if (Stream.CanRead)
		{
			return TransportClient.Client.Available > 0;
		}
		return false;
	}

	protected override int GetAvailableData()
	{
		return TransportClient.Client.Available;
	}

	[Conditional("DEBUG")]
	private void LogCertificateValidationCallback(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback logging.");
		LogHelper.Instance.Log("Policy Errors: {0}", sslPolicyErrors);
		LogHelper.Instance.Log("Certificate: {0}", certificate);
		chain?.ChainStatus.ForEach(delegate(X509ChainStatus cs)
		{
			LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", cs.Status, cs.StatusInformation);
		});
	}
}
