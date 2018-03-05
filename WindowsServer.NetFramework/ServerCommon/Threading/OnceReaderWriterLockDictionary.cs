using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServer.Threading
{
    internal class OnceReaderLock : IDisposable
    {
        private ReaderWriterLockSlim _syncObject = null;
        internal OnceReaderLock(ReaderWriterLockSlim syncObject)
        {
            _syncObject = syncObject;
            if (_syncObject != null)
            {
                _syncObject.EnterReadLock();
            }
        }
        public void Dispose()
        {
            if (_syncObject != null)
            {
                _syncObject.ExitReadLock();
            }
        }
    }

    internal class OnceWriterLock : IDisposable
    {
        private OnceReaderWriterLockDictionary _container = null;
        private string _key = null;
        private ReaderWriterLockSlim _syncObject = null;

        internal OnceWriterLock(OnceReaderWriterLockDictionary container, string key, ReaderWriterLockSlim syncObject)
        {
            _container = container;
            _key = key;
            _syncObject = syncObject;
            syncObject.EnterWriteLock();
        }

        public void Dispose()
        {
            _syncObject.ExitWriteLock();
            _container.RemoveWriterLock(_key);
        }
    }

    public class OnceReaderWriterLockDictionary
    {
        private readonly Dictionary<string, ReaderWriterLockSlim> _writeLocks = new Dictionary<string, ReaderWriterLockSlim>();

        public IDisposable AcquireWriterLock(string key)
        {
            lock(_writeLocks)
            {
                ReaderWriterLockSlim writerLock;
                if (!_writeLocks.TryGetValue(key, out writerLock))
                {
                    writerLock = new ReaderWriterLockSlim();
                    _writeLocks.Add(key, writerLock);
                }
                return new OnceWriterLock(this, key, writerLock);
            }
        }

        public IDisposable AcquireReaderLock(string key)
        {
            lock (_writeLocks)
            {
                ReaderWriterLockSlim writerLock;
                if (!_writeLocks.TryGetValue(key, out writerLock))
                {
                    return new OnceReaderLock(null);
                }
                return new OnceReaderLock(writerLock);
            }
        }

        internal void RemoveWriterLock(string key)
        {
            lock(_writeLocks)
            {
                _writeLocks.Remove(key);
            }
        }
    }
}
