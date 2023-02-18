using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common.Interfaces
{
    public interface ITaskQueueProvider
    {
        IBackgroundTaskQueue GetTaskQueue(Type type);
    }
}
