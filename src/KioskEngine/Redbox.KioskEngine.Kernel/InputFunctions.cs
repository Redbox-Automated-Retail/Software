using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class InputFunctions
{
	[KernelFunction(Name = "kernel.clearidlenotify")]
	internal static void ClearIdleNotify()
	{
		ServiceLocator.Instance.GetService<IInputService>().ClearIdleHandlers();
	}

	[KernelFunction(Name = "kernel.registeridlenotify")]
	internal static void RegisterIdleNotify(string name, IdleEventHandler idleHandler)
	{
		ServiceLocator.Instance.GetService<IInputService>().RegisterIdleHandler(name, idleHandler);
	}

	[KernelFunction(Name = "kernel.removeidlenotify")]
	internal static void RemoveIdleNotify(string name)
	{
		ServiceLocator.Instance.GetService<IInputService>().RemoveIdleHandler(name);
	}

	[KernelFunction(Name = "kernel.clearactivatednotify")]
	internal static void ClearActivatedNotify()
	{
		ServiceLocator.Instance.GetService<IInputService>().ClearActivatedHandlers();
	}

	[KernelFunction(Name = "kernel.removeactivatednotify")]
	internal static void RemoveActivatedNotify(string name)
	{
		ServiceLocator.Instance.GetService<IInputService>().RemoveActivatedHandler(name);
	}

	[KernelFunction(Name = "kernel.registeractivatednotify")]
	internal static void RegisterActivatedNotify(string name, ActivatedEventHandler activatedHandler)
	{
		ServiceLocator.Instance.GetService<IInputService>().RegisterActivatedHandler(name, activatedHandler);
	}

	[KernelFunction(Name = "kernel.clearclicknotify")]
	internal static void ClearClickNotify()
	{
		ServiceLocator.Instance.GetService<IInputService>().ClearMouseClickHandlers();
	}

	[KernelFunction(Name = "kernel.registerclicknotify")]
	internal static void RegisterMouseClickNotify(string name, MouseClickHandler mouseClickHandler)
	{
		ServiceLocator.Instance.GetService<IInputService>().RegisterMouseClickHandler(name, mouseClickHandler);
	}

	[KernelFunction(Name = "kernel.removeclicknotify")]
	internal static void RemoveMouseClickNotify(string name)
	{
		ServiceLocator.Instance.GetService<IInputService>().RemoveMouseClickHandler(name);
	}

	[KernelFunction(Name = "kernel.clearkeypressnotify")]
	internal static void ClearKeyPressNotify()
	{
		ServiceLocator.Instance.GetService<IInputService>().ClearKeyPressHandlers();
	}

	[KernelFunction(Name = "kernel.registerkeypressnotify")]
	internal static void RegisterKeyPressNotify(string name, KeyPressHandler keyPressHandler)
	{
		ServiceLocator.Instance.GetService<IInputService>().RegisterKeyPressHandler(name, keyPressHandler);
	}

	[KernelFunction(Name = "kernel.removekeypressnotify")]
	internal static void RemoveKeyPressNotify(string name)
	{
		ServiceLocator.Instance.GetService<IInputService>().RemoveKeyPressHandler(name);
	}

	[KernelFunction(Name = "kernel.cleardoubleclicknotify")]
	internal static void ClearDoubleClickNotify()
	{
		ServiceLocator.Instance.GetService<IInputService>().ClearMouseDoubleClickHandlers();
	}

	[KernelFunction(Name = "kernel.registerdoubleclicknotify")]
	internal static void RegisterDoubleClickNotify(string name, MouseClickHandler mouseClickHandler)
	{
		ServiceLocator.Instance.GetService<IInputService>().RegisterMouseDoubleClickHandler(name, mouseClickHandler);
	}

	[KernelFunction(Name = "kernel.showmouse")]
	internal static void ShowMouse()
	{
		Cursor.Show();
	}

	[KernelFunction(Name = "kernel.hidemouse")]
	internal static void HideMouse()
	{
		Cursor.Hide();
	}

	[KernelFunction(Name = "kernel.getmouseposscreen")]
	internal static LuaTable GetMousePositionInScreen()
	{
		LuaTable result = new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["x"] = Cursor.Position.X,
			["y"] = Cursor.Position.Y
		};
		KernelService.Instance.LuaRuntime["mouse_position"] = null;
		return result;
	}

	[KernelFunction(Name = "kernel.getmousepos")]
	internal static LuaTable GetMousePositionInClient()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		Point point = Application.OpenForms[0].PointToClient(Cursor.Position);
		luaTable["x"] = point.X;
		luaTable["y"] = point.Y;
		KernelService.Instance.LuaRuntime["mouse_position"] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.setmousepos")]
	internal static void SetMousePosition(int x, int y)
	{
		Cursor.Position = Application.OpenForms[0].PointToScreen(new Point(x, y));
	}

	[KernelFunction(Name = "kernel.mousesingleclick")]
	internal static void PerformSingleClick()
	{
		ServiceLocator.Instance.GetService<IInputService>()?.PerformSingleClick();
	}

	[KernelFunction(Name = "kernel.mousedoubleclick")]
	internal static void PerformDoubleClick()
	{
		ServiceLocator.Instance.GetService<IInputService>()?.PerformDoubleClick();
	}

	[KernelFunction(Name = "kernel.confirm")]
	internal static string Confirm(string message)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return MessageBox.Show(message, service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question).ToString();
	}

	[KernelFunction(Name = "kernel.showmessage")]
	internal static void ShowMessage(string message)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		MessageBox.Show(message, service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	[KernelFunction(Name = "kernel.sendkeys")]
	internal static void SendKeys(string keys)
	{
		ServiceLocator.Instance.GetService<IInputService>()?.SimulateKeyStrokes(keys);
	}
}
