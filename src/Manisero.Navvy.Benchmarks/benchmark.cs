using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Dataflow;
using Manisero.Navvy.PipelineProcessing;

namespace Manisero.Navvy.Benchmarks
{
    public class benchmark
    {
        private const int BatchesCount = 100;
        private const int BatchSize = 10000;

        [Benchmark(Baseline = true)]
        public long plain_sum()
        {
            return GetInput().SelectMany(x => x).Sum();
        }

        [Benchmark]
        public long basic_processing()
        {
            var sum = 0L;

            var taskExecutor = new TaskExecutorBuilder().Build();
            
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Sum",
                        () => sum = GetInput().SelectMany(x => x).Sum())
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___sequential___update_per_item()
        {
            var sum = 0L;

            var taskExecutor = new TaskExecutorBuilder().Build();

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.ItemBody(
                                "Update Sum",
                                x => sum += x)
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___sequential___update_per_batch()
        {
            var sum = 0L;
            
            var taskExecutor = new TaskExecutorBuilder().Build();
            
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.BatchBody(
                                "Update Sum",
                                x => sum += x.Sum())
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___Dataflow___not_parallel___update_per_item()
        {
            var sum = 0L;

            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.ItemBody(
                                "Update Sum",
                                x => sum += x)
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___Dataflow___not_parallel___update_per_batch()
        {
            var sum = 0L;

            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();
            
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.BatchBody(
                                "Update Sum",
                                x => sum += x.Sum())
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___Dataflow___parallel___update_per_item()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var sum = 0L;

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.ItemBody(
                                "Update Sum",
                                x => Interlocked.Add(ref sum, x),
                                true)
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        [Benchmark]
        public long pipeline_processing___Dataflow___parallel___update_per_batch()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var sum = 0L;

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<long>(
                        "Sum",
                        GetInput(),
                        BatchesCount,
                        new List<PipelineBlock<long>>
                        {
                            PipelineBlock<long>.BatchBody(
                                "Update Sum",
                                x => Interlocked.Add(ref sum, x.Sum()),
                                true)
                        })
                }
            };

            taskExecutor.Execute(task);

            return sum;
        }

        private IEnumerable<ICollection<long>> GetInput()
        {
            for (var i = 0; i < BatchesCount; i++)
            {
                yield return Enumerable.Repeat(1L, BatchSize).ToList();
            }
        }
    }
}
