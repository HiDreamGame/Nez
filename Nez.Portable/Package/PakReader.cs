using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Package
{
	public class PakReader : IDisposable
	{
		public readonly MemoryMappedFile file;
		public readonly PakHeader header;
		public readonly long dataOffset;
		private readonly Dictionary<PakFileData, WeakReference<byte[]>> decompressedDataCache = [];

		public bool isSteam;
		public bool isItch;
		public unsafe PakReader(string pakPath)
		{
			file = MemoryMappedFile.CreateFromFile(pakPath, FileMode.Open);
			using var stream = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
			using var reader = new BinaryReader(stream);

			var magic = reader.ReadUInt64();
			if(magic != PakHeader.MAGIC_NUMBER_COMMON)
			{
				throw new InvalidOperationException();
			}

		    header = MemoryPackSerializer.Deserialize<PakHeader>(reader.ReadBytes(reader.ReadInt32()));
			dataOffset = stream.Position;
		}

		public unsafe Stream OpenFile(PakFileData data)
		{
			var vs = file.CreateViewStream(dataOffset + data.offset, data.size);
			var handle = vs.SafeMemoryMappedViewHandle;
			byte* filePointer = null;
			handle.AcquirePointer(ref filePointer);
			filePointer += vs.PointerOffset;
			try
			{
				var crc = Crc64.Compute(filePointer, (int)data.size);
				if(data.crc != crc)
				{
					throw new InvalidDataException();
				}
				if (data.decompressedSize > 0)
				{
					if (decompressedDataCache.TryGetValue(data, out var cache) && cache.TryGetTarget(out var resultBuffer))
					{
						return new MemoryStream(resultBuffer, false);
					}
					resultBuffer = new byte[data.decompressedSize];

					if (LZ4Codec.Decode(new Span<byte>(filePointer, (int)data.size), resultBuffer) != data.decompressedSize)
					{
						throw new InvalidDataException();
					}

					cache = new(resultBuffer);
					decompressedDataCache[data] = cache;
					return new MemoryStream(resultBuffer, false);
				}
				return vs;
			}
			finally
			{
				handle.ReleasePointer();
			}
		}

		public bool Exists(string path)
		{
			return header.files.ContainsKey(PakFileData.GetStandardPath(path));
		}

		public bool TryOpenFile(string path, out Stream stream)
		{
			stream = null;
			if(!header.files.TryGetValue(PakFileData.GetStandardPath(path), out var data))
			{
				return false;
			}
			stream = OpenFile(data);
			return true;
		}
		public Stream OpenFile(string path)
		{
			return TryOpenFile(path, out var result) ? result : throw new FileNotFoundException(null, path);

		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			file.Dispose();
		}
	}
}
