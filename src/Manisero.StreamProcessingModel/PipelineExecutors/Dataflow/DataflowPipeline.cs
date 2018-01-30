using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.StreamProcessingModel.PipelineExecutors.Dataflow
{
    public class DataflowPipeline<TInput>
    {
        public IEnumerable<TInput> Input { get; }

        public ITargetBlock<TInput> StartBlock { get; }

        public Task Completion { get; set; }

        public DataflowPipeline(
            IEnumerable<TInput> input,
            ITargetBlock<TInput> startBlock,
            Task completion)
        {
            Input = input;
            StartBlock = startBlock;
            Completion = completion;
        }
    }
}