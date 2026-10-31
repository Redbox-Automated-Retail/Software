using System.Collections.Specialized;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

namespace Redbox.NAnt.TimeKeeper.Tasks;

public class Remoting
{
	public const string HTTP_CHANNEL = "http";

	protected const int DEFAULT_PORT = 0;

	private int _timeOut = 30000;

	public virtual void Initialize(int timeOut)
	{
		if (timeOut > 0)
		{
			_timeOut = timeOut;
		}
		if (IsRegistrationRequired())
		{
			RegisterHttpChannel();
		}
	}

	public bool IsRegistrationRequired()
	{
		return ChannelServices.GetChannel("http") == null;
	}

	public void RegisterHttpChannel()
	{
		HttpChannel chnl = new HttpChannel(GetProperties(), new BinaryClientFormatterSinkProvider(), new BinaryServerFormatterSinkProvider());
		ChannelServices.RegisterChannel(chnl);
	}

	public ListDictionary GetProperties()
	{
		ListDictionary listDictionary = new ListDictionary();
		listDictionary.Add("port", 0);
		listDictionary.Add("timeout", _timeOut);
		return listDictionary;
	}
}
