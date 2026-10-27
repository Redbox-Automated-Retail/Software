using System;
using System.Windows;

namespace Redbox.Controls.Utilities;

public static class Dependency<TOwner>
{
	public static void DefaultOverrideMetadata(Action<Type, PropertyMetadata> overrideMetadata)
	{
		overrideMetadata(typeof(TOwner), new FrameworkPropertyMetadata(typeof(TOwner)));
	}

	public static PropertyMetadata CreatePropertyMetadata()
	{
		return new FrameworkPropertyMetadata
		{
			AffectsRender = true
		};
	}

	public static FrameworkPropertyMetadata CreatePropertyMetadata(object defaultValue)
	{
		return new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = true
		};
	}

	public static PropertyMetadata CreatePropertyMetadata(PropertyChangedCallback callback)
	{
		return new FrameworkPropertyMetadata(callback.Invoke)
		{
			AffectsRender = true
		};
	}

	public static FrameworkPropertyMetadata CreatePropertyMetadata(object defaultValue, PropertyChangedCallback callback)
	{
		return new FrameworkPropertyMetadata(defaultValue, callback.Invoke)
		{
			AffectsRender = true
		};
	}

	public static DependencyProperty CreateDependencyProperty(string propertyName, Type propertyType, object defaultValue = null)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner), CreatePropertyMetadata(defaultValue));
	}

	public static DependencyProperty CreateDependencyProperty(string propertyName, Type propertyType, object defaultValue, PropertyChangedCallback callback)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner), CreatePropertyMetadata(defaultValue, callback));
	}
}
