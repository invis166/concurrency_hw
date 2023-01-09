using System.Threading;

namespace CustomThreadPool.Collections
{
    public class WorkStealingQueue<T> 
        where T : class
    {
        private const int InitialSize = 32;
        private T[] array = new T[InitialSize]; 
        private int mask = InitialSize - 1;
        private volatile int headIndex = 0;
        private volatile int tailIndex = 0;
        private readonly object foreignLock = new object();

        public bool IsEmpty => headIndex >= tailIndex;

        public int Count => tailIndex - headIndex;

        public void LocalPush(T obj)
        {
            var tail = tailIndex;
            if(tail < headIndex + mask)
            {
                array[tail & mask] = obj; // safe! только в этом методе пишем в m_array
                tailIndex = tail + 1; // safe! только local-операции меняют m_tailIndex
            }
            else
            {
                lock (foreignLock)
                {
                    var head = headIndex;
                    var count = tailIndex - headIndex;
                    if(count >= mask)
                    {
                        var newArray = new T[array.Length << 1];
                        for(var i = 0; i < array.Length; i++)
                        {
                            newArray[i] = array[(i + head) & mask];
                        }
                        array = newArray;

                        // Reset the field values, incl. the mask.
                        headIndex = 0;
                        tailIndex = tail = count;
                        mask = (mask << 1) | 1;
                    }

                    array[tail & mask] = obj;
                    tailIndex = tail + 1;
                }
            }
        }

        public bool LocalPop(out T obj)
        {
            var tail = tailIndex;
            if(headIndex >= tail) // m_headIndex может действительно уехать вперед, см. TrySteal
            {
                obj = null;
                return false;
            }

            tail -= 1;
            Interlocked.Exchange(ref tailIndex, tail); // Interlocked, чтобы гарантировать, что запись не произойдет позже чтения m_headIndex в следующей строчке (C# memory model)

            if(headIndex <= tail)   
            {
                obj = array[tail & mask];
                return true;
            }
            else
            {
                lock (foreignLock)
                {
                    if(headIndex <= tail)
                    {
                        obj = array[tail & mask];
                        return true;
                    }
                    else
                    {
                        tailIndex = tail + 1;
                        obj = null;
                        return false;
                    }
                }
            }
        }

        public bool TrySteal(out T obj)
        {
            obj = default;
            var taken = false;
            try
            {
                Monitor.TryEnter(foreignLock, ref taken);
                if(taken)
                {
                    var head = headIndex;
                    Interlocked.Exchange(ref headIndex, head + 1);  // Interlocked по аналогичным причинам, что и в LocalPop

                    if(head < tailIndex)
                    {
                        obj = array[head & mask];
                        return true;
                    }
                    else
                    {
                        headIndex = head; // проиграли гонку
                        return false;
                    }
                }
            }
            finally
            {
                if (taken)
                {
                    Monitor.Exit(foreignLock);
                }
            }

            return false;
        }
    }
}