using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace KioskAdmin;

public class SecurityService
{
	private const string AdminUserName = "rbAdmin";

	private const string AdminPassword = "<redacted>";

	private const string DefaultUserPassword = "<redacted>";

	private const string PowerUserGroup = "power users";

	private const string AdminUserGroup = "administrators";

	private static SecurityService _securityService;

	public static SecurityService Instance
	{
		get
		{
			if (_securityService == null)
			{
				_securityService = new SecurityService();
			}
			return _securityService;
		}
	}

	private SecurityService()
	{
	}

	public void Run()
	{
		try
		{
			List<string> allAdministrators = GetAllAdministrators();
			allAdministrators.ForEach(delegate(string s)
			{
				LogService.Log("administrator", s);
			});
			List<string> allUsers = GetAllUsers();
			allUsers.ForEach(delegate(string s)
			{
				LogService.Log("user", s);
			});
			string defaultUser = GetDefaultUser();
			LogService.Log("current default user", defaultUser);
			if (ChangeDefaultUserPassword(defaultUser, "<redacted>"))
			{
				ChangePasswords(allAdministrators, "Admin", defaultUser, "<redacted>");
			}
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured changing passwords exception: {0} inner exception: {1}", ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
		}
	}

	public void DemoteDefaultUser()
	{
		try
		{
			List<string> allAdministrators = GetAllAdministrators();
			allAdministrators.ForEach(delegate(string s)
			{
				LogService.Log("administrator", s);
			});
			List<string> allUsers = GetAllUsers();
			allUsers.ForEach(delegate(string s)
			{
				LogService.Log("user", s);
			});
			string currentDefaultUser = GetDefaultUser();
			if (string.IsNullOrEmpty(currentDefaultUser))
			{
				LogService.Log("error", "default user does not exist");
				return;
			}
			LogService.Log("current default user", currentDefaultUser);
			if (!allAdministrators.Any((string s) => s.Equals("rbAdmin", StringComparison.CurrentCultureIgnoreCase)) && !CreateUser("rbAdmin", "<redacted>", "administrators"))
			{
				LogService.Log("Create Default Admin", string.Format("Default Admin create :{0} failed", "rbAdmin"));
				return;
			}
			if (!allAdministrators.Any((string s) => s.Equals(currentDefaultUser, StringComparison.CurrentCultureIgnoreCase)))
			{
				LogService.Log("Info", $"Default User:{currentDefaultUser} is not an admin, no changes are needed");
				return;
			}
			if (currentDefaultUser.Equals("rbAdmin", StringComparison.CurrentCultureIgnoreCase))
			{
				LogService.Log("Default is Admin Name", string.Format("Default user is named the same as the admin user: {0}", "rbAdmin"));
				return;
			}
			if (AddGroup(currentDefaultUser, "power users"))
			{
				RemoveGroup(currentDefaultUser, "administrators");
			}
			if (ChangeDefaultUserPassword(currentDefaultUser, "<redacted>"))
			{
				ChangePasswords(allAdministrators, "Admin", currentDefaultUser, "<redacted>");
			}
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured creating default user exception: {0} inner exception: {1}", ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
		}
	}

	public void SetBackgroundAsUser(string backgroundPath, string user, string password)
	{
		string location = Assembly.GetExecutingAssembly().Location;
		ShellHelper.RunProcessAsUser(location, string.Format("-{0}:\"{1}\"", "setbackground", backgroundPath), user, password);
	}

	public void SetRunOnce(string name, string application, string parameters, bool machine)
	{
		string keyName = (machine ? "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce" : "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
		string value = $"{application} {parameters}";
		Registry.SetValue(keyName, name, value);
		LogService.Log($"Set {name} RunOnce", value);
	}

	public bool AddGroup(string userName, string groupName)
	{
		try
		{
			if (IsMember(userName, groupName))
			{
				LogService.Log("Group Member", $"User: {userName} is already a member of: {groupName}");
				return true;
			}
			DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
			DirectoryEntry directoryEntry2 = directoryEntry.Children.Find(groupName, "group");
			if (directoryEntry2 == null)
			{
				LogService.Log("Error", $"Could not locate group: {groupName}");
				return false;
			}
			DirectoryEntry user = GetUser(userName);
			directoryEntry2.Invoke("Add", user.Path.ToString());
			directoryEntry2.CommitChanges();
			LogService.Log("Added user to group", $"Removed user: {userName} from group: {groupName}");
			return true;
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured adding user {0} to group {1} exception: {2} inner exception: {3}", userName, groupName, ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
			return false;
		}
	}

	public bool RemoveGroup(string userName, string groupName)
	{
		try
		{
			if (!IsMember(userName, groupName))
			{
				LogService.Log("Group Member", $"User: {userName} is not a member of: {groupName}");
				return true;
			}
			DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
			DirectoryEntry directoryEntry2 = directoryEntry.Children.Find(groupName, "group");
			if (directoryEntry2 == null)
			{
				LogService.Log("Error", $"Could not locate group: {groupName}");
				return false;
			}
			DirectoryEntry user = GetUser(userName);
			directoryEntry2.Invoke("Remove", user.Path.ToString());
			directoryEntry2.CommitChanges();
			LogService.Log("Remove user from group", $"Removed user: {userName} from group: {groupName}");
			return true;
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured removing user {0} from group {1} exception: {2} inner exception: {3}", userName, groupName, ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
			return false;
		}
	}

	public bool CreateUser(string userName, string password, string groupName)
	{
		try
		{
			DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
			DirectoryEntry directoryEntry2 = directoryEntry.Children.Find(groupName, "group");
			if (directoryEntry2 == null)
			{
				LogService.Log("Error", $"Could not locate group: {groupName}");
				return false;
			}
			DirectoryEntry directoryEntry3 = directoryEntry.Children.Add(userName, "user");
			directoryEntry3.Invoke("SetPassword", password);
			directoryEntry3.Invoke("Put", "Description", $"Redbox {groupName} user");
			directoryEntry3.CommitChanges();
			LogService.Log("Created User", userName);
			directoryEntry2.Invoke("Add", directoryEntry3.Path.ToString());
			LogService.Log("Added User to Group", groupName);
			directoryEntry3.Properties["UserFlags"].Value = (int)directoryEntry3.Properties["UserFlags"].Value | 0x10000;
			directoryEntry3.CommitChanges();
			LogService.Log("Set Password to not expire", userName);
			return true;
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured creating user {0} exception: {1} inner exception: {2}", userName, ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
			return false;
		}
	}

	public bool ChangeDefaultUserPassword(string defaultUser, string password)
	{
		if (!string.IsNullOrEmpty(defaultUser) && SetPassword(defaultUser, password))
		{
			LogService.Log("password change default user", defaultUser);
			SetDefaultUser(defaultUser, password);
			return true;
		}
		return false;
	}

	public void ChangePasswords(List<string> users, string type, string excludeDefaultUser, string password)
	{
		users.ForEach(delegate(string s)
		{
			if (!s.Equals(excludeDefaultUser, StringComparison.CurrentCultureIgnoreCase) && SetPassword(s, password))
			{
				LogService.Log($"password change {type} user: {s}", s);
			}
		});
	}

	public List<string> GetAllAdministrators()
	{
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
		DirectoryEntry directoryEntry2 = directoryEntry.Children.Find("administrators", "group");
		object obj = directoryEntry2.Invoke("members", null);
		foreach (object item in (IEnumerable)obj)
		{
			DirectoryEntry directoryEntry3 = new DirectoryEntry(item);
			if (directoryEntry3.SchemaClassName == "User")
			{
				list.Add(directoryEntry3.Name);
			}
		}
		return list.Distinct().ToList();
	}

	public bool IsMember(string userName, string groupName)
	{
		List<string> allMembers = GetAllMembers(groupName);
		return allMembers.Any((string s) => s.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
	}

	public List<string> GetAllMembers(string groupName)
	{
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
		DirectoryEntry directoryEntry2 = directoryEntry.Children.Find(groupName, "group");
		object obj = directoryEntry2.Invoke("members", null);
		foreach (object item in (IEnumerable)obj)
		{
			DirectoryEntry directoryEntry3 = new DirectoryEntry(item);
			if (directoryEntry3.SchemaClassName == "User")
			{
				list.Add(directoryEntry3.Name);
			}
		}
		return list.Distinct().ToList();
	}

	public List<string> GetAllUsers()
	{
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
		foreach (DirectoryEntry child in directoryEntry.Children)
		{
			if (child.SchemaClassName == "User")
			{
				list.Add(child.Name);
			}
		}
		return list.Distinct().ToList();
	}

	public DirectoryEntry GetUser(string user)
	{
		new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
		return directoryEntry.Children.Find(user, "User");
	}

	public bool SetPassword(string user, string password)
	{
		try
		{
			DirectoryEntry user2 = GetUser(user);
			user2.Invoke("SetPassword", password);
			return true;
		}
		catch (Exception ex)
		{
			LogService.Log("error", string.Format("An error occured changing the user: {0} password exception: {1} inner exception: {2}", user, ex.Message, (ex.InnerException == null) ? "none" : ex.InnerException.Message));
			return false;
		}
	}

	public string GetDefaultUser()
	{
		return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "DefaultUserName", string.Empty);
	}

	public void SetDefaultUser(string user, string password)
	{
		string keyName = "HKEY_CURRENT_USER\\Software\\Sysinternals\\Autologon";
		Registry.SetValue(keyName, "EulaAccepted", 1);
		ProcessFunctions.Execute("c:\\temp\\Autologon.exe", $"{user} {Environment.MachineName} {password}", "c:\\temp", null);
	}
}
