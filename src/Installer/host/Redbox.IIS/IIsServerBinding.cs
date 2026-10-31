namespace Redbox.IIS;

public class IIsServerBinding
{
	private const string AllUnassigned = "(All Unassigned)";

	private const string NoHostHeader = "(No Host Header)";

	public string TcpPort { get; set; }

	public string IpAddress { get; set; }

	public string HostHeader { get; set; }

	public IIsServerBinding(string value)
	{
		string[] array = value.Split(':');
		if (array.Length == 3)
		{
			IpAddress = array[0];
			TcpPort = array[1];
			HostHeader = array[2];
		}
		SetDefaults();
	}

	public IIsServerBinding(string ipAddress, string tcpPort, string hostHeader)
	{
		TcpPort = tcpPort;
		IpAddress = ipAddress;
		HostHeader = hostHeader;
		SetDefaults();
	}

	public string ToMetabaseString()
	{
		return string.Format("{0}:{1}:{2}", (IpAddress == "(All Unassigned)") ? null : IpAddress, TcpPort, (HostHeader == "(No Host Header)") ? null : HostHeader);
	}

	public override string ToString()
	{
		return $"{IpAddress}:{TcpPort}:{HostHeader}";
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(obj, this))
		{
			return true;
		}
		if (!(obj is IIsServerBinding isServerBinding))
		{
			return false;
		}
		if (isServerBinding.TcpPort == TcpPort && isServerBinding.IpAddress == IpAddress)
		{
			return isServerBinding.HostHeader == HostHeader;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	private void SetDefaults()
	{
		if (string.IsNullOrEmpty(IpAddress))
		{
			IpAddress = "(All Unassigned)";
		}
		if (string.IsNullOrEmpty(HostHeader))
		{
			HostHeader = "(No Host Header)";
		}
		if (string.IsNullOrEmpty(TcpPort))
		{
			TcpPort = "80";
		}
	}
}
