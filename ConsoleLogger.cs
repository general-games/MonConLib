using System;
using System.Collections.Generic;

namespace MonCon
{

    public class ConsoleLogger
    {
        private readonly List<string> _logEntries = new();
        public List<string> Logs => _logEntries;

        public void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("[HH:mm:ss] ");
            _logEntries.Add(timestamp + " | " + message);
        }
    }
}
