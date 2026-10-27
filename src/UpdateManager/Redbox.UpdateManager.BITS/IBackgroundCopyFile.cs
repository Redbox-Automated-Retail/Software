using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[ComImport]
[Guid("01B7BD23-FB88-4A77-8490-5891D3E4653A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IBackgroundCopyFile
{
	void GetRemoteName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

	void GetLocalName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

	void GetProgress(out BG_FILE_PROGRESS pVal);
}
