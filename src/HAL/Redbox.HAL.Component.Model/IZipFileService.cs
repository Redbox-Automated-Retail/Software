using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IZipFileService
{
	ZipResult Zip(IEnumerable<string> files, string zipPath);

	UnzipResult Unzip(string zipFile);

	UnzipResult Unzip(string zipFile, string outputPath);
}
