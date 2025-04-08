using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
    static class CriticalTasks
    {
        static HashSet<Task> tasks = new HashSet<Task>();
        static object locker = new object();

        // when starting a Task
        public static void Add(Task t)
        {
            lock (locker)
                tasks.Add(t);
        }

        // When a Tasks completes
        public static void Remove(Task t)
        {
            lock (locker)
                tasks.Remove(t);
        }

        // call it regularly
        public static void Cleanup()
        {
            lock (locker)
                tasks.RemoveWhere(t => t.Status != TaskStatus.Running);
        }

        public static void WaitOnExit()
        {
            var waitfor = tasks.Where(t => t.Status != TaskStatus.Canceled && t.Status != TaskStatus.Faulted && t.Status != TaskStatus.RanToCompletion).ToArray();
            Task.WaitAll(waitfor, 40000);
        }
    }
}
