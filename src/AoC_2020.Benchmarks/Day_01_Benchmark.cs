using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_01_Benchmark
    {
        private readonly Day_01 _problem = new Day_01();

        [Benchmark]
        public string CalculateIteratively() => _problem.CalculateIteratively();

        [Benchmark]
        public string CalculateUsingCombinations() => _problem.CalculateUsingCombinations(3);
    }
}
