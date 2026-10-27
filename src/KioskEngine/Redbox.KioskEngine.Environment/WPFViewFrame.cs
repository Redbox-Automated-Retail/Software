using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class WPFViewFrame : BaseViewFrame, IWPFViewFrame, IBaseViewFrame
{
	private IViewDefinition ViewDefinition { get; set; }

	public override void UpdateScene(IViewFrameInstance viewFrameInstance)
	{
		try
		{
			base.Scene.SuspendDrawing();
			base.Scene.SceneRenderType = RenderType.WPF;
			bool flag = true;
			if (base.Clear)
			{
				base.Scene.Clear();
			}
			else if (base.Scene.GetWPFGridChild(viewFrameInstance.Id) != null)
			{
				flag = false;
			}
			if (flag)
			{
				FrameworkElement frameworkElement = CreateControlInstance();
				frameworkElement.Tag = viewFrameInstance?.Id;
				base.Scene.AddWPFGridChild(frameworkElement);
			}
			base.Scene.WPFGrid?.Focus();
			base.Scene.ResumeDrawing();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WPFViewFrame.UpdateScene.", e);
		}
	}

	private FrameworkElement CreateControlInstance()
	{
		return ServiceLocator.Instance.GetService<IThemeService>()?.GetNewThemedControlInstance(ViewDefinition?.ControlName);
	}

	public override bool RaiseOnEnter(IViewFrameInstance viewFrameInstance)
	{
		bool result = false;
		if (ViewDefinition.OnEnter != null)
		{
			result = true;
			ViewDefinition.OnEnter?.Invoke(viewFrameInstance);
		}
		return result;
	}

	public override void RaiseOnLeave(IViewFrameInstance viewFrameInstance)
	{
		ViewDefinition.OnLeave?.Invoke(viewFrameInstance);
	}

	public static void CacheWPFViewFrames(string assemblyName)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service == null)
		{
			return;
		}
		string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
		if (File.Exists(text))
		{
			Assembly assembly = Assembly.LoadFile(text);
			if (assembly != null)
			{
				foreach (Type item in from x in assembly.GetTypes()
					where x.IsClass && x.IsDefined(typeof(ViewDefinitionAttribute))
					select x)
				{
					if (Activator.CreateInstance(item) is IViewDefinition viewDefinition)
					{
						IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
						IWPFViewFrame baseViewFrame = new WPFViewFrame
						{
							ViewName = viewDefinition.ViewName,
							ActiveFlag = viewDefinition.ActiveFlag.ToString(),
							IsStartOverView = viewDefinition.IsStartOverView,
							IsMaintenanceModeView = viewDefinition.IsMaintenanceModeView,
							Clear = viewDefinition.Clear,
							ViewDefinition = viewDefinition,
							Scene = service2?.GetScene("Default")
						};
						service.CacheViewFrame(baseViewFrame);
					}
				}
				return;
			}
			LogHelper.Instance.Log("Unable to load assembly {0}", text);
		}
		else
		{
			LogHelper.Instance.Log("Unable to find assembly {0}", text);
		}
	}
}
