using ClipboardIntercept.Common;
using ClipboardIntercept.Common.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common
{
    public class FileWriteConfigurationService : IFileWriteConfigurationService
    {
        private readonly ClipboardIntercept.Common.FileWriteConfiguration _config;
        public FileWriteConfigurationService(IOptions<ClipboardIntercept.Common.FileWriteConfiguration> config)
        {
            _config = config.Value;
        }
        public FileWriteConfiguration GetConfig()
        {
            return _config;
        }
    }
}
