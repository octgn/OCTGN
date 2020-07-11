using System;
using System.Threading.Tasks;

namespace Octgn.Core
{
    public static class TaskContinuationHelpers
    {
        public static Task Error(this Task anticedent, Action<Exception> onError) {
            return anticedent.ContinueWith((completeTask) => {
                if (completeTask.IsFaulted) {
                    onError(completeTask.Exception);
                }
            });
        }
    }
}
