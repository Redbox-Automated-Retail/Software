using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "recommendation_on_pickup_view")]
public class RecommendationOnPickupDefinition : BaseViewDefinition, IViewDefinition
{
	public RecommendationOnPickupDefinition()
	{
		base.ViewName = "recommendation_on_pickup_view";
		base.ActiveFlag = true;
		base.ControlName = "RecommendationOnPickupView";
		base.OnEnter = delegate
		{
			RecommendationOnPickupViewController.ConfigureView();
		};
		base.OnLeave = delegate
		{
			RecommendationOnPickupViewController.CleanupRecommendationOnPickupView();
		};
	}
}
