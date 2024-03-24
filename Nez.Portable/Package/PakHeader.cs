using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Package
{
	[MemoryPackable]
	public partial class PakHeader
	{
		public const int MAGIC_NUMBER = 0x45524448;
		public Dictionary<string, PakFileData> files = [];
	}
	[MemoryPackable]
	public partial class PakFileData
	{
		public static string GetStandardPath(string path)
		{
			return path.ToLower().Replace('\\', '/').Replace("//", "/");
		}

		public long offset;
		public long size;
		public long decompressedSize = -1;
		public ulong crc;
		public List<string> tags = [];
	}
}
