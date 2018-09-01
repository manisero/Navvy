using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core;
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
        public TaskResult pipeline_processing___Dataflow___not_parallel___not_batched()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_NotBatched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___not_parallel___batched()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_Batched(false);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___parallel___not_batched()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_NotBatched(true);

            return taskExecutor.Execute(task);
        }

        [Benchmark]
        public TaskResult pipeline_processing___Dataflow___parallel___batched()
        {
            var taskExecutor = new TaskExecutorBuilder()
                .RegisterDataflowExecution()
                .Build();

            var task = GetPipelineTask_Batched(true);

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
                            x => sum += x,
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
                            x => sum += x.Sum(),
                            parallel)
                    }));
        }
    }
}
