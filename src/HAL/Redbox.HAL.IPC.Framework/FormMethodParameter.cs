using System;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Compression;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.IPC.Framework;

internal class FormMethodParameter
{
	public int Index { get; set; }

	public string KeyName { get; set; }

	public ParameterInfo Parameter { get; set; }

	public CommandKeyValueAttribute MetaData { get; set; }

	public bool IsRequired()
	{
		if (!Parameter.ParameterType.IsValueType)
		{
			if (MetaData != null)
			{
				return MetaData.IsRequired;
			}
			return false;
		}
		if (MetaData != null)
		{
			return MetaData.IsRequired;
		}
		return false;
	}

	public object ConvertValue(string value)
	{
		if (Parameter.ParameterType != typeof(byte[]))
		{
			return ConversionHelper.ChangeType(value, Parameter.ParameterType);
		}
		byte[] array = MetaData.BinaryEncoding switch
		{
			BinaryEncoding.Hex => StringExtensions.HexToBytes(value), 
			BinaryEncoding.Base64 => StringExtensions.Base64ToBytes(value), 
			BinaryEncoding.Ascii95 => throw new NotImplementedException("Ascii95 support is not implemented."), 
			_ => throw new ArgumentException("The named paramete accepts binary data and the command form method doesn't specify a valid BinaryEncoding."), 
		};
		if (array != null && MetaData.CompressionType != CompressionType.None)
		{
			array = CompressionAlgorithmFactory.GetAlgorithm(MetaData.CompressionType).Decompress(array);
		}
		return array;
	}
}
