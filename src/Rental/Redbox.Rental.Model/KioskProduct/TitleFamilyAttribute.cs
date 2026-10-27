using System;

namespace Redbox.Rental.Model.KioskProduct;

[AttributeUsage(AttributeTargets.Field)]
public class TitleFamilyAttribute : Attribute
{
	public TitleFamily TitleFamily { get; set; }
}
