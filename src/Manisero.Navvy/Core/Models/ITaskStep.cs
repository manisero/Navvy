using System;

namespace Manisero.Navvy.Core.Models
{
    public interface ITaskStep
    {
        string Name { get; }

        Func<TaskOutcome, bool> ExecutionCondition { get; }
    }

    public static class TaskStepUtils
    {
        public static readonly Func<TaskOutcome, bool> AlwaysExecuteCondition = _ => true;
    }
}
