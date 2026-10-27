using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IKioskFunctionCheckService
{
	bool Add(IKioskFunctionCheckData data);

	IList<IKioskFunctionCheckData> Load();

	int CleanOldEntries();
}
