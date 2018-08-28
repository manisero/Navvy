using System.Collections.Generic;
using System.Diagnostics;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Dataflow.StepExecution
{
    public class DataBatch<TData>
    {
        public int Number { get; set; }

        public ICollection<TData> Data { get; set; }

        public Stopwatch ProcessingStopwatch { get; set; }
    }
}
