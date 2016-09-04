using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyCache.UnitTests
{
    public static class AsyncHelper
    {
        public static Task<T> CreateCancelledTask<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task<T> CreateTaskWithException<T, TException>() where TException : Exception
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(Activator.CreateInstance<TException>());
            return tcs.Task;
        }
    }
}
