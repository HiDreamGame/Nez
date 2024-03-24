using Nez.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Assets
{
	public static class Resources
	{
		public const string RESOURCES_FILE_NAME = "resources.pak";
		public static readonly PakReader pak;
		static Resources()
		{
			var pakPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RESOURCES_FILE_NAME);
			if(File.Exists(pakPath))
			{
				pak = new(pakPath);
			}
		}
		public static Stream OpenFile(string path)
		{
			var realPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", path);
			if(File.Exists(realPath) && false)
			{
				return File.Open(realPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			if(pak != null)
			{
				return pak.OpenFile(path);
			}
			throw new FileNotFoundException(null, path);
		}
		
	}
}
