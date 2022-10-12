using System;
using System.Threading.Tasks;

namespace Manisero.Navvy.PipelineProcessing.Models
{
    public interface IPipelineBlock
    {
        string Name { get; }
    }

    public class PipelineBlock<TItem> : IPipelineBlock
    {
        public string Name { get; }

        public Func<TItem, Task> Body { get; }

        public int MaxDegreeOfParallelism { get; }

        public PipelineBlock(
            string name,
            Func<TItem, Task> body,
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

        public int ItemNumber { get; set; }
    }

    public static class PipelineBlockExtensions
    {
        public static PipelineBlockExceptionData GetExceptionData<TData>(
            this PipelineBlock<TData> block,
            int itemNumber)
            => new PipelineBlockExceptionData
            {
                BlockName = block.Name,
                ItemNumber = itemNumber
            };
    }
}
