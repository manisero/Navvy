using System;

namespace Manisero.Navvy
{
    /// <summary>Step of <see cref="TaskDefinition"/>. Defines single piece of work to be done. Executed conditionally (see <see cref="ExecutionCondition"/>).</summary>
    public interface ITaskStep
    {
        string Name { get; }

        /// <summary>
        /// Step execution condition accepting most severe outcome among previous steps. On false, step will not be executed.
        /// For typical steps just doing some work, use <see cref="TaskStepUtils.DefaultExecutionCondition"/>.
        /// For steps that should be executed even if previous steps failed (e.g. they free resources), use  <see cref="TaskStepUtils.AlwaysExecuteCondition"/>.
        /// </summary>
        Func<TaskOutcome, bool> ExecutionCondition { get; }
    }

    public static class TaskStepUtils
    {
        /// <summary>x => x == <see cref="TaskOutcome.Successful"/></summary>
        public static readonly Func<TaskOutcome, bool> DefaultExecutionCondition = x => x == TaskOutcome.Successful;

        /// <summary>_ => true</summary>
        public static readonly Func<TaskOutcome, bool> AlwaysExecuteCondition = _ => true;
    }
}
