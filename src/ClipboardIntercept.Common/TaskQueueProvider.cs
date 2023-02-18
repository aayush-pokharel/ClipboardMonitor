using ClipboardIntercept.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common
{
    public class TaskQueueProvider : Interfaces.ITaskQueueProvider
    {
        private readonly IServiceProvider _serviceProvider;
        public TaskQueueProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IBackgroundTaskQueue GetTaskQueue(Type type) =>
            _serviceProvider.GetServices<IBackgroundTaskQueue>().First(q => q.GetType() == type);
    }
}
