using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common.Logging
{
    public class RollingFileLoggerConfiguration
    {
        public int EventId { get; set; }
        public string? LogLevel { get; set; }
        public string[] LogFields { get; init; }
        public Common.FileWriteConfiguration FileWriteConfiguration { get; set; }
    }
}
