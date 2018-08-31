using BenchmarkDotNet.Running;

namespace Manisero.Navvy.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<benchmark>();
        }
    }
}
