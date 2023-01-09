using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace laba2
{
    public interface IMultiLock
    {
        public IDisposable AcquireLock(params string[] keys);
    }

    public class Lock : IDisposable
    {
        private readonly object[] locks;

        public Lock(object[] locks) => this.locks = locks;

        public void Dispose()
        {
            foreach (var _lock in locks.Reverse())
            {
                Monitor.Exit(_lock);
            }
        }
    }

    public class MultiLock : IMultiLock
    {
        private static Dictionary<string, object> objectsToLock = new Dictionary<string, object>();
        
        public MultiLock(string[] possibleKeys)
        {
            if (possibleKeys is null)
                throw new NullReferenceException();
            
            foreach (var key in possibleKeys)
            {
                if (key is null)
                    throw new ArgumentException();
                objectsToLock[key] = new object();
            }
        }

        public IDisposable AcquireLock(params string[] keys)
        {
            if (keys.Length == 0)
                throw new ArgumentException();
            
            Array.Sort(keys);
            
            var locks = new object[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!objectsToLock.TryGetValue(keys[i], out var obj))
                    throw new ArgumentException();
                locks[i] = obj;
            }

            var index = 0;
            try
            {
                for (; index < keys.Length; index++)
                {
                    Monitor.Enter(locks[index]);
                }
            }
            catch
            {
                ReleaseLocks(index, locks);
                throw;
            }

            return new Lock(locks);
        }

        private void ReleaseLocks(int index, object[] locks)
        {
            if (Monitor.IsEntered(locks[index]))
            {
                Monitor.Exit(locks[index]);
            }

            for (var j = index - 1; j >= 0; j--)
            {
                Monitor.Exit(locks[j]);
            }
        }
    }
}