using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Common;

public static class ActorStyles
{
	public static IActor AddRedButtonStyle(this IActor actor)
	{
		actor.GradientAngle = 90f;
		actor.BorderColor = Color.FromArgb(134, 12, 19);
		actor.BackgroundColor = Color.FromArgb(181, 18, 27);
		actor.GradientTargetColor = Color.FromArgb(143, 14, 21);
		actor.ForegroundColor = Color.White;
		actor.StrokeWeight = 3f;
		actor.VerticalAlignment = StringAlignment.Center;
		actor.HorizontalAlignment = StringAlignment.Center;
		actor.HitFlags = HitTestFlags.Enabled;
		actor.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.DrawBorder | RenderOptionFlags.BackgroundGradient;
		IStyleSheetStyle styleSheetStyle = (actor.Style = ServiceLocator.Instance.GetService<IStyleSheetService>().GetStyleSheet("style_sheet_common").GetStyle("red_button_style"));
		actor.StyleName = styleSheetStyle.Name;
		return actor;
	}

	public static IActor AddRubineButtonStyle(this IActor actor)
	{
		actor.GradientAngle = 90f;
		actor.BorderColor = Color.White;
		actor.BackgroundColor = Color.FromArgb(228, 32, 69);
		actor.GradientTargetColor = Color.FromArgb(228, 32, 69);
		actor.ForegroundColor = Color.White;
		actor.StrokeWeight = 3f;
		actor.VerticalAlignment = StringAlignment.Center;
		actor.HorizontalAlignment = StringAlignment.Center;
		actor.HitFlags = HitTestFlags.Enabled;
		actor.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.DrawBorder | RenderOptionFlags.BackgroundGradient;
		IStyleSheetStyle styleSheetStyle = (actor.Style = ServiceLocator.Instance.GetService<IStyleSheetService>().GetStyleSheet("style_sheet_common").GetStyle("rubine_button_style"));
		actor.StyleName = styleSheetStyle.Name;
		return actor;
	}

	public static IActor AddGrayButtonStyle(this IActor actor)
	{
		actor.GradientAngle = 90f;
		actor.BorderColor = Color.FromArgb(157, 157, 157);
		actor.BackgroundColor = Color.FromArgb(111, 111, 111);
		actor.GradientTargetColor = Color.FromArgb(70, 70, 70);
		actor.ForegroundColor = Color.White;
		actor.StrokeWeight = 2.5f;
		actor.VerticalAlignment = StringAlignment.Center;
		actor.HorizontalAlignment = StringAlignment.Center;
		actor.HitFlags = HitTestFlags.Enabled;
		actor.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.DrawBorder | RenderOptionFlags.BackgroundGradient;
		return actor;
	}

	public static IActor AddDarkGrayButtonStyle(this IActor actor)
	{
		actor.GradientAngle = 90f;
		actor.BorderColor = Color.White;
		actor.BackgroundColor = Color.FromArgb(48, 53, 57);
		actor.GradientTargetColor = Color.FromArgb(48, 53, 57);
		actor.ForegroundColor = Color.White;
		actor.StrokeWeight = 2.5f;
		actor.VerticalAlignment = StringAlignment.Center;
		actor.HorizontalAlignment = StringAlignment.Center;
		actor.HitFlags = HitTestFlags.Enabled;
		actor.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.DrawBorder | RenderOptionFlags.BackgroundGradient;
		return actor;
	}
}
