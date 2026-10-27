using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Kernel;

public static class UpdateServiceFunctions
{
	[KernelFunction(Name = "kernel.uploadfile")]
	internal static LuaTable UploadFile(string file)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IUpdateService>().UploadFile(file);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e.ToString(), e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.pollupdateservice")]
	internal static LuaTable PollUpdateService()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IUpdateService>().Poll();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e.ToString(), e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.serverpoll")]
	internal static LuaTable ServerPoll()
	{
		ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll();
		errors.ToLogHelper();
		return CreateResultTable(errors);
	}

	[KernelFunction(Name = "kernel.startupdate")]
	internal static LuaTable StartUpate()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IUpdateService>().StartDownloads();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e.ToString(), e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.finishupdate")]
	internal static LuaTable FinishUpdate(string jobId)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IUpdateService>().FinishDownload(new Guid(jobId));
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e.ToString(), e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.finishallupdates")]
	internal static LuaTable FinishUpdates()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IUpdateService>().FinishDownloads();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e.ToString(), e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.dowork")]
	internal static LuaTable DoWork(string filter)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime);
	}

	[KernelFunction(Name = "kernel.updateaddstoretorepository")]
	internal static LuaTable AddStoreToRepository(string store, string name)
	{
		return CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().AddStoreToRepository(store, name));
	}

	[KernelFunction(Name = "kernel.updateaddgrouptorepository")]
	internal static LuaTable AddGroupToRepository(string group, string name)
	{
		return CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().AddGroupToRepository(group, name));
	}

	[KernelFunction(Name = "kernel.updateremovestorefromrepository")]
	internal static LuaTable RemoveStoreFromRepository(string number, string name)
	{
		return CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().RemoveStoreFromRepository(number, name));
	}

	[KernelFunction(Name = "kernel.updateremovegroupfromrepository")]
	internal static LuaTable RemoveGroupFromRepository(string group, string name)
	{
		return CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().RemoveGroupFromRepository(group, name));
	}

	[KernelFunction(Name = "kernel.startinstaller")]
	internal static LuaTable StartInstaller(string repositoryHash, string frontEndVersion)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		ErrorList errorList = service.StartInstaller(repositoryHash, frontEndVersion, out var response);
		if (errorList.ContainsError())
		{
			LogErrors(errorList);
			luaTable = CreateResultTable(errorList);
			luaTable["success"] = bool.FalseString;
		}
		else
		{
			luaTable["success"] = bool.TrueString;
			foreach (KeyValuePair<string, string> item in response)
			{
				luaTable[item.Key] = item.Value;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.finishinstaller")]
	internal static bool FinishInstaller(string guid, LuaTable data)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		string text = data.Keys.Cast<object>().ToDictionary((object k) => k as string, (object v) => data[v] as string).ToJson();
		if (text.Length > 252)
		{
			text = string.Empty;
		}
		ErrorList errorList = service.FinishInstaller(guid, "FinishInstallerData", text);
		if (errorList.ContainsError())
		{
			LogErrors(errorList);
			return false;
		}
		return true;
	}

	[KernelFunction(Name = "kernel.sendstatusmessage")]
	internal static bool StatusMessage(string type, string key, string subKey, string description, string data, bool encode)
	{
		ErrorList errorList = new ErrorList();
		if (!Extensions.TryParse<StatusMessage.StatusMessageType>(type, out var result, ignoreCase: true))
		{
			result = Redbox.UpdateService.Model.StatusMessage.StatusMessageType.Info;
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SSM01", "The type " + type + " is unparsable", "Defaulting to 'info'"));
			LogErrors(errorList);
			errorList.Clear();
		}
		errorList.AddRange(StatusMessageService.Instance.EnqueueMessage(result, key, subKey, description, data, encode));
		if (errorList.ContainsError())
		{
			LogErrors(errorList);
			return false;
		}
		return true;
	}

	[KernelFunction(Name = "kernel.sendscreenshot")]
	internal static bool SendScreenShot(double size)
	{
		try
		{
			Bitmap bitmap = TakeScreenshot();
			int width = (int)Math.Round((double)bitmap.Width * size);
			int height = (int)Math.Round((double)bitmap.Height * size);
			string data = BitmapToString(ResizeBitmap(bitmap, width, height));
			return StatusMessage("Info", "screenshot", string.Empty, "A kiosk screenshot", data, encode: false);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in kernel.sendscreenshot", e);
			return false;
		}
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}

	private static void LogErrors(IEnumerable<Redbox.UpdateManager.ComponentModel.Error> errors)
	{
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.Log(error.Description, LogEntryType.Debug);
		}
	}

	private static string BitmapToString(Image bitmap)
	{
		try
		{
			byte[] inArray;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, ImageFormat.Jpeg);
				inArray = memoryStream.ToArray();
			}
			return Convert.ToBase64String(inArray);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in BitmapToString", e);
			return null;
		}
	}

	private static Bitmap ResizeBitmap(Image bitmap, int width, int height)
	{
		try
		{
			Bitmap bitmap2 = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap2))
			{
				graphics.InterpolationMode = InterpolationMode.High;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.DrawImage(bitmap, 0, 0, width, height);
			}
			return bitmap2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in ResizeBitmap", e);
			return null;
		}
	}

	private static Bitmap TakeScreenshot()
	{
		try
		{
			Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
			}
			return bitmap;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in TakeScreenshot", e);
			return null;
		}
	}
}
