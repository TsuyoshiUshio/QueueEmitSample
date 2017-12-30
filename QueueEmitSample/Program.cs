using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueEmitSample
{
    class Program
    {
        static void Main(string[] args)
        {
            new StorageQueueSample().ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}
