using System;

namespace Manisero.StreamProcessingModel.Core.Models
{
    public interface ITaskStep
    {
        string Name { get; }

        Func<TaskOutcome, bool> ExecutionCondition { get; }
    }
}
