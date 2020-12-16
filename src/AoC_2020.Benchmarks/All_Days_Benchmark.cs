using AoCHelper;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AoC_2020.Benchmarks
{
    public class All_Days_Benchmark : BaseDayBenchmark
    {
        private readonly List<Type> _allDays = Assembly.GetAssembly(typeof(Day_01))
                .GetTypes()
                .Where(type => typeof(BaseProblem).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();

        [Benchmark]
        public void SolveAll()
        {
            foreach (var type in _allDays)
            {
                var instance = Activator.CreateInstance(type) as BaseDay;

                instance.Solve_1();
                instance.Solve_2();
            }
        }
    }
}
