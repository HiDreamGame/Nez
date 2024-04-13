using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using MemoryPack;
using System;
using System.Buffers;
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
					offset = this.data.Position
				};

				var buffer = new byte[data.Length];
				var compressedSize = LZ4Codec.Encode(data, buffer, LZ4Level.L12_MAX);
				if(compressedSize <= 0 || compressedSize / (float)data.Length > 0.7f)
				{
					fd.decompressedSize = -1;
					fd.crc = Crc64.Compute(data, data.Length);
					fd.size = data.Length;
					this.data.Write(data);
				}
				else
				{
					fd.tags.Add("COMPRESSED");
					fd.decompressedSize = data.LongLength;
					fd.size = compressedSize;
					fd.crc = Crc64.Compute(buffer, compressedSize);
					this.data.Write(buffer, 0, compressedSize);
				}

				
				hashCache.Add(sha, fd);
			}
			header.files.Add(PakFileData.GetStandardPath(path), fd);
		}

		public void Generate(Stream output, ulong magic = PakHeader.MAGIC_NUMBER_COMMON)
		{
			using var s = new BinaryWriter(output, Encoding.UTF8, true);
			s.Write(magic);
			var header = MemoryPackSerializer.Serialize(this.header);
			s.Write(header.Length);
			s.Write(header);
			data.Position = 0;
			data.CopyTo(output);
		}
	}
}
