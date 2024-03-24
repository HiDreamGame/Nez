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
		public const ulong MAGIC_NUMBER_STEAM = 0x5453_534552474448;
		public const ulong MAGIC_NUMBER_ITCH = 0x5449_534552474448;

		public const ulong MAGIC_NUMBER_COMMON = 0x0000_534552474448;
		public const ulong MAGIC_NUMBER_MASK = 0x0000_ffffffffffff;
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
