using System;

namespace Manisero.Navvy.Core.Models
{
    public interface ITaskStep
    {
        string Name { get; }

        Func<TaskOutcome, bool> ExecutionCondition { get; }
    }
}
