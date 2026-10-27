using System.Runtime.InteropServices;

namespace UpdateClientService.API.Services.Transfer;

[ComImport]
[Guid("97EA99C7-0186-4AD4-8DF9-C5B4E0ED6B22")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IBackgroundCopyCallback
{
	void JobTransferred([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob);

	void JobError([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, [MarshalAs(UnmanagedType.Interface)] IBackgroundCopyError pError);

	void JobModification([MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, uint dwReserved);
}
