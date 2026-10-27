using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelExtensionHost : IDisposable
{
	IKernelExtension Extension { get; set; }

	ErrorList PreLoad();

	ErrorList Load();

	ErrorList Reset();

	ErrorList Activate();

	ErrorList Deactivate();

	void HandleHostCrash(Exception e);

	bool CanSwitch(out ErrorList errors);

	void RegisterCommunicationActivity(Action<object, EventArgs> action);
}
