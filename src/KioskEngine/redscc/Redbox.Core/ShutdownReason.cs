using System;

namespace Redbox.Core;

[Flags]
public enum ShutdownReason : uint
{
	MajorApplication = 0x40000u,
	MajorHardware = 0x10000u,
	MajorLegacyApi = 0x70000u,
	MajorOperatingSystem = 0x20000u,
	MajorOther = 0u,
	MajorPower = 0x60000u,
	MajorSoftware = 0x30000u,
	MajorSystem = 0x50000u,
	MinorBlueScreen = 0xFu,
	MinorCordUnplugged = 0xBu,
	MinorDisk = 7u,
	MinorEnvironment = 0xCu,
	MinorHardwareDriver = 0xDu,
	MinorHotfix = 0x11u,
	MinorHung = 5u,
	MinorInstallation = 2u,
	MinorMaintenance = 1u,
	MinorMMC = 0x19u,
	MinorNetworkConnectivity = 0x14u,
	MinorNetworkCard = 9u,
	MinorOther = 0u,
	MinorOtherDriver = 0xEu,
	MinorPowerSupply = 0xAu,
	MinorProcessor = 8u,
	MinorReconfig = 4u,
	MinorSecurity = MinorHotfix | MinorInstallation,
	MinorSecurityFix = 0x12u,
	MinorSecurityFixUninstall = 0x18u,
	MinorServicePack = 0x10u,
	MinorServicePackUninstall = MinorNetworkConnectivity | MinorInstallation,
	MinorTermSrv = 0x20u,
	MinorUnstable = 6u,
	MinorUpgrade = 3u,
	MinorWMI = MinorHotfix | MinorReconfig,
	FlagUserDefined = 0x40000000u,
	FlagPlanned = 0x80000000u
}
