using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

public static class ActorFunctions
{
	[KernelFunction(Name = "kernel.createactor")]
	public static void CreateActor(string actorName, bool addToFront, int x, int y, int width, int height, string text, string font, string image, object backgroundColor, object foregroundColor, object textRegion, object textTranslationPoint, object textRotationAngle, object hotSpot, string renderOptions, string hitOptions)
	{
		if (string.IsNullOrEmpty(actorName))
		{
			return;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IScene scene = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default");
		if (scene == null)
		{
			return;
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
	}

	[KernelFunction(Name = "kernel.markactordirty")]
	public static void MarkActorDirty(string actorName)
	{
		GetActor(actorName)?.MarkDirty();
	}

	[KernelFunction(Name = "kernel.setactortag")]
	public static void SetActorTag(string actorName, object tag)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Tag = tag;
		}
	}

	[KernelFunction(Name = "kernel.setactortext")]
	public static void SetActorText(string actorName, string text)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Text = text;
		}
	}

	[KernelFunction(Name = "kernel.setactorhtml")]
	public static void SetActorHtml(string actorName, string html)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Html = html;
		}
	}

	[KernelFunction(Name = "kernel.setrelativeactor")]
	public static void SetReleativeToActor(string actorName, string relativeActor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.RelativeToActorName = relativeActor;
		}
	}

	[KernelFunction(Name = "kernel.setactortextalignx")]
	public static void SetActorTextAlignX(string actorName, string alignment)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.HorizontalAlignment = Enum<StringAlignment>.ParseIgnoringCase(alignment, StringAlignment.Near);
		}
	}

	[KernelFunction(Name = "kernel.setactortextaligny")]
	public static void SetActorTextAlignY(string actorName, string alignment)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.VerticalAlignment = Enum<StringAlignment>.ParseIgnoringCase(alignment, StringAlignment.Near);
		}
	}

	[KernelFunction(Name = "kernel.setactorforegroundcolor")]
	public static void SetActorForegroundColor(string actorName, object foregroundColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && foregroundColor != null)
		{
			Color? colorFromValue = GetColorFromValue(foregroundColor);
			if (colorFromValue.HasValue)
			{
				actor.ForegroundColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorbackgroundcolor")]
	public static void SetActorBackgroundColor(string actorName, object backgroundColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && backgroundColor != null)
		{
			Color? colorFromValue = GetColorFromValue(backgroundColor);
			if (colorFromValue.HasValue)
			{
				actor.BackgroundColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorgradienttargetcolor")]
	public static void SetActorGradientTargetColor(string actorName, object gradientTargetColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && gradientTargetColor != null)
		{
			Color? colorFromValue = GetColorFromValue(gradientTargetColor);
			if (colorFromValue.HasValue)
			{
				actor.GradientTargetColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorgradientangle")]
	public static void SetActorGradientAngle(string actorName, float gradientAngle)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.GradientAngle = gradientAngle;
		}
	}

	[KernelFunction(Name = "kernel.setactorimage")]
	public static void SetActorImage(string actorName, string resourceName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && !string.IsNullOrEmpty(resourceName))
		{
			if (resourceName.StartsWith("cache:"))
			{
				IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
				actor.Image = service.GetImage(resourceName.Substring(6));
			}
			else
			{
				IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
				actor.Image = service2.GetBitmap(resourceName);
			}
		}
	}

	[KernelFunction(Name = "kernel.isactorimageset")]
	public static bool IsActorImageSet(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return false;
		}
		return actor.Image != null;
	}

	[KernelFunction(Name = "kernel.isactorvisible")]
	public static bool IsActorVisible(string actorName)
	{
		return GetActor(actorName)?.Visible ?? false;
	}

	[KernelFunction(Name = "kernel.isactorenabled")]
	public static bool IsActorEnabled(string actorName)
	{
		return GetActor(actorName)?.Enabled ?? false;
	}

	[KernelFunction(Name = "kernel.clearactorimage")]
	public static void ClearActorImage(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return;
		}
		try
		{
			actor.Image = null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ActorFunctions.ClearActorImage.", e);
		}
	}

	[KernelFunction(Name = "kernel.setactoropacity")]
	public static void SetActorOpacity(string actorName, float opacity)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Opacity = opacity;
		}
	}

	[KernelFunction(Name = "kernel.setactorlocation")]
	public static void SetActorLocation(string actorName, int x, int y)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			if (ServiceLocator.Instance.GetService<IViewService>().PeekViewFrame()?.ViewFrame is IViewFrame { ViewWindow: not null } viewFrame)
			{
				actor.X = x + viewFrame.ViewWindow.Value.X;
				actor.Y = y + viewFrame.ViewWindow.Value.Y;
			}
			else
			{
				actor.X = x;
				actor.Y = y;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorframe")]
	public static void SetActorFrame(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.FrameNumber++;
		}
	}

	[KernelFunction(Name = "kernel.getactorx")]
	public static int GetActorX(string actorName)
	{
		return GetActor(actorName)?.X ?? 0;
	}

	[KernelFunction(Name = "kernel.getactory")]
	public static int GetActorY(string actorName)
	{
		return GetActor(actorName)?.Y ?? 0;
	}

	[KernelFunction(Name = "kernel.moveactorx")]
	public static void MoveActorX(string actorName, int pixels)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.X += pixels;
		}
	}

	[KernelFunction(Name = "kernel.moveactory")]
	public static void MoveActorY(string actorName, int pixels)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Y += pixels;
		}
	}

	[KernelFunction(Name = "kernel.setactorx")]
	public static void SetActorX(string actorName, int x)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			if (ServiceLocator.Instance.GetService<IViewService>().PeekViewFrame()?.ViewFrame is IViewFrame { ViewWindow: not null } viewFrame)
			{
				actor.X = x + viewFrame.ViewWindow.Value.X;
			}
			else
			{
				actor.X = x;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactory")]
	public static void SetActorY(string actorName, int y)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			if (ServiceLocator.Instance.GetService<IViewService>().PeekViewFrame()?.ViewFrame is IViewFrame { ViewWindow: not null } viewFrame)
			{
				actor.Y = y + viewFrame.ViewWindow.Value.Y;
			}
			else
			{
				actor.Y = y;
			}
		}
	}

	[KernelFunction(Name = "kernel.getactorheight")]
	public static int GetActorHeight(string actorName)
	{
		return GetActor(actorName)?.Height ?? 0;
	}

	[KernelFunction(Name = "kernel.getactorwidth")]
	public static int GetActorWidth(string actorName)
	{
		return GetActor(actorName)?.Width ?? 0;
	}

	[KernelFunction(Name = "kernel.setactorheight")]
	public static void SetActorHeight(string actorName, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Height = height;
		}
	}

	[KernelFunction(Name = "kernel.setactorwidth")]
	public static void SetActorWidth(string actorName, int width)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Width = width;
		}
	}

	[KernelFunction(Name = "kernel.setactordimension")]
	public static void SetActorDimension(string actorName, int width, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Width = width;
			actor.Height = height;
		}
	}

	[KernelFunction(Name = "kernel.measureactortext")]
	public static LuaTable MeasureActorText(string actorName)
	{
		IScene scene = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default");
		if (scene == null || string.IsNullOrEmpty(actorName))
		{
			return null;
		}
		IActor actor = scene.GetActor(actorName);
		if (actor == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		MeasureTextResult measureTextResult = actor.MeasureText();
		luaTable["width"] = measureTextResult.Size.Width;
		luaTable["height"] = measureTextResult.Size.Height;
		luaTable["characters_fitted"] = measureTextResult.CharactersFitted;
		luaTable["lines_filled"] = measureTextResult.LinesFilled;
		KernelService.Instance.LuaRuntime[actorName] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hideactor")]
	public static void HideActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Visible = false;
		}
	}

	[KernelFunction(Name = "kernel.showactor")]
	public static void ShowActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Visible = true;
		}
	}

	public static void SetActorVisible(string actorName, bool value)
	{
		if (value)
		{
			ShowActor(actorName);
		}
		else
		{
			HideActor(actorName);
		}
	}

	[KernelFunction(Name = "kernel.removeactor")]
	public static void RemoveActor(string actorName)
	{
		ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.Remove(actorName)?.Dispose();
	}

	[KernelFunction(Name = "kernel.removeactors")]
	public static void RemoveActors(LuaTable actorNames)
	{
		IScene scene = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default");
		if (scene == null || actorNames == null)
		{
			return;
		}
		scene.SuspendDrawing();
		foreach (object key in actorNames.Keys)
		{
			scene.Remove((string)actorNames[key])?.Dispose();
		}
		scene.ResumeDrawing();
		actorNames.Dispose();
	}

	[KernelFunction(Name = "kernel.clearactors")]
	public static void ClearActors()
	{
		ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.Clear();
	}

	[KernelFunction(Name = "kernel.centeractorx")]
	public static void CenterActorX(string actorName)
	{
		GetActor(actorName)?.CenterHorizontally();
	}

	[KernelFunction(Name = "kernel.centeractory")]
	public static void CenterActorY(string actorName)
	{
		GetActor(actorName)?.CenterVertically();
	}

	[KernelFunction(Name = "kernel.enableactor")]
	public static void EnableActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Enabled = true;
		}
	}

	[KernelFunction(Name = "kernel.disableactor")]
	public static void DisableActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Enabled = false;
		}
	}

	[KernelFunction(Name = "kernel.registeractorhitnotify")]
	public static void RegisterActorHitNotify(string actorName, HitHandler hitHandler)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return;
		}
		if (hitHandler == null)
		{
			LogHelper.Instance.Log("WARNING: hitHandler is null in kernel.registeractorhitnotify for actorName = '{0}'!", actorName);
			return;
		}
		actor.ClearHit();
		actor.Hit += delegate
		{
			hitHandler();
		};
	}

	[KernelFunction(Name = "kernel.executehitnotify")]
	public static void ExecuteHitNotify(string actorName)
	{
		GetActor(actorName)?.RaiseHit();
	}

	[KernelFunction(Name = "kernel.getactorsbytag")]
	public static LuaTable GetActorsByTag(string tag)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		List<IActor> actorByTag = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default").GetActorByTag(tag);
		int num = 1;
		foreach (IActor item in actorByTag)
		{
			luaTable[num++] = item.Name;
		}
		KernelService.Instance.LuaRuntime["actors_by_tag"] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getactorbytaborder")]
	public static string GetActorByTabOrder(int tabOrder)
	{
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default").GetActorByTabOrder(tabOrder)?.Name;
	}

	[KernelFunction(Name = "kernel.getactortag")]
	public static object GetActorTag(string name)
	{
		return GetActor(name)?.Tag;
	}

	[KernelFunction(Name = "kernel.getactortext")]
	public static string GetActorText(string name)
	{
		return GetActor(name)?.Text;
	}

	[KernelFunction(Name = "kernel.setactortabstops")]
	public static void SetActorTabStops(string name, object tabStops)
	{
		IActor actor = GetActor(name);
		if (actor != null && tabStops is LuaTable luaTable)
		{
			actor.TabStops = luaTable.ToList<float>().ToArray();
			luaTable.Dispose();
		}
	}

	[KernelFunction(Name = "kernel.gettaborder")]
	public static int? GetTabOrder(string actorName)
	{
		return GetActor(actorName)?.TabOrder;
	}

	[KernelFunction(Name = "kernel.settaborder")]
	public static void SetTabOrder(string actorName, int tabOrder)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.TabOrder = tabOrder;
		}
	}

	[KernelFunction(Name = "kernel.setbordercolor")]
	public static void SetBorderColor(string actorName, object value)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			Color? colorFromValue = GetColorFromValue(value);
			if (colorFromValue.HasValue)
			{
				actor.BorderColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setborderstrokeweight")]
	public static void SetBorderStrokeWeight(string actorName, float weight)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.StrokeWeight = weight;
		}
	}

	[KernelFunction(Name = "kernel.setcornersize")]
	public static void SetCornerSize(string actorName, int width, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.CornerSize = new Size(width, height);
		}
	}

	[KernelFunction(Name = "kernel.clearcornersize")]
	public static void ClearCornerSize(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.CornerSize = null;
		}
	}

	[KernelFunction(Name = "kernel.getlasthitactortag")]
	public static object GetLastHitActorTag()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.LastHitActorTag;
	}

	[KernelFunction(Name = "kernel.setlasthitactortag")]
	public static void SetLastHitActorTag(string actorTag)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.LastHitActorTag = actorTag;
		}
	}

	[KernelFunction(Name = "kernel.getlasthitactorname")]
	public static string GetLastHitActorName()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.LastHitActorName;
	}

	[KernelFunction(Name = "kernel.setlasthitactorname")]
	public static void SetLastHitActorName(string actorName)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.LastHitActorName = actorName;
		}
	}

	[KernelFunction(Name = "kernel.getactortextregion")]
	public static string GetActorTextRegion(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return "0, 0, 0, 0";
		}
		Rectangle value = actor.TextRegion.Value;
		return value.X + ", " + value.Y + ", " + value.Width + ", " + value.Height;
	}

	[KernelFunction(Name = "kernel.getactoropacity")]
	public static float GetActorOpacity(string actorName)
	{
		return GetActor(actorName)?.Opacity ?? 1f;
	}

	public static IActor GetActor(string name)
	{
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetActor(name);
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
