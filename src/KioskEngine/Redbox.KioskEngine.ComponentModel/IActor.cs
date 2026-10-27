using System;
using System.Drawing;
using System.Windows;

namespace Redbox.KioskEngine.ComponentModel;

public interface IActor : IDisposable
{
	bool HitExists { get; }

	int X { get; set; }

	int Y { get; set; }

	Font Font { get; set; }

	int Width { get; set; }

	int Height { get; set; }

	object Tag { get; set; }

	string Name { get; set; }

	string Text { get; set; }

	string Html { get; set; }

	Image Image { get; set; }

	bool Visible { get; set; }

	bool? Locked { get; set; }

	bool Enabled { get; set; }

	IScene Scene { get; set; }

	bool IsDirty { get; }

	int? TabOrder { get; set; }

	float Opacity { get; set; }

	int FrameCount { get; }

	int FrameNumber { get; set; }

	Rectangle Bounds { get; }

	string StyleName { get; set; }

	string ErrorText { get; set; }

	float[] TabStops { get; set; }

	System.Drawing.Size? CornerSize { get; set; }

	float StrokeWeight { get; set; }

	Rectangle? HotSpot { get; set; }

	Color? BorderColor { get; set; }

	float? GradientAngle { get; set; }

	Rectangle? TextRegion { get; set; }

	HitTestFlags HitFlags { get; set; }

	Color ForegroundColor { get; set; }

	Color BackgroundColor { get; set; }

	float? CornerSweepAngle { get; set; }

	float? TextRotationAngle { get; set; }

	Color? GradientTargetColor { get; set; }

	string RelativeToActorName { get; set; }

	System.Drawing.Point? TextTranslationPoint { get; set; }

	RenderStateFlags StateFlags { get; set; }

	RenderOptionFlags OptionFlags { get; set; }

	StringAlignment VerticalAlignment { get; set; }

	StringAlignment HorizontalAlignment { get; set; }

	IStyleSheetStyle Style { get; set; }

	FrameworkElement WPFFrameworkElement { get; set; }

	string WPFControlName { get; set; }

	string WPFControlAssemblyName { get; set; }

	event EventHandler Hit;

	event WPFHitHandler OnWPFHit;

	void ClearHit();

	void RaiseHit();

	void MarkDirty();

	MeasureTextResult MeasureText();

	void CenterVertically();

	void CenterHorizontally();

	void Render(Graphics context);
}
