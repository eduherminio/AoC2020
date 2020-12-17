using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_17_Benchmark : BaseDayBenchmark
    {
        private readonly Day_17 _problem = new();

        [Benchmark(Baseline = true)]
        public string Part2_Dictionary() => _problem.Part2_Dictionary();

        [Benchmark]
        public string Part2_Set() => _problem.Part2_Set();

        [Benchmark]
        public string Part2_GameOfLife() => _problem.Part2_GameOfLife();

        [Benchmark]
        public string Part2_GameOfLife_CachingNeighbours() => _problem.Part2_GameOfLife_CachingNeighbours();
    }
}
