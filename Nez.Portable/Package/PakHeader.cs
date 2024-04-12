using MemoryPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Package
{
	[MemoryPackable]
	public partial class PakHeader
	{
		public const ulong MAGIC_NUMBER_COMMON = 0x0000_534552474448;
		public Dictionary<string, PakFileData> files = [];
	}
	[MemoryPackable]
	public partial class PakFileData
	{
		public static string GetStandardPath(string path)
		{
			var ch = ArrayPool<char>.Shared.Rent(path.Length);
			var chi = -1;
			for (int i = 0; i < path.Length; i++)
			{
				var c = path[i];
				if (c == '\\')
				{
					c = '/';
				}
				else if (c >= 'A' && c <= 'Z')
				{
					c = (char)(c - 'A' + 'a');
				}
				ch[++chi] = c;
				if (c == '/')
				{
					if (chi == 0)
					{
						chi = -1;
						continue;
					}
					var prev1 = ch[chi - 1];
					if (prev1 == '/')
					{
						chi--;
						continue;
					}
					else if (prev1 == '.')
					{
						if (chi == 1)
						{
							chi = -1;
							continue;
						}
						else
						{
							var prev2 = ch[chi - 2];
							if (prev2 == '.')
							{
								if (chi == 2)
								{
									chi = -1;
									continue;
								}
								else
								{
									var prev3 = ch[chi - 3];
									if (prev3 == '/')
									{
										chi -= 4;
										while (chi >= 0 && ch[--chi] != '/') ;
										continue;
									}
								}
							}
							else if (prev2 == '/')
							{
								chi -= 2;
								continue;
							}
						}
					}
				}
			}
			path = new string(ch, 0, chi + 1);
			ArrayPool<char>.Shared.Return(ch);

			return path;
		}

		public long offset;
		public long size;
		public long decompressedSize = -1;
		public ulong crc;
		public List<string> tags = [];
	}
}
