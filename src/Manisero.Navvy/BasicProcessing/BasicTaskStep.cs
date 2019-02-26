using System;
using System.Threading;
using Manisero.Navvy.Core;

namespace Manisero.Navvy.BasicProcessing
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public Action<IProgress<byte>, CancellationToken> Body { get; }

        public BasicTaskStep(
            string name,
            Action body,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, (p, c) => body(), executionCondition)
        {
        }

        public BasicTaskStep(
            string name,
            Action<IProgress<byte>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? (x => x == TaskOutcome.Successful);
            Body = body;
        }

        public static BasicTaskStep Empty(string name)
        {
            return new BasicTaskStep(name, () => { });
        }
    }

    public static class TaskStepBuilderUtils
    {
        public static BasicTaskStep Basic(
            this TaskStepBuilder builder,
            string name,
            Action body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(name, body, executionCondition);

        public static BasicTaskStep Basic(
            this TaskStepBuilder builder,
            string name,
            Action<IProgress<byte>, CancellationToken> body,
            Func<TaskOutcome, bool> executionCondition = null)
            => new BasicTaskStep(name, body, executionCondition);
    }
}
