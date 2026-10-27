using System.Collections.Generic;
using Redbox.Rental.Model.Planogram;

namespace Redbox.Rental.Model.KioskClientService.Planogram;

public interface IKioskClientServicePlanogram
{
	List<Redbox.Rental.Model.Planogram.Planogram> GetActivePlanograms();

	Redbox.Rental.Model.Planogram.Planogram GetSelectedPlanogram();

	void SaveSelectedPlanogram(Redbox.Rental.Model.Planogram.Planogram planogram);

	bool SyncPlanograms(bool forceRefresh);
}
