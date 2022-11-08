using System;
using System.Diagnostics;
using System.Threading;


namespace laba 
{
    class Program
    {
        const long delta = 10;

        public static void Main()
        {
            SetProcessAffinity();
            SetHighProcessPriority();

            int experimentsCount = 10;
            double timeQuantumsSum = 0;
            for (int i = 0; i < experimentsCount; i++)
            {
                // tasksCount must be equal to 2*(CPU cores)
                var result = GetExperimetResult(16);
                Console.WriteLine($"Experiment {i + 1}: {result}");
                timeQuantumsSum += result;
            }

            Console.WriteLine($"Time quantum is {timeQuantumsSum / experimentsCount}");
        }
        private static void SetHighProcessPriority()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        }
        private static void SetProcessAffinity()
        {
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) (1 << 0 | 2 << 1);
        }

        private static double GetExperimetResult(int tasksCount = 16)
        {
            var tasks = new List<Task<double>>();
            for (int i = 0; i < tasksCount; i++)
            {
                tasks.Add(new Task<double>(() => ComputeTimeQuantim()));
            }

            foreach (var task in tasks) 
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());

            double totalElapsed = 0;
            int nonZeroTimeQuantumsCount = 0;
            foreach (var task in tasks)
            {
                totalElapsed += task.Result;
                if (task.Result > 0) 
                {
                    nonZeroTimeQuantumsCount++;
                }
            }

            return (double) (totalElapsed) / nonZeroTimeQuantumsCount;
        }

        private static double ComputeTimeQuantim(int iterationsCount = 10000000)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            long totalElapsed = 0;
            var sw = new Stopwatch();

            sw.Start();
            var switchesCount = 0;
            var previousTime = sw.ElapsedMilliseconds;
            for (var i = 0; i < iterationsCount; i++)
            {
                var currentTime = sw.ElapsedMilliseconds;
                var elapsed = currentTime - previousTime;
                // if more than delta ms has elapsed, then a thread switch has probably occurred
                if (elapsed > delta) {
                    totalElapsed += elapsed;
                    switchesCount++;
                }
                previousTime = currentTime;
            }
            sw.Stop();

            // it may happen that no thead switchings has occurred
            return switchesCount > 0 ? totalElapsed / switchesCount : 0;
        }
    }
}
