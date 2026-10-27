using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IControllerService
{
	void Initialize(ErrorList errors, IDictionary<string, object> initProperties);

	void Shutdown();

	IGetResult Get();

	IGetResult Get(IGetObserver observer);

	IGetFromResult GetFrom(ILocation location);

	IGetFromResult GetFrom(IGetFromObserver observer, ILocation location);

	IPutResult Put(string id);

	IPutResult Put(IPutObserver observer, string id);

	IPutToResult PutTo(string id, ILocation location);

	IPutToResult PutTo(IPutToObserver observer, string id, ILocation location);

	ITransferResult Transfer(ILocation source, ILocation destination);

	ITransferResult Transfer(ILocation source, ILocation destination, bool preserveFlags);

	ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver o);

	ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer, bool preserveFlags);

	ErrorCodes PushOut();

	ErrorCodes ClearGripper();

	IVendItemResult VendItemInPicker();

	IVendItemResult VendItemInPicker(int attempts);

	ErrorCodes AcceptDiskAtDoor();

	ErrorCodes RejectDiskInPicker();

	ErrorCodes RejectDiskInPicker(int attempts);
}
