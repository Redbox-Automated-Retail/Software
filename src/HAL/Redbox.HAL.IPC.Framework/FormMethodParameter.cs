using System;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Compression;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.IPC.Framework;

internal class FormMethodParameter
{
	public int Index { get; set; }

	// KeyName and Parameter are set by CommandRepository.GetCommand.
	public string KeyName { get; set; } = null!;

	public ParameterInfo Parameter { get; set; } = null!;

	public CommandKeyValueAttribute? MetaData { get; set; }

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

	public object? ConvertValue(string? value)
	{
		if (Parameter.ParameterType != typeof(byte[]))
		{
			return ConversionHelper.ChangeType(value, Parameter.ParameterType);
		}
		byte[] bytes = MetaData!.BinaryEncoding switch
		{
			BinaryEncoding.Hex => StringExtensions.HexToBytes(value!), 
			BinaryEncoding.Base64 => StringExtensions.Base64ToBytes(value!), 
			BinaryEncoding.Ascii95 => throw new NotImplementedException("Ascii95 support is not implemented."), 
			_ => throw new ArgumentException("The named paramete accepts binary data and the command form method doesn't specify a valid BinaryEncoding."), 
		};
		if (bytes != null && MetaData.CompressionType != CompressionType.None)
		{
			bytes = CompressionAlgorithmFactory.GetAlgorithm(MetaData.CompressionType).Decompress(bytes);
		}
		return bytes;
	}
}
