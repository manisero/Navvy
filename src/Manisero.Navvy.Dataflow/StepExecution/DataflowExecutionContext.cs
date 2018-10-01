using System;
using System.Collections.Concurrent;
using Manisero.Navvy.Core;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowExecutionContext
    {
        public TaskStepExecutionContext StepContext { get; set; }

        public ExecutionEventsGroup<PipelineExecutionEvents> Events { get; set; }

        public ConcurrentDictionary<string, TimeSpan> TotalBlockDurations { get; set; }
    }
}
