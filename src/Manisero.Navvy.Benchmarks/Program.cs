using BenchmarkDotNet.Running;

namespace Manisero.Navvy.Benchmarks
{
    class Program
    {
        public static bool IsTestRun { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length != 0 && args[0] == "test")
            {
                IsTestRun = true;
            }

            BenchmarkRunner.Run<benchmark>();
        }
    }
}
