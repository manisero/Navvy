using System;

namespace Manisero.StreamProcessingModel.Models
{
    public interface ITaskStep
    {
        string Name { get; }

        Func<TaskOutcome, bool> ExecutionCondition { get; }
    }
}
