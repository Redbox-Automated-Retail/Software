using System;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.Rental.Services.Controllers;

public class ActorFunctions
{
	public static IActor CreateActor(string actorName, bool addToFront, int x, int y, int width, int height, string text, string font, string image, object backgroundColor, object foregroundColor, object textRegion, object textTranslationPoint, object textRotationAngle, object hotSpot, string renderOptions, string hitOptions)
	{
		if (string.IsNullOrEmpty(actorName))
		{
			return null;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IScene scene = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default");
		if (scene == null)
		{
			return null;
		}
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		IActor actor = scene.CreateActor();
		actor.Name = actorName;
		if (service2.PeekViewFrame()?.ViewFrame is IViewFrame { ViewWindow: not null } viewFrame)
		{
			actor.X = x + viewFrame.ViewWindow.Value.X;
			actor.Y = y + viewFrame.ViewWindow.Value.Y;
		}
		else
		{
			actor.X = x;
			actor.Y = y;
		}
		actor.Text = text;
		if (textRotationAngle != null)
		{
			float value = Convert.ToSingle(textRotationAngle);
			actor.TextRotationAngle = value;
		}
		if (textTranslationPoint is LuaTable luaTable)
		{
			actor.TextTranslationPoint = luaTable.ToPoint();
			luaTable.Dispose();
		}
		Color? colorFromValue = GetColorFromValue(backgroundColor);
		if (colorFromValue.HasValue)
		{
			actor.BackgroundColor = colorFromValue.Value;
		}
		Color? colorFromValue2 = GetColorFromValue(foregroundColor);
		if (colorFromValue2.HasValue)
		{
			actor.ForegroundColor = colorFromValue2.Value;
		}
		if (textRegion is LuaTable luaTable2)
		{
			actor.TextRegion = luaTable2.ToRectangle();
			luaTable2.Dispose();
		}
		if (hotSpot is LuaTable luaTable3)
		{
			actor.HotSpot = luaTable3.ToRectangle();
			luaTable3.Dispose();
		}
		if (!string.IsNullOrEmpty(renderOptions))
		{
			actor.OptionFlags = (RenderOptionFlags)ConversionHelper.ChangeType(renderOptions, typeof(RenderOptionFlags));
		}
		if (!string.IsNullOrEmpty(hitOptions))
		{
			actor.HitFlags = (HitTestFlags)ConversionHelper.ChangeType(hitOptions, typeof(HitTestFlags));
		}
		if (!string.IsNullOrEmpty(font))
		{
			actor.Font = service.GetFont(font, out var resourceFound);
			if (!resourceFound)
			{
				actor.OptionFlags |= RenderOptionFlags.DrawBorder;
				actor.ErrorText = $"Font resource '{font}' not found.";
			}
		}
		if (!string.IsNullOrEmpty(image))
		{
			if (image.StartsWith("cache:"))
			{
				string text2 = image.Substring(6);
				IBitmapCacheService service3 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
				if (service3 == null)
				{
					actor.OptionFlags |= RenderOptionFlags.DrawBorder;
					actor.ErrorText = "IBitmapCacheService not available from ServiceLocator.";
				}
				else
				{
					Image image2 = service3.GetImage(text2);
					if (image2 == null)
					{
						actor.OptionFlags |= RenderOptionFlags.DrawBorder;
						actor.ErrorText = $"Bitmap '{text2}' not in cache.";
					}
					else
					{
						actor.Image = image2;
					}
				}
			}
			else
			{
				Image bitmap = service.GetBitmap(image);
				if (bitmap == null)
				{
					actor.OptionFlags |= RenderOptionFlags.DrawBorder;
					actor.ErrorText = $"Bitmap resource '{image}' not found.";
				}
				else
				{
					actor.Image = bitmap;
				}
			}
		}
		actor.Width = width;
		actor.Height = height;
		if (addToFront)
		{
			scene.AddFirst(actor);
		}
		else
		{
			scene.AddLast(actor);
		}
		return actor;
	}

	public static void RemoveActor(string actorName)
	{
		ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.Remove(actorName)?.Dispose();
	}

	public static IActor GetActor(string name)
	{
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetActor(name);
	}

	public static IActor GetActorByNameAndTag(string name, string tag)
	{
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetActorByNameAndTag(name, tag);
	}

	public static void SetActorTag(string actorName, object tag)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Tag = tag;
		}
	}

	public static void SetActorOpacity(string actorName, float opacity)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Opacity = opacity;
		}
	}

	public static void SetActorTextAlignY(string actorName, string alignment)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.VerticalAlignment = Enum<StringAlignment>.ParseIgnoringCase(alignment, StringAlignment.Near);
		}
	}

	private static Color? GetColorFromValue(object value)
	{
		if (value is LuaTable luaTable)
		{
			Color? color = luaTable.ToColor();
			if (color.HasValue)
			{
				return color.Value;
			}
			luaTable.Dispose();
		}
		else if (value is string name)
		{
			return Color.FromName(name);
		}
		return null;
	}
}
