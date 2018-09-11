using System;
using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.BasicProcessing
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public Action Body { get; }

        public BasicTaskStep(
            string name,
            Action body,
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
}
