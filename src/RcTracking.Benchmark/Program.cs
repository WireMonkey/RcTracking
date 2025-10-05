using BenchmarkDotNet.Running;
using BenchmarkSuite1;

namespace BenchmarkSuite2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Run only our specific sorting benchmark
            BenchmarkRunner.Run<FlightServiceSortByDateBenchmark>();
        }
    }
}
