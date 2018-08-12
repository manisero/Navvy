using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Dataflow.StepExecution
{
    public class DataBatch<TData>
    {
        public int Number { get; set; }

        public ICollection<TData> Data { get; set; }
    }
}
