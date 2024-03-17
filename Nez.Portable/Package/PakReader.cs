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
		public readonly MemoryMappedViewAccessor accessor;
		public readonly PakHeader header;
		public readonly long dataOffset;
		public PakReader(string pakPath)
		{
			file = MemoryMappedFile.CreateFromFile(pakPath, FileMode.Open);
			accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
			
			using var stream = file.CreateViewStream();
			using var reader = new BinaryReader(stream);
			if(reader.ReadInt32() != PakHeader.MAGIC_NUMBER)
			{
				throw new InvalidOperationException();
			}
		    header = MemoryPackSerializer.Deserialize<PakHeader>(reader.ReadBytes(reader.ReadInt32()));
			dataOffset = stream.Position;
		}

		public MemoryMappedViewStream OpenFile(PakFileData data)
		{
			return file.CreateViewStream(dataOffset + data.offset, data.size);
		}

		public bool Exists(string path)
		{
			return header.files.ContainsKey(path);
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
