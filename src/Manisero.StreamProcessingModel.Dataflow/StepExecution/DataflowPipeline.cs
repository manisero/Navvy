using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.StreamProcessingModel.Dataflow.StepExecution
{
    internal class DataflowPipeline<TData>
    {
        public ITargetBlock<DataBatch<TData>> InputBlock { get; set; }

        public Task Completion { get; set; }
    }
}
