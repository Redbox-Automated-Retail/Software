using Redbox.FMA.Controllers;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.FMA.ViewDefinitions;

[ViewDefinition(ViewName = "fma_popup_message_view")]
public class PopupMessageViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PopupMessageViewDefinition()
	{
		base.ViewName = "fma_popup_message_view";
		base.ActiveFlag = true;
		base.ControlName = "FmaPopupMessageView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PopupMessageViewController.ConfigureView(viewFrameInstance?.Parameter as PopupMessageViewParameters);
		};
	}
}
