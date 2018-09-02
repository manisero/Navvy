using System;
using System.Collections.Concurrent;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    public class DataflowExecutionContext
    {
        public TaskStepExecutionContext StepContext { get; set; }

        public PipelineExecutionEvents Events { get; set; }

        public ConcurrentDictionary<string, TimeSpan> TotalBlockDurations { get; set; }
    }
}
