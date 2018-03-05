using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Middlewares
{
    internal class RequestTrackerHijackStream : Stream
    {
        private byte[] _hijackBytes = null;
        private int _hijackIndex = 0;

        public int TotalLength { get; private set; }

        public Stream BaseStream { get; private set; }

        public RequestTrackerHijackStream(Stream baseStream, int hijackSize)
        {
            BaseStream = baseStream;
            _hijackBytes = new byte[hijackSize];
        }

        public override bool CanRead
        {
            get { return BaseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return BaseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return BaseStream.CanWrite; }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Length
        {
            get { return BaseStream.Length; }
        }

        public override long Position
        {
            get
            {
                return BaseStream.Position;
            }
            set
            {
                BaseStream.Position = value; ;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Hijack
            if (_hijackIndex < _hijackBytes.Length)
            {
                if ((_hijackIndex + count ) < _hijackBytes.Length)
                {
                    Array.Copy(buffer, offset, _hijackBytes, _hijackIndex, count);
                    _hijackIndex += count;
                }
                else
                {
                    var size = _hijackBytes.Length - _hijackIndex;
                    Array.Copy(buffer, offset, _hijackBytes, _hijackIndex, size);
                    _hijackIndex += size;
                }
            }

            TotalLength += count;

            BaseStream.Write(buffer, offset, count);
        }

        public byte[] GenerateHijackSnapshot()
        {
            var bytes = new byte[_hijackIndex];
            Array.Copy(_hijackBytes, bytes, _hijackIndex);
            return bytes;
        }
    }
}
