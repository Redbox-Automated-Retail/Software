using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("CA51E165-C365-424C-8D41-24AAA4FF3C40")]
public interface IEnumBackgroundCopyFiles
{
	void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile rgelt, out uint pceltFetched);

	void Skip(uint celt);

	void Reset();

	void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles ppenum);

	void GetCount(out uint puCount);
}
