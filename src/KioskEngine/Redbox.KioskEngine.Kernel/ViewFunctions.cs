using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ViewFunctions
{
	[KernelFunction(Name = "kernel.pushview")]
	internal static void PushView(string viewName)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.Push(viewName);
	}

	[KernelFunction(Name = "kernel.popview")]
	internal static string PopView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Pop();
	}

	[KernelFunction(Name = "kernel.popviewdiscard")]
	internal static string PopViewDiscard()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.PopDiscard();
	}

	[KernelFunction(Name = "kernel.peekview")]
	internal static string PeekView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Peek();
	}

	[KernelFunction(Name = "kernel.peekpriorview")]
	internal static string PeekPriorView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Peek(1);
	}

	[KernelFunction(Name = "kernel.getviewframe")]
	internal static LuaTable GetViewFrame()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service == null)
		{
			return null;
		}
		if (!(service.PeekViewFrame()?.ViewFrame is IViewFrame { ViewWindow: var viewWindow } viewFrame))
		{
			return null;
		}
		if (viewWindow.HasValue)
		{
			return new LuaTable(KernelService.Instance.LuaRuntime)
			{
				["x"] = viewFrame.ViewWindow.Value.X,
				["y"] = viewFrame.ViewWindow.Value.Y,
				["width"] = viewFrame.ViewWindow.Value.Width,
				["height"] = viewFrame.ViewWindow.Value.Height
			};
		}
		return null;
	}

	[KernelFunction(Name = "kernel.getviewrendertype")]
	internal static string GetViewRenderType()
	{
		string result = null;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			IViewFrameInstance viewFrameInstance = service.PeekViewFrame();
			if (viewFrameInstance?.ViewFrame.Scene != null)
			{
				result = viewFrameInstance.ViewFrame.Scene.SceneRenderType.ToString();
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.showview")]
	internal static void ShowView()
	{
		ServiceLocator.Instance.GetService<IViewService>()?.Show();
	}

	[KernelFunction(Name = "kernel.setanalyticsviewmessage")]
	internal static void SetAnalyticsViewMessage(string text)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.SetAnalyticsEventProperty("ViewMessage", text);
	}

	[KernelFunction(Name = "kernel.reenterview")]
	internal static void ReenterView()
	{
		ServiceLocator.Instance.GetService<IViewService>()?.EnterTopOfStock();
	}

	[KernelFunction(Name = "kernel.getlastshownview")]
	internal static string GetLastShownView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.GetLastShownView();
	}

	[KernelFunction(Name = "kernel.safe_to_change_view")]
	internal static bool SafeToChangeView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.IsSafeToChangeView() ?? false;
	}

	[KernelFunction(Name = "kernel.get_start_over_view_name")]
	internal static string GetStartOverViewName()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.GetStartOverViewName();
	}

	[KernelFunction(Name = "kernel.view_goback")]
	internal static void ViewGoBack(object context, bool? popOnly)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			bool popOnly2 = popOnly.HasValue && popOnly.Value;
			service.GoBack(context, popOnly2);
		}
	}

	[KernelFunction(Name = "kernel.view_goback_safe")]
	internal static void ViewGoBackSafe(object context, bool? popOnly)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			bool popOnly2 = popOnly.HasValue && popOnly.Value;
			service.GoBackSafe(context, popOnly2);
		}
	}
}
