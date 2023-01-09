using System;

namespace CustomThreadPool.ThreadPools
{
    public interface IThreadPool
    {
        void EnqueueAction(Action task);
        long GetTasksProcessedCount();  
    }
}