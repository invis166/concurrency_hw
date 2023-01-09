using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataParallelismTask.LogParsers
{
    public class ThreadLogParser : ILogParser 
    {
        private readonly FileInfo file;
        private readonly Func<string, string?> tryGetIdFromLine;

        public ThreadLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }
        
        public string[] GetRequestedIdsFromLogFile()
        {
            var lines = File.ReadLines(file.FullName).ToArray();
            var cpuCount = Environment.ProcessorCount;
            var threads = new List<Thread>(cpuCount);
            var leftBorder = 0;
            var segmentLenght = 1 + lines.Length / cpuCount;
            for (int i = 1; i <= cpuCount; i++)
            {
                var rightBorder = Math.Min(lines.Length, i * segmentLenght);
                var lBorder = leftBorder;
                threads.Add(new Thread(() =>
                {
                    for (int j = lBorder; j < rightBorder; j++)
                    {
                        lines[j] = tryGetIdFromLine(lines[j]);
                    }
                }));
                threads[^1].Start();
                leftBorder = rightBorder;
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            return lines
                .Where(id => id != null)
                .ToArray();
        }
    }
}