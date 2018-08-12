using System;

namespace Manisero.StreamProcessingModel.Models
{
    public class TaskExecutionException : Exception
    {
        public string StepName { get; }

        public object AdditionalData { get; }

        public TaskExecutionException(
            Exception innerException,
            ITaskStep taskStep,
            object additionalData = null)
            : base("Errors while executing task. See inner exception.", innerException)
        {
            StepName = taskStep.Name;
            AdditionalData = additionalData;
        }
    }
}
