using System.Linq;
using System.Reflection;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services;

public static class EnumExtensions
{
	public static TitleFamily? GetTitleFamily(this TitleType val)
	{
		return val.GetType().GetMember(val.ToString()).FirstOrDefault()?.GetCustomAttribute<TitleFamilyAttribute>(inherit: false)?.TitleFamily;
	}
}
