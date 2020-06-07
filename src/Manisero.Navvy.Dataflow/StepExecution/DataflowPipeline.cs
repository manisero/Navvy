using System;
using System.Threading.Tasks;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipeline
    {
        public Func<Task> Execute { get; set; }
    }
}
