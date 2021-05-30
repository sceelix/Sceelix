using System;
using System.IO;
using Assimp;
using Sceelix.Core.Resources;

namespace Sceelix.Meshes.IO
{
    public class CustomIOSystem : IOSystem
    {
        private readonly string _directory;
        private readonly IResourceManager _resourceManager;



        public CustomIOSystem(IResourceManager resourceManager, string directory)
        {
            _resourceManager = resourceManager;
            _directory = directory;
        }



        public override IOStream OpenFile(string pathToFile, FileIOMode fileMode)
        {
            return new CustomIOStream(Path.Combine(_directory, pathToFile), fileMode, _resourceManager);
        }



        public class CustomIOStream : IOStream
        {
            private Stream _stream;



            public CustomIOStream(string pathToFile, FileIOMode fileMode, IResourceManager resourceManager)
                : base(pathToFile, fileMode)
            {
                _stream = resourceManager.Load<Stream>(pathToFile);
            }



            public override bool IsValid => _stream != null;



            protected override void Dispose(bool disposing)
            {
                if (!IsDisposed && disposing)
                {
                    if (_stream != null)
                        _stream.Close();

                    _stream = null;

                    base.Dispose(disposing);
                }
            }



            public override void Flush()
            {
                if (_stream == null)
                    return;

                _stream.Flush();
            }



            public override long GetFileSize()
            {
                if (_stream == null)
                    return 0;

                return _stream.Length;
            }



            public override long GetPosition()
            {
                if (_stream == null)
                    return -1;

                return _stream.Position;
            }



            public override long Read(byte[] dataRead, long count)
            {
                if (dataRead == null)
                    throw new ArgumentOutOfRangeException("dataRead", "Array to store data in cannot be null.");

                if (count < 0 || dataRead.Length < count)
                    throw new ArgumentOutOfRangeException("count", "Number of bytes to read is greater than data store size.");

                if (_stream == null || !_stream.CanRead)
                    throw new IOException("Stream is not readable.");

                return _stream.Read(dataRead, (int) _stream.Position, (int) count);
            }



            public override ReturnCode Seek(long offset, Origin seekOrigin)
            {
                if (_stream == null || !_stream.CanSeek)
                    throw new IOException("Stream does not support seeking.");

                SeekOrigin orig = SeekOrigin.Begin;
                switch (seekOrigin)
                {
                    case Origin.Set:
                        orig = SeekOrigin.Begin;
                        break;
                    case Origin.Current:
                        orig = SeekOrigin.Current;
                        break;
                    case Origin.End:
                        orig = SeekOrigin.End;
                        break;
                }

                _stream.Seek(offset, orig);

                return ReturnCode.Success;
            }



            public override long Write(byte[] dataToWrite, long count)
            {
                if (dataToWrite == null)
                    throw new ArgumentOutOfRangeException("dataToWrite", "Data to write cannot be null.");

                if (count < 0 || dataToWrite.Length < count)
                    throw new ArgumentOutOfRangeException("count", "Number of bytes to write is greater than data size.");

                if (_stream == null || !_stream.CanWrite)
                    throw new IOException("Stream is not writable.");

                _stream.Write(dataToWrite, (int) _stream.Position, (int) count);

                return count;
            }
        }
    }
}