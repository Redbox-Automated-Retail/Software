using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IApplicationState
{
	bool IsADAAccessible { get; set; }

	bool IsABEOn { get; }

	string InCartTemplateName { get; set; }

	bool VerboseLogging { get; }

	List<IKioskProduct> BrowseProducts { get; set; }

	bool IsStandAlone { get; set; }

	bool NeedToReloadCarousel { get; set; }

	bool IsInMaintenanceMode { get; }

	bool HasMinutesInMaintenanceModeSource { get; }

	bool HasMaintenanceModeReason(MaintenanceModeSource source, string code);

	List<IMaintenanceModeReason> GetMaintenanceModeReasons();

	void AddMaintenanceModeReason(MaintenanceModeSource source, string code);

	void RemoveMaintenanceModeReason(MaintenanceModeSource source, string code);

	void RemoveMaintenanceModeReason(MaintenanceModeSource source);
}
