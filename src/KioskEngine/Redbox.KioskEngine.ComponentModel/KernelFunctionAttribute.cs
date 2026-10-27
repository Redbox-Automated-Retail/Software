using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Redbox.KioskEngine.ComponentModel;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class KernelFunctionAttribute : Attribute
{
	public string Name { get; set; }

	public string ExtensionName { get; set; }

	public static ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctionInfos(Assembly assembly, string extension)
	{
		List<KernelFunctionInfo> list = new List<KernelFunctionInfo>();
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute));
				ObsoleteAttribute obsoleteAttribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ObsoleteAttribute));
				KernelFunctionAttribute[] array = (KernelFunctionAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(KernelFunctionAttribute));
				if (array != null && array.Length != 0)
				{
					KernelFunctionInfo kernelFunctionInfo = new KernelFunctionInfo
					{
						Method = methodInfo,
						Extension = extension,
						Description = descriptionAttribute?.Description,
						DeprecationWarning = obsoleteAttribute?.Message
					};
					kernelFunctionInfo.InnerAttributes.AddRange(array);
					list.Add(kernelFunctionInfo);
				}
			}
		}
		return list.AsReadOnly();
	}

	public string GetNamespace()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(Name))
		{
			string[] array = Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(".");
				}
				stringBuilder.Append(array[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
