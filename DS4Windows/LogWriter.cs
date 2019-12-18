using System.Collections.Generic;
using System.IO;

namespace DS4WinWPF
{
    public class LogWriter
    {
        private string filename;
        private List<LogItem> logCol;

        public LogWriter(string filename, List<LogItem> col)
        {
            this.filename = filename;
            logCol = col;
        }

        public void Process()
        {
            List<string> outputLines = new List<string>();
            foreach(LogItem item in logCol)
            {
                outputLines.Add($"{item.Datetime}: {item.Message}");
            }

            try
            {
                StreamWriter stream = new StreamWriter(filename);
                foreach(string line in outputLines)
                {
                    stream.WriteLine(line);
                }
                stream.Close();
            }
            catch { }
        }
    }
}
