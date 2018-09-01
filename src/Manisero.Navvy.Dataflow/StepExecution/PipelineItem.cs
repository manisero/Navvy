using System.Diagnostics;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class PipelineItem<TItem>
    {
        public int Number { get; set; }

        public TItem Item { get; set; }

        public Stopwatch ProcessingStopwatch { get; set; }
    }
}
