using System;

namespace Manisero.StreamProcessingModel.Models.TaskSteps
{
    public class BasicTaskStep : ITaskStep
    {
        public string Name { get; set; }

        public Action Body { get; set; }
    }
}
