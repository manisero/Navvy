using System.Collections.Generic;
using System.Diagnostics;

namespace Manisero.StreamProcessingModel.Dataflow.StepExecution
{
    internal class DataBatch<TData>
    {
        public int Number { get; set; }

        public ICollection<TData> Data { get; set; }

        public Stopwatch ProcessingStopwatch { get; set; }
    }
}
