using System.Reflection;

namespace Redbox.HAL.Component.Model;

public interface IIpcServiceHostFactory
{
	IIpcServiceHost Create(IIpcProtocol protocol);

	IIpcServiceHost Create(IIpcProtocol protocol, IHostInfo info);

	IHostInfo Create(Assembly a);
}
