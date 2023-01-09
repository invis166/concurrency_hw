using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CustomThreadPool.Collections;

namespace CustomThreadPool.ThreadPools
{
    public class MyThreadPool : IThreadPool
    {
        private WorkStealingQueue<Action> maxStealingQueue;
        private readonly ConcurrentQueue<Action> generalQueue = new ConcurrentQueue<Action>();
        private readonly Dictionary<int, WorkStealingQueue<Action>> stealingQueues = new Dictionary<int, WorkStealingQueue<Action>>();

        private long processedTask;

        public long GetTasksProcessedCount() => Interlocked.Read(ref processedTask);
        
        public MyThreadPool() : this(Environment.ProcessorCount) {}

        public MyThreadPool(int workersCount)
        {
            if (workersCount <= 0)
            {
                throw new ArgumentException();
            }

            var workers = new Thread[workersCount];
            for (int i = 0; i < workersCount; i++)
            {
                var workStealingQueue = new WorkStealingQueue<Action>();
                workers[i] = new Thread(() => Worker(workStealingQueue)) { IsBackground = true };
                stealingQueues[workers[i].ManagedThreadId] = workStealingQueue;
            }

            maxStealingQueue = stealingQueues[workers[0].ManagedThreadId];

            foreach (var worker in workers)
            {
                worker.Start();
            }
        }

        private void Worker(WorkStealingQueue<Action> currentQueue)
        {
            Action task;
            while (true)
            {
                LocalCompleteTasks(currentQueue);
                if (generalQueue.TryDequeue(out task) 
                    || maxStealingQueue.TrySteal(out task))
                {
                    CompleteTask(task, currentQueue);
                }
                else
                {
                    lock (generalQueue)
                    {
                        Monitor.Wait(generalQueue);
                    }
                }
            }
        }

        private void LocalCompleteTasks(WorkStealingQueue<Action> currentQueue)
        {
            while (currentQueue.LocalPop(out var task))
            {
                CompleteTask(task, currentQueue);
            }
        }

        private void CompleteTask(Action task, WorkStealingQueue<Action> currentQueue)
        {
            task();
            if (maxStealingQueue.Count < currentQueue.Count)
                maxStealingQueue = currentQueue;
            Interlocked.Increment(ref processedTask);     
        }

        public void EnqueueAction(Action task)
        {
            if (task is null)
                throw new ArgumentException();
            
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (stealingQueues.TryGetValue(threadId, out var sq))
            {
                sq.LocalPush(task);
                return;
            }
            generalQueue.Enqueue(task);
            lock (generalQueue)
            {
                Monitor.Pulse(generalQueue);
            }
        }
    }
}
