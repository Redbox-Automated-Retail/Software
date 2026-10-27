using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal abstract class AbstractMotionController
{
	protected readonly TextWriter LogFile;

	protected void WriteToLog(string fmt, params object[] stuff)
	{
		fmt = string.Format(fmt, stuff);
		WriteToLog(fmt);
	}

	protected void WriteToLog(string msg)
	{
		try
		{
			DateTime now = DateTime.Now;
			string value = $"{now.ToShortDateString()} {now.ToShortTimeString()} {msg}";
			LogFile.WriteLine(value);
		}
		catch
		{
		}
	}

	protected AbstractMotionController()
	{
		try
		{
			string path = Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), "MotionControlErrorLog.log");
			LogFile = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = true
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[MotionControl] Create motion control error log caught an exception.", e);
			LogFile = StreamWriter.Null;
		}
	}

	internal abstract bool OnStartup();

	internal abstract bool OnShutdown();

	internal abstract IMotionControlLimitResponse ReadLimits();

	internal abstract bool CommunicationOk();

	internal abstract IControllerPosition ReadPositions();

	internal abstract ErrorCodes MoveToTarget(ref MoveTarget target);

	internal abstract ErrorCodes MoveToVend(MoveMode mode);

	internal abstract ErrorCodes HomeAxis(Axis axis);

	internal abstract bool OnResetDeviceDriver();

	internal virtual void OnConfigurationLoad()
	{
	}

	internal virtual void OnConfigurationChangeStart()
	{
	}

	internal virtual void OnConfigurationChangeEnd()
	{
	}
}
