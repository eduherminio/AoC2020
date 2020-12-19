using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static AoC_2020.Day_19;

namespace AoC_2020.Benchmarks
{
    public class Day_19_Benchmark : BaseDayBenchmark
    {
        private readonly List<string> _messages;
        private readonly List<Regex> _regexes;

        public Day_19_Benchmark()
        {
            (List<Rule> rules, List<string> messages) = new Day_19().ParseInput();

            var rule8 = rules.First(r => r.Id == 8);
            rule8.Content = "42 | 42 8";

            var rule11 = rules.First(r => r.Id == 11);
            rule11.Content = "42 31 | 42 11 31";

            //_regexes = GenerateCombinations(rule8, rule11, rules);
            _regexes = GenerateCombinations(rule8, rule11, rules, RegexOptions.Compiled);

            _messages = messages;
        }

        [Benchmark(Baseline = true)]
        public long CountMatches_NestedLoops() => Day_19.CountMatches_NestedLoops(_messages, _regexes);

        [Benchmark]
        public long CountMatches_Linq() => Day_19.CountMatches_Linq(_messages, _regexes);

        [Benchmark]
        public long CountMatches_ParallelForEach_ConcurrentDictionary() => Day_19.CountMatches_ParallelForEach_ConcurrentDictionary(_messages, _regexes);

        [Benchmark]
        public long CountMatches_ParallelForEach_Interlock() => Day_19.CountMatches_ParallelForEach_Interlock(_messages, _regexes);

        [Benchmark]
        public long CountMatches_ParallelForEach_Interlock_Break() => Day_19.CountMatches_ParallelForEach_Interlock_Break(_messages, _regexes);

    }
}
