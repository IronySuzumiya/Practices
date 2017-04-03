using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nadeko.Utilities
{
    public static class CodeTimer
    {
        public static void Initialize()
        {
            // Avoid the interference of CPU scheduling
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // Warm-up the Timer
            Time("", 1, () => { });
        }

        public static void Time(string name, int iteration, Action action)
        {
            if (string.IsNullOrEmpty(name)) return;

            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(name);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            var gcCnts = new int[GC.MaxGeneration + 1];
            for(int i = 0; i <= GC.MaxGeneration; ++i)
            {
                gcCnts[i] = GC.CollectionCount(i);
            }

            var watch = new Stopwatch();
            watch.Start();
            var cycleCnt = GetCycleCnt();
            for(int i = 0; i < iteration; ++i)
            {
                action();
            }
            var cpuCycles = GetCycleCnt() - cycleCnt;
            watch.Stop();

            Console.ForegroundColor = currentForeColor;
            Console.WriteLine("\tTime Elapsed:\t" + watch.ElapsedMilliseconds.ToString("N0") + "ms");
            Console.WriteLine("\tCPU Cycles:\t" + cpuCycles.ToString("N0"));

            for(int i = 0; i <= GC.MaxGeneration; ++i)
            {
                int cnt = GC.CollectionCount(i) - gcCnts[i];
                Console.WriteLine("\tGen " + i + ": \t\t" + cnt);
            }

            Console.WriteLine();
        }

        private static ulong GetCycleCnt()
        {
            ulong cycleCnt = 0;
            QueryThreadCycleTime(GetCurrentThread(), ref cycleCnt);
            return cycleCnt;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();
    }
}
