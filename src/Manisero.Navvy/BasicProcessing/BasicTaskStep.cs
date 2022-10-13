using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manisero.Navvy.BasicProcessing
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; }

        /// <inheritdoc />
        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public Func<TaskOutcome, IProgress<float>, CancellationToken, Task> Body { get; }

        /// <param name="body">TaskOutcome parameter is most severe outcome among previous steps. Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public BasicTaskStep(
            string name,
            Func<TaskOutcome, IProgress<float>, CancellationToken, Task> body,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? TaskStepUtils.DefaultExecutionCondition;
            Body = body;
        }

        public static BasicTaskStep Empty(string name)
        {
            return new BasicTaskStep(
                name,
                (to, p, c) => Task.CompletedTask);
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
            => new BasicTaskStep(
                name,
                (o, p, c) =>
                {
                    body();
                    return Task.CompletedTask;
                },
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Func<Task> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                async (o, p, c) => await body(),
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">TaskOutcome parameter is most severe outcome among previous steps.</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Action<TaskOutcome> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                (o, p, c) =>
                {
                    body(o);
                    return Task.CompletedTask;
                },
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">TaskOutcome parameter is most severe outcome among previous steps.</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Func<TaskOutcome, Task> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                async (o, p, c) => await body(o),
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Action<IProgress<float>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                (o, p, c) =>
                {
                    body(p, c);
                    return Task.CompletedTask;
                },
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Func<IProgress<float>, CancellationToken, Task> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                async (o, p, c) => await body(p, c),
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">TaskOutcome parameter is most severe outcome among previous steps. Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Action<TaskOutcome, IProgress<float>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(
                name,
                (o, p, c) =>
                {
                    body(o, p, c);
                    return Task.CompletedTask;
                },
                executionCondition);

        /// <summary>Builds <see cref="BasicTaskStep"/>.</summary>
        /// <param name="body">TaskOutcome parameter is most severe outcome among previous steps. Reported progress values should be between 0.0f and 1.0f (1.0f meaning 100%).</param>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static BasicTaskStep Basic(
            this TaskStepBuilder _,
            string name,
            Func<TaskOutcome, IProgress<float>, CancellationToken, Task> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(name, body, executionCondition);
    }
}
