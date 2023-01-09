using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataParallelismTask.LogParsers
{
    public class ParallelLogParser : ILogParser 
    {
        private readonly FileInfo file;
        private readonly Func<string, string?> tryGetIdFromLine;

        public ParallelLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }
        
        public string[] GetRequestedIdsFromLogFile()
        {
            var lines = File.ReadLines(file.FullName).ToArray();
            Parallel.For(0, lines.Length, i => lines[i] = tryGetIdFromLine(lines[i]));
            return lines
                .Where(id => id != null)
                .ToArray()!;
        }
    }
}