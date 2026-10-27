using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IControllerService
{
	void Initialize(ErrorList errors, IDictionary<string, object> initProperties);

	void Shutdown();

	IGetResult Get(IFormattedLog log);

	IGetResult Get(IGetObserver observer, IFormattedLog log);

	IPutResult Put(string id, IFormattedLog log);

	IPutResult Put(IPutObserver observer, string id, IFormattedLog log);

	ITransferResult Transfer(ILocation source, ILocation destination, IFormattedLog log);

	ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IFormattedLog log);

	ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer, IFormattedLog log);
}
