using System;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class PerksMemberInfo
{
	public uint Points { get; set; }

	public uint ExpirePoints { get; set; }

	public DateTime? ExpireDate { get; set; }

	public uint YearRentals { get; set; }

	public string TierName { get; set; }

	public string[] Bullets { get; set; }

	public Action CancelAction { get; set; }

	public MemberPerksModel PerksModel { get; set; }

	public MemberPerksLogic PerksLogic { get; set; }
}
