using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("63F6480C-997E-4FDE-AD63-A24E5F0FFDC7")]
[CoClass(typeof(CiRepairClass))]
[ComVisible(true)]
public interface CiRepair : ICiRepair
{
}
