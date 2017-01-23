using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AuthenticodeLint.Core
{
    //This is a stream that acts as a middleperson for hashing.
    public sealed class BlockHashStream : Stream
    {
        public int BufferSize { get; } = 0x1000;
        private long _length = 0;

        private readonly HashAlgorithm _hashAlgorithm;
        private readonly byte[] _writeBuffer;
        private readonly int _hashSizeBytes;
        private readonly ManualResetEventSlim _readEvent, _writeEvent;
        private readonly Task<byte[]> _work;
        private volatile bool _complete;
        private int _bytesAvailable = 0, _offset = 0;

        public BlockHashStream(HashAlgorithm hashAlgorithm, int bufferSize = 0x1000)
        {
            _hashAlgorithm = hashAlgorithm;
            BufferSize = bufferSize;
            _hashSizeBytes = bufferSize;
            _writeBuffer = new byte[_hashSizeBytes];
            _readEvent = new ManualResetEventSlim(false);
            _writeEvent = new ManualResetEventSlim(true);
            _work = Task.Run(() =>
            {
                return this._hashAlgorithm.ComputeHash(this);
            });
            _complete = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //Console.WriteLine("WAITING TO READ");
            _readEvent.Wait();
            //Console.WriteLine("READING");
            if (_complete && _bytesAvailable == 0)
            {
                //Console.WriteLine("COMPLETE");
                _writeEvent.Set();
                _readEvent.Set();
                return 0;
            }
            Debug.Assert(_bytesAvailable > 0);
            int read;
            if (_bytesAvailable >= count)
            {
                //Console.WriteLine("BIG READ");
                Buffer.BlockCopy(_writeBuffer, _offset, buffer, offset, count);
                _bytesAvailable -= count;
                _offset += count;
                read = count;
            }
            else
            {
                //Console.WriteLine("LITTLE READ");
                Buffer.BlockCopy(_writeBuffer, _offset, buffer, offset, _bytesAvailable);
                read = _bytesAvailable;
                _bytesAvailable = 0;
            }
            if (_bytesAvailable == 0)
            {
                _offset = 0;
                //Console.WriteLine("BLOCKING READ");
                _readEvent.Reset();
                //Console.WriteLine("ALLOWING WRITE");
                _writeEvent.Set();
            }
            _length += read;
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > _hashSizeBytes)
            {
                throw new InvalidOperationException($"Cannot write data larger than {count}.");
            }
            if (_complete)
            {
                throw new InvalidOperationException("Stream is complete.");
            }
            //Console.WriteLine("WAITING TO WRITE");
            _writeEvent.Wait();
            //Console.WriteLine("WRITING");
            Debug.Assert(_bytesAvailable == 0);
            Buffer.BlockCopy(buffer, offset, _writeBuffer, 0, count);
            _bytesAvailable = count;
            //Console.WriteLine("BLOCKING WRITE");
            _writeEvent.Reset();
            //Console.WriteLine("ALLOWING READ");
            _readEvent.Set();
        }

        public void WriteNativeBlock(IntPtr handle, int offset, int count)
        {
            if (count > _hashSizeBytes)
            {
                throw new InvalidOperationException($"Cannot write data larger than {count}.");
            }
            if (_complete)
            {
                throw new InvalidOperationException("Stream is complete.");
            }
            _writeEvent.Wait();
            Debug.Assert(_bytesAvailable == 0);
            Marshal.Copy(handle + offset, _writeBuffer, 0, count);
            _bytesAvailable = count;
            _writeEvent.Reset();
            _readEvent.Set();
        }

        public void Write(SafeHandle handle, int offset, int count)
        {
            bool handled = false;
            handle.DangerousAddRef(ref handled);
            if (!handled)
            {
                throw new InvalidOperationException("Unable to read native memory.");
            }
            var address = handle.DangerousGetHandle();
            try
            {
                if (count <= _hashSizeBytes)
                {
                    WriteNativeBlock(address, offset, count);
                    return;
                }
                var blocks = count / _hashSizeBytes;
                var remainder = count % _hashSizeBytes;
                for (var i = 0; i < blocks; i++)
                {
                    WriteNativeBlock(address, offset + (i * _hashSizeBytes), _hashSizeBytes);
                }
                if (remainder != 0)
                {
                    WriteNativeBlock(address, offset + (blocks * _hashSizeBytes), remainder);
                }
            }
            finally
            {
                handle.DangerousRelease();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public async Task<ArraySegment<byte>> Digest()
        {
            _complete = true;
            _writeEvent.Wait(); //Wait until the stream is writable, signaling the digest has read everything.
            _readEvent.Set(); //Let the digest know it can read "zero".
            Debug.Assert(_bytesAvailable == 0);
            return new ArraySegment<byte>(await _work.ConfigureAwait(false));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readEvent.Dispose();
                _writeEvent.Dispose();
            }
        }
    }
}