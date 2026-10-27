using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "join_text_club_promo_view")]
public class JoinedTextClubDefinition : BaseViewDefinition, IViewDefinition
{
	public JoinedTextClubDefinition()
	{
		base.ViewName = "join_text_club_promo_view";
		base.ActiveFlag = true;
		base.ControlName = "JoinTextClubPromoView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigureTextView(viewFrameInstance?.Parameter as SimpleInfo);
		};
	}
}
