using System;
using System.Threading;

namespace CustomThreadPool.ThreadPools
{
    public class DotNetThreadPoolWrapper : IThreadPool
    {
        private long processedTask = 0L;
        
        public void EnqueueAction(Action task)
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate
            {
                task.Invoke();
                Interlocked.Increment(ref processedTask);
            }, null);
        }

        public long GetTasksProcessedCount() => processedTask;
    }
}