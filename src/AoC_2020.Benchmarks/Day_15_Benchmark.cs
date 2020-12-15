using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_15_Benchmark : BaseDayBenchmark
    {
        private readonly Day_15 _problem = new();

        [Benchmark]
        public string PlayMemoryGame_Dictionary() => _problem.PlayMemoryGame_Dictionary(30_000_000);

        [Benchmark]
        public string PlayMemoryGame_List() => _problem.PlayMemoryGame_List(30_000_000);

        [Benchmark]
        public string PlayMemoryGame_Array() => _problem.PlayMemoryGame_Array(30_000_000);
    }
}
