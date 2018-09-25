using System;

namespace Manisero.Navvy.PipelineProcessing.Models
{
    public interface IPipelineBlock
    {
        string Name { get; }
    }

    public class PipelineBlock<TItem> : IPipelineBlock
    {
        public string Name { get; }

        public Action<TItem> Body { get; }

        public int MaxDegreeOfParallelism { get; }

        public PipelineBlock(
            string name,
            Action<TItem> body,
            int maxDegreeOfParallelism = 1)
        {
            Name = name;
            Body = body;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }
    }

    public class PipelineBlockExceptionData
    {
        public string BlockName { get; set; }
    }

    public static class PipelineBlockExtensions
    {
        public static PipelineBlockExceptionData GetExceptionData<TData>(
            this PipelineBlock<TData> block)
            => new PipelineBlockExceptionData
            {
                BlockName = block.Name
            };
    }
}
