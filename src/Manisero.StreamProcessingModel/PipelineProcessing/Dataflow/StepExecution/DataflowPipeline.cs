using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Dataflow.StepExecution
{
    public class DataflowPipeline<TData>
    {
        public ITargetBlock<DataBatch<TData>> InputBlock { get; set; }

        public Task Completion { get; set; }
    }
}
