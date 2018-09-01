using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipeline<TItem>
    {
        public ITargetBlock<PipelineItem<TItem>> InputBlock { get; set; }

        public Task Completion { get; set; }
    }
}
