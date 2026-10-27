using System.ComponentModel;

namespace DeviceService.ComponentModel;

public enum DeviceInputType
{
	[Description("Please Swipe Card")]
	M = 1,
	[Description("Please Tap Card")]
	C = 2,
	[Description("Please Insert Card")]
	S = 4,
	[Description("Please Tap or Swipe Card")]
	MC = 3,
	[Description("Please Insert or Swipe Card")]
	MS = 5,
	[Description("Please Tap or Insert Card")]
	CS = 6,
	[Description("Please Tap or Insert Card")]
	MCS = 7
}
