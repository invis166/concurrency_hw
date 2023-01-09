using System;
using System.IO;
using System.Linq;

namespace DataParallelismTask.LogParsers
{
    public class PLinqLogParser : ILogParser 
    {
        private readonly FileInfo file;
        private readonly Func<string, string?> tryGetIdFromLine;

        public PLinqLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }
        
        public string[] GetRequestedIdsFromLogFile()
        {
            var lines = File.ReadLines(file.FullName);
            return lines
                .AsParallel()
                .AsOrdered()
                .Select(tryGetIdFromLine)
                .AsSequential()
                .Where(id => id != null)
                .ToArray()!;
        }
    }
}