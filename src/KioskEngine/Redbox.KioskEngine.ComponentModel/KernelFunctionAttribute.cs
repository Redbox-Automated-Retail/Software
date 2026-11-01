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
		List<KernelFunctionInfo> kernelFunctionInfoList = new List<KernelFunctionInfo>();
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute));
				ObsoleteAttribute obsoleteAttribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ObsoleteAttribute));
				KernelFunctionAttribute[] attributes = (KernelFunctionAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(KernelFunctionAttribute));
				if (attributes != null && attributes.Length != 0)
				{
					KernelFunctionInfo kernelFunctionInfo = new KernelFunctionInfo
					{
						Method = methodInfo,
						Extension = extension,
						Description = descriptionAttribute?.Description,
						DeprecationWarning = obsoleteAttribute?.Message
					};
					kernelFunctionInfo.InnerAttributes.AddRange(attributes);
					kernelFunctionInfoList.Add(kernelFunctionInfo);
				}
			}
		}
		return kernelFunctionInfoList.AsReadOnly();
	}

	public string GetNamespace()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(Name))
		{
			string[] parts = Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parts.Length - 1; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(".");
				}
				stringBuilder.Append(parts[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
