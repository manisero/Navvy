using BenchmarkDotNet.Attributes;

namespace Manisero.Navvy.Benchmarks
{
    public class benchmark
    {
        [Benchmark(Baseline = true)]
        public void option1()
        {
        }

        [Benchmark]
        public void option2()
        {
        }
    }
}
