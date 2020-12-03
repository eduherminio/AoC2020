using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_03_Benchmark : BaseDayBenchmark
    {
        private readonly Day_03 _problem = new();

        [Benchmark]
        public string Part2_UnidimensionalStringList() => _problem.Part2_UnidimensionalStringList();

        [Benchmark]
        public string Part2_MultidimensionalCharArray() => _problem.Part2_MultidimensionalCharArray();

        [Benchmark]
        public string Part2_MultidimensionalCharList() => _problem.Part2_MultidimensionalCharList();
    }
}
