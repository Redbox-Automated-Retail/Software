using System;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class PointsMemberInfo
{
	public uint Points { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public MemberPointsModel PointsModel { get; set; }

	public MemberPointsLogic PointsLogic { get; set; }

	public IEstimateRedemptionPointsResult EstimateRedemptionPoints { get; set; }

	public int RemainingPoints { get; set; }
}
