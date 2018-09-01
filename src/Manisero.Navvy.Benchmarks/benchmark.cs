using System;
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
        private const int TotalCount = BatchesCount * BatchSize;

        [Benchmark(Baseline = true)]
        public long plain_sum()
        {
            return GetInput_NotBatched().Sum();
        }

        [Benchmark]
        public TaskResult basic_processing()
        {
            var sum = 0L;

            var taskExecutor = new TaskExecutorBuilder().Build();

            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Sum",
                    () => sum = GetInput_NotBatched().Sum()));

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___sequential___not_batched()
        {
            var taskExecutor = new TaskExecutorBuilder().Build();
            var task = GetPipelineTask_NotBatched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___sequential___batched()
        {
            var taskExecutor = new TaskExecutorBuilder().Build();
            var task = GetPipelineTask_Batched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___not_batched___not_parallel()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_NotBatched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___not_parallel()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_Batched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___not_batched___parallel()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_NotBatched(true);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___parallel()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_Batched(true);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___parallel___no_Interlock()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_Batched_Parallel_NoInterlock();

            return taskExecutor.Execute(task);
        }

        private IEnumerable<long> GetInput_NotBatched()
        {
            return Enumerable.Repeat(1L, TotalCount);
        }

        private IEnumerable<ICollection<long>> GetInput_Batched()
        {
            for (var i = 0; i < BatchesCount; i++)
            {
                yield return Enumerable.Repeat(1L, BatchSize).ToList();
            }
        }

        private TaskDefinition GetPipelineTask_NotBatched(
            bool parallel)
        {
            var sum = 0L;

            return new TaskDefinition(
                new PipelineTaskStep<long>(
                    "Sum",
                    GetInput_NotBatched(),
                    TotalCount,
                    new List<PipelineBlock<long>>
                    {
                        new PipelineBlock<long>(
                            "Update Sum",
                            parallel
                                ? (Action<long>)(x => Interlocked.Add(ref sum, x))
                                : x => sum += x,
                            parallel)
                    }));
        }

        private TaskDefinition GetPipelineTask_Batched(
            bool parallel)
        {
            var sum = 0L;

            return new TaskDefinition(
                new PipelineTaskStep<ICollection<long>>(
                    "Sum",
                    GetInput_Batched(),
                    BatchesCount,
                    new List<PipelineBlock<ICollection<long>>>
                    {
                        new PipelineBlock<ICollection<long>>(
                            "Update Sum",
                            parallel
                                ? (Action<ICollection<long>>)(x => Interlocked.Add(ref sum, x.Sum()))
                                : x => sum += x.Sum(),
                            parallel)
                    }));
        }

        private struct NumbersWithTotal
        {
            public ICollection<long> Numbers { get; set; }
            public long Total { get; set; }
        }

        private TaskDefinition GetPipelineTask_Batched_Parallel_NoInterlock()
        {
            var sum = 0L;

            return new TaskDefinition(
                new PipelineTaskStep<NumbersWithTotal>(
                    "Sum",
                    GetInput_Batched().Select(x => new NumbersWithTotal { Numbers = x }),
                    BatchesCount,
                    new List<PipelineBlock<NumbersWithTotal>>
                    {
                        new PipelineBlock<NumbersWithTotal>(
                            "Calculate Total",
                            x => x.Total = x.Numbers.Sum(),
                            true),
                        new PipelineBlock<NumbersWithTotal>(
                            "Update Sum",
                            x => sum += x.Total)
                    }));
        }
    }
}
