using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Reflection;

namespace AoC_2020.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, new DebugInProcessConfig());
#else
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
#endif
        }
    }
}
