using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutors.DataflowPipelineStepExecution
{
    public class DataBatch<TData>
    {
        public int Number { get; set; }

        public ICollection<TData> Data { get; set; }
    }
}
