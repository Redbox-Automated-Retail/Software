using System;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewFrame : IBaseViewFrame, IDisposable
{
	int Width { get; }

	int Height { get; }

	bool Cached { get; }

	string ViewVersion { get; }

	RenderType ViewRenderType { get; }

	string SceneName { get; }

	Rectangle? ViewWindow { get; }

	Color BackgroundColor { get; }

	Image BackgroundImage { get; }

	string OnEnterResourceName { get; }

	string OnLeaveResourceName { get; }
}
