using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace UpdateClientService.API.Services.Transfer;

[ComImport]
[Guid("4991D34B-80A1-4291-83B6-3328366B9097")]
[ClassInterface(ClassInterfaceType.None)]
[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
internal class BackgroundCopyManager
{
}
