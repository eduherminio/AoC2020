using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace AoC_2020.Benchmarks
{
    [MarkdownExporterAttribute.GitHub]
    [HtmlExporter]
    [MemoryDiagnoser]
    //[NativeMemoryProfiler]
    public class BaseDayBenchmark
    {
    }
}
