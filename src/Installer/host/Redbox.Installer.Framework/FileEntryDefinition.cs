using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Redbox.Compression;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "file")]
[DisplayName("File Entry")]
public class FileEntryDefinition : Definition
{
	private string m_sourcePath;

	private string m_destinationPath;

	private string m_explodeTargetPath;

	private FontFamily[] m_fontFamilies;

	private string m_tempFile;

	[DefaultValue(true)]
	[ElementProperty(Name = "copy")]
	public bool Copy { get; set; }

	[ElementProperty(Name = "external")]
	[DefaultValue(false)]
	public bool External { get; set; }

	[ElementProperty(Name = "source-path", IsRequired = true)]
	public string SourcePath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_sourcePath);
		}
		set
		{
			m_sourcePath = value;
		}
	}

	public string FileEntry
	{
		get
		{
			if (string.IsNullOrEmpty(m_tempFile))
			{
				return null;
			}
			return m_tempFile;
		}
	}

	public InstallState State { get; set; }

	public int CompressedSize { get; set; }

	[ElementProperty(Name = "resource-name")]
	public string ResourceName { get; set; }

	[ElementProperty(Name = "destination-path")]
	public string DestinationPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_destinationPath);
		}
		set
		{
			m_destinationPath = value;
		}
	}

	[ElementProperty(Name = "delete-after-explode")]
	public bool DeleteAfterExplode { get; set; }

	[ElementProperty(Name = "explode-target-path")]
	public string ExplodeTargetPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_explodeTargetPath);
		}
		set
		{
			m_explodeTargetPath = value;
		}
	}

	[ElementProperty(Name = "explode-on-copy")]
	public bool ExplodeArchiveOnCopy { get; set; }

	[ElementProperty(Name = "self-register")]
	[DefaultValue(true)]
	public bool SelfRegisterComServer { get; set; }

	[ElementProperty(Name = "execute-assembly-installer")]
	[DefaultValue(true)]
	public bool ExecuteAssemblyInstaller { get; set; }

	[ElementProperty(Name = "compression")]
	[DefaultValue(CompressionType.None)]
	public CompressionType CompressionType { get; set; }

	[DefaultValue(2)]
	public override int InstallSortOrder { get; set; }

	internal FontFamily[] FontFamilies
	{
		get
		{
			if (m_fontFamilies == null)
			{
				m_fontFamilies = GetFontFamilies(m_tempFile);
			}
			return m_fontFamilies;
		}
	}

	public FileEntryDefinition()
	{
		SelfRegisterComServer = true;
		ExecuteAssemblyInstaller = true;
	}

	public override string ToExecutionMessage()
	{
		string targetPath = GetTargetPath(FontFamilies != null, out var _);
		return $"{GetExecutionAction()} {targetPath}";
	}

	public override void LoadFromResource(LibraryModule module)
	{
		if (External)
		{
			if (File.Exists(SourcePath))
			{
				FileInfo fileInfo = new FileInfo(SourcePath);
				base.Size = Convert.ToInt32(fileInfo.Length);
			}
		}
		else
		{
			byte[] resource = ResourceTool.GetResource(module, "FILE", ResourceName);
			if (resource != null)
			{
				if (CompressionType == CompressionType.None)
				{
					m_tempFile = Path.GetTempFileName();
					File.WriteAllBytes(m_tempFile, resource);
				}
				else
				{
					CompressedSize = resource.Length;
					string tempFileName = Path.GetTempFileName();
					File.WriteAllBytes(tempFileName, resource);
					m_tempFile = Path.GetTempFileName();
					using (FileStream source = File.OpenRead(tempFileName))
					{
						using FileStream target = File.Create(m_tempFile);
						CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType);
						algorithm.Decompress(source, target);
					}
					File.Delete(tempFileName);
				}
				FileInfo fileInfo2 = new FileInfo(m_tempFile);
				base.Size = Convert.ToInt32(fileInfo2.Length);
			}
		}
		base.LoadFromResource(module);
	}

	public override void SaveToResource(ResourceUpdateSession session, int number, XmlDocument xmlDocument, string basePath, ErrorList errors)
	{
		if (xmlDocument == null || xmlDocument.DocumentElement == null || External)
		{
			return;
		}
		string sourceFile = Path.Combine(basePath, SourcePath);
		if (!File.Exists(sourceFile))
		{
			errors.Add(Error.NewError("C006", $"The file '{sourceFile}' was not found.", "Correct the source-path attribute of the file element and recompile."));
			return;
		}
		byte[] contents = StringExtensions.ReadToBuffer(sourceFile);
		int fileSize = contents.Length;
		int compressedSize = 0;
		if (CompressionType != CompressionType.None)
		{
			contents = contents.Compress(CompressionType);
			compressedSize = contents.Length;
		}
		InstallerProject.Instance.ActiveConsole.Write("\tEmbedding file '" + SourcePath + "', size ");
		InstallerProject.Instance.ActiveConsole.Write(Int32Extensions.FormatAsByteSize(fileSize));
		if (CompressionType != CompressionType.None)
		{
			InstallerProject.Instance.ActiveConsole.Write(", compressed file size ");
			InstallerProject.Instance.ActiveConsole.Write(Int32Extensions.FormatAsByteSize(compressedSize));
		}
		InstallerProject.Instance.ActiveConsole.Write("...");
		ResourceName = number.ToString("0000");
		session.AddResource("FILE", ResourceName, contents);
		XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(string.Format("{0}[@id='{1}']", "file", base.ID));
		if (xmlNode == null)
		{
			errors.Add(Error.NewError("C006", $"The file node with ID '{base.ID}' was not found.", "The compiler was attempting to mark up the file node with a resource-name attribute but the node wasn't found."));
			InstallerProject.Instance.ActiveConsole.WriteLine("error!");
			return;
		}
		XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("resource-name");
		xmlAttribute.Value = ResourceName;
		xmlNode.Attributes.Append(xmlAttribute);
		InstallerProject.Instance.ActiveConsole.WriteLine("done.");
	}

	public override int GetCountValue()
	{
		if (Copy)
		{
			return 1;
		}
		return 0;
	}

	public override int GetTaskCount()
	{
		if (!Copy)
		{
			return 0;
		}
		if (!IsLoadableModule())
		{
			return 1;
		}
		return 2;
	}

	public bool IsLoadableModule()
	{
		string fileName = Path.GetFileName(SourcePath).ToLower();
		if (!fileName.EndsWith(".dll") && !fileName.EndsWith(".exe"))
		{
			return fileName.EndsWith(".ocx");
		}
		return true;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			CopyToDestination(errors);
			State = ((!Copy) ? InstallState.Skipped : InstallState.Copied);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "An error occurred while copying files or creating directories; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			RemoveFromDestination(errors);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "An error occurred while removing files or directories; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	internal void CopyToDestination(ErrorList errors)
	{
		if ((string.IsNullOrEmpty(m_tempFile) && !External) || !Copy)
		{
			return;
		}
		string targetPath = GetTargetPath(FontFamilies != null, out var customPathUsed);
		string directoryName = Path.GetDirectoryName(targetPath);
		try
		{
			if (customPathUsed)
			{
				Directory.CreateDirectory(directoryName);
			}
			if (External)
			{
				string directoryName2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				File.Copy(Path.Combine(directoryName2, Path.GetFileName(SourcePath)), targetPath, overwrite: true);
			}
			else
			{
				File.Delete(targetPath);
				File.Copy(m_tempFile, targetPath);
				File.Delete(m_tempFile);
				m_tempFile = null;
			}
			if (ExplodeArchiveOnCopy && FontFamilies == null)
			{
				ZipHelper.ExtractAllFiles(targetPath, ExplodeTargetPath ?? directoryName);
				if (DeleteAfterExplode)
				{
					File.Delete(targetPath);
				}
			}
			if (!customPathUsed && FontFamilies != null)
			{
				FontHelper.AddFontResource(targetPath, FontFamilies[0].Name, isTrueType: true);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I001", $"Unable to copy file {Path.GetFileName(SourcePath)} to target directory {targetPath}.", ex.ToString()));
		}
	}

	internal void RemoveFromDestination(ErrorList errors)
	{
		string targetPath = GetTargetPath(FontFamilies != null, out var customPathUsed);
		try
		{
			string directoryName = Path.GetDirectoryName(targetPath);
			if (!customPathUsed && FontFamilies != null)
			{
				FontHelper.RemoveFontResource(targetPath, FontFamilies[0].Name, isTrueType: true);
			}
			if (File.Exists(targetPath))
			{
				File.Delete(targetPath);
			}
			if (ExplodeArchiveOnCopy && FontFamilies == null)
			{
				if (ExplodeTargetPath != null)
				{
					Directory.Delete(ExplodeTargetPath, recursive: true);
				}
				else
				{
					Directory.Delete(Path.Combine(InstallerProject.Instance.InstallPath, Path.GetFileNameWithoutExtension(SourcePath)), recursive: true);
				}
			}
			if (customPathUsed && Directory.GetFileSystemEntries(directoryName).Length == 0)
			{
				Directory.Delete(directoryName, recursive: true);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I001", $"Unable to remove file {targetPath}.", ex.ToString()));
		}
	}

	internal void InstallModule(InstallMode mode, string[] args, ErrorList errors)
	{
		try
		{
			State = InstallState.AssemblyInstalled;
			if (SelfRegistrationTool.IsModuleSelfRegistering(GetTargetPath()) && SelfRegisterComServer)
			{
				if (mode == InstallMode.Install)
				{
					SelfRegistrationTool.Register(GetTargetPath());
					InstallerProject.Instance.ActiveConsole.WriteLine("DllRegisterServer called for module: " + GetTargetPath());
				}
				else
				{
					SelfRegistrationTool.Unregister(GetTargetPath());
					InstallerProject.Instance.ActiveConsole.WriteLine("DllUnregisterServer called for module: " + GetTargetPath());
				}
			}
			if (ExecuteAssemblyInstaller)
			{
				InstallerProject.Instance.RebootRequired = AssemblyInstallerHelper.ExecuteInAppDomain(GetTargetPath(), args, mode, errors);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I001", $"Install module failed for file '{GetTargetPath()}'.", ex.ToString()));
		}
	}

	internal string GetExecutionAction()
	{
		if (State == InstallState.AssemblyInstalled)
		{
			if (InstallerProject.Instance.Mode != InstallMode.Install)
			{
				return "Uninstall";
			}
			return "Install";
		}
		if (InstallerProject.Instance.Mode != InstallMode.Install)
		{
			return "Remove";
		}
		return "Copy";
	}

	internal static FontFamily[] GetFontFamilies(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Length > 10485760)
			{
				return null;
			}
			byte[] fontData = File.ReadAllBytes(path);
			PrivateFontCollection privateFontCollection = new PrivateFontCollection();
			GCHandle gCHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
			privateFontCollection.AddMemoryFont(gCHandle.AddrOfPinnedObject(), fontData.Length);
			gCHandle.Free();
			return privateFontCollection.Families;
		}
		catch (Exception)
		{
			return null;
		}
	}

	internal string GetTargetPath()
	{
		bool customPathUsed;
		return GetTargetPath(isFont: false, out customPathUsed);
	}

	internal string GetTargetPath(bool isFont, out bool customPathUsed)
	{
		customPathUsed = false;
		string installPath = InstallerProject.Instance.InstallPath;
		string fileName = Path.GetFileName(SourcePath);
		string destinationPath = DestinationPath;
		if (destinationPath != null)
		{
			destinationPath = Path.Combine(installPath, DestinationPath);
			customPathUsed = true;
			return Path.Combine(destinationPath, fileName);
		}
		if (!isFont)
		{
			return Path.Combine(installPath, fileName);
		}
		return Path.Combine(Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), "Fonts"), fileName);
	}
}
