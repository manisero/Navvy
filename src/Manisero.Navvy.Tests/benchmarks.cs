using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.Navvy.Tests
{
    public class benchmarks
    {
        private readonly ITestOutputHelper _output;

        public benchmarks(
            ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(100, 10000)]
        public void test(int batchesCount, int batchSize)
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterTaskExecutionEvents(
                    new TaskExecutionEvents(
                        taskEnded: x => _output.WriteLine($"Duration: {x.Duration.TotalMilliseconds}ms.")))
                .Build();

            var sum = 0L;

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(batchesCount, batchSize),
                        batchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.BatchBody(
                                "Update Sum",
                                x => Interlocked.Add(ref sum, x.Sum()))
                        })
                }
            };

            taskExecutor.Execute(task);

            sum.ShouldBeEquivalentTo((long)batchesCount * batchSize);
        }

        private IEnumerable<ICollection<long>> GetInput(
            int batchesCount,
            int batchSize)
        {
            for (var i = 0; i < batchesCount; i++)
            {
                yield return Enumerable.Repeat(1L, batchSize).ToList();
            }
        }
    }
}
