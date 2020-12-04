using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_04_Benchmark : BaseDayBenchmark
    {
        private readonly Day_04 _problem = new();

        [Benchmark]
        public string Part2_AsLittleRegexAsPossible() => _problem.Part2_AsLittleRegexAsPossible();

        [Benchmark]
        public string Part2_Regex() => _problem.Part2_Regex();

        [Benchmark]
        public string Part2_Regex_RawFile() => _problem.Part2_Regex_RawFile();
    }
}
