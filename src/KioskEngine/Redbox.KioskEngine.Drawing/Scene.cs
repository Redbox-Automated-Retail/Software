using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Drawing;

public class Scene : IScene, IDisposable
{
	private System.Drawing.Image m_backgroundImage;

	private Color m_backgroundColor;

	private bool m_isDrawingSuspended;

	private LinkedList<IActor> m_actors;

	private List<Rectangle> m_dirtyRectangles;

	private readonly object m_syncObject = new object();

	private ElementHost m_elementHost;

	private Grid m_wpfGrid;

	public int ActiveHitHandlers
	{
		get
		{
			int num = 0;
			for (LinkedListNode<IActor> linkedListNode = Actors.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
			{
				IActor value = linkedListNode.Value;
				if ((value.HitFlags & HitTestFlags.Enabled) != HitTestFlags.None && value.Visible && value.Enabled && value.HitExists)
				{
					num++;
				}
			}
			return num;
		}
	}

	public RenderType SceneRenderType { get; set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public string Name { get; private set; }

	public Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			if (!(m_backgroundColor == value))
			{
				m_backgroundColor = value;
				MakeDirty(null);
			}
		}
	}

	public System.Drawing.Image BackgroundImage
	{
		get
		{
			return m_backgroundImage;
		}
		set
		{
			m_backgroundImage = value;
			MakeDirty(null);
		}
	}

	public Grid WPFGrid => m_wpfGrid;

	internal Bitmap BackBuffer { get; set; }

	internal Rectangle Bounds { get; set; }

	internal bool IsDirty { get; set; }

	internal RenderType LastSceneRenderType { get; set; }

	public List<Rectangle> DirtyRectangles
	{
		get
		{
			if (m_dirtyRectangles == null)
			{
				m_dirtyRectangles = new List<Rectangle>();
			}
			return m_dirtyRectangles;
		}
	}

	public LinkedList<IActor> Actors
	{
		get
		{
			if (m_actors == null)
			{
				m_actors = new LinkedList<IActor>();
			}
			return m_actors;
		}
	}

	public event WPFHitHandler OnWPFHit;

	public event WPFKeyEvent OnWPFKeyDown;

	public event WPFKeyEvent OnWPFKeyUp;

	public Scene(string name, int width, int height, Color backgroundColor, ElementHost elementHost)
	{
		Name = name;
		Width = width;
		Height = height;
		BackgroundColor = backgroundColor;
		Bounds = new Rectangle(0, 0, Width, Height);
		InitializeWPFElementHost(elementHost);
	}

	public void Clear()
	{
		lock (m_syncObject)
		{
			foreach (IActor actor in Actors)
			{
				actor.Scene = null;
				actor.ClearHit();
			}
			Actors.Clear();
			if (SceneRenderType == RenderType.WPF)
			{
				WPFGrid.Children.Clear();
			}
			MakeDirty(null);
		}
		BackgroundImage = null;
	}

	public IActor HitTest(int x, int y)
	{
		for (LinkedListNode<IActor> linkedListNode = Actors.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			IActor value = linkedListNode.Value;
			if ((value.HitFlags & HitTestFlags.Enabled) != HitTestFlags.None && value.Visible && value.Enabled)
			{
				Rectangle hitTestRectangle = ((Actor)value).GetHitTestRectangle();
				Rectangle rect = new Rectangle(x, y, 2, 2);
				if (hitTestRectangle.IntersectsWith(rect))
				{
					((Actor)value).RaiseHit();
					return value;
				}
			}
			if ((value.HitFlags & HitTestFlags.Obstruction) != HitTestFlags.None)
			{
				break;
			}
		}
		return null;
	}

	public IActor CreateActor()
	{
		Actor actor = new Actor();
		((IActor)actor).OnWPFHit += OnWPFActorHit;
		return actor;
	}

	public void PrcoessWPFHit()
	{
		OnWPFHit?.Invoke(null);
	}

	public void Dispose()
	{
		foreach (IActor actor in Actors)
		{
			actor.Dispose();
		}
		if (BackBuffer != null)
		{
			BackBuffer.Dispose();
		}
	}

	public void Render(Graphics context, out bool changed)
	{
		changed = false;
		if (!IsDirty || m_isDrawingSuspended)
		{
			return;
		}
		try
		{
			m_elementHost.Visible = SceneRenderType == RenderType.WPF;
			if (SceneRenderType == RenderType.GDI)
			{
				DrawBackground(context, Bounds);
				for (LinkedListNode<IActor> linkedListNode = Actors.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					linkedListNode.Value.Render(context);
				}
				if (LastSceneRenderType == RenderType.WPF)
				{
					System.Windows.Forms.Application.OpenForms["HostForm"]?.Focus();
				}
			}
		}
		finally
		{
			LastSceneRenderType = SceneRenderType;
			changed = true;
			IsDirty = false;
		}
	}

	public void ResumeDrawing()
	{
		m_isDrawingSuspended = false;
	}

	public void SuspendDrawing()
	{
		m_isDrawingSuspended = true;
		DirtyRectangles.Clear();
		IsDirty = true;
	}

	public IActor Remove(string name)
	{
		lock (m_syncObject)
		{
			IActor actor = null;
			foreach (IActor actor2 in Actors)
			{
				if (string.Compare(actor2.Name, name, ignoreCase: true) == 0)
				{
					actor = actor2;
					break;
				}
			}
			if (actor == null)
			{
				return null;
			}
			actor.Scene = null;
			Actors.Remove(actor);
			MakeDirty(new Rectangle[1] { actor.Bounds });
			return actor;
		}
	}

	public void Remove(IActor actor)
	{
		lock (m_syncObject)
		{
			actor.Scene = null;
			Actors.Remove(actor);
			MakeDirty(new Rectangle[1] { actor.Bounds });
		}
	}

	public void AddWPFGridChild(FrameworkElement frameworkElement)
	{
		if (frameworkElement != null)
		{
			WPFGrid.Children.Add(frameworkElement);
		}
	}

	public void RemoveWPFGridChild(FrameworkElement frameworkElement)
	{
		if (frameworkElement != null)
		{
			WPFGrid.Children.Remove(frameworkElement);
		}
	}

	public FrameworkElement GetWPFGridChild(object tag)
	{
		FrameworkElement result = null;
		if (WPFGrid != null)
		{
			WPFGrid.Dispatcher.Invoke(delegate
			{
				foreach (object child in WPFGrid.Children)
				{
					if (child is FrameworkElement { Tag: not null } frameworkElement && frameworkElement.Tag.Equals(tag))
					{
						result = frameworkElement;
					}
				}
			});
		}
		return result;
	}

	public void AddLast(IActor actor)
	{
		lock (m_syncObject)
		{
			actor.Scene = this;
			Actors.AddLast(actor);
			switch (SceneRenderType)
			{
			case RenderType.GDI:
				MakeDirty(new Rectangle[1] { actor.Bounds });
				break;
			case RenderType.WPF:
				WPFGrid.Children.Add(actor.WPFFrameworkElement);
				break;
			}
		}
	}

	public void AddFirst(IActor actor)
	{
		lock (m_syncObject)
		{
			actor.Scene = this;
			Actors.AddFirst(actor);
			MakeDirty(new Rectangle[1] { actor.Bounds });
		}
	}

	public IActor GetActor(string name)
	{
		lock (m_syncObject)
		{
			foreach (IActor actor in Actors)
			{
				if (string.Compare(actor.Name, name, ignoreCase: true) == 0)
				{
					return actor;
				}
			}
			return null;
		}
	}

	public IActor GetActorByNameAndTag(string name, string tag)
	{
		IActor result = null;
		lock (m_syncObject)
		{
			foreach (IActor actor in Actors)
			{
				if (actor.Name == name && actor.Tag as string == tag)
				{
					result = actor;
					break;
				}
			}
		}
		return result;
	}

	public List<IActor> GetActorByTag(string tag)
	{
		List<IActor> list = new List<IActor>();
		lock (m_syncObject)
		{
			foreach (IActor actor in Actors)
			{
				if (actor.Tag != null && !string.IsNullOrEmpty(actor.Tag.ToString()) && new List<string>(actor.Tag.ToString().Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)).Contains(tag))
				{
					list.Add(actor);
				}
			}
			return list;
		}
	}

	public IActor GetActorByTabOrder(int tabOrder)
	{
		IActor result = null;
		lock (m_syncObject)
		{
			foreach (IActor actor in Actors)
			{
				if (actor.TabOrder.HasValue && actor.TabOrder == tabOrder)
				{
					result = actor;
					break;
				}
			}
		}
		return result;
	}

	public void MakeDirty(Rectangle[] rectangles)
	{
		if (m_isDrawingSuspended)
		{
			return;
		}
		IsDirty = true;
		if (rectangles == null || rectangles.Length == 0)
		{
			return;
		}
		lock (m_syncObject)
		{
			foreach (Rectangle item in rectangles)
			{
				if (!DirtyRectangles.Contains(item))
				{
					DirtyRectangles.Add(item);
				}
			}
		}
	}

	internal void DrawBackground(Graphics context, Rectangle rect)
	{
		if (BackgroundImage != null)
		{
			context.DrawImage(BackgroundImage, rect, rect.Left, rect.Top, rect.Width, rect.Height, GraphicsUnit.Pixel);
			return;
		}
		using SolidBrush brush = new SolidBrush(BackgroundColor);
		context.FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
	}

	internal List<IActor> GetDirtyActors()
	{
		List<IActor> list = new List<IActor>();
		foreach (IActor actor in Actors)
		{
			if (actor.IsDirty)
			{
				list.Add(actor);
			}
		}
		return list;
	}

	private void InitializeWPFElementHost(ElementHost elementHost)
	{
		m_elementHost = elementHost;
		System.Windows.Controls.UserControl userControl = new System.Windows.Controls.UserControl();
		m_wpfGrid = new Grid();
		m_wpfGrid.Focusable = true;
		m_wpfGrid.KeyDown += WPFGrid_KeyDown;
		m_wpfGrid.KeyUp += WPFGrid_KeyUp;
		userControl.Content = m_wpfGrid;
		m_elementHost.Child = userControl;
	}

	private void WPFGrid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (OnWPFKeyUp != null)
		{
			OnWPFKeyUp(sender, e);
		}
	}

	private void WPFGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (OnWPFKeyDown != null)
		{
			OnWPFKeyDown(sender, e);
		}
	}

	private void OnWPFActorHit(IActor actor)
	{
		if (OnWPFHit != null)
		{
			OnWPFHit(actor);
		}
	}
}
