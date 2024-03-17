using MemoryPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Package
{
	public class PakWriter
	{
		public readonly MemoryStream data = new();
		public readonly PakHeader header = new();
		public readonly Dictionary<string, PakFileData> hashCache = [];
		public void AddFile(string path, byte[] data)
		{
			if (path.StartsWith('/') || path.StartsWith('\\')) path = path[1..];
			var sha = BitConverter.ToString(SHA512.HashData(data));
			if(!hashCache.TryGetValue(sha, out var fd))
			{
				fd = new()
				{
					offset = this.data.Position,
					size = data.LongLength
				};
				this.data.Write(data);
				hashCache.Add(sha, fd);
			}
			header.files.Add(PakFileData.GetStandardPath(path), fd);
		}

		public void Generate(Stream output)
		{
			using var s = new BinaryWriter(output, Encoding.UTF8, true);
			s.Write(PakHeader.MAGIC_NUMBER);
			var header = MemoryPackSerializer.Serialize(this.header);
			s.Write(header.Length);
			s.Write(header);
			data.Position = 0;
			data.CopyTo(output);
		}
	}
}
