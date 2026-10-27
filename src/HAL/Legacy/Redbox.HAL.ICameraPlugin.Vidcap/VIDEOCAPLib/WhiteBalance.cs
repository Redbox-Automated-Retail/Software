using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("2FA52E2E-E17A-4160-B6EA-C889249999BE")]
[CoClass(typeof(WhiteBalanceClass))]
[ComVisible(true)]
public interface WhiteBalance : IWhiteBalance
{
}
