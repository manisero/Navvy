using System;
using System.Threading;

namespace Manisero.Navvy.BasicProcessing
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; }

        /// <inheritdoc />
        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public Action<IProgress<float>, CancellationToken> Body { get; }

        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public BasicTaskStep(
            string name,
            Action body,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, (p, c) => body(), executionCondition)
        {
        }

        /// <param name="body">Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public BasicTaskStep(
            string name,
            Action<IProgress<float>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? TaskStepUtils.DefaultExecutionCondition;
            Body = body;
        }

        public static BasicTaskStep Empty(string name)
        {
            return new BasicTaskStep(name, () => { });
        }
    }

    public static class TaskStepBuilderUtils
    {
        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Action body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(name, body, executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Action<IProgress<float>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(name, body, executionCondition);
    }
}
