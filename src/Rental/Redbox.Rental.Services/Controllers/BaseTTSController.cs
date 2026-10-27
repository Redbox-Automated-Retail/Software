using System;
using System.Reflection;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;

namespace Redbox.Rental.Services.Controllers;

public class BaseTTSController<TController, TModel>
{
	private static string IdleTimerName = "idle_timer";

	private static void AddSpeechParts(ISpeechControl speechControl, TModel model)
	{
		MethodInfo method = typeof(TController).GetMethod("AddSpeechParts");
		if (method == null)
		{
			throw new NotImplementedException("The derived class must implement public void AddSpeechParts(ISpeechControl speechControl, [TModel] model)");
		}
		method.Invoke(null, new object[2] { speechControl, model });
	}

	public static ISpeechControl GetSpeechControl(Func<TModel> getModel)
	{
		ISpeechControl speechControl = new SpeechControl();
		speechControl.Name = $"tts_{GetViewName()}";
		AddSpeechParts(speechControl, getModel());
		speechControl.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = speechControl;
		});
		return speechControl;
	}

	protected static IBaseViewFrame GetViewFrame()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame;
	}

	protected static string GetViewName()
	{
		return GetViewFrame()?.ViewName ?? "unknown_view";
	}

	protected static void StartIdleTimer()
	{
		OrchestrationService.StartIdleTimer(IdleTimerName);
	}

	protected static void StopIdleTimer()
	{
		OrchestrationService.StopIdleTimer(IdleTimerName);
	}
}
