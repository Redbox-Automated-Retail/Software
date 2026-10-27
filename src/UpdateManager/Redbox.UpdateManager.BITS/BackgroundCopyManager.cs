using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Redbox.UpdateManager.BITS;

[ComImport]
[Guid("4991D34B-80A1-4291-83B6-3328366B9097")]
[ClassInterface(ClassInterfaceType.None)]
[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
public class BackgroundCopyManager
{
}
