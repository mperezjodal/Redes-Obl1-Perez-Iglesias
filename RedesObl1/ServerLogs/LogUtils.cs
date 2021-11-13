using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace ServerLogs
{
    class LogUtils
    {
        public List<LogEntry> logEntries { get; set; }

        public LogUtils()
        {
            logEntries = new List<LogEntry>();
        }

    }
}
