using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Redbox.Remoting.Nrbf;

/// <summary>
/// Turns the reader's untyped graph into instances of known .NET types, driven by the declared target type (a method
/// parameter, then field types). Fields are matched by their serialized name (auto-properties serialize as
/// "&lt;Name&gt;k__BackingField", which the C# compiler emits the same way here). Members the target type does not have are
/// ignored and missing ones keep their default, as BinaryFormatter does for version-tolerant types.
/// Handles List&lt;T&gt; (_items/_size), arrays, enums (value__), and primitives (decimal and DateTime are already
/// converted by the reader).
/// </summary>
internal sealed class NrbfObjectMapper
{
	private readonly Dictionary<object, object> _materialized = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

	public static object Map(object value, Type targetType) => new NrbfObjectMapper().MapValue(value, targetType);

	public static T Map<T>(object value) => (T)Map(value, typeof(T));

	private object MapValue(object value, Type targetType)
	{
		if (value == null)
		{
			return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null ? Activator.CreateInstance(targetType) : null;
		}
		Type nullableUnderlying = Nullable.GetUnderlyingType(targetType);
		if (nullableUnderlying != null)
		{
			targetType = nullableUnderlying;
		}
		switch (value)
		{
			case NrbfObject objectRecord:
				return MapObject(objectRecord, targetType);
			case NrbfArray arrayRecord:
				return MapArray(arrayRecord, targetType);
			default:
				return ConvertPrimitive(value, targetType);
		}
	}

	private static object ConvertPrimitive(object value, Type targetType)
	{
		if (targetType.IsInstanceOfType(value))
		{
			return value;
		}
		if (targetType.IsEnum)
		{
			return Enum.ToObject(targetType, value);
		}
		return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
	}

	private object MapObject(NrbfObject obj, Type targetType)
	{
		if (_materialized.TryGetValue(obj, out object done))
		{
			return done;
		}
		if (targetType.IsEnum)
		{
			return Enum.ToObject(targetType, obj["value__"] ?? 0);
		}
		if (targetType == typeof(object))
		{
			throw new InvalidDataException($"Cannot map {obj.TypeName} to System.Object without a declared type.");
		}
		if (IsGenericList(targetType, out Type itemType))
		{
			return MapList(obj, targetType, itemType);
		}
		object instance = RuntimeHelpers.GetUninitializedObject(targetType);
		_materialized[obj] = instance;
		Dictionary<string, FieldInfo> fields = FieldsByName(targetType);
		for (int i = 0; i < obj.MemberNames.Count; i++)
		{
			if (fields.TryGetValue(obj.MemberNames[i], out FieldInfo field))
			{
				field.SetValue(instance, MapValue(obj.Values[i], field.FieldType));
			}
		}
		return instance;
	}

	// List<T> is serialized as { _items: T[capacity], _size, _version }; only the first _size items are real.
	private object MapList(NrbfObject obj, Type listType, Type itemType)
	{
		IList list = (IList)Activator.CreateInstance(listType);
		_materialized[obj] = list;
		object[] items = (obj["_items"] as NrbfArray)?.Items ?? Array.Empty<object>();
		int size = obj["_size"] is int count ? Math.Min(count, items.Length) : items.Length;
		for (int i = 0; i < size; i++)
		{
			list.Add(MapValue(items[i], itemType));
		}
		return list;
	}

	private object MapArray(NrbfArray array, Type targetType)
	{
		if (_materialized.TryGetValue(array, out object done))
		{
			return done;
		}
		Type elementType = targetType.IsArray ? targetType.GetElementType() : typeof(object);
		Array result = Array.CreateInstance(elementType, array.Items.Length);
		_materialized[array] = result;
		for (int i = 0; i < array.Items.Length; i++)
		{
			result.SetValue(MapValue(array.Items[i], elementType), i);
		}
		return result;
	}

	private static bool IsGenericList(Type type, out Type itemType)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			itemType = type.GetGenericArguments()[0];
			return true;
		}
		itemType = null;
		return false;
	}

	private static Dictionary<string, FieldInfo> FieldsByName(Type type)
	{
		Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
		for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
		{
			foreach (FieldInfo field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				string name = current == type ? field.Name : current.Name + "+" + field.Name;
				fields.TryAdd(name, field);
			}
		}
		return fields;
	}
}
