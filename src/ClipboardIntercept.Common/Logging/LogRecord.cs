using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common.Logging
{
    public class LogRecord
    {
        public string? LogLevel { get; set; }
        public int ThreadId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public string? Message { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
        public string? ExceptionSource { get; set; }

    }
}
