using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors.DataflowPipelineStepExecution
{
    public class DataBatch<TData>
    {
        public int Number { get; set; }

        public ICollection<TData> Data { get; set; }
    }
}
