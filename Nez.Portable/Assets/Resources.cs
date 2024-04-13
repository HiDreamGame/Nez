using Nez.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Assets
{
	public enum ResourceType
	{
		XNB = 1,
		Normal = 2
	}
	public static class Resources
	{

		public readonly static HashSet<string> XNBExtions = [
			".png", ".mp3", ".mp4", ".jpg", ".json", "", null
		];
		public const string RESOURCES_FILE_NAME = "app.res";
		public static readonly bool allowLoadUnpackFile = string.Equals(Environment.GetEnvironmentVariable("HDG_allowLoadUnpackFile"), 
										"true", StringComparison.OrdinalIgnoreCase);
		public static readonly PakReader pak;
		static Resources()
		{
			var pakPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RESOURCES_FILE_NAME);
			if(File.Exists(pakPath))
			{
				pak = new(pakPath);
			}
		}
		private static Stream InternalOpenFile(string path, bool nothrow)
		{
			if (allowLoadUnpackFile)
			{
				var realPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", path);
				if (File.Exists(realPath))
				{
					return File.Open(realPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
			}
			if (pak?.TryOpenFile(path, out var result) ?? false)
			{
				return result;
			}
			if(nothrow)
			{
				return null;
			}
			throw new FileNotFoundException(null, path);
		}
		public static Stream OpenFile(string path)
		{
			if(XNBExtions.Contains(Path.GetExtension(path)?.ToLower()))
			{
				return InternalOpenFile(path, true) ?? InternalOpenFile(Path.ChangeExtension(path, "xnb"), false);
			}
			else
			{
				return InternalOpenFile(path, false);
			}
		}
		private static ResourceType? InternalGetType(string path)
		{
			var result =  Path.GetExtension(path)?.ToLower() == ".xnb" ? ResourceType.XNB : ResourceType.Normal;
			if (allowLoadUnpackFile)
			{
				var realPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", path);
				if (File.Exists(realPath))
				{
					return result;
				}
			}
			if(pak?.Exists(path) ?? false)
			{
				return result;
			}
			return null;
		}
		public static ResourceType? GetType(string path)
		{
			if (XNBExtions.Contains(Path.GetExtension(path)?.ToLower()))
			{
				return InternalGetType(path) ?? InternalGetType(Path.ChangeExtension(path, "xnb"));
			}
			else
			{
				return InternalGetType(path);
			}
		}
		public static bool IsXNB(string path) => GetType(path) == ResourceType.XNB;
		public static byte[] ReadBytes(string path)
		{
			using var stream = OpenFile(path);
			var buffer = new byte[stream.Length];
			stream.ReadExactly(buffer);
			return buffer;
		}
		public static string ReadString(string path)
		{
			return Encoding.UTF8.GetString(ReadBytes(path));
		}
	}
}
