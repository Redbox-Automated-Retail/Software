// RECONSTRUCTED - not decompiled.
// Status.Client was built against DeviceService.ComponentModel 1.1.0.47, which is not in the kiosk image.
// Neither the shipped DeviceService.ComponentModel (1.10.13.0) nor any version in the update-store history has
// RebootStatusRequest, so it is rebuilt from the members Status.Client uses. Other members of the original are unknown.
namespace DeviceService.ComponentModel.Requests;

public class RebootStatusRequest
{
	public string? ApiKey { get; set; }

	public string? ApiUrl { get; set; }

	public RebootStatus Status { get; set; } = null!;
}
