using System;

namespace Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; set; }

        public Action Body { get; set; }
    }
}
