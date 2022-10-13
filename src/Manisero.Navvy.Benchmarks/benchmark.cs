using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Dataflow;
using Manisero.Navvy.PipelineProcessing;

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
                    AddJob(Job.Dry);
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
                TaskStepBuilder.Build.Basic(
                    "Sum",
                    () => sum = GetInput_NotBatched_Materialized().Sum()));
        }

        [Benchmark]
        public async Task basic_processing()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___sequential___not_batched))]
        public void setup___pipeline_processing___sequential___not_batched()
        {
            _executor = new TaskExecutorBuilder().Build();
            _task = GetPipelineTask_NotBatched(false);
        }

        [Benchmark]
        public async Task pipeline_processing___sequential___not_batched()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___sequential___batched))]
        public void setup___pipeline_processing___sequential___batched()
        {
            _executor = new TaskExecutorBuilder().Build();
            _task = GetPipelineTask_Batched(false);
        }

        [Benchmark]
        public async Task pipeline_processing___sequential___batched()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___not_batched___not_parallel))]
        public void setup___pipeline_processing___Dataflow___not_batched___not_parallel()
        {
            _executor = new TaskExecutorBuilder()
                .UseDataflowPipelineExecution()
                .Build();

            _task = GetPipelineTask_NotBatched(false);
        }

        [Benchmark]
        public async Task pipeline_processing___Dataflow___not_batched___not_parallel()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___not_parallel))]
        public void setup___pipeline_processing___Dataflow___batched___not_parallel()
        {
            _executor = new TaskExecutorBuilder()
                .UseDataflowPipelineExecution()
                .Build();

            _task = GetPipelineTask_Batched(false);
        }

        [Benchmark]
        public async Task pipeline_processing___Dataflow___batched___not_parallel()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___not_batched___parallel))]
        public void setup___pipeline_processing___Dataflow___not_batched___parallel()
        {
            _executor = new TaskExecutorBuilder()
                .UseDataflowPipelineExecution()
                .Build();

            _task = GetPipelineTask_NotBatched(true);
        }

        [Benchmark]
        public async Task pipeline_processing___Dataflow___not_batched___parallel()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___parallel))]
        public void setup___pipeline_processing___Dataflow___batched___parallel()
        {
            _executor = new TaskExecutorBuilder()
                .UseDataflowPipelineExecution()
                .Build();

            _task = GetPipelineTask_Batched(true);
        }

        [Benchmark]
        public async Task pipeline_processing___Dataflow___batched___parallel()
        {
            await _executor.Execute(_task);
        }

        [GlobalSetup(Target = nameof(pipeline_processing___Dataflow___batched___parallel___no_Interlock))]
        public void setup___pipeline_processing___Dataflow___batched___parallel___no_Interlock()
        {
            _executor = new TaskExecutorBuilder()
                .UseDataflowPipelineExecution()
                .Build();

            _task = GetPipelineTask_Batched_Parallel_NoInterlock();
        }

        [Benchmark]
        public async Task pipeline_processing___Dataflow___batched___parallel___no_Interlock()
        {
            await _executor.Execute(_task);
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
                TaskStepBuilder.Build.Pipeline<long>(
                        "Sum")
                    .WithInput(
                        GetInput_NotBatched_NotMaterialized(),
                        TotalCount)
                    .WithBlock(
                        "Update Sum",
                        parallel
                            ? (Action<long>)(x => Interlocked.Add(ref sum,
                                x))
                            : x => sum += x,
                        parallel ? 6 : 1)
                    .Build());
        }

        private TaskDefinition GetPipelineTask_Batched(
            bool parallel)
        {
            var sum = 0L;

            return new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<ICollection<long>>(
                        "Sum")
                    .WithInput(
                        GetInput_Batched(),
                        BatchesCount)
                    .WithBlock(
                        "Update Sum",
                        parallel
                            ? (Action<ICollection<long>>)(x => Interlocked.Add(ref sum,
                                x.Sum()))
                            : x => sum += x.Sum(),
                        parallel ? 6 : 1)
                    .Build());
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
                TaskStepBuilder.Build.Pipeline<NumbersWithTotal>(
                        "Sum")
                    .WithInput(
                        GetInput_Batched().Select(x => new NumbersWithTotal { Numbers = x }),
                        BatchesCount)
                    .WithBlock(
                        "Calculate Total",
                        x => x.Total = x.Numbers.Sum(),
                        6)
                    .WithBlock(
                        "Update Sum",
                        x => sum += x.Total)
                    .Build());
        }
    }
}
