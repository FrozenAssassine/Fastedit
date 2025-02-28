using System;
using System.Diagnostics;

namespace Fastedit.Helper;

internal class BenchmarkHelper
{
    public static void Benchmark(Action action, string text)
    {
        Stopwatch sw = new Stopwatch();
        long memoryBefore = GC.GetTotalMemory(true);
        sw.Start();
        action?.Invoke();
        sw.Stop();
        long memoryAfter = GC.GetTotalMemory(true);
        long memoryUsage = memoryAfter - memoryBefore;

        Debug.WriteLine($"Benchmark {text} took {sw.ElapsedMilliseconds + ":" + sw.ElapsedTicks} and used " + (memoryUsage / 1000) + "KB of memory");
    }
}
