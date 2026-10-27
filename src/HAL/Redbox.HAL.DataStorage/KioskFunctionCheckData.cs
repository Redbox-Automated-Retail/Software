using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class KioskFunctionCheckData : IKioskFunctionCheckData
{
	public string VerticalSlotTestResult { get; internal set; }

	public string InitTestResult { get; internal set; }

	public string VendDoorTestResult { get; internal set; }

	public string TrackTestResult { get; internal set; }

	public string SnapDecodeTestResult { get; internal set; }

	public string TouchscreenDriverTestResult { get; internal set; }

	public string CameraDriverTestResult { get; internal set; }

	public DateTime Timestamp { get; internal set; }

	public string UserIdentifier { get; internal set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Kiosk Check Data");
		stringBuilder.AppendLine($"  Vertical slot result = {VerticalSlotTestResult}");
		stringBuilder.AppendLine($"  Init result = {InitTestResult}");
		stringBuilder.AppendLine($"  Vend door result = {VendDoorTestResult}");
		stringBuilder.AppendLine($"  Track result = {TrackTestResult}");
		stringBuilder.AppendLine($"  Snap and decode result = {SnapDecodeTestResult}");
		stringBuilder.AppendLine($"  Camera driver result = {CameraDriverTestResult}");
		stringBuilder.AppendLine($"  Touchscreen driver result = {TouchscreenDriverTestResult}");
		stringBuilder.AppendLine($"  Timestamp = {Timestamp.ToString()}");
		stringBuilder.AppendLine($"  User token = {UserIdentifier}");
		return stringBuilder.ToString();
	}

	internal KioskFunctionCheckData()
	{
	}
}
