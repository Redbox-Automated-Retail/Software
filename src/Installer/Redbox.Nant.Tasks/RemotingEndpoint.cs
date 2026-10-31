using System.Xml;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("add_endpoint")]
public class RemotingEndpoint : Task
{
	private const string DEFAULT_PUBLIC_KEY_TOKEN = "a85d6bf8ca5f760d";

	private string _version;

	private string _file;

	private string _publicKeyToken;

	[TaskAttribute("version", Required = true)]
	public string Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
		}
	}

	[TaskAttribute("file", Required = true)]
	public string File
	{
		get
		{
			return _file;
		}
		set
		{
			_file = value;
		}
	}

	[TaskAttribute("publickeytoken", Required = false)]
	public string PublicKeyToken
	{
		get
		{
			if (_publicKeyToken == null || _publicKeyToken == string.Empty)
			{
				_publicKeyToken = "a85d6bf8ca5f760d";
			}
			return _publicKeyToken;
		}
		set
		{
			_publicKeyToken = value;
		}
	}

	protected override void ExecuteTask()
	{
		AddService();
	}

	private void AddService()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(_file);
		xmlDocument.SelectNodes("/configuration/system.runtime.remoting/application/service");
	}
}
