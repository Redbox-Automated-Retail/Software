using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Reflection;

namespace Redbox.Installer.Framework;

public class AssemblyInstallerHelper : MarshalByRefObject
{
	public static bool ExecuteInAppDomain(string targetPath, string[] args, InstallMode mode, ErrorList errors)
	{
		AppDomain appDomain = null;
		try
		{
			appDomain = AppDomain.CreateDomain("AssemblyInstallerDomain");
			AssemblyInstallerHelper assemblyInstallerHelper = (AssemblyInstallerHelper)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(AssemblyInstallerHelper).FullName);
			List<string> list = new List<string>();
			bool result = assemblyInstallerHelper.Execute(targetPath, args, mode.ToString(), list);
			foreach (string item in list)
			{
				errors.Add(Error.NewError("C999", "An unhandled exception was thrown while running AssemblyInstaller.", item));
			}
			return result;
		}
		finally
		{
			if (appDomain != null)
			{
				AppDomain.Unload(appDomain);
			}
		}
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
				if (dictionary["rebootrequiredflag"] != null)
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
		catch
		{
		}
		return false;
	}
}
