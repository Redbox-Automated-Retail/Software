using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.Integration;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Drawing;

public class RenderingService : IRenderingService
{
	private IScene m_activeScene;

	private IDictionary<string, Scene> m_scenes;

	public static RenderingService Instance => Singleton<RenderingService>.Instance;

	public IScene ActiveScene
	{
		get
		{
			return m_activeScene;
		}
		set
		{
			if (m_activeScene != value)
			{
				m_activeScene = value;
				m_activeScene.MakeDirty(null);
			}
		}
	}

	public Color BackgroundColor
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service == null)
			{
				return Color.FromArgb(153, 153, 152);
			}
			return (Color)ConversionHelper.ChangeType(service.GetValue("Core", "BackgroundColor", "153,153,152"), typeof(Color));
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "BackgroundColor", ConversionHelper.ChangeType(value, typeof(string)));
		}
	}

	internal IDictionary<string, Scene> Scenes
	{
		get
		{
			if (m_scenes == null)
			{
				m_scenes = new Dictionary<string, Scene>();
			}
			return m_scenes;
		}
	}

	public ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize rendering service.");
		ErrorList result = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(IRenderingService), Instance);
		return result;
	}

	public IScene CreateScene(string name, int width, int height, Color backgroundColor, ElementHost elementHost)
	{
		if (Scenes.ContainsKey(name))
		{
			return Scenes[name];
		}
		Scene scene = new Scene(name, width, height, backgroundColor, elementHost);
		Scenes[name] = scene;
		return scene;
	}

	public IScene GetScene(string name)
	{
		if (!Scenes.ContainsKey(name))
		{
			return null;
		}
		return Scenes[name];
	}

	public void RemoveScene(string name)
	{
		IScene scene = GetScene(name);
		if (scene != null)
		{
			Scenes.Remove(name);
			scene.Dispose();
		}
	}

	public bool IsBitmapReferenced(Image image)
	{
		foreach (KeyValuePair<string, Scene> scene in Scenes)
		{
			if (scene.Value.BackgroundImage == image)
			{
				return true;
			}
			foreach (IActor actor in scene.Value.Actors)
			{
				if (actor.Image == image)
				{
					return true;
				}
			}
		}
		return false;
	}

	private RenderingService()
	{
	}
}
