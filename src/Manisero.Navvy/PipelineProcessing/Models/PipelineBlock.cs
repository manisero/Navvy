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

        public bool Parallel { get; }

        public PipelineBlock(
            string name,
            Action<TItem> body,
            bool parallel = false)
        {
            Name = name;
            Body = body;
            Parallel = parallel;
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
