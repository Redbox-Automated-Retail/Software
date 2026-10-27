using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Reflection;

namespace Redbox.Core;

public class AssemblyInstallerHelper : MarshalByRefObject
{
	public static bool ExecuteInAppDomain(string targetPath, string[] args, InstallMode mode, List<string> errorList)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string location = executingAssembly.Location;
		if (string.IsNullOrEmpty(location))
		{
			return false;
		}
		AppDomain appDomain = null;
		bool result;
		try
		{
			appDomain = AppDomain.CreateDomain("AssemblyInstallerDomain");
			AssemblyInstallerHelper assemblyInstallerHelper = (AssemblyInstallerHelper)appDomain.CreateInstanceFromAndUnwrap(location, typeof(AssemblyInstallerHelper).FullName);
			result = assemblyInstallerHelper.Execute(targetPath, args, mode.ToString(), errorList);
		}
		finally
		{
			if (appDomain != null)
			{
				AppDomain.Unload(appDomain);
			}
		}
		return result;
	}

	internal bool Execute(string targetPath, string[] args, string mode, List<string> errors)
	{
		try
		{
			AssemblyInstaller.CheckIfInstallable(targetPath);
			using AssemblyInstaller assemblyInstaller = new AssemblyInstaller(targetPath, args);
			assemblyInstaller.UseNewContext = true;
			IDictionary dictionary = new Hashtable();
			try
			{
				if (mode == "Install")
				{
					assemblyInstaller.Install(dictionary);
				}
				else
				{
					assemblyInstaller.Uninstall(dictionary);
				}
				assemblyInstaller.Commit(dictionary);
				if (dictionary["RebootRequired"] != null)
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				errors.Add(ex.ToString());
				assemblyInstaller.Rollback(dictionary);
			}
		}
		catch (Exception ex2)
		{
			errors.Add(ex2.ToString());
		}
		return false;
	}
}
