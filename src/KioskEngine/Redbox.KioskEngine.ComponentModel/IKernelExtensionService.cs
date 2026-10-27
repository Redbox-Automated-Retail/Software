using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelExtensionService
{
	ReadOnlyCollection<IKernelExtension> Extensions { get; }

	string[] SearchPath { get; set; }

	ErrorList Initialize(List<Type> kernelExtensionHostTypes);

	IKernelExtension GetExtension(Guid id);

	ErrorList LoadExtension(Type extensionHostType);

	ErrorList LoadAllExtensions();

	ErrorList ResetAllExtensions();

	ErrorList ActivateAllExtensions();

	ErrorList DeactivateAllExtensions();

	bool CanSwitch(out ErrorList errors);

	void NotifyOfHostCrash(Exception e);
}
