namespace Redbox.HAL.Component.Model;

public interface IIpcProtocolFactory
{
	IIpcProtocol Parse(string uri);

	bool Validate(string uri);
}
