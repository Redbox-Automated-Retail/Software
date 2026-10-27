using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DeviceService.ComponentModel.Requests;

namespace DeviceService.ComponentModel;

public static class Extensions
{
	public static string GetDescription(this Enum value)
	{
		return value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description;
	}

	public static bool IsNullOrDefault(this Version value)
	{
		if (!(value == null))
		{
			return value == null;
		}
		return true;
	}

	public static CardBrandEnum? GetBrand(this CardBrandAndSource brandAndSource)
	{
		switch (brandAndSource)
		{
		case CardBrandAndSource.AmexChip:
		case CardBrandAndSource.AmexTap:
			return CardBrandEnum.Amex;
		case CardBrandAndSource.DiscoverChip:
		case CardBrandAndSource.DiscoverTap:
			return CardBrandEnum.Discover;
		case CardBrandAndSource.MasterCardChip:
		case CardBrandAndSource.MasterCardTap:
			return CardBrandEnum.Mastercard;
		case CardBrandAndSource.VisaChip:
		case CardBrandAndSource.VisaTap:
			return CardBrandEnum.VISA;
		default:
			return null;
		}
	}

	public static GeneralSourceType GetGeneralSourceType(this CardBrandAndSource brandAndSource)
	{
		switch (brandAndSource)
		{
		case CardBrandAndSource.AmexChip:
		case CardBrandAndSource.DiscoverChip:
		case CardBrandAndSource.MasterCardChip:
		case CardBrandAndSource.VisaChip:
			return GeneralSourceType.Insert;
		case CardBrandAndSource.AmexTap:
		case CardBrandAndSource.DiscoverTap:
		case CardBrandAndSource.MasterCardTap:
		case CardBrandAndSource.VisaTap:
			return GeneralSourceType.Tap;
		default:
			return GeneralSourceType.None;
		}
	}

	public static GeneralSourceType GetGeneralSourceType(this CardSourceType sourceType)
	{
		switch (sourceType)
		{
		case CardSourceType.Swipe:
			return GeneralSourceType.Swipe;
		case CardSourceType.EMVContact:
		case CardSourceType.QuickChip:
			return GeneralSourceType.Insert;
		case CardSourceType.MSDContactless:
		case CardSourceType.EMVContactless:
		case CardSourceType.Mobile:
			return GeneralSourceType.Tap;
		default:
			return GeneralSourceType.None;
		}
	}
}
