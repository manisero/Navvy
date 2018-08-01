using System;

namespace Manisero.StreamProcessingModel.Models.TaskSteps
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; }

        public Action Body { get; }

        public BasicTaskStep(
            string name,
            Action body)
        {
            Name = name;
            Body = body;
        }

        public static BasicTaskStep Empty(string name)
        {
            return new BasicTaskStep(name, () => { });
        }
    }
}
