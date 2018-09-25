using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Dataflow;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Benchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser, RankColumn]
    public class benchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                if (Program.IsTestRun)
                {
                    Add(Job.Dry);
                }
            }
        }

        [Params(1, 100)]
        public int BatchesCount { get; set; }

        [Params(1, 10000)]
        public int BatchSize { get; set; }

        private int TotalCount => BatchesCount * BatchSize;

        private ITaskExecutor _executor;
        private TaskDefinition _task;

        [Benchmark(Baseline = true)]
        public long plain_sum()
        {
            return GetInput_NotBatched_Materialized().Sum();
        }

        [GlobalSetup(Target = nameof(basic_processing))]
        public void setup___basic_processing()
        {
            _executor = new TaskExecutorBuilder().Build();

            var sum = 0L;

            _task = new TaskDefinition(
                new BasicTaskStep(
                    "Sum",
                    () => sum = GetInput_NotBatched_Materialized().Sum()));
        }

        [Benchmark]
        public TaskResult basic_processing()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___sequential___not_batched))]
        public void setup___pipeline_processing___sequential___not_batched()
        {
            _executor = new TaskExecutorBuilder().Build();
            _task = GetPipelineTask_NotBatched(false);
        }

        [Benchmark]
        public TaskResult pipeline_processing___sequential___not_batched()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___sequential___batched))]
        public void setup___pipeline_processing___sequential___batched()
        {
            _executor = new TaskExecutorBuilder().Build();
            _task = GetPipelineTask_Batched(false);
        }

        [Benchmark]
        public TaskResult pipeline_processing___sequential___batched()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___not_batched___not_parallel))]
        public void setup___pipeline_processing___Dataflow___not_batched___not_parallel()
        {
            _executor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            _task = GetPipelineTask_NotBatched(false);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___not_batched___not_parallel()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___not_parallel))]
        public void setup___pipeline_processing___Dataflow___batched___not_parallel()
        {
            _executor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            _task = GetPipelineTask_Batched(false);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___not_parallel()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___not_batched___parallel))]
        public void setup___pipeline_processing___Dataflow___not_batched___parallel()
        {
            _executor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            _task = GetPipelineTask_NotBatched(true);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___not_batched___parallel()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___parallel))]
        public void setup___pipeline_processing___Dataflow___batched___parallel()
        {
            _executor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            _task = GetPipelineTask_Batched(true);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___parallel()
        {
            return _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___parallel___no_Interlock))]
        public void setup___pipeline_processing___Dataflow___batched___parallel___no_Interlock()
        {
            _executor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            _task = GetPipelineTask_Batched_Parallel_NoInterlock();
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___batched___parallel___no_Interlock()
        {
            return _executor.Execute(_task);
        }

        private ICollection<long> GetInput_NotBatched_Materialized()
        {
            var input = new long[TotalCount];

            for (var i = 0; i < BatchSize; i++)
            {
                input[i] = 1L;
            }

            return input;
        }

        private IEnumerable<long> GetInput_NotBatched_NotMaterialized()
        {
            return Enumerable.Repeat(1L, TotalCount);
        }

        private IEnumerable<ICollection<long>> GetInput_Batched()
        {
            for (var batchIndex = 0; batchIndex < BatchesCount; batchIndex++)
            {
                var batch = new long[BatchSize];

                for (var itemIndex = 0; itemIndex < BatchSize; itemIndex++)
                {
                    batch[itemIndex] = 1L;
                }

                yield return batch;
            }
        }

        private TaskDefinition GetPipelineTask_NotBatched(
            bool parallel)
        {
            var sum = 0L;

            return new TaskDefinition(
                new PipelineTaskStep<long>(
                    "Sum",
                    GetInput_NotBatched_NotMaterialized(),
                    TotalCount,
                    new List<PipelineBlock<long>>
                    {
                        new PipelineBlock<long>(
                            "Update Sum",
                            parallel
                                ? (Action<long>)(x => Interlocked.Add(ref sum, x))
                                : x => sum += x,
                            parallel ? 6 : 1)
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
                            parallel ? 6 : 1)
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
                            6),
                        new PipelineBlock<NumbersWithTotal>(
                            "Update Sum",
                            x => sum += x.Total)
                    }));
        }
    }
}
